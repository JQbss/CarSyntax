using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CarSyntax
{
    internal sealed class CarOleCommandTarget : IOleCommandTarget
    {
		internal CarOleCommandTarget(IVsTextView vsTextView, ITextView textView,
			ICompletionBroker completionBroker, SVsServiceProvider serviceProvider)
		{
			CompletionBroker = completionBroker;
			ServiceProvider = serviceProvider;
			TextView = textView;

			vsTextView.AddCommandFilter(this, out nextCommandTarget);
		}
		private ICompletionSession completionSession;
		private IOleCommandTarget nextCommandTarget;
		internal ICompletionBroker CompletionBroker { get; }

		internal SVsServiceProvider ServiceProvider { get; }

		internal ITextView TextView { get; }
		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt,
			IntPtr pvaIn, IntPtr pvaOut)
		{
			ThreadHelper.ThrowIfNotOnUIThread();



			if (VsShellUtilities.IsInAutomationFunction(ServiceProvider))
				return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

			char? typedChar = GetTypedChar(pguidCmdGroup, nCmdID, pvaIn);

			if (HandleCommit(nCmdID, typedChar))
				return VSConstants.S_OK;

			int result = nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt,
				pvaIn, pvaOut);

			return ErrorHandler.Succeeded(result) ?
				HandleCompletion(nCmdID, typedChar, result) : result;
		}

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds,
			OLECMD[] prgCmds, IntPtr pCmdText) =>
			nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		private char? GetTypedChar(Guid commandGroup, uint commandId, IntPtr pvaIn) =>
			commandGroup == VSConstants.VSStd2K &&
			commandId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR ?
			(char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn) : (char?)null;

		private bool HasCompletionSession() =>
			completionSession != null && !completionSession.IsDismissed;

		private bool IsCommitCommand(uint commandId) =>
			commandId == (uint)VSConstants.VSStd2KCmdID.RETURN ||
			commandId == (uint)VSConstants.VSStd2KCmdID.TAB;

		private bool IsCommitChar(char? typedChar) =>
			typedChar.HasValue && (Char.IsWhiteSpace(typedChar.Value) ||
				Char.IsPunctuation(typedChar.Value));

		private bool IsNotProjection(ITextBuffer textBuffer) =>
			!textBuffer.ContentType.IsOfType("projection");

		private bool HandleCommit(uint commandId, char? typedChar)
		{
			if (!HasCompletionSession() || (!IsCommitCommand(commandId) && !IsCommitChar(typedChar)))
				return false;

			if (!completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
			{
				completionSession.Dismiss();
				return false;
			}
			completionSession.Commit();
			return true;
		}

		private int HandleCompletion(uint commandId, char? typedChar, int result)
		{
			if (typedChar.HasValue && (Char.IsLetterOrDigit(typedChar.Value) || typedChar.Equals('<') || typedChar.Equals('\"')
				|| typedChar.Equals(' ') || typedChar.Equals('?') || (Keyboard.IsKeyDown(Key.LeftCtrl) && typedChar.Equals(' '))))
			{
				if (!TriggerCompletion())
					return result;
			}
			else
				if ((commandId != (uint)VSConstants.VSStd2KCmdID.BACKSPACE &&
					 commandId != (uint)VSConstants.VSStd2KCmdID.DELETE) ||
					!HasCompletionSession())
				return result;

			if (!(typedChar.Equals('<') || typedChar.Equals('\"') || typedChar.Equals('?') || typedChar.Equals(' ') || (Keyboard.IsKeyDown(Key.LeftCtrl) && typedChar.Equals(' '))))
			{
				if (completionSession != null)
				{
					completionSession.Filter();
				}
			}

			return VSConstants.S_OK;
		}

		private void OnSessionDismissed(object sender, EventArgs e)
		{
			completionSession.Dismissed -= OnSessionDismissed;
			completionSession = null;
		}

		private bool TriggerCompletion()
		{
			if (HasCompletionSession())
				return true;

			SnapshotPoint? caretPoint = TextView.Caret.Position.Point.GetPoint(
				IsNotProjection, PositionAffinity.Predecessor);
			if (!caretPoint.HasValue)
				return false;

			ITrackingPoint trackingPoint = caretPoint.Value.Snapshot.CreateTrackingPoint(
				caretPoint.Value.Position, PointTrackingMode.Positive);

			completionSession = CompletionBroker.CreateCompletionSession(
				TextView, trackingPoint, true);

			completionSession.Dismissed += OnSessionDismissed;

			completionSession.Start();
			return true;
		}
	}
}
