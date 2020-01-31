using System;
using System.Net;
using System.IO;
using static System.Console;
using static ExplosmScrapper.ConsolePrint;

namespace ExplosmScrapper
{
    public class DownloadHelper: IDisposable
    {
        private readonly WebClient _client;
        public DownloadHelper(WebClient client)
        {
            _client = client;
            _client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
            _client.Headers.Add("Content-Type", "application / zip, application / octet - stream");
            _client.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            _client.Headers.Add("Referrer", "http://google.com");
            _client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        }

        public void Download(DownloadItem item, bool verbose = false)
        {
            var uri = new Uri(item.ImageLink);
            var fileName = ExtractFileName(uri);

            var filePath = PathHelper.MakeValidFileName(fileName);
            if (verbose)
            {
                Write("Downloading: ");
                WriteSuccess(fileName);
                Write("Location: ");
                WriteInfo(filePath);
                Write("Source: ");
                WriteWarning(item.ImageLink);
                WriteLine();
            }

            if (File.Exists(filePath)) return;
            _client.DownloadFile(item.ImageLink, filePath);
            
            // Todo - Parse Comic Info from item.Info
            // Todo - Add Additional Information (Comic Info)
        }

        public string ExtractFileName(Uri uri) =>
            uri.LocalPath.Substring(uri.LocalPath.IndexOf("comics/") + 6);

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}