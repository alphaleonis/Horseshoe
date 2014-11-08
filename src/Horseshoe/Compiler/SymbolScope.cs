using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.Compiler
{
   class SymbolScope
   {
      private readonly ParserRuleContext m_context;
      private readonly Dictionary<string, Symbol> m_symbols;

      public SymbolScope(ParserRuleContext context)
      {
         m_context = context;
         m_symbols = new Dictionary<string, Symbol>();
      }

      public ParserRuleContext Context
      {
         get
         {
            return m_context;
         }
      }

      public void Add(Symbol symbol)
      {
         if (m_symbols.ContainsKey(symbol.Name))
            throw new InvalidOperationException(String.Format("Redefinition of symbol {0}", symbol.Name));

         m_symbols.Add(symbol.Name, symbol);
      }

      public Symbol TryGetSymbol(string name)
      {
         Symbol symbol;
         if (m_symbols.TryGetValue(name, out symbol))
            return symbol;

         return null;
      }

      public bool ContainsSymbol(string name)
      {
         return m_symbols.ContainsKey(name);
      }
   }
}

