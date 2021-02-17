using CarSyntax.keywords;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;

namespace CarSyntax
{
    class CarSyntaxChecker
    {
		private Dictionary<int, string> SavedMarkups = new Dictionary<int, string>();
		private static int previousNumberOfLines = 1;
		private static string keywordName = "markup";
		private static string attributeName = "";
		public static string HighMarkup { get; set; } = "markup";
		public static bool MarkupFlag { get; private set; } = false;
		public static bool AttributeFlag { get; private set; } = false;
		public static bool ValueFlag { get; private set; } = false;

		internal CarSyntaxChecker(IStandardClassificationService classifications) =>
			Classifications = classifications;

		internal IStandardClassificationService Classifications { get; }
		internal int CheckWord(string text, int index, out IClassificationType classification, int numberOfLine, int numberOfAllLines)
		{
			int length = text.Length;
			if (index >= length)
				throw new ArgumentOutOfRangeException(nameof(index));


			UpdateKeys(numberOfLine, numberOfAllLines);
			int start = index;
			index = WhileWithPredicate(text, index, chr => Char.IsWhiteSpace(chr));
			if (index > start)
			{
				classification = Classifications.WhiteSpace;
				return index;
			}

			start = index;

			if (text[index] == '<')
			{
				MarkupFlag = true;
				if (numberOfLine > 1)
				{
					HighMarkup = GetPreviosMarkup(numberOfLine);
				}
				CarCompletionSource.SetCompletionList();
			}
			if (text[index] == '>')
			{
				MarkupFlag = false;
				AttributeFlag = false;
			}

			if (text[index] == '\"')
			{
				if ((index + 1) < length)
				{
					index++;
				}
				while (text[index] != '\"' && index < length - 1)
				{
					index++;
				}
				index++;
			}
			else if (Char.IsLetter(text[index]))
			{
				index = WhileWithPredicate(text, index, chr => Char.IsLetter(chr));
			}
			else
			{
				index++;
			}

			string word = text.Substring(start, index - start);

			if (ValueFlag && word.StartsWith("\"") && word.EndsWith("\""))
			{
				classification = Classifications.StringLiteral;
				ValueFlag = false;
				CarCompletionSource.SetCompletionList();
			}
			else if (KeyWordsStatment.ContainsMarkup(word) && MarkupFlag)
			{

				if (KeyWordsStatment.ContainsMarkupInNestedDictionary(word))
				{
					if ((index - word.Length - 1) > 0 && text[index - word.Length - 1].Equals('/'))
					{
						word = "/" + word;
					}
					SavedMarkups[numberOfLine] = word;
				}
				classification = Classifications.Keyword;
				keywordName = word;
				AttributeFlag = true;
				CarCompletionSource.SetCompletionList();
			}
			else if (AttributeFlag)
			{
				if (!keywordName.Equals("markup") && KeyWordsStatment.ContainsAttributeInMarkup(word, keywordName))
				{
					classification = Classifications.NumberLiteral;
					ValueFlag = true;
					attributeName = word;
					CarCompletionSource.SetCompletionList();
				}
				else
				{
					if (word.Equals("<") || word.Equals(">") || word.Equals("?") || word.Equals("=") || word.Equals("/"))
					{
						classification = Classifications.ExcludedCode;
					}
					else
					{
						classification = Classifications.Other;
					}
				}
			}
			else
			{
				if (word.Equals("<") || word.Equals(">") || word.Equals("?") || word.Equals("=") || word.Equals("/"))
				{
					classification = Classifications.ExcludedCode;
				}
				else
				{
					classification = Classifications.Other;
				}
			}
			previousNumberOfLines = numberOfAllLines;
			return index;
		}
		private int WhileWithPredicate(string text, int index, Func<char, bool> predicate)
		{
			for (int length = text.Length; index < length && predicate(text[index]); index++) ;
			return index;
		}
		private void UpdateKeys(int numberOfLine, int numberOfAllLines)
		{
			Dictionary<int, string> MarkupsToUpdate = new Dictionary<int, string>();
			if (numberOfAllLines < previousNumberOfLines)
			{
				foreach (int dKey in SavedMarkups.Keys)
				{
					if (dKey > numberOfLine)
					{
						MarkupsToUpdate[dKey - 1] = SavedMarkups[dKey];
					}
				}
				foreach (var v in MarkupsToUpdate)
				{
					SavedMarkups.Remove(v.Key + 1);
					SavedMarkups[v.Key] = v.Value;
				}
			}
			if (numberOfAllLines > previousNumberOfLines)
			{

				foreach (int dKey in SavedMarkups.Keys)
				{
					if (dKey > numberOfLine)
					{
						MarkupsToUpdate[dKey + 1] = SavedMarkups[dKey];
					}
				}
				foreach (var v in MarkupsToUpdate)
				{
					SavedMarkups.Remove(v.Key - 1);
					SavedMarkups[v.Key] = v.Value;
				}
			}
		}
		internal static string GetKeywordName()
		{
			return keywordName;
		}

		internal static string GetAttributeName()
		{
			return attributeName;

		}
		private string GetPreviosMarkup(int numberOfLine)
		{
			string previousMarkup = "";
			foreach (var v in SavedMarkups)
			{
				if (v.Key < numberOfLine)
				{
					previousMarkup = v.Value;
				}
			}
			return previousMarkup;

		}
	}
}
