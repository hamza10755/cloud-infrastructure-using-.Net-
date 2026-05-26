namespace cloud_infrastructure.Models
{
    public class ServerSoftware
    {
        public int ServerInstanceId { get; set; }
        public ServerInstance? ServerInstance { get; set; }

        public int SoftwarePackageId { get; set; }
        public SoftwarePackage? SoftwarePackage { get; set; }
    }
}
