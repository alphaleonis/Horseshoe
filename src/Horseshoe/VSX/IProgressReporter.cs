using System;
namespace Alphaleonis.Horseshoe.VSX
{
   public interface IProgressReporter
   {
      void GenerateError(string errorMessage);
      void GenerateError(string errorMessage, int sourceFileLineNumber);
      void GenerateError(string errorMessage, int sourceFileLineNumber, int sourceFileColumnNumber);
      void GenerateWarning(string errorMessage);
      void GenerateWarning(string errorMessage, int sourceFileLineNumber);
      void GenerateWarning(string errorMessage, int sourceFileLineNumber, int sourceFileColumnNumber);
      void ReportProgress(int currentStep, int totalSteps);
   }
}
