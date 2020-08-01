using System;

namespace ExplosmScrapper
{
    public class Comic
    {
        public int Id { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public string? Author { get; set; }
        public string? PageUrl { get; set; }
        public string? ImageLink { get; set; }
        public string? LocalFileName { get; set; }
    }
}