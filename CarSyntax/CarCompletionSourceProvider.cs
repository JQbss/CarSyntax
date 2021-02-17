using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CarSyntax
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name(nameof(CarCompletionSourceProvider))]
    [ContentType(Car.ContentType)]
    internal class CarCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        private ITextStructureNavigatorSelectorService navigatorService = null;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) =>
            new CarCompletionSource(textBuffer, navigatorService);
    }
}
