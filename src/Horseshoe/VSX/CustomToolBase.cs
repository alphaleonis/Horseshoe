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
   [ComVisible(true)]
   public abstract class CustomToolBase : IVsSingleFileGenerator, IObjectWithSite
   {
      public CustomToolBase(string fileExtension)
      {
         FileExtension = fileExtension;
      }

      #region IVsSingleFileGenerator Members

      protected abstract string Generate(string inputfilePath, string inputFileContents, string defaultNamespace, ProjectItem projectItem, IProgressReporter progress);

      int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
      {
         pbstrDefaultExtension = FileExtension;
         return 0;
      }
      
      int IVsSingleFileGenerator.Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
      {
         ProgressReporter reporter = new ProgressReporter(pGenerateProgress);

         bool fail = false;
         string output;
         try
         {
            output = Generate(wszInputFilePath, bstrInputFileContents, wszDefaultNamespace, new ServiceProvider(Site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GetService(typeof(ProjectItem)) as ProjectItem, reporter);
         }
         catch (Exception ex)
         {
            output = "An unhandled exception occurred during generation; " + ex.Message;
            reporter.GenerateError(ex.Message);
            fail = true;
         }


         byte[] bytes = Encoding.UTF8.GetBytes(output);

         int outputLength = bytes.Length;
         rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
         Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
         pcbOutput = (uint)outputLength;

         return fail ? VSConstants.E_FAIL : VSConstants.S_OK;
      }
      #endregion

      #region IObjectWithSite Members
      
      void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
      {
         IntPtr pUnk = Marshal.GetIUnknownForObject(Site);
         IntPtr intPointer = IntPtr.Zero;
         Marshal.QueryInterface(pUnk, ref riid, out intPointer);
         ppvSite = intPointer;
      }

      void IObjectWithSite.SetSite(object pUnkSite)
      {
         Site = pUnkSite;
      }

      #endregion

      #region Public Members
      
      /// <summary>
      /// Necessary for the IObjectWithSite interface
      /// implementation
      /// </summary>
      public object Site { get; private set; }

      public string FileExtension { get; private set; }
      #endregion

      class ProgressReporter : Alphaleonis.Horseshoe.VSX.IProgressReporter
      {
         private readonly IVsGeneratorProgress m_progress;

         public ProgressReporter(IVsGeneratorProgress progress)
         {
            m_progress = progress;
         }

         public void GenerateError(string errorMessage)
         {
            GenerateError(errorMessage, 0, 0);
         }

         public void GenerateError(string errorMessage, int sourceFileLineNumber)
         {
            GenerateError(errorMessage, sourceFileLineNumber, 0);
         }

         public void GenerateError(string errorMessage, int sourceFileLineNumber, int sourceFileColumnNumber)
         {
            m_progress.GeneratorError(0, 0, errorMessage, (uint)sourceFileLineNumber, (uint)sourceFileColumnNumber);
         }

         public void GenerateWarning(string errorMessage)
         {
            GenerateWarning(errorMessage, 0, 0);
         }

         public void GenerateWarning(string errorMessage, int sourceFileLineNumber)
         {
            GenerateWarning(errorMessage, sourceFileLineNumber, 0);
         }

         public void GenerateWarning(string errorMessage, int sourceFileLineNumber, int sourceFileColumnNumber)
         {
            m_progress.GeneratorError(1, 0, errorMessage, (uint)sourceFileLineNumber, (uint)sourceFileColumnNumber);
         }

         public void ReportProgress(int currentStep, int totalSteps)
         {
            m_progress.Progress((uint)currentStep, (uint)totalSteps);
         }
      }
   }
}
