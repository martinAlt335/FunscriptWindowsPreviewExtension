using System.Runtime.InteropServices;
using System.IO;
using SharpShell.Attributes;
using SharpShell.SharpPreviewHandler;

namespace FunscriptPreviewHandler
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".funscript")]
    [PreviewHandler]
    public class FunscriptPreviewHandler : SharpPreviewHandler
    {
        protected override PreviewHandlerControl DoPreview()
        {
            // Create preview control
            var control = new FunscriptPreviewControl();
            
            // Tell the control to render the file
            if (!string.IsNullOrEmpty(SelectedFilePath))
            {
                control.HandlePreview(SelectedFilePath);
            }
            
            return control;
        }
    }
}