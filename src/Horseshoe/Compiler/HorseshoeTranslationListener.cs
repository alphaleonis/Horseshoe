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
   class HorseshoeTranslationListener : HorseshoeParserBaseListener
   {
      private const string VAR_TemplateResult = "$templateResult";
      private const string VAR_DataContext = "$dataContext";

      private StringWriter m_result = new StringWriter();
      private IndentedTextWriter m_writer;
      private readonly SymbolTable m_symbols = new SymbolTable();
      private readonly StringBuilder m_buffer = new StringBuilder();
      private bool m_inModule;
      private bool m_trimLeadingWhitespaceFromBody;
      private bool m_invertTrim;

      public string Result
      {
         get
         {
            return m_result.ToString();
         }
      }

      public HorseshoeTranslationListener()
      {
         m_symbols.AddSymbol(new Symbol(VAR_DataContext, null));
         m_symbols.AddSymbol(new Symbol(VAR_TemplateResult, null));
         m_writer = new IndentedTextWriter(m_result, "   ");
      }

      public override void EnterModule(HorseshoeParser.ModuleContext context)
      {
         m_writer.WriteLine("module {0} {{", context.name.GetText());
         PushIndent();
         m_symbols.PushScope(context);
         m_inModule = true;
      }

      public override void ExitModule(HorseshoeParser.ModuleContext context)
      {
         PopIndent();
         m_writer.WriteLine("}");
         m_symbols.PopScope();
         m_inModule = false;
      }

      public override void EnterTemplateDecl(HorseshoeParser.TemplateDeclContext context)
      {
         if (m_inModule)
            m_writer.Write("export ");

         if (context.name == null)
            throw new Exception("No name specified for template.");

         m_writer.WriteLine("module {0} {{", context.name.GetText());
         PushIndent();
         m_writer.WriteLine("export function render({0} : {1}) : string {{", VAR_DataContext, context.contextType.GetText());
         PushIndent();
         m_writer.WriteLine("var {0} : string = '';", VAR_TemplateResult);
         m_symbols.PushScope(context);
         m_invertTrim = context.invertTrim != null;
         m_trimLeadingWhitespaceFromBody = context.openTrimEnd != null;

      }

      public override void ExitTemplateDecl(HorseshoeParser.TemplateDeclContext context)
      {
         FlushBuffer(context.closeTrimStart != null);
         m_writer.WriteLine("return {0};", VAR_TemplateResult);
         PopIndent();
         m_writer.WriteLine("}");
         PopIndent();
         m_writer.WriteLine("}");

         m_symbols.PopScope();
      }

      public override void EnterBody(HorseshoeParser.BodyContext context)
      {
         string text = context.GetText();
         if (m_trimLeadingWhitespaceFromBody ^ m_invertTrim)
         {
            if (String.IsNullOrWhiteSpace(text))
               text = String.Empty;

            text = text.TrimStart();
         }

         m_trimLeadingWhitespaceFromBody = false;
         m_buffer.Append(text);
      }


      private string GetVariableName(HorseshoeParser.ScopeQualifiedIdentifierContext context)
      {
         string variableName = context.id.GetText();
         string scopeName = variableName;
         if (scopeName != null)
         {
            int index = scopeName.IndexOf('.');
            if (index != -1)
               scopeName = scopeName.Substring(0, index);
         }

         if (context.scope != null || !m_symbols.CurrentScope.ContainsSymbol(scopeName))
            variableName = VAR_DataContext + "." + variableName;
         return variableName;
      }


      public override void EnterSubstitution(HorseshoeParser.SubstitutionContext context)
      {
         FlushBuffer((context.trimStart != null) ^ m_invertTrim);
         string variableName = GetVariableName(context.id);
         m_writer.WriteLine("{0} += _.escape(String({1}));", VAR_TemplateResult, variableName);
      }

      public override void ExitSubstitution(HorseshoeParser.SubstitutionContext context)
      {
         m_trimLeadingWhitespaceFromBody = (context.trimEnd != null) ^ m_invertTrim;
      }

      public override void EnterUnescapedSubstitution(HorseshoeParser.UnescapedSubstitutionContext context)
      {
         FlushBuffer(context.trimStart != null);
         string variableName = GetVariableName(context.id);
         m_writer.WriteLine("{0} += {1};", VAR_TemplateResult, variableName);
      }

      public override void ExitUnescapedSubstitution(HorseshoeParser.UnescapedSubstitutionContext context)
      {
         m_trimLeadingWhitespaceFromBody = context.trimEnd != null;
      }

      public override void EnterListIteration(HorseshoeParser.ListIterationContext context)
      {
         FlushBuffer(context.openTrimStart != null);
         m_symbols.PushScope(context);
         m_symbols.AddSymbol(new Symbol(context.variable.GetText(), context.variable));
         m_writer.WriteLine("_.each({0}, function ({1} : {2}) {{", GetVariableName(context.collection), context.variable.GetText(), context.type.GetText());
         PushIndent();
         m_trimLeadingWhitespaceFromBody = context.openTrimEnd != null;
      }

      public override void ExitListIteration(HorseshoeParser.ListIterationContext context)
      {
         FlushBuffer(context.closeTrimStart != null);
         PopIndent();
         m_writer.WriteLine("});");
         m_symbols.PopScope();
         m_trimLeadingWhitespaceFromBody = context.closeTrimEnd != null;
      }


      public override void EnterConditional(HorseshoeParser.ConditionalContext context)
      {
         FlushBuffer(context.openTrimStart != null);
         m_writer.WriteLine("if ({0}{1}) {{", context.NOT() != null ? "!" : "", GetVariableName(context.id));
         PushIndent();
         m_symbols.PushScope(context);
         m_trimLeadingWhitespaceFromBody = context.openTrimEnd != null;
      }

      public override void ExitConditional(HorseshoeParser.ConditionalContext context)
      {
         FlushBuffer(context.closeTrimStart != null);
         PopIndent();
         m_writer.WriteLine("}");
         m_symbols.PopScope();
         m_trimLeadingWhitespaceFromBody = context.closeTrimEnd != null;
      }

      public override void EnterElseClause(HorseshoeParser.ElseClauseContext context)
      {
         FlushBuffer(context.trimStart != null);
         PopIndent();
         m_writer.WriteLine("} else {");
         PushIndent();
         m_trimLeadingWhitespaceFromBody = context.trimEnd != null;
      }

      public override void EnterInvoke(HorseshoeParser.InvokeContext context)
      {
         FlushBuffer(context.trimStart != null);
         m_writer.WriteLine("{0} += {1}({2});", VAR_TemplateResult, context.method.GetText(), GetVariableName(context.argument));
         m_trimLeadingWhitespaceFromBody = context.trimEnd != null;
      }

      private void FlushBuffer(bool trimTrailingWhitespace)
      {
         string text = m_buffer.ToString();
         if (trimTrailingWhitespace ^ m_invertTrim)
         {
            text = text.TrimEnd();
         }
         m_buffer.Length = 0;
         if (text.Length > 0)
         {
            m_writer.WriteLine("{0} += {1};", VAR_TemplateResult, EncodeJsString(text));
         }
         m_trimLeadingWhitespaceFromBody = false;
      }

      private void PushIndent()
      {
         m_writer.Indent += 1;
      }

      private void PopIndent()
      {
         if (m_writer.Indent > 0)
            m_writer.Indent -= 1;
      }

      private static string EncodeJsString(string s)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("\"");
         foreach (char c in s)
         {
            switch (c)
            {
               case '\"':
                  sb.Append("\\\"");
                  break;
               case '\\':
                  sb.Append("\\\\");
                  break;
               case '\b':
                  sb.Append("\\b");
                  break;
               case '\f':
                  sb.Append("\\f");
                  break;
               case '\n':
                  sb.Append("\\n");
                  break;
               case '\r':
                  sb.Append("\\r");
                  break;
               case '\t':
                  sb.Append("\\t");
                  break;
               default:
                  int i = (int)c;
                  if (i < 32 || i > 127)
                  {
                     sb.AppendFormat("\\u{0:X04}", i);
                  }
                  else
                  {
                     sb.Append(c);
                  }
                  break;
            }
         }
         sb.Append("\"");

         return sb.ToString();
      }
   }
}

