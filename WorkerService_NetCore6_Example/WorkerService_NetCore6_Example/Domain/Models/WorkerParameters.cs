namespace WorkerService_NetCore6_Example.Domain.Models
{
    public class WorkerParameters
    {
        public int LoopService { get; set; }
        public List<HostParameter> Hosts { get; set; }
    }
}
