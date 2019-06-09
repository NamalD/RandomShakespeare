using Markov;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ShakespeareGenerator
{
    public class Generator
    {
        private const string _wordSplitPattern = @"(\w+['-]?\w*)|([;:,.!?\-])";
        private readonly Random _random;
        private readonly MarkovChain<string> _markovChain;

        public Generator(IEnumerable<string> corpus)
        {
            _random = new Random();
            _markovChain = new MarkovChain<string>(3);

            foreach (var line in corpus)
            {
                var words = SplitLineToWords(line);
                _markovChain.Add(words, 1);
            }
        }

        public IEnumerable<string> NextLines(int lineCount)
        {
            var sentences = new List<string>();
            for (var i = 0; i < lineCount; i++)
            {
                // Get a random-walk chain and turn into a valid sentence
                var wordChain = _markovChain.Chain(_random);
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

        public IEnumerable<string> NextSonnet()
        {
            return NextLines(14);
        }

        public IEnumerable<string> NextVerse()
        {
            return NextLines(7);
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