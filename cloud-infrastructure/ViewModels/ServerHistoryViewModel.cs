using cloud_infrastructure.Models;

namespace cloud_infrastructure.ViewModels
{
    public class ServerHistoryViewModel
    {
        public string? Search { get; set; }
        public string? SortOrder { get; set; }
        public List<ServerInstance> Servers { get; set; } = new();
    }
}
