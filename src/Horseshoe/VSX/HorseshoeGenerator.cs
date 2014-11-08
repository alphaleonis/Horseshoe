using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Alphaleonis.Horseshoe.VSX;
using VSLangProj80;
using Alphaleonis.Horseshoe.Compiler;

namespace Alphaleonis.Horseshoe
{
   [Microsoft.VisualStudio.Shell.CodeGeneratorRegistrationAttribute(typeof(HorseshoeGenerator), "SimpleGenerator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
   [ComVisible(true)]
   [Guid("A11D72BA-797D-4844-AFC3-E4381C7D908C")]
   [ProvideObjectAttribute(typeof(HorseshoeGenerator))]
   public sealed class HorseshoeGenerator : CustomToolBase
   {
      public HorseshoeGenerator()
         : base(".ts")
      {         
      }

      protected override string Generate(string inputfilePath, string inputFileContents, string defaultNamespace, EnvDTE.ProjectItem projectItem, IProgressReporter progress)
      {
         return HorseshoeCompiler.Compile(inputFileContents, new ErrorListener(progress));
      }

      class ErrorListener : Antlr4.Runtime.BaseErrorListener
      {
         private IProgressReporter m_reporter;

         public ErrorListener(IProgressReporter reporter)
         {
            m_reporter = reporter;
         }

         public override void SyntaxError(Antlr4.Runtime.IRecognizer recognizer, Antlr4.Runtime.IToken offendingSymbol, int line, int charPositionInLine, string msg, Antlr4.Runtime.RecognitionException e)
         {
            m_reporter.GenerateError(msg, line, charPositionInLine);
         }
      }
   }
}

