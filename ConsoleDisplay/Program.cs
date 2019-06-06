using System;
using ShakespeareGenerator;

namespace ConsoleDisplay
{
    class Program
    {
        static void Main()
        {
            // Download and extract text from poems
            var scraper = new PoemScraper();
            var corpus = scraper.GetPoemsAsync().Result;

            // Generate
            var generator = new Generator();

            while (true)
            {
                Console.ResetColor();

                // Generate and display poem
                var poem = generator.GeneratePoem(corpus);

                foreach (var line in poem)
                {
                    Console.WriteLine(line);
                }

                // Restart if requested
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nPress Enter to restart");
                var response = Console.ReadKey();

                if (response.Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                }
                else
                {
                    return;
                }
            }
        }
    }
}
