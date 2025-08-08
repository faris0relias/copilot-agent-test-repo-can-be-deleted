using Microsoft.EntityFrameworkCore;
using Relias.ContentLibraryService.Common;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.Certificate;
using Relias.ContentLibraryService.Domain.Content;
using Relias.ContentLibraryService.Domain.ContentType;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;
using Relias.ContentLibraryService.Domain.Language;
using Relias.ContentLibraryService.Domain.Lesson;
using Relias.ContentLibraryService.Domain.LessonType;
using Relias.ContentLibraryService.Domain.Library;
using Relias.ContentLibraryService.Domain.Module;
using Relias.ContentLibraryService.Domain.Organization;
using Relias.ContentLibraryService.Domain.Permission;
using Relias.ContentLibraryService.Domain.Policy;
using Relias.ContentLibraryService.Domain.PolicyTag;
using Relias.ContentLibraryService.Domain.Setting;
using Relias.ContentLibraryService.Domain.Status;
using System.Reflection;
using Module = Relias.ContentLibraryService.Domain.Module.Module;


namespace Relias.ContentLibraryService.Infra.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDomainEventService _domainEventService;
        private readonly TimeProvider _timeProvider;

        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<ModuleCategory> ModuleCategories => Set<ModuleCategory>();
        public DbSet<ModuleType> ModuleTypes => Set<ModuleType>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<Library> Libraries => Set<Library>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<OrganizationLibrary> OrganizationLibraries => Set<OrganizationLibrary>();
        public DbSet<ModuleLanguage> ModuleLanguages => Set<ModuleLanguage>();
        public DbSet<Contributor> Contributors => Set<Contributor>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<Certificate> Certificates => Set<Certificate>();
        public DbSet<LessonType> LessonTypes => Set<LessonType>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Policy> Policy => Set<Policy>();
        public DbSet<Tag> Tag => Set<Tag>();        
        public DbSet<Content> Content => Set<Content>();
        public DbSet<ContentType> ContentType => Set<ContentType>();
        public DbSet<ScheduledCourse> ScheduledCourses => Set<ScheduledCourse>();
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDomainEventService domainEventService,
            TimeProvider timeProvider) : base(options)
        {
        
            _currentUserService = currentUserService;
            _domainEventService = domainEventService;
            _timeProvider = timeProvider;
        }
        
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.Created = _timeProvider.GetLocalNow().DateTime;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModified = _timeProvider.GetLocalNow().DateTime;
                        break;
                    case EntityState.Detached:
                        break;
                    case EntityState.Unchanged:
                        break;
                    case EntityState.Deleted:
                        break;
                    default:
                        break;
                }
            }
        
            var events = ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .Where(domainEvent => !domainEvent.IsPublished)
                .ToArray();
        
            var result = await base.SaveChangesAsync(cancellationToken);
        
            await DispatchEvents(events);
        
            return result;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        private async Task DispatchEvents(DomainEvent[] events)
        {
            foreach (var @event in events)
            {
                @event.IsPublished = true;
                await _domainEventService.Publish(@event);
            }
        }
    }
}