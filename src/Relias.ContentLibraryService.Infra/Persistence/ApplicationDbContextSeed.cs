using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Relias.ContentLibraryService.Domain.Certificate;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.ReliasCourse;
using Relias.ContentLibraryService.Domain.Language;
using Relias.ContentLibraryService.Domain.Lesson;
using Relias.ContentLibraryService.Domain.LessonType;
using Relias.ContentLibraryService.Domain.Library;
using Relias.ContentLibraryService.Domain.Module;
using Relias.ContentLibraryService.Domain.Organization;
using Relias.ContentLibraryService.Domain.Setting;
using Relias.ContentLibraryService.Domain.Status;

namespace Relias.ContentLibraryService.Infra.Persistence
{
    public static class ApplicationDbContextSeed
    {
        // Seeding capability should not exist locally. Remove later
        private static string SeedDataRootPath => Environment.CurrentDirectory;
            
            //Path.Combine("C:\\Users\\cyeung\\RiderProjects\\content-library-service\\src\\Relias.ContentLibraryService.Infra\\", "SeedData");

        public static async Task<IServiceProvider> SeedDataAsync(this IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
            if (!await dbContext.Settings.AnyAsync())
            {
                var settingsData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Setting)}.json"));
                if (!string.IsNullOrEmpty(settingsData))
                {
                    var settingList = JsonConvert.DeserializeObject<List<Setting>>(settingsData);
                    if (settingList != null)
                    {
                        settingList.ForEach(setting =>
                        {
                            setting.Created = DateTime.UtcNow;
                        });
                        dbContext.Settings.AddRange(settingList);
                    }
                }
            }
        
            if (!await dbContext.Contributors.AnyAsync())
            {
                var contributorsData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Contributor)}.json"));
                if (!string.IsNullOrEmpty(contributorsData))
                {
                    var contributorList = JsonConvert.DeserializeObject<List<Contributor>>(contributorsData);
                    if (contributorList != null)
                    {
                        contributorList.ForEach(contributor =>
                        {
                            contributor.Created = DateTime.UtcNow;
                        });
                        dbContext.Contributors.AddRange(contributorList);
                    }
                }
            }
        
            if (!await dbContext.Statuses.AnyAsync())
            {
                var statusesData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Status)}.json"));
                if (!string.IsNullOrEmpty(statusesData))
                {
                    var statusList = JsonConvert.DeserializeObject<List<Status>>(statusesData);
                    if (statusList != null)
                    {
                        dbContext.Statuses.AddRange(statusList);
                    }
                }
            }
        
            if (!await dbContext.Languages.AnyAsync())
            {
                var languageData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Language)}.json"));
                if (!string.IsNullOrEmpty(languageData))
                {
                    var languages = JsonConvert.DeserializeObject<List<Language>>(languageData);
                    if (languages != null)
                    {
                        dbContext.Languages.AddRange(languages);
                    }
                }
            }
        
            if (!await dbContext.ModuleCategories.AnyAsync())
            {
                var moduleCategoryData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(ModuleCategory)}.json"));
                if (!string.IsNullOrEmpty(moduleCategoryData))
                {
                    var moduleCategories = JsonConvert.DeserializeObject<List<ModuleCategory>>(moduleCategoryData);
                    if (moduleCategories != null)
                    {
                        moduleCategories.ForEach(moduleCategory => moduleCategory.Created = DateTime.UtcNow);
                        dbContext.ModuleCategories.AddRange(moduleCategories);
                    }
                }
            }
        
            if (!await dbContext.ModuleTypes.AnyAsync())
            {
                var moduleTypeData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(ModuleType)}.json"));
                if (!string.IsNullOrEmpty(moduleTypeData))
                {
                    var moduleTypes = JsonConvert.DeserializeObject<List<ModuleType>>(moduleTypeData);
                    if (moduleTypes != null)
                    {
                        moduleTypes.ForEach(moduleCategory => moduleCategory.Created = DateTime.UtcNow);
                        dbContext.ModuleTypes.AddRange(moduleTypes);
                    }
                }
            }
            
            if (!await dbContext.Modules.AnyAsync())
            {
                var moduleData = await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Module)}.json"));
                if (!string.IsNullOrEmpty(moduleData))
                {
                    var moduleList = JsonConvert.DeserializeObject<List<Module>>(moduleData);
                     if (moduleList != null)
                     {
                         moduleList.ForEach(module =>
                         {
                             module.Created = DateTime.UtcNow;
                             module.LanguageIds =
                                 JsonConvert.DeserializeObject<List<Guid>>(
                                     JsonConvert.SerializeObject(module.LanguageIds));
                             module.SettingIds =
                                 JsonConvert.DeserializeObject<List<Guid>>(
                                     JsonConvert.SerializeObject(module.SettingIds));
                             module.TargetAudienceIds =
                                 JsonConvert.DeserializeObject<List<Guid>>(
                                     JsonConvert.SerializeObject(module.TargetAudienceIds));
                         });
                      dbContext.Modules.AddRange(moduleList);
                    }
                }
            }
        
            if (!await dbContext.Organizations.AnyAsync())
            {
                var organizationData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Organization)}.json"));
                if (!string.IsNullOrEmpty(organizationData))
                {
                    var organization = JsonConvert.DeserializeObject<List<Organization>>(organizationData);
                    if (organization != null)
                    {
                        organization.ForEach(organizations => organizations.Created = DateTime.UtcNow);
                        dbContext.Organizations.AddRange(organization);
                    }
                }
            }

            if (!await dbContext.Courses.AnyAsync())
            {
                var courseData = await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Course)}.json"));
                if (!string.IsNullOrEmpty(courseData))
                {
                    var courseList = JsonConvert.DeserializeObject<List<Course>>(courseData);
                    if (courseList != null)
                    {
                        courseList.ForEach(course =>
                        {
                            course.Created = DateTime.UtcNow;
                            course.CreatedBy = "81cac04b-efb5-482a-8f50-92bf89b89126";
                            course.LastModified = DateTime.UtcNow;
                            course.LastModifiedBy = "81cac04b-efb5-482a-8f50-92bf89b89126";

                            course.LanguageIds = JsonConvert.DeserializeObject<List<Guid>>(
                                JsonConvert.SerializeObject(course.LanguageIds)) ?? new List<Guid>();
                        });

                        dbContext.Courses.AddRange(courseList);
                    }
                }
            }

            if (!await dbContext.Libraries.AnyAsync())
            {
                var libraryData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Library)}.json"));
                if (!string.IsNullOrEmpty(libraryData))
                {
                    var libraryList = JsonConvert.DeserializeObject<List<Library>>(libraryData);
                    if (libraryList != null)
                    {
                        libraryList.ForEach(library =>
                        {
                            library.Created = DateTime.UtcNow;
                            library.ModuleIds =
                                JsonConvert.DeserializeObject<List<Guid>>(
                                    JsonConvert.SerializeObject(library.ModuleIds));
                        });
                        dbContext.Libraries.AddRange(libraryList);
                    }
                }
            }
        
            if (!await dbContext.OrganizationLibraries.AnyAsync())
            {
                var orgLibData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(OrganizationLibrary)}.json"));
                if (!string.IsNullOrEmpty(orgLibData))
                {
                    var orgLibList = JsonConvert.DeserializeObject<List<OrganizationLibrary>>(orgLibData);
                    if (orgLibList != null)
                    {
                        orgLibList.ForEach(orgLib => orgLib.Created = DateTime.UtcNow);
                        dbContext.OrganizationLibraries.AddRange(orgLibList);
                    }
                }
            }
        
            if (!await dbContext.ModuleLanguages.AnyAsync())
            {
                var modLangData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(ModuleLanguage)}.json"));
                if (!string.IsNullOrEmpty(modLangData))
                {
                    var modLangList = JsonConvert.DeserializeObject<List<ModuleLanguage>>(modLangData);
                    if (modLangList != null)
                    {
                        modLangList.ForEach(modLang => modLang.Created = DateTime.UtcNow);
                        dbContext.ModuleLanguages.AddRange(modLangList);
                    }
                }
            }
        
            if (!await dbContext.LessonTypes.AnyAsync())
            {
                var lessonTypeData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(LessonType)}.json"));
                if (!string.IsNullOrEmpty(lessonTypeData))
                {
                    var lessonTypeList = JsonConvert.DeserializeObject<List<LessonType>>(lessonTypeData);
                    if (lessonTypeList != null)
                    {
                        lessonTypeList.ForEach(organizations => organizations.Created = DateTime.UtcNow);
                        dbContext.LessonTypes.AddRange(lessonTypeList);
                    }
                }
            }
        
            if (!await dbContext.Lessons.AnyAsync())
            {
                var lessonData = await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Lesson)}.json"));
                if (!string.IsNullOrEmpty(lessonData))
                {
                    var lessonList = JsonConvert.DeserializeObject<List<Lesson>>(lessonData);
                    if (lessonList != null)
                    {
                        lessonList.ForEach(lesson =>
                        {
                            lesson.Created = DateTime.UtcNow;
                            lesson.TopicIds =
                                JsonConvert.DeserializeObject<List<Guid>>(JsonConvert.SerializeObject(lesson.TopicIds));
                        });
                        dbContext.Lessons.AddRange(lessonList);
                    }
                }
            }
        
            if (!await dbContext.Certificates.AnyAsync())
            {
                var certificateData =
                    await File.ReadAllTextAsync(Path.Combine(SeedDataRootPath, $"{nameof(Certificate)}.json"));
                if (!string.IsNullOrEmpty(certificateData))
                {
                    var certificateList = JsonConvert.DeserializeObject<List<Certificate>>(certificateData);
                    if (certificateList != null)
                    {
                        certificateList.ForEach(certificate =>
                        {
                            certificate.Created = DateTime.UtcNow;
                        });
                        dbContext.Certificates.AddRange(certificateList);
                    }
                }
            }
            
            await dbContext.SaveChangesAsync();
            return serviceProvider;
        }
    }
}