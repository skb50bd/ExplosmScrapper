
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LtGt;
using Microsoft.Extensions.Options;
using static ExplosmScrapper.ConsolePrint;

namespace ExplosmScrapper
{
    public class Explosm : IDisposable
    {
        private readonly HttpClient _client;
        private readonly DownloadHelper _helper;
        private readonly ExplosmOptions _opts;

        public Explosm(
            HttpClient client, 
            DownloadHelper helper,
            IOptionsMonitor<ExplosmOptions> opts)
        {
            _client = client;
            _helper = helper;
            _opts = opts.CurrentValue;
        }

        public async Task<DownloadItem> GetDownloadItem(string pageUrl)
        {
            string html;

            try
            {
                html = await _client.GetStringAsync(pageUrl);
            }
            catch
            {
                WriteError($"Error Occurred Getting Page: {pageUrl}");
                return null;
            }

            var result = Html.TryParseDocument(html);

            if (!result.IsOk)
                return null;

            var link =
                result.ResultValue
                      .GetElementById(_opts.ImageId)
                     ?.Attributes
                      .FirstOrDefault(a => a.Name == "src")
                     ?.Value;

            var comicInfo = 
                result.ResultValue
                      .GetElementById(_opts.ImageId)
                      .GetInnerText();
            return new DownloadItem { ImageLink = $"http:{link}", ComicInfo = comicInfo };
        }

        public void Dispose()
        {
            _client.Dispose();
            _helper.Dispose();
        }
    }
}
