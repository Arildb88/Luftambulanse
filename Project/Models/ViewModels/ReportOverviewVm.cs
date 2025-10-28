namespace Gruppe4NLA.Models.ViewModels
{
    public class ReportsOverviewVm
    {
        public IReadOnlyList<ReportModel> Drafts { get; init; } = [];
        public IReadOnlyList<ReportModel> Submitted { get; init; } = [];
    }
}
