using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarSyntax.utils
{
    public static class ReadKeywordsFromFile
    {        
        public static Dictionary<string, List<string>> FileToDictionary(string properties)
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            string line;
            StringReader reader = new StringReader(properties);
            while ((line = reader.ReadLine()) != null)
            {
                List<string> lista = new List<string>();
                string[] test = line.Split(' ');
                if (test.Length > 1)
                {
                    string values = line.Split(' ')[1];
                    lista = values.Split(',').ToList();
                }
                dictionary[test[0]] = lista;
            }
            return dictionary;
        }
    }
}
