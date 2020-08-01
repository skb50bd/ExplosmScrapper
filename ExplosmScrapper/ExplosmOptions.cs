namespace ExplosmScrapper {
    public class ExplosmOptions {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string? BaseUrl { get; set; }
        public string? ImageId { get; set; }
        public string? InfoId { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
    }
}