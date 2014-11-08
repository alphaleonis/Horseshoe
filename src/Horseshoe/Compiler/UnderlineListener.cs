using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.Compiler
{
   public class UnderlineListener : BaseErrorListener
   {
      public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
      {
         Console.Error.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
         underlineError(recognizer, (IToken)offendingSymbol,
      line, charPositionInLine);

      }

      protected void underlineError(IRecognizer recognizer, IToken offendingToken, int line, int charPositionInLine)
      {
         CommonTokenStream tokens = (CommonTokenStream)recognizer.InputStream;
         String input = tokens.TokenSource.InputStream.ToString();
         String[] lines = input.Split('\n');
         String errorLine = lines[line - 1];
         Console.Error.WriteLine(errorLine);
         for (int i = 0; i < charPositionInLine; i++)
            Console.Error.Write(" ");
         int start = offendingToken.StartIndex;
         int stop = offendingToken.StopIndex;
         if (start >= 0 && stop >= 0)
         {
            for (int i = start; i <= stop; i++) Console.Error.Write("^");
         }
         Console.Error.WriteLine();
      }
   }
}