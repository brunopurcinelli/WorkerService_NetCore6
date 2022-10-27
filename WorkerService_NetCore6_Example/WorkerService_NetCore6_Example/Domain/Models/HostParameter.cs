namespace WorkerService_NetCore6_Example.Domain.Models
{
    public class HostParameter
    {
        public string Protocol { get; set; }
        public string HostName { get; set; }
        public string Port { get; set; }
        public string LogonType { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string FingerPrint { get; set; }
        public string DeleteFilesOtherThanDays { get; set; }
        public string DefaultFolder { get; set; }
    }
}
