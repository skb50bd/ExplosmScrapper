using System.Net.Http;
using System;
using static System.Console;
using static ExplosmScrapper.ConsolePrint;
using System.Threading.Tasks;

namespace ExplosmScrapper
{
    public class DownloadHelper : IDisposable
    {
        private readonly HttpClient _client;
        public DownloadHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task Download(Comic comic, bool verbose = false)
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
                return;
            }

            try
            {
                var response = await _client.GetAsync(comic.ImageLink);
                if (response is null) {
                    throw new Exception("HTTP GET request failed.");
                }
                await FileHelper.SaveFile(comic.LocalFileName!, response);
            }
            catch (Exception e)
            {
                WriteError($"Error downloading {comic.ImageLink}. Skipping...\n{e.Message}\n");
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}