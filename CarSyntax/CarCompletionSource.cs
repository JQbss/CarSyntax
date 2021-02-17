using CarSyntax.keywords;
using CarSyntax.utils;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace CarSyntax
{
    class CarCompletionSource : ICompletionSource
    {
		private static List<Completion> completions;

		internal CarCompletionSource(ITextBuffer buffer,
			ITextStructureNavigatorSelectorService navigatorService)
		{
			Navigator = navigatorService.GetTextStructureNavigator(buffer);
			Buffer = buffer;
			SetCompletionList();
		}

		internal const string Moniker = "All";

		internal static readonly string DisplayName = Moniker;

		internal ITextBuffer Buffer { get; }
		internal ITextStructureNavigator Navigator { get; }

		private static readonly BitmapImage carImage = BaseToImageConverter.Base64StringToBitmap(icons.Icons.CAR_IMAGE_BASE64);
		private static readonly BitmapImage partsImage = BaseToImageConverter.Base64StringToBitmap(icons.Icons.PARTS_IMAGE_BASE64);
		private static readonly BitmapImage informationImage = BaseToImageConverter.Base64StringToBitmap(icons.Icons.INFORMATION_IMAGE_BASE64);
		private static readonly BitmapImage detailImage = BaseToImageConverter.Base64StringToBitmap(icons.Icons.DETAIL_IMAGE_BASE64);
		private static readonly BitmapImage vehiclesImage = BaseToImageConverter.Base64StringToBitmap(icons.Icons.VEHICLES_IMAGE_BASE64);

		public void AugmentCompletionSession(ICompletionSession session,
			IList<CompletionSet> completionSets)
		{
			ITrackingSpan wordToComplete = GetTrackingSpanForWordToComplete(session);
			CompletionSet completionSet = new CompletionSet(Moniker, DisplayName,
				wordToComplete, completions, null);
			completionSets.Add(completionSet);
		}

		public void Dispose()
		{
		}
		private ITrackingSpan GetTrackingSpanForWordToComplete(ICompletionSession session)
		{
			SnapshotPoint currentPoint;
			char value = (session.TextView.Caret.Position.BufferPosition - 1).GetChar();
			if (value.Equals('<') || value.Equals('\"') || value.Equals('?'))
			{
				currentPoint = session.TextView.Caret.Position.BufferPosition;
			}
			else
			{
				currentPoint = session.TextView.Caret.Position.BufferPosition - 1;
			}
			TextExtent extent = Navigator.GetExtentOfWord(currentPoint);
			return currentPoint.Snapshot.CreateTrackingSpan(extent.Span,
				SpanTrackingMode.EdgeInclusive);
		}
		public static void SetCompletionList()
		{
			string keywordName = CarSyntaxChecker.GetKeywordName();
			string highMarkup = CarSyntaxChecker.HighMarkup;
			string attributeName = CarSyntaxChecker.GetAttributeName();
			if (KeyWordsStatment.ContainsMarkupInNestedDictionary(highMarkup) && CarSyntaxChecker.MarkupFlag && !CarSyntaxChecker.AttributeFlag)
			{
				completions = KeyWordsStatment.GetAllNestedMarkups(highMarkup).Select(keyword => new Completion(keyword)).ToList();
				foreach (var c in completions)
				{
					if (c.DisplayText.EndsWith("/") || c.DisplayText.Equals("vehicle"))
					{
						c.InsertionText = c.DisplayText + ">\n";
					}
					else
					{
						c.InsertionText = c.DisplayText + ">\n</" + c.DisplayText + ">\n";
					}

				}
				foreach (var c in completions)
				{
                    if (c.DisplayText.Equals("vehicle"))
                    {
						c.IconSource = vehiclesImage;
					}
					else if (c.DisplayText.Equals("car"))
					{
						c.IconSource = carImage;
					}
					else
					{
						c.IconSource = partsImage;
					}

				}
			}
			else if (CarSyntaxChecker.ValueFlag)
			{
				if (KeyWordsStatment.values.ContainsKey(attributeName))
				{
					completions = KeyWordsStatment.GetAllValuesByAttruute(attributeName).Select(keyword => new Completion(keyword)).ToList();
					foreach (var c in completions)
					{
						c.InsertionText = c.DisplayText + "\" ";
					}
					completions.ForEach(completion => completion.IconSource = detailImage);
				}
				else
				{
					completions = new List<Completion>();
				}
			}
			else if (CarSyntaxChecker.AttributeFlag)
			{
				completions = KeyWordsStatment.GetAllAttributesByMarkup(keywordName).Select(keyword => new Completion(keyword)).ToList();
				foreach (var c in completions)
				{
					c.InsertionText = " " + c.DisplayText + "= ";
				}
				completions.ForEach(completion => completion.IconSource = informationImage);
			}
			else
			{
				completions = new List<Completion>();
			}
		}
	}
}
