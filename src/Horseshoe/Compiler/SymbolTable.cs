using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.Compiler
{
   class SymbolTable
   {
      private Stack<SymbolScope> m_frames = new Stack<SymbolScope>();

      public SymbolTable()
      {
         m_frames.Push(new SymbolScope(null));
      }

      public void PushScope(ParserRuleContext context)
      {
         m_frames.Push(new SymbolScope(context));
      }

      public void PopScope()
      {
         if (m_frames.Count == 1)
            throw new InvalidOperationException("Cannot pop the global scope.");

         m_frames.Pop();
      }

      public SymbolScope CurrentScope
      {
         get
         {
            if (m_frames.Count == 0)
               return null;

            return m_frames.Peek();
         }
      }

      public void AddSymbol(Symbol symbol)
      {
         CurrentScope.Add(symbol);
      }

      public Symbol TryGetSymbol(string name)
      {
         foreach (var scope in m_frames)
         {
            var symbol = scope.TryGetSymbol(name);
            if (symbol != null)
               return symbol;
         }

         return null;
      }


   }
}
