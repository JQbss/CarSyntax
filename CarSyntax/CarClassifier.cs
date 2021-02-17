using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;

namespace CarSyntax
{
    internal sealed class CarClassifier : IClassifier
    {
		internal CarClassifier(ITextBuffer buffer,
			IStandardClassificationService classifications,
			IClassificationTypeRegistryService classificationRegistry)
		{
			ClassificationRegistry = classificationRegistry;
			Classifications = classifications;
			Buffer = buffer;

			tokenizer = new CarSyntaxChecker(classifications);
		}
		private readonly CarSyntaxChecker tokenizer;
#pragma warning disable 67
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67
		internal ITextBuffer Buffer { get; }

		internal IClassificationTypeRegistryService ClassificationRegistry { get; }

		internal IStandardClassificationService Classifications { get; }
		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			var list = new List<ClassificationSpan>();
			ITextSnapshot snapshot = span.Snapshot;
			string text = span.GetText();
			int length = span.Length;
			int numberOfLine = GetLineNumber(snapshot, text);
			int index = 0;


			while (index < length)
			{
				int start = index;
				index = tokenizer.CheckWord(text, start, out IClassificationType type, numberOfLine, snapshot.LineCount);

				list.Add(new ClassificationSpan(new SnapshotSpan(snapshot,
					new Span(span.Start + start, index - start)), type));
			}
			return list;
		}

		private int GetLineNumber(ITextSnapshot snapshot, string currentText)
		{
			int counter = 1;
			currentText = currentText.Trim();
			currentText = currentText.Trim('\t');
			IEnumerable<ITextSnapshotLine> textSnapshotLine = snapshot.Lines;
			foreach (var t in textSnapshotLine)
			{
				if (t.GetText().Trim('\t').Equals(currentText))
				{
					return counter;
				}
				counter++;
			}
			return counter;
		}
	}
}
