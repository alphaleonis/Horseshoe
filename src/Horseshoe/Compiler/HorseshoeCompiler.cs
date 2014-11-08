using Alphaleonis.Horseshoe.Grammars;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.Compiler
{
   public static class HorseshoeCompiler
   {
      public static string Compile(string input)
      {
         return Compile(input, null);
      }


      public static string Compile(string input, BaseErrorListener errorListener)
      {         
         AntlrInputStream stream = new AntlrInputStream(input);
         HorseshoeLexer lexer = new HorseshoeLexer(stream);
         CommonTokenStream tokenStream = new CommonTokenStream(lexer);
         HorseshoeParser parser = new HorseshoeParser(tokenStream);
         if (errorListener != null)
         {
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
         }
         var context = parser.document();
         HorseshoeTranslationListener listener = new HorseshoeTranslationListener();
         ParseTreeWalker walker = new ParseTreeWalker();
         walker.Walk(listener, context);
         return listener.Result;
      }
   }
}

