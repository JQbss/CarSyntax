using CarSyntax.utils;
using System.Collections.Generic;

namespace CarSyntax.keywords
{
    public class KeywordsStatment
    {
        internal static Dictionary<string, List<string>> keywordsDictionary = ReadKeywordsFromFile.FileToDictionary(Properties.Resources.KeywordsStatment);
        internal static Dictionary<string, List<string>> valuesDictionary = ReadKeywordsFromFile.FileToDictionary(Properties.Resources.AttributesStatment);
        internal static Dictionary<string, List<string>> nestingDictionary = ReadKeywordsFromFile.FileToDictionary(Properties.Resources.MarkupNesting);

        public static List<string> GetAllMarkups()
        {
            List<string> markups = new List<string>();
            foreach (var t in keywordsDictionary.Keys)
            {
                markups.Add(t);
            }
            return markups;
        }
        internal static bool ContainsMarkup(string word)
        {
            return GetAllMarkups().Contains(word);
        }

        internal static List<string> GetAllAttributesByMarkup(string markup)
        {
            if (keywordsDictionary.ContainsKey(markup))
            {
                return keywordsDictionary[markup];
            }
            return new List<string>();
        }
        internal static bool ContainsAttributeInMarkup(string word, string markup)
        {
            return GetAllAttributesByMarkup(markup).Contains(word);
        }
        internal static List<string> GetAllValuesByAttruute(string attribute)
        {
            if (valuesDictionary.ContainsKey(attribute))
            {
                return valuesDictionary[attribute];
            }
            return new List<string>();
        }
        internal static List<string> GetAllNestedMarkups(string markup)
        {
            if (nestingDictionary.ContainsKey(markup))
            {
                return nestingDictionary[markup];
            }
            return GetAllMarkups();
        }
        internal static bool ContainsMarkupInNestedDictionary(string markup)
        {
            return nestingDictionary.ContainsKey(markup);
        }
    }
}
