using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetPendingReportsAsync();
    Task<IEnumerable<Report>> GetReportsByTargetAsync(string targetType, Guid targetId);
    Task<IEnumerable<Report>> GetReportsByReporterAsync(Guid reporterId);
    Task<(IEnumerable<Report> Items, int TotalCount)> GetReportsPagedAsync(int page, int pageSize, string? status = null);
}
