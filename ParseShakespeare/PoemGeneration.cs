using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShakespeareGenerator
{
    public static class PoemGeneration
    {
        /// <summary>
        /// Generates A shakespeare poem that will fill your heart
        /// </summary>
        /// <param name="customCorpus"></param>
        /// <returns>True Beauty (Poem as a string with line breaks)</returns>
        public static async Task<string> GeneratePoem(IEnumerable<string> customCorpus = null)
        {
            var scrapper = new PoemScraper();

            var corpus = customCorpus ?? await scrapper.GetPoemsAsync();

            var generator = new Generator(corpus);

            var sonnet = generator.NextSonnet();

            var poem = sonnet.Aggregate(string.Empty, (current, line) => current + $"{Environment.NewLine}{line}");

            return poem;
        }
    }
}
