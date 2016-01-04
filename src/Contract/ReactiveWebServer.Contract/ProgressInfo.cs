namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Represent a piece of data to report about the progress.
    /// </summary>
    public class ProgressInfo
    {
        public long Iteration { get; set; }
        public long TotalIterations { get; set; }
        public string CustomMessage { get; set; }
    }
}
