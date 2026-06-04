using cloud_infrastructure.Models;

namespace cloud_infrastructure.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalServers { get; set; }
        public int TotalPackages { get; set; }
        public int PendingServers { get; set; }
        public int ApprovedServers { get; set; }
        public int RejectedServers { get; set; }

        public IEnumerable<ServerInstance> PendingRequests { get; set; } = new List<ServerInstance>();
    }
}