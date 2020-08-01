using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Console;
using static ExplosmScrapper.ConsolePrint;

namespace ExplosmScrapper
{
    public class App
    {
        private readonly Explosm _explosm;
        public App(Explosm explosm)
        {
            _explosm = explosm;
        }

        public async Task Run()
        {
            WriteLine("Starting Application...");

            var sw = new Stopwatch();
            sw.Start();

            var comics = await FileHelper.GetComicsDb();
            if (comics.Any()) {
                WriteLine("Do you want to sync ComicsDB?");
                WriteLine("1. Yes - Incremental Sync (default)");
                WriteLine("2. Yes - Full Sync");
                WriteLine("3. No");
                Write("Enter choice [1-3]: ");
                var choice = ReadLine();

                comics = choice switch {
                    "2" => await _explosm.FetchAllComics(),
                    "3" => comics,
                    _ => await _explosm.FetchNewComics(comics)
                };
            }
            else
            {
                comics = await _explosm.FetchAllComics();
            }

            comics = comics.OrderBy(c => c.Id).ToList();
            await FileHelper.SaveComicsDb(comics);

            // Ensure Local Availability
            await _explosm.DownloadComicFiles(comics);

            _explosm.Dispose();
            sw.Stop();
            WriteLine("\n");
            WriteInfo($"Took {(sw.ElapsedMilliseconds / 1000F):0.00} seconds");
        }
    }
}