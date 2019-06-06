using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markov;

namespace ShakespeareGenerator
{
    public class Generator
    {
        private const string _wordSplitPattern = @"(\w+['-]?\w*)|([;:,.!?\-])";

        /// <summary>
        /// Generate a new poem from an existing body of text
        /// </summary>
        /// <param name="corpus"></param>
        /// <returns></returns>
        public IEnumerable<string> GeneratePoem(IEnumerable<string> corpus)
        {
            // Use Markov chains to generate new poems
            var chain = new MarkovChain<string>(3);

            // Add split words from corpus to chain
            foreach (var line in corpus)
            {
                var words = SplitLineToWords(line);
                chain.Add(words, 1);
            }

            // Generate new lines
            var rand = new Random();
            var sentences = new List<string>();

            for (var i = 0; i < 10; i++)
            {
                // Get a random-walk chain and turn into a valid sentence
                var wordChain = chain.Chain(rand);
                var sentence = ChainToSentence(wordChain);

                // Ensure first sentence does not start with conjunction
                if (sentence.FirstWord().IsConjunction())
                {
                    i--;
                    continue;
                }

                // Ensure last sentence ends with full stop
                if (i == 9 && !sentence.EndsWith('.'))
                {
                    // If last character is a punctuation mark, convert to full stop, append otherwise
                    var lastIndex = sentence.Length - 1;
                    var lastChar = sentence[lastIndex];

                    if (lastChar.IsPunctuation())
                    {
                        sentence = sentence.Remove(lastIndex) + '.';
                    }
                    else
                    {
                        sentence += '.';
                    }
                }

                sentences.Add(sentence);
            }

            return sentences;
        }

        private string ChainToSentence(IEnumerable<string> wordChain)
        {
            var sentence = string.Empty;

            foreach (var word in wordChain)
            {
                // TODO: Skip chained punctuation
                if (sentence == string.Empty || word.IsPunctuation())
                {
                    sentence += word;
                }
                else
                {
                    sentence += " " + word;
                }

                // Halt sentence on full stop
                if (word == ".")
                {
                    break;
                }
            }

            return sentence;
        }

        private IEnumerable<string> SplitLineToWords(string line)
        {
            var words = new List<string>();

            var matches = Regex.Matches(line, _wordSplitPattern);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    words.Add(match.Value);
                }
            }

            return words;
        }
    }
}
