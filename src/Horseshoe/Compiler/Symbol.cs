using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.Horseshoe.Compiler
{
   class Symbol
   {
      private readonly string m_name;
      private readonly ParserRuleContext m_context;

      public Symbol(string name, ParserRuleContext context)
      {
         m_context = context;
         m_name = name;
      }

      public string Name
      {
         get
         {
            return m_name;
         }
      }

      public ParserRuleContext Context
      {
         get
         {
            return m_context;
         }
      }
   }
}