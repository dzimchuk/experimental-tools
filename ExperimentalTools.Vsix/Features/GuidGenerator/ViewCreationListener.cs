using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Composition;

namespace ExperimentalTools.Vsix.Features.GuidGenerator
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class ViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService adapterService = null;

        [Import]
        internal IOptions options = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = adapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
            {
                return;
            }

            textView.Properties.GetOrCreateSingletonProperty(
                () => new CommandHandler(textViewAdapter, textView, options));
        }
    }
}
