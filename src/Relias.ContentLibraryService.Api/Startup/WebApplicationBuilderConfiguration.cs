using Asp.Versioning;
using AutoMapper;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Relias.ContentLibraryService.Api.Authorization;
using Relias.ContentLibraryService.Api.Authorization.RequirementHandlers;
using Relias.ContentLibraryService.Api.Logging;
using Relias.ContentLibraryService.Api.Versioning;
using Relias.ContentLibraryService.App.Features.Content.Mappings;
using Relias.ContentLibraryService.App.Features.Content.Queries;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Mappings;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Features.Course.Validators;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.App.Interfaces.Content;
using Relias.ContentLibraryService.App.Services;
using Relias.ContentLibraryService.App.Services.Content;
using Relias.ContentLibraryService.Common.Behaviors;
using Relias.ContentLibraryService.Common.Interfaces;
using Relias.ContentLibraryService.Common.Models;
using Relias.ContentLibraryService.Common.Resilience;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Infra.Cosmos;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;
using Relias.ContentLibraryService.Infra.Storage;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;


namespace Relias.ContentLibraryService.Api.Startup;


[CustomWebAppBuilderConfiguration, ExcludeFromCodeCoverage]
public class WebAppBuilderConfigurations : INeedWebAppBuilderConfiguration
{
    const string apiName = "content-library-service-api";
    const string authenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    const int InputFormatterMemoryBufferThreshold = 5 * 1024 * 1024;

    public WebApplicationBuilder Configure(WebApplicationBuilder builder)
    {
        // Get logger early to track configuration progress
        var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<WebAppBuilderConfigurations>>();

        logger.LogInformation("Starting Content Library Service API configuration");

        logger.LogDebug("Loading application settings from configuration");
        var appSettings = builder.Configuration.GetRequiredSection(nameof(AppSettings)).Get<AppSettings>()!;
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
        builder.Services.AddOptions<AppSettings>(nameof(AppSettings));
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        builder.Services.ConfigureCosmos(builder.Configuration, builder.Environment);
        builder.Services.ConfigureBlobStorage(builder.Configuration, builder.Environment);

        logger.LogInformation("Setting up SQL Server connection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                appSettings.SqlServerConnectionString,
                b =>
                {
                    Assembly assembly = typeof(ApplicationDbContext).Assembly;
                    b.MigrationsAssembly(assembly.FullName);
                }));
        logger.LogInformation("SQL Server configuration completed");

        builder.Services.AddAuthentication(authenticationScheme)
            .AddJwtBearer(authenticationScheme, options =>
            {
                options.Authority = appSettings.Identity;
                options.Audience = apiName;
            });

        // Setup Authorization
        builder.Services.AddAuthorizationBuilder()
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


        // Cors
        builder.Services.AddCors(options => options.AddDefaultPolicy(corsPolicyBuilder => _ = corsPolicyBuilder
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddApiVersioning(setupAction =>
        {
            setupAction.AssumeDefaultVersionWhenUnspecified = true;
            setupAction.DefaultApiVersion = new ApiVersion(1, 0);
        }).AddApiExplorer(options =>
        {
            options.SubstituteApiVersionInUrl = true;
            options.GroupNameFormat = "'v'VVV";
        });

        builder.Services.ConfigureOptions<SwaggerGenOptionsWithVersionSupport>();

        builder.Services.AddControllers(o =>
        {
            o.SuppressOutputFormatterBuffering = false;
            o.SuppressInputFormatterBuffering = false;
            o.Filters.Add(typeof(DBSaveChangesFilter));
        })
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.OutputFormatterMemoryBufferThreshold = Int32.MaxValue;
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MemoryBufferThreshold = 50 * 1024 * 1024; // 50 MB
            options.MultipartBodyLengthLimit = InputFormatterMemoryBufferThreshold;
            options.BufferBody = true;
            options.BufferBodyLengthLimit = long.MaxValue;
        });

        builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
        builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
        builder.Services.ConfigureOptions<SwaggerGenOptionsWithVersionSupport>();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAutoMapper(typeof(Program).Assembly);
        builder.Services.AddAutoMapper(typeof(LanguageMappingProfile).Assembly);
        builder.Services.AddAutoMapper(typeof(CourseMappingProfile).Assembly);
        builder.Services.AddAutoMapper(typeof(LearningContentMappingProfile).Assembly);

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(CreateCourseCommand).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(DeleteCourseCommand).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(FinalExamMappingProfile).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<GetCourseByIdQueryContractValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateFinalExamCommandContractValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateLearningContentCommandContractValidator>();

        builder.Services.AddScoped<ILanguageContextService, LanguageContextService>();

        builder.Services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(Program).Assembly);
            options.RegisterServicesFromAssembly(typeof(GetContentTypesQuery).Assembly);
            options.RegisterServicesFromAssembly(typeof(CreateCourseCommand).Assembly);
            options.RegisterServicesFromAssembly(typeof(DeleteCourseCommand).Assembly);
            options.RegisterServicesFromAssemblyContaining<GetCourseByIdQuery>();
            options.RegisterServicesFromAssemblyContaining<CreateFinalExamCommand>();

            options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        builder.Services.AddContentLibraryResilience();

        builder.Services.AddTransient(_ => TimeProvider.System);
        builder.Services.AddSingleton<ICurrentUserService, CurrentUserServiceStub>();
        builder.Services.AddScoped<IDomainEventService, DomainEventService>();
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<IContentService, ContentService>();
        builder.Services.AddScoped<IContentRepository, ContentRepository>();
        builder.Services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
        builder.Services.AddScoped<IContentTypeService, ContentTypeService>();
        builder.Services.AddScoped<IFinalExamRepository, FinalExamRepository>();
        builder.Services.AddScoped<IFinalExamService, FinalExamService>();
        builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
        builder.Services.AddScoped<ILanguageService, LanguageService>();
        builder.Services.AddScoped<ILearningContentRepository, LearningContentRepository>();
        builder.Services.AddScoped<ILearningContentService, LearningContentService>();
        builder.Services.AddScoped<IMainBlobStorageRepository, MainBlobStorageRepository>();
        builder.Services.AddScoped<IScheduledCourseRepository, ScheduledCourseRepository>();
        builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuthorizationHandler, OrganizationAccessRequirementHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CourseOrganizationAccessRequirementHandler>();

        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();

        logger.LogInformation("Content Library Service API configuration completed");

        return builder;
    }
}