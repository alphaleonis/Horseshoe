using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.VSX
{
   public sealed class HorseshoeContentDefinitions
   {
      [BaseDefinition("html")]
      [BaseDefinition("code")]
      [Export(typeof(ContentTypeDefinition))]
      [Name("Horseshoe")]
      public ContentTypeDefinition HorseshoeContentType { get; set; }

      [Export(typeof(FileExtensionToContentTypeDefinition))]
      [ContentType("Horseshoe")]
      [FileExtension(".hrs")]
      public FileExtensionToContentTypeDefinition HorseshoeFileExtension { get; set; }
   }
}

