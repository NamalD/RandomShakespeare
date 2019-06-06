using System;

namespace ShakespeareGenerator
{
    static internal class StringUtils
    {
        static internal bool IsPunctuation(this string word)
        {
            if (word.Length > 1)
            {
                return false;
            }

            return word[0].IsPunctuation();
        }

        static internal bool IsPunctuation(this char character)
        {
            char[] punctuation = { ';', ':', ',', '.', '!', '?' };

            return Array.Exists(punctuation, p => character == p);
        }

        static internal bool IsConjunction(this string word)
        {
            string[] conjunctions = { "for", "and", "nor", "but", "or", "yet", "so" };

            return Array.Exists(conjunctions, c => c == word.ToLower().Trim());
        }

        static internal string FirstWord(this string sentence)
        {
            return sentence.Split(' ')[0];
        }
    }
}
