using System.Diagnostics;
using System.Threading.Tasks;
using static System.Console;
using static ExplosmScrapper.ConsolePrint;
using Microsoft.Extensions.Options;

namespace ExplosmScrapper
{
    public class App
    {
        private readonly Explosm _explosm;
        private readonly DownloadHelper _helper;
        private readonly ExplosmOptions _opts;
        public App(
            Explosm explosm,
            DownloadHelper helper,
            IOptionsMonitor<ExplosmOptions> opts)
        {
            _explosm = explosm;
            _helper = helper;
            _opts = opts.CurrentValue;
        }

        public async Task Run()
        {
            WriteLine("Starting Application...\n");

            var sw = new Stopwatch();
            sw.Start();

            var first = _opts.StartIndex;
            var last = _opts.EndIndex;

            for (var c = first; c <= last; c++)
            {
                var link = _opts.BaseUrl + c; 
                WriteInfo($"\n{link}");

                try
                {
                    var item = await _explosm.GetDownloadItem(link);

                    if (!(item is null))
                    {
                        _helper.Download(item, true);
                    }
                    else
                    {
                        WriteError("Nothing found");
                    }
                }
                catch
                {
                    WriteError($"Error Occurred for /comics/{c}");
                }
            }
            _explosm.Dispose();
            sw.Stop();
            WriteInfo($"Took {(sw.ElapsedMilliseconds / 1000):0.00} seconds");
        }
    }
}