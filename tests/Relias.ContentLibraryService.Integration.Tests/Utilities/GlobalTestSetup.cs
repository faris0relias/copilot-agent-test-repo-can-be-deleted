using Asp.Versioning;
using DotNet.Testcontainers.Builders;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Relias.ContentLibraryService.Api.Authorization;
using Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;
using Relias.ContentLibraryService.App.Features.Content.Mappings;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Mappings;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.App.Services.Content;
using Relias.ContentLibraryService.Common.Behaviors;
using Relias.ContentLibraryService.Common.Interfaces;
using Relias.ContentLibraryService.Common.Resilience;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.LearningContent;
using Relias.ContentLibraryService.Domain.Language;
using Relias.ContentLibraryService.Domain.Status;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Persistence.Cosmos;
using Relias.ContentLibraryService.Infra.Repositories;
using Reqnroll;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testcontainers.Azurite;
using Testcontainers.CosmosDb;
using Testcontainers.MsSql;
using Relias.ContentLibraryService.Domain.Course.Enums;

namespace Relias.ContentLibraryService.Integration.Tests.Utilities;

[Binding]
public class GlobalTestSetup
{
    private const string IntegrationEnvironment = "Integration";

    private static readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
        .Build();

    private static readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(10000))
        .Build();


    private static CosmosDbContainer _cosmosContainer = default!;

    public static HttpClient? Client { get; private set; }
    public static WebApplicationFactory<Program>? Factory { get; set; }

    // Used for saving changes to entities that inherit from the auditable entity model
    private static ApplicationDbContext? AuditableEntityCompatibleDbContext { get; set; }
    public static CosmosClientWrapper? CosmosClient { get; private set; }

    public static Guid NewAuditableEntityCreatedByUserId { get; private set; }

    public static List<Guid> AccessCourseOrgCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid> NoAccessOrgCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid> UpdateCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid> PublishCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid?> UpdateContentIds { get; private set; } = new List<Guid?>();
    public static List<Guid> PatchCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid> DeleteAccessOrgCourseIds { get; private set; } = new List<Guid>();
    public static List<Guid?> DeleteAccessOrgContentIds { get; private set; } = new List<Guid?>();
    public static List<Guid> DeleteAccessLearningContentIds { get; private set; } = new List<Guid>();
    public static List<LearningContent> LearningContent { get; private set; } = new List<LearningContent>();
    public static List<Guid> GetContentInfoContentIds { get; private set; } = new List<Guid>();

    public static IMainBlobStorageRepository? MainBlobStorageRepository { get; private set; }

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        await _sqlContainer.StartAsync();
        await _azuriteContainer.StartAsync();
        CosmosClient = await CreateCosmosClientWithRetry();

        Factory = GetWebApplicationFactory(CosmosClient);
        Client = Factory.CreateClient();
        AddAuthentication(Client);

        CreateContext();

        AuditableEntityCompatibleDbContext = Factory.Services.GetRequiredService<ApplicationDbContext>();

        await SeedStaticTables(AuditableEntityCompatibleDbContext);
        await SeedOrganizationSpecificData(AuditableEntityCompatibleDbContext);

        MainBlobStorageRepository = Factory.Services.GetRequiredService<IMainBlobStorageRepository>();

        await EnsureMainContainerExists();

        // Once we're no longer using the CurrentUserServiceStub we should remove this code since there won't be a hardcoded
        // UserId being used for creating new courses and other auditable entities
        var currentUserService = Factory.Services.GetRequiredService<ICurrentUserService>();
        if (currentUserService.UserId != null)
        {
            NewAuditableEntityCreatedByUserId = new Guid(currentUserService.UserId);
        }
    }


    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }
        await _sqlContainer.DisposeAsync().AsTask();
        await _azuriteContainer.DisposeAsync().AsTask();
        await _cosmosContainer.DisposeAsync().AsTask();
    }

    private static WebApplicationFactory<Program> GetWebApplicationFactory(CosmosClientWrapper cosmosClientWrapper) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(IntegrationEnvironment);
                builder.UseSetting("CosmosRepositoryOptions:CosmosConnectionString", _cosmosContainer.GetConnectionString());

                builder.ConfigureServices(services =>
                {
                    services.AddControllers().AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    });
                    services.AddTestSqlDatabase(_sqlContainer.GetConnectionString());
                    services.AddTestAzurite(_azuriteContainer.GetConnectionString());
                    services.AddTestCosmos(cosmosClientWrapper);

                    services.AddTestAuthentication();

                    services.AddAuthorizationBuilder()
                        .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .AddAuthenticationSchemes(IntegrationTestAuthHandler.TestAuthScheme)
                            .Build())
                        .AddPolicy(AuthorizationPolicyNames.OrganizationAccess, policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new OrganizationAccessRequirement());
                        })
                        .AddPolicy(AuthorizationPolicyNames.CourseOrganizationAccess, policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new CourseOrganizationAccessRequirement());
                        });

                    services.AddHealthChecks();
                    services.AddHttpContextAccessor();
                    services.AddScoped<IAuthorizationHandler, OrganizationAccessRequirementHandler>();
                    services.AddScoped<IAuthorizationHandler, CourseOrganizationAccessRequirementHandler>();
                    services.AddValidatorsFromAssembly(typeof(CreateCourseCommand).Assembly);

                    services.AddMediatR(cfg =>
                    {
                        cfg.RegisterServicesFromAssemblyContaining<GetContentTypesQuery>();
                        cfg.RegisterServicesFromAssemblyContaining<GetLanguagesQuery>();
                        cfg.RegisterServicesFromAssemblyContaining<CreateCourseCommand>();
                        cfg.RegisterServicesFromAssemblyContaining<GetCoursesQuery>();

                        cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
                        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                        cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
                    });

                    services.AddTransient(_ => TimeProvider.System);
                    services.AddSingleton<ICurrentUserService, CurrentUserServiceStub>();
                    services.AddScoped<IDomainEventService, DomainEventService>();

                    services.AddContentLibraryResilience();

                    services.AddScoped<IUnitOfWork, EfUnitOfWork>();
                    services.AddScoped<IContentService, ContentService>();
                    services.AddScoped<IContentTypeService, ContentTypeService>();
                    services.AddScoped<IContentRepository, ContentRepository>();
                    services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
                    services.AddScoped<ILanguageService, LanguageService>();
                    services.AddScoped<ILanguageRepository, LanguageRepository>();
                    services.AddScoped<ICourseService, CourseService>();
                    services.AddScoped<ICourseRepository, CourseRepository>();
                    services.AddScoped<IScheduledCourseRepository, ScheduledCourseRepository>();
                    services.AddScoped<ILearningContentService, LearningContentService>();
                    services.AddScoped<ILearningContentRepository, LearningContentRepository>();
                    services.AddScoped<IFinalExamRepository, FinalExamRepository>();
                    services.AddScoped<IFinalExamService, FinalExamService>();
                    services.AddSingleton<ILanguageContextService, LanguageContextService>();
                    services.AddAutoMapper(typeof(FinalExamMappingProfile).Assembly);
         
                    services.AddScoped<IMainBlobStorageRepository, MainBlobStorageRepository>();
                    services.AddScoped<ScenarioService>();

                    services.AddAutoMapper(typeof(Program).Assembly);
                    services.AddAutoMapper(typeof(CourseMappingProfile).Assembly);
                    services.AddAutoMapper(typeof(LanguageMappingProfile).Assembly);

                    services.AddAutoMapper(typeof(LearningContentMappingProfile).Assembly);

                    services.AddApiVersioning(options =>
                    {
                        options.ReportApiVersions = true;
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                    }).AddApiExplorer(options =>
                    {
                        options.SubstituteApiVersionInUrl = true;
                        options.GroupNameFormat = "'v'VVV";
                    });
                });
            });



    protected internal static ApplicationDbContext CreateContext()
    {
        var scope = Factory!.Services.CreateScope();

        var currentUserService = new CurrentUserServiceStub();
        var domainEventService = scope.ServiceProvider.GetRequiredService<IDomainEventService>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var clsDbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options, currentUserService, domainEventService, timeProvider);

        clsDbContext.Database.EnsureCreated();

        return clsDbContext;
    }

    protected internal static ApplicationDbContext GetAuditableEntityCompatibleContext() =>
        AuditableEntityCompatibleDbContext ?? throw new InvalidOperationException("AuditableEntityCompatibleDbContext was not set to the main DB Context.");

    private static void AddAuthentication(HttpClient client)
    {
        var authHeaderValue = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(new List<KeyValuePair<string, string>>
        {
            new(UserTokenKeys.OrganizationIds, "1"),
            new(UserTokenKeys.OrganizationIds, "8")
        });

        client.DefaultRequestHeaders.Authorization = authHeaderValue;
    }

    private static async Task SeedStaticTables(ApplicationDbContext context)
    {
        // Clear existing data
        context.ContentType.RemoveRange(context.ContentType);
        context.Languages.RemoveRange(context.Languages);
        context.Statuses.RemoveRange(context.Statuses);
        await context.SaveChangesAsync();

        // Add ContentTypes for testing
        List<ContentType> contentTypes =
        [
            new ContentType { ContentTypeDescription = "Policy" },
            new ContentType { ContentTypeDescription = "Course" }
        ];

        await context.ContentType.AddRangeAsync(contentTypes);

        // Add Statuses for testing
        List<Status> statuses =
        [
            new Status
            {
                StatusId = 1,
                Name = "Draft"
            },
            new Status
            {
                StatusId = 2,
                Name = "Archived"
            },
            new Status
            {
                StatusId = 3,
                Name = "Published"
            }
        ];

        await context.Statuses.AddRangeAsync(statuses);

        // Seed Languages
        await context.Languages.AddRangeAsync([
            new Language { Code = "en", Name = "English" },
            new Language { Code = "es", Name = "Spanish" }
        ]);

        await context.SaveChangesAsync();
    }

    private static async Task SeedOrganizationSpecificData(ApplicationDbContext context)
    {
        // Ensure there's no existing course related data for the AccessCourseOrg
        var existingAccessCourseOrgCourseData =
            await context.Courses
                .Where(c => c.OrganizationId == TestOrgIds.AccessCourseOrg)
                .ToListAsync();
        if (existingAccessCourseOrgCourseData.Count > 0)
        {
            context.Courses.RemoveRange(existingAccessCourseOrgCourseData);
            var contentIdsToRemove = existingAccessCourseOrgCourseData.Select(c => c.ContentId).ToList();
            var contentToRemove = await context.Content.Where(c => contentIdsToRemove.Contains(c.ContentId)).ToListAsync();
            context.Content.RemoveRange(contentToRemove);
            await context.SaveChangesAsync();
        }

        // Ensure there's no existing course related data for the UpdateOrg
        var existingUpdateCourseOrgCourseData =
          await context.Courses
              .Where(c => c.OrganizationId == TestOrgIds.UpdateCourseOrg)
              .ToListAsync();
        if (existingUpdateCourseOrgCourseData.Count > 0)
        {
            context.Courses.RemoveRange(existingUpdateCourseOrgCourseData);
            var contentIdsToRemove = existingUpdateCourseOrgCourseData.Select(c => c.ContentId).ToList();
            var contentToRemove = await context.Content.Where(c => contentIdsToRemove.Contains(c.ContentId)).ToListAsync();
            context.Content.RemoveRange(contentToRemove);
            await context.SaveChangesAsync();
        }

        // Ensure there's no existing course related data for the EmptyOrg
        var existingEmptyOrgCourseData = await context.Courses
            .Where(c => c.OrganizationId == TestOrgIds.EmptyOrg)
            .ToListAsync();
        if (existingEmptyOrgCourseData.Count > 0)
        {
            context.Courses.RemoveRange(existingEmptyOrgCourseData);
            var contentIdsToRemove = existingEmptyOrgCourseData.Select(c => c.ContentId).ToList();
            var contentToRemove = await context.Content.Where(c => contentIdsToRemove.Contains(c.ContentId)).ToListAsync();
            context.Content.RemoveRange(contentToRemove);
            await context.SaveChangesAsync();
        }

        // Ensure there's no existing course related data for the NoAccessOrg
        var existingNoAccessOrgCourseData = await context.Courses
            .Where(c => c.OrganizationId == TestOrgIds.NoAccessOrg)
            .ToListAsync();
        if (existingNoAccessOrgCourseData.Count > 0)
        {
            context.Courses.RemoveRange(existingNoAccessOrgCourseData);
            var contentIdsToRemove = existingNoAccessOrgCourseData.Select(c => c.ContentId).ToList();
            var contentToRemove = await context.Content.Where(c => contentIdsToRemove.Contains(c.ContentId)).ToListAsync();
            context.Content.RemoveRange(contentToRemove);
            await context.SaveChangesAsync();
        }

        // Get references to the content types and statuses needed for seeding data
        ContentType courseContentType =
            await context.ContentType.SingleOrDefaultAsync(ct =>
                ct.ContentTypeId == 2 && ct.ContentTypeDescription == "Course") ??
            throw new InvalidOperationException("Course content type does not exist or was created with the wrong ID.");

        Status draftStatus = await context.Statuses.SingleOrDefaultAsync(s => s.StatusId == 1 && s.Name == "Draft") ??
                          throw new InvalidOperationException(
                              "Draft status does not exist or was created with the wrong ID.");

        Status archivedStatus = await context.Statuses.SingleOrDefaultAsync(s => s.StatusId == 2 && s.Name == "Archived") ??
                             throw new InvalidOperationException(
                                 "Archived status does not exist or was created with the wrong ID.");

        Status publishedStatus = await context.Statuses.SingleOrDefaultAsync(s => s.StatusId == 3 && s.Name == "Published") ??
                             throw new InvalidOperationException(
                                 "Published status does not exist or was created with the wrong ID.");

        // Create the content entities that will be associated with the seeded course data
        List<Content> contents =
        [
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            }
            ,
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            },
            new Content
            {
                ContentId = Guid.NewGuid(),
                ContentTypeId = courseContentType.ContentTypeId,
                ContentType = courseContentType
            }
        ];

        Guid firstCourseGuid = Guid.NewGuid();
        Guid secondCourseGuid = Guid.NewGuid();
        Guid thirdCourseGuid = Guid.NewGuid();
        Guid fourthCourseGuid = Guid.NewGuid();
        Guid fifthCourseGuid = Guid.NewGuid();
        Guid sixCourseGuid = Guid.NewGuid();
        Guid sevenCourseGuid = Guid.NewGuid();
        Guid eightCourseGuid = Guid.NewGuid();
        Guid ninthCourseGuid = Guid.NewGuid();
        Guid tenthCourseGuid = Guid.NewGuid();
        Guid eleventhCourseGuid = Guid.NewGuid();
        Guid twelfthCourseGuid = Guid.NewGuid();
        Guid thirteenthCourseGuid = Guid.NewGuid();
        Guid fourteenthCourseGuid = Guid.NewGuid();


        // Create the course data that will be seeded for test organizations
        List<Course> courses =
        [
            new Course
            {
                CourseId = firstCourseGuid,
                ContentId = contents[0].ContentId,
                OrganizationId = TestOrgIds.DefaultOrg,
                ContentCode = "C101",
                Title = "Test Course 1",
                BriefDescription = "Brief 1",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = secondCourseGuid,
                ContentId = contents[1].ContentId,
                OrganizationId = TestOrgIds.AccessCourseOrg,
                ContentCode = "C102",
                Title = "Test Course 2",
                BriefDescription = "Brief 2",
                StatusId = archivedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            // Course created in an org that no user has access to
            new Course
            {
                CourseId = thirdCourseGuid,
                ContentId = contents[2].ContentId,
                OrganizationId = TestOrgIds.NoAccessOrg,
                ContentCode = "C103",
                Title = "Test Course 3",
                BriefDescription = "Brief 3",
                StatusId = archivedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            // Courses created in DeleteCourseOrg
            new Course
            {
                CourseId = fourthCourseGuid,
                ContentId = contents[3].ContentId,
                OrganizationId = TestOrgIds.DeleteCourseOrg,
                ContentCode = "C104",
                Title = "Test Course 4",
                BriefDescription = "Brief 4",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = fifthCourseGuid,
                ContentId = contents[4].ContentId,
                OrganizationId = TestOrgIds.DeleteCourseOrg,
                ContentCode = "C105",
                Title = "Test Course 5",
                BriefDescription = "Brief 5",
                StatusId = archivedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },       
            // add course for update 
            new Course
            {
                CourseId = sixCourseGuid,
                ContentId = contents[5].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "C106",
                Title = "Test Course 6",
                BriefDescription = "Brief 6",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = sevenCourseGuid,
                ContentId = contents[6].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "C107",
                Title = "Test Course 7",
                BriefDescription = "Brief 7",
                StatusId = archivedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = eightCourseGuid,
                ContentId = contents[7].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "C108",
                Title = "Test Course 8",
                BriefDescription = "Brief 8",
                StatusId = archivedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = ninthCourseGuid,
                ContentId = contents[8].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "C109",
                Title = "Test Course 9",
                BriefDescription = "Brief 9",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = tenthCourseGuid,
                ContentId = contents[9].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "C110",
                Title = "Test Course 10",
                BriefDescription = "Brief 10",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            new Course
            {
                CourseId = eleventhCourseGuid,
                ContentId = contents[10].ContentId,
                OrganizationId = TestOrgIds.DefaultOrg,
                ContentCode = "C111",
                Title = "Test Course 11",
                BriefDescription = "Brief 11",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            },
            // Relias Owned Course for testing update
            new Course
            {
                CourseId = twelfthCourseGuid,
                ContentId = contents[11].ContentId,
                OrganizationId = TestOrgIds.UpdateCourseOrg,
                ContentCode = "REL-MOCK",
                Title = "Relias Course",
                BriefDescription = "Brief 12",
                StatusId = publishedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString(),
                IsRelias = true
            },
            // Relias Owned Course for testing delete
            new Course
            {
                CourseId = thirteenthCourseGuid,
                ContentId = contents[12].ContentId,
                OrganizationId = TestOrgIds.DeleteCourseOrg,
                ContentCode = "REL-MOCK-DELETE",
                Title = "Relias Course",
                BriefDescription = "Brief 13",
                StatusId = publishedStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString(),
                IsRelias = true
            },
            new Course
            {
                CourseId = fourteenthCourseGuid,
                ContentId = contents[13].ContentId,
                OrganizationId = TestOrgIds.DefaultOrg,
                ContentCode = "C113",
                Title = "Relias Course",
                BriefDescription = "Brief 14",
                StatusId = draftStatus.StatusId,
                Created = DateTime.Now,
                CreatedBy = NewAuditableEntityCreatedByUserId.ToString()
            }
        ];

        var firstLearningContentGuid = Guid.NewGuid();
        var secondLearningContentGuid = Guid.NewGuid();
        var thirdLearningContentGuid = Guid.NewGuid();
        var fourthLearningContentGuid = Guid.NewGuid();
        var fifthLearningContentGuid = Guid.NewGuid();
        var sixthLearningContentGuid = Guid.NewGuid();
        var seventhLearningContentGuid = Guid.NewGuid();
        var eighthLearningContentGuid = Guid.NewGuid();
        var ninthLearningContentGuid = Guid.NewGuid();
        var tenthLearningContentGuid = Guid.NewGuid();
        var eleventhLearningContentGuid = Guid.NewGuid();

        LearningContent = [
            new LearningContent
            {
                Id = firstLearningContentGuid,
                CourseId = firstCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = secondLearningContentGuid,
                CourseId = secondCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = thirdLearningContentGuid,
                CourseId = thirdCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = fourthLearningContentGuid,
                CourseId = fourthCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = fifthLearningContentGuid,
                CourseId = fifthCourseGuid,
                Sections = []
            },
            new LearningContent
            {
                Id = sixthLearningContentGuid,
                CourseId = sixCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = seventhLearningContentGuid,
                CourseId = sevenCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = eighthLearningContentGuid,
                CourseId = eightCourseGuid,
                Sections = [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = ninthLearningContentGuid,

                CourseId = ninthCourseGuid,
                Sections =
                [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects =
                        [
                            new Lesson
                            {
                                LearningObjectType = LearningObjectType.Lesson,
                                LearningObjectId = Guid.NewGuid(),
                                LessonType = "file",
                                Name = new LocalizedString { En = "Lesson 1" },
                                DurationMinutes = 30,
                                RequiredForCompletion = true,
                                RequiresAudio = false,
                                RequiresVideo = false,
                                OpensInNewTab = false,
                                ContentPath = "main",
                                FileName = "lesson1.mp4",
                                FileSize = "100",
                                FormatType = LessonFormatType.Video
                            }
                        ]
                    },
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 2" },
                        LearningObjects = []
                    },
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 3" },
                        LearningObjects = []
                    },
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 4" },
                        LearningObjects = []
                    }
                ]
            },
            new LearningContent
            {
                Id = tenthLearningContentGuid,

                CourseId = tenthCourseGuid,
                Sections =
                [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects =
                        [
                            new Lesson
                            {
                                LearningObjectType = LearningObjectType.Lesson,
                                LearningObjectId = Guid.NewGuid(),
                                LessonType = "file",
                                Name = new LocalizedString { En = "Lesson 1" },
                                DurationMinutes = 30,
                                RequiredForCompletion = true,
                                RequiresAudio = false,
                                RequiresVideo = false,
                                OpensInNewTab = false,
                                ContentPath = "main",
                                FileName = "lesson1.mp4",
                                FileSize = "100",
                                FormatType = LessonFormatType.Video
                            }
                        ]
                    }
                ]
            },
            new LearningContent
            {
                Id = eleventhLearningContentGuid,

                CourseId = fourteenthCourseGuid,
                Sections =
                [
                    new LearningContentSection
                    {
                        SectionId = Guid.NewGuid(),
                        Name = new LocalizedString { En = "Section 1" },
                        LearningObjects =
                        [
                            new Lesson
                            {
                                LearningObjectType = LearningObjectType.Lesson,
                                LearningObjectId = Guid.NewGuid(),
                                LessonType = "file",
                                Name = new LocalizedString { En = "Lesson 14" },
                                DurationMinutes = 30,
                                RequiredForCompletion = true,
                                RequiresAudio = false,
                                RequiresVideo = false,
                                OpensInNewTab = false,
                                ContentPath = "main",
                                FileName = "lesson14.mp4",
                                FileSize = "100"
                            }
                        ]
                    }
                ]
            }
        ];



        AccessCourseOrgCourseIds = [firstCourseGuid, secondCourseGuid];
        NoAccessOrgCourseIds = [thirdCourseGuid];
        DeleteAccessOrgCourseIds = [courses[3].CourseId, courses[4].CourseId, courses[12].CourseId];
        DeleteAccessOrgContentIds = [courses[3].ContentId, courses[12].ContentId];
        DeleteAccessLearningContentIds = [LearningContent[3].Id];
        UpdateCourseIds = [courses[5].CourseId, courses[6].CourseId, courses[4].CourseId, courses[9].CourseId, courses[11].CourseId, courses[13].CourseId];
        UpdateContentIds = [courses[5].ContentId, courses[6].ContentId, courses[4].ContentId, courses[11].ContentId];
        PublishCourseIds = [courses[10].CourseId];
        PatchCourseIds = [courses[8].CourseId];
        GetContentInfoContentIds = [courses[0].ContentId!.Value, courses[1].ContentId!.Value];

        await context.Content.AddRangeAsync(contents);
        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();

        foreach (var lc in LearningContent)
        {
            await CosmosClient!.CreateItemAsync(lc);
        }
    }

    private static async Task<CosmosClientWrapper> CreateCosmosClientWithRetry(int maxAttempts = 5)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var container = new CosmosDbBuilder()
                .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
                .WithCommand("--protocol", "https")
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(8081)
                        .UntilMessageIsLogged("Now listening on: https://0.0.0.0:8081")
                )
                .Build();

            try
            {
                await container.StartAsync();

                var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                jsonOptions.Converters.Add(new LearningObjectConverter());

                var client = new CosmosClient(
                    container.GetConnectionString(),
                    new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Gateway,
                        HttpClientFactory = () => container.HttpClient,
                        Serializer = new SystemTextJsonCosmosSerializer(jsonOptions),
                    });

                var linqOptions = new CosmosLinqSerializerOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                };

                var linqSerializer = new CosmosSystemTextJsonLinqSerializer(jsonOptions);

                typeof(CosmosLinqSerializerOptions)
                    .GetProperty("CosmosLinqSerializer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.SetValue(linqOptions, linqSerializer);

                var db = (await client.CreateDatabaseIfNotExistsAsync(IntegrationEnvironment)).Database;

                await db.CreateContainerIfNotExistsAsync(CosmosDbConstants.ContainerNames.FinalExam, "/courseId");
                await db.CreateContainerIfNotExistsAsync(CosmosDbConstants.ContainerNames.LearningContent, "/courseId");

                _cosmosContainer = container;
                return new CosmosClientWrapper(client, IntegrationEnvironment, linqOptions);
            }
            catch
            {
                await container.DisposeAsync();
                if (attempt == maxAttempts)
                    throw new InvalidOperationException("Cosmos Emulator failed to start after multiple attempts.");
            }
        }

        throw new InvalidOperationException("Unexpected retry loop failure.");
    }

    private static async Task EnsureMainContainerExists()
    {
        if (MainBlobStorageRepository != null)
        {
            var blobServiceClient = Factory!.Services.GetRequiredService<Azure.Storage.Blobs.BlobServiceClient>();
            var containerClient = blobServiceClient.GetBlobContainerClient("main");
            await containerClient.CreateIfNotExistsAsync();
        }
    }
}

public static class UserTokenKeys
{
    public const string Subject = ClaimTypes.NameIdentifier;
    public const string UserId = "sub";
    public const string OrganizationId = "org_id";
    public const string OrganizationIds = "org_ids";
    public const string Permissions = "p";
    public const string ClientId = "client_id";
}

public static class TestOrgIds
{
    public const int InvalidOrgId = 0;
    public const int Site1Org = 1;
    public const int CreateCourseOrg = 2;
    public const int AccessCourseOrg = 3;
    [Description("This OrgId should only be used for tests that require no organization associted data to exist. No organization associated data should be added using this OrgId.")]
    public const int EmptyOrg = 4;
    [Description("This OrgId should only be used when creating courses for an organization the user does not have access to. No Organization ID related claims should be set to use this OrgId.")]
    public const int NoAccessOrg = 5;
    public const int DefaultOrg = 8;
    public const int DeleteCourseOrg = 9;
    public const int UpdateCourseOrg = 10;
}