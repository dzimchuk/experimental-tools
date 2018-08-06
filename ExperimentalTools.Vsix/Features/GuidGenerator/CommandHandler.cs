using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace ExperimentalTools.Vsix.Features.GuidGenerator
{
    internal class CommandHandler : IOleCommandTarget
    {
        private const string triggerString = "nguid";
        private readonly int triggerStringLength = triggerString.Length;

        private readonly ITextView textView;
        private readonly IOleCommandTarget nextCommandHandler;
        private readonly IOptions options;

        public CommandHandler(IVsTextView textViewAdapter, ITextView textView, IOptions options)
        {
            this.textView = textView;
            this.options = options;

            textViewAdapter.AddCommandFilter(this, out nextCommandHandler);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (options.IsFeatureEnabled(FeatureIdentifiers.GenerateGuid)
                && IsTab(pguidCmdGroup, nCmdID) 
                && !textView.TextBuffer.EditInProgress)
            {
                var bufferPosition = textView.Caret.Position.BufferPosition;
                if (bufferPosition.Position >= triggerStringLength)
                {
                    var span = new Span(bufferPosition.Position - triggerStringLength, triggerStringLength);

                    var text = textView.TextBuffer.CurrentSnapshot.GetText(span);
                    if (triggerString.Equals(text, StringComparison.OrdinalIgnoreCase))
                    {
                        textView.TextBuffer.Replace(span, Guid.NewGuid().ToString().ToUpper());
                        return VSConstants.S_OK;
                    }
                }
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private static bool IsTab(Guid cmdGroup, uint nCmdID)
        {
            return cmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB;
        }
    }
}
