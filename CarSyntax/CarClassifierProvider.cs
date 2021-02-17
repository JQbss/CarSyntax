using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CarSyntax
{
	[Export(typeof(IClassifierProvider))]
	[Name(nameof(GimlClassifierProvider))]
	[ContentType(Car.ContentType)]
	internal sealed class GimlClassifierProvider : IClassifierProvider
	{
		[Import]
		private IClassificationTypeRegistryService classificationRegistry = null;
		[Import]
		private IStandardClassificationService classifications = null;
		public IClassifier GetClassifier(ITextBuffer buffer) =>
			buffer.Properties.GetOrCreateSingletonProperty(() =>
				new CarClassifier(buffer, classifications, classificationRegistry));
	}
}
