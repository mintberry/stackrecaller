using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

using System.Text.RegularExpressions;
using DDW.Names;

namespace Prototype
{
	public class Token
	{
		public Token(string value, Color color)
		{
			this.value = value;
			this.color = color;
		}
		
		string value;
		Color color;
		
		public string Value
		{
			get { return value; }
		}

		public Color Color
		{
			get { return color; }
		}	
	}
	
	public class Highlighter
	{
        private const string KEYWORD = "¤keyword¤";
        private const string COMMENT = "¤comment¤";
        private const string NORMAL = "¤normal¤";
        private const string STRING = "¤str¤";
        private const string PRE = "¤pre¤";
        private const string TYPE = "¤type¤";
        private const string IDENT = "¤ident¤";

        public static Token[] TokenizeLine(string line)
        {
            Regex re = new Regex("(" 
                + KEYWORD + "|" 
                + COMMENT + "|"
                + NORMAL + "|" 
                + STRING + "|" 
                + PRE + "|" 
                + TYPE + "|" 
                + IDENT + ")");
            List<Token> tokens = new List<Token>();

            Color color = Color.Black;

            foreach (string s in re.Split(FormatLine(line)))
            {
                switch (s)
                {
                    case NORMAL:
                        color = Color.Black;
                        break;

                    case STRING:
                        color = Color.Green;
                        break;

                    case COMMENT:
                        color = Color.Green;
                        break;

                    case PRE:
                        color = Color.Red;
                        break;

                    case KEYWORD:
                        color = Color.Blue;
                        break;

                    case IDENT:
                        color = Color.Teal;
                        break;

                    case TYPE:
                        color = Color.Blue;
                        break;

                    default:
                        tokens.Add(new Token(s, color));
                        break;
                }
            }

            return tokens.ToArray();
        }
		
		private static string FormatLine(string line)
		{
			Regex re;
			
			// Replace all strings in green
			re = new Regex( "\"[^\"\\\r\n]*(\\.[^\"\\\r\n]*)*\"", RegexOptions.Singleline );
			line = re.Replace( line, new MatchEvaluator( StringHandler ) );

			// Replace comments
			re = new Regex( @"//.*$", RegexOptions.Multiline );
			line = re.Replace( line, new MatchEvaluator( CommentHandler ) );
            if (line.IndexOf("¤comment¤") > 0)
                return line;
			
			// Commented out multi line comments
			//re = new Regex( @"/\*.*?\*/", RegexOptions.Singleline );
			//code = re.Replace( code, new MatchEvaluator( CommentHandler ) );

			// Replace prepocessor commands
			//re = new Regex( @"^\s*#.*", RegexOptions.Multiline );
            re = new Regex(@"^(?<space>\s*)(?<token>#.*)$", RegexOptions.Multiline);
			line = re.Replace( line, new MatchEvaluator( PreHandler ) );

			// Replace keywords
			string keywords =
				"abstract as base break case catch checked " +
				"class const continue default delegate do " +
				"else enum event explicit extern false finally fixed " +
				"for foreach goto if implicit in interface " +
				"internal is lock long namespace new null operator " +
				"out override params private protected public readonly " +
				"ref return sealed sizeof stackalloc static " +
				"struct switch this throw true try typeof " +
				"unchecked unsafe using virtual void while partial";



			keywords = @"\b" + keywords.Replace(" ", @"\b|\b") + @"\b";
			re = new Regex( keywords, RegexOptions.Multiline );
			line = re.Replace( line, new MatchEvaluator( KeywordHandler ) );

            string identifiers = "";
            foreach (IdentifierName idn in Model.Default.ParsedDocument.NameTable)
            {
                identifiers += idn.FullyQualifiedName[0] + " ";
            }

            identifiers = identifiers.TrimEnd();
            identifiers = @"\b" + identifiers.Replace(" ", @"\b|\b") + @"\b";
            re = new Regex(identifiers, RegexOptions.Multiline);
            line = re.Replace(line, new MatchEvaluator(IdentifyerHandler));

			// Replace types
            string types = "byte char decimal double float int sbyte uint ulong ushort List Dictionary bool Point PointF Rectangle String string";

			types = @"\b" + types.Replace(" ", @"\b|\b") + @"\b";
			re = new Regex( types, RegexOptions.Multiline );
			line = re.Replace( line, new MatchEvaluator( TypeHandler ) );

//			// Replace newlines
//			re = new Regex( @"^" );
//			code = re.Replace( code, new MatchEvaluator( LineHandler ) );

			return line;
		}

		private static string StringHandler( Match m )
		{
			return STRING + m.Value + NORMAL;
		}

		private static string CommentHandler( Match m )
		{
            return COMMENT + m.Value + NORMAL;
		}

		private static string PreHandler( Match m )
		{
            if(m.Groups["token"].Success)
                return m.Groups["space"] + PRE + m.Groups["token"].Value + NORMAL;
            else 
                return PRE + m.Value + NORMAL;
		}

		private static string KeywordHandler( Match m )
		{
            return KEYWORD + m.Value + NORMAL;
		}

        private static string IdentifyerHandler(Match m)
        {
            return IDENT + m.Value + NORMAL;
        }
		
		private static string TypeHandler( Match m )
		{
            return TYPE + m.Value + NORMAL;
		}

//		private static string LineHandler( Match m )
//		{
//			return "" + m.Value + "¤newline¤";
//		}
	}
}
