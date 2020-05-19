namespace GPUCluster.Shared.Events
{
    public class StatusChangedEventArgs
    {
        public string Message { get; set; }
        public int Code { get; set; }
        public string Status { get; set; }
    }
}