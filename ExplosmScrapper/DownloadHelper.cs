using System.Net.Http;
using System;
using static System.Console;
using static ExplosmScrapper.ConsolePrint;
using System.Threading.Tasks;
using System.Net;

namespace ExplosmScrapper
{
    public class DownloadHelper : IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebClient _webClient;
        public DownloadHelper(HttpClient client, WebClient webClient)
        {
            _client = client;
            _webClient = webClient;

            _webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
            _webClient.Headers.Add("Content-Type", "application / zip, application / octet - stream");
            _webClient.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            _webClient.Headers.Add("Referrer", "http://google.com");
            _webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        }

        public async Task<bool> Download(Comic comic, bool verbose = false)
        {
            if (verbose)
            {
                Write("Downloading at: ");
                WriteSuccess(comic.LocalFileName!);
                Write("Source: ");
                WriteWarning(comic.ImageLink!);
                WriteLine();
            }

            if (FileHelper.Exists(comic.LocalFileName!))
            {
                if (verbose)
                    WriteInfo($"File {comic.LocalFileName} already exists. Skipping download...");
                return true;
            }

            try
            {
                var response = await _client.GetAsync(comic.ImageLink);
                if (response is null)
                {
                    throw new Exception("HTTP GET request failed.");
                }
                await FileHelper.SaveFile(comic.LocalFileName!, response);
            }
            catch (Exception e)
            {
                WriteError($"Error downloading {comic.ImageLink}. Skipping...\n{e.Message}\n");
                return false;
            }
            return true;
        }

        public bool DownloadLegacy(Comic comic, bool verbose = false)
        {
            try
            {
                var filePath = FileHelper.GetFilePath(comic.LocalFileName!);
                _webClient.DownloadFile(comic.ImageLink, filePath);
            } catch(Exception e) {
                WriteError($"Error legacy downloading {comic.ImageLink}. Skipping...\n{e.Message}\n");
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            _client.Dispose();
            _webClient.Dispose();
        }
    }
}