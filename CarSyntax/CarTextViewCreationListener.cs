using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CarSyntax
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name(nameof(CarTextViewCreationListener))]
    [ContentType(Car.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class CarTextViewCreationListener : IVsTextViewCreationListener
    {
		[Import]
		private IVsEditorAdaptersFactoryService adapterService = null;

		[Import]
		private ICompletionBroker completionBroker = null;

		[Import]
		private SVsServiceProvider serviceProvider = null;
		public void VsTextViewCreated(IVsTextView vsTextView)
		{
			ITextView textView = adapterService.GetWpfTextView(vsTextView);

			textView.Properties.GetOrCreateSingletonProperty(() =>
				new CarOleCommandTarget(vsTextView, textView,
					completionBroker, serviceProvider));
		}
	}
}
