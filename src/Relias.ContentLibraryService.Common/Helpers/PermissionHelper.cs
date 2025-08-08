namespace Relias.ContentLibraryService.Common.Helpers;

public class PermissionHelper
{
    public enum LegacyPermission
    {
        ReportSupervisor = 2,
        EnrollmentSupervisor = 4,
        UserSupervisor = 8,
        Administrator = 32,
        SitesAdministrator = 64,
        CurriculumEnrollmentSupervisor = 8192,
        ModuleEnrollmentSupervisor = 16384,
        Vendor = 32768,
        Helpdesk = 65536,
        Facility = 131072,
        TranscriptTransferManager = 1048576,
        EvaluationManagement = 4194304,
        EvaluationEnrollment = 8388608,
        EvaluationInteraction = 16777216
    }

    public static bool HasPermission(int permissions, params LegacyPermission[] anyPermissions)
    {
        return anyPermissions.Select(permission => (int)permission).Any(value => (permissions & value) == value);
    }
}
