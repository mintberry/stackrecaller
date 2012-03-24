using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using DDW.Collections;

// two optimizations are done by the lexer : 
//
// generics : 
//
// the lexer keeps a stack of all '<' saw by him self.
// The lexer pop the last '<' when it reads a '>' or when 
// it is clear that a '<' can not be a generic delcaration
//
// to determine if the '<' begins a generic, the lexer will check each character read until it 
// found a '>'. if the character is not in ['ident', '>', '<', ',', 'white space', '.'], the lexer
// invalidate the '<' and it is removed from the stack  
//
// In parallel, the lexer keeps a trace of all '[' and ']' found between '<' and '>'. -> because
// type parameter can have attributes, so while the position is between a '[' and a ']', all characters are valid, 
// and the '<' is never invalidated.
//
//
// nullable types.
//
// A similar mechanism is used for '?'. Valid characters are different.
// The lexer register the number of opened '[' and '(' after the '?'. 
// when it reachs a ']' or a ')', if this number is equals to zero, this a '?' for 
// a nullable type, else it have to continue the analysis.
// 
// if it reads a ':' after a '?', the lexer invalidates the '?' which can not be a nullable type declaration.

namespace DDW
{
	public class Lexer
	{
		private static SortedList<string, TokenID> keywords = new SortedList<string, TokenID>();
		private int c;
		private int curLine = 1;
        private int curCol = 0;
		private TokenCollection tokens;
		private StreamReader src;
		private List<string> strings;
        private bool prefixedWithAt = false;

        public static bool IsKeyWord(TokenID tokId)
        {
            return keywords.ContainsKey(tokId.ToString().ToLower());
        }

		public Lexer( StreamReader source )
		{
			this.src = source;
		}
		public List<string> StringLiterals
		{
			get
			{
				return strings;
			}
		}

		public TokenCollection Lex()
		{
			tokens = new TokenCollection();
			strings = new List<string>();
			StringBuilder sb = new StringBuilder( );
			int loc = 0;

            // this is a stack to keep all characters '<' in memory.
            // this field is used in the evaluation optimization of the field <see cref="Token.GenericStart"/>.
            Stack<LinkedListNode<Token>> lessers = new Stack<LinkedListNode<Token>>();

            // this is the last previous token '<' that the lexer has read.
            // This field is used in the evaluation optimization of the field <see cref="Token.GenericStart"/>.
            LinkedListNode<Token> lastLesser = null;

            // this integer is used to track attribute declaration
            // see the end of the while loop for his use.
            int possibleAttribute = 0;
            Stack<int> possibleAttributes = new Stack<int>();

            // conditional variables : they help to dertermine if 
            // a '?' is a conditional test or a nullable type declaration
            LinkedListNode<Token> lastQuestion = null;
            // this declaration allow to handle this case :
            // expr ? a<name?>.StaticMethod() : other<name?>.StaticMethod2()
            Stack < LinkedListNode < Token >> oldQuestions = new Stack<LinkedListNode<Token>>();
            Stack<int> oldOpenedBracket = new Stack<int>();
            Stack<int> oldOpenedParent = new Stack<int>();

            // to handle the case  long? method ( int? param )
            // if the number of '(' is equals to zero when reading the ')', this close the '?'
            // same for the ']'
            int openedBracket = 0;
            int openedParent = 0;

			c = src.Read();
readLoop:
			while (c != -1)
			{
                //this variable is setted to true
                // when the lexer reads a token ident, coma or any possible attributes.
                bool possibleGeneric = false;

                bool possibleNullableType = false;
                // this boolean is set to true when the lexer reads the characters ; 
                // '<', '>', '[', ']', '(', ')', '.', ';'
                bool closePossibleNullableType = false;

				switch (c)
				{
				#region EOS
				case -1:
				{
					goto readLoop; // eos 
				}
				#endregion

				#region WHITESPACE
				case (int)'\t':
				{
					//dont add whitespace tokens
					while (c == (int)'\t') 
                    {
                        c = src.Read();
                        curCol += 4;//tabluation are 4 characters ... is the right default value ? 
                    } // check for dups of \t

                    possibleGeneric = true;
                    possibleNullableType = true;
					break;
				}
				case (int)' ':
				{
					//dont add tokens whitespace
					while (c == (int)' ')
                    {
                        c = src.Read();
                        curCol++; 
                    }// check for dups of ' '
                    possibleGeneric = true;
                    possibleNullableType = true;
					break;
				}
				case (int)'\r':
				{
                    tokens.AddLast(new Token(TokenID.Newline, curLine, curCol));
                    c = src.Read();
                    curCol++;
					if (c == (int)'\n')
                        c = src.Read();
					curLine++;
                    curCol = 0;
                    possibleGeneric = true;
                    possibleNullableType = true;
					break;
				}
				case (int)'\n':
				{
                    tokens.AddLast(new Token(TokenID.Newline, curLine, curCol));
                    c = src.Read();
                    curCol++;
					curLine++;
                    curCol = 0;
                    possibleGeneric = true;
                    possibleNullableType = true;
					break;
				}
				#endregion

				#region	STRINGS
				case (int)'@':
				case (int)'\'':
				case (int)'"':
				{
                    int startCol = curCol;
					bool isVerbatim = false;
					if (c == (int)'@')
					{
						isVerbatim = true;
                        c = src.Read(); // skip to follow quote
                        curCol++;

                        if (c != '\'' && c != '"')
                        {
                            //this is not a string, but this is an identifier usig a keyword id
                            prefixedWithAt = true;
                            goto default;
                        }
					}
					sb.Length = 0;
					int quote = c;
					bool isSingleQuote = (c == (int)'\'');
                    c = src.Read();
                    curCol++;
					while (c != -1)
					{
						if (c == (int)'\\' && !isVerbatim) // normal escaped chars
						{
                            c = src.Read();
                            curCol++;
							switch (c)
							{
								//'"\0abfnrtv
								case -1:
								{
									goto readLoop;
								}
								case 0:
								{
                                    sb.Append("\\0");
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'a':
								{
									sb.Append( "\\a" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'b':
								{
									sb.Append( "\\b" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'f':
								{
									sb.Append( "\\f" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'n':
								{
									sb.Append( "\\n" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'r':
								{
									sb.Append( "\\r" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'t':
								{
									sb.Append( "\\t" );
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'v':
								{
									sb.Append("\\v");
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'\\':
								{
									sb.Append("\\\\");
                                    c = src.Read();
                                    curCol++;
									break;
								}
								case (int)'\'':
									{
										sb.Append("\\\'");
                                        c = src.Read();
                                        curCol++;
										break;
									}
								case (int)'\"':
									{
										// strings are always stored as verbatim for now, so the double quote is needed
										sb.Append("\\\"\\\"");
                                        c = src.Read();
                                        curCol++;
										break;
									}
								default:
								{
									sb.Append( (char)c );
									break;
								}
							}
						}
						else if (c == (int)'\"') 
						{
                            c = src.Read();
                            curCol++;
							// two double quotes are escapes for quotes in verbatim mode
							if (c == (int)'\"' && isVerbatim)// verbatim escape
							{
								sb.Append("\"\"");
                                c = src.Read();
                                curCol++;
							}
							else if (isSingleQuote)
							{
								sb.Append('\"');
                                curCol++;
							}
							else
							{
								break;
							}
						}
						else // non escaped
						{
							if (c == quote)
							{
								break;
							}

							sb.Append((char)c);
                            c = src.Read();
                            curCol++;
						}

					}
					if (c != -1)
					{
						if (c == quote)
						{
                            c = src.Read(); // skip last quote
                            curCol++;
						}

						loc = strings.Count;
						strings.Add( sb.ToString( ) );
						if (quote == '"')
                            tokens.AddLast(new Token(TokenID.StringLiteral, loc, curLine, startCol));
						else
                            tokens.AddLast(new Token(TokenID.CharLiteral, loc, curLine, startCol));
					}
					break;
				}
				#endregion

				#region PUNCTUATION
				case (int)'!':
				{
                    c = src.Read();
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.NotEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;

					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Not, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'#':
				{
					// preprocessor
                    tokens.AddLast(new Token(TokenID.Hash, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'$':
				{
                    tokens.AddLast(new Token(TokenID.Dollar, curLine, curCol)); // this is error in C#
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'%':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.PercentEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Percent, curLine, curCol));
					}
                    curCol++;
					break;
				}
				case (int)'&':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.BAndEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'&')
					{
                        tokens.AddLast(new Token(TokenID.And, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.BAnd, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'(':
				{
                    if (lastQuestion != null)
                    {
                        openedParent++;
                    }

                    tokens.AddLast(new Token(TokenID.LParen, curLine, curCol));
                    possibleNullableType = true;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)')':
				{
                    if (lastQuestion != null)
                    {
                        if (openedParent == 0)
                        {
                            closePossibleNullableType = true;
                        }
                        else
                        {
                            openedParent--;
                        }
                    }

                    tokens.AddLast(new Token(TokenID.RParen, curLine, curCol));
                    possibleNullableType = true;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'*':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.StarEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Star, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'+':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.PlusEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'+')
					{
                        tokens.AddLast(new Token(TokenID.PlusPlus, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Plus, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)',':
				{
                    if (tokens.Last == lastQuestion)
                    {
                        closePossibleNullableType = true;
                    }

                    tokens.AddLast(new Token(TokenID.Comma, curLine, curCol));
                    // comma is authorized in generics
                    possibleGeneric = true;
                    possibleNullableType = true;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'-':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.MinusEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'-')
					{
                        tokens.AddLast(new Token(TokenID.MinusMinus, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'>')
					{
                        tokens.AddLast(new Token(TokenID.MinusGreater, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Minus, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'/':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.SlashEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'/')
					{
                        c = src.Read();
                        int startCol = curCol;
                        curCol++;
						sb.Length = 0;
                        while (c != '\n' && c != '\r' && c != -1)
						{
							sb.Append( (char)c );
                            c = src.Read();
                            curCol++;
						}
						int index = this.strings.Count;
						this.strings.Add(sb.ToString());
                        tokens.AddLast(new Token(TokenID.SingleComment, index, curLine, startCol));
                        possibleGeneric = true;
                        possibleNullableType = true;
					}
					else if (c == (int)'*')
					{
                        c = src.Read();
                        int startCol = curCol;
                        curCol++;
						sb.Length = 0;
						for (bool exit=false; !exit; )
						{
                            switch( c )
                            {
                                case (int)'\r':
                                    {
                                        c = src.Read();
                                        curCol++;
                                        if (c == (int)'\n')
                                            c = src.Read();
                                        curLine++;
                                        curCol = 0;
                                        break;
                                    }
                                case (int)'\n':
                                    {
                                        c = src.Read();
                                        curCol++;
                                        curLine++;
                                        curCol = 0;
                                        break;
                                    }
                                case (int)'*':
                                    {
                                        c = src.Read();
                                        curCol++;
                                        if (c == -1 || c == (int)'/')
                                        {
                                            c = src.Read();
                                            curCol++;
                                            exit = true;
                                        }
                                        else
                                        {
                                            sb.Append('*');
                                        }
                                        break;
                                    }
                                case (int)-1:
                                    {
                                        exit = true;
                                        break;
                                    }
                                default:
                                    {
                                        sb.Append((char)c);
                                        c = src.Read();
                                        curCol++;
                                        break;
                                    }

                            }
						}
						int index = this.strings.Count;
						this.strings.Add(sb.ToString());
                        tokens.AddLast(new Token(TokenID.MultiComment, index, curLine, startCol));
                        possibleGeneric = true;
                        possibleNullableType = true;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Slash, curLine, curCol));
					}

                    curCol++;
					break;
				}

				case (int)':':
				{
                    c = src.Read();
                    
					if (c == (int)':')
					{
                        tokens.AddLast(new Token(TokenID.ColonColon, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Colon, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)';':
				{
                    tokens.AddLast(new Token(TokenID.Semi, curLine, curCol));
                    possibleNullableType = true;
                    closePossibleNullableType = true;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'<':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.LessEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'<')
					{
                        c = src.Read();
                        
						if (c == (int)'=')
						{
                            tokens.AddLast(new Token(TokenID.ShiftLeftEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
						}
						else
						{
                            tokens.AddLast(new Token(TokenID.ShiftLeft, curLine, curCol));
						}

                        curCol++;
					}
					else
					{
                        // it is probably a generic open token
                        // if another generic was opened
                        // it pushes the previous '<' on the stack.
                        if (lastLesser != null )
                        {
                            lessers.Push(lastLesser);
                            possibleAttributes.Push(possibleAttribute);
                            possibleAttribute = 0;
                        }

                        tokens.AddLast(new Token(TokenID.Less, curLine, curCol));

                        lastLesser = tokens.Last;
                        possibleGeneric = true;
					}

                    curCol++;
					break;
				}
				case (int)'=':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.EqualEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Equal, curLine, curCol));
                        possibleNullableType = true;
                        closePossibleNullableType = true;
					}
                    curCol++;
					break;
				}
				case (int)'>':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.GreaterEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'>')
					{
                        c = src.Read();
                        
						if (c == (int)'=')
						{
                            tokens.AddLast(new Token(TokenID.ShiftRightEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
						}
						else
						{
                            bool convertIt = false;
                            // case of the shift right operator
                            // it might be a double generic closure 
                            // so we must analyze it to convert it in the right token

                            // if there is a previous '<'
                            // and this previous '<' has not been set as a "no generic opening" token
                            // and if there is a "previous-1" '<' and this "previous-1" '<' has not been set as a "no generic opening" token
                            // then this shift right is a double generic closure
                            if (lastLesser != null)
                            {
                                if (lastLesser.Value.GenericStart != false)
                                {
                                    if (lessers.Count > 0 && lessers.Peek().Value.GenericStart != false)
                                    {
                                        convertIt = true;
                                    }
                                }
                            }

                            if (!convertIt)
                            {
                                tokens.AddLast(new Token(TokenID.ShiftRight, curLine, curCol));
                            }
                            else
                            {
                                if (tokens.Last == lastQuestion)
                                {
                                    closePossibleNullableType = true;
                                }

                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol++));
                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));

                                // pop the previous-1 '<'.
                                lastLesser = lessers.Pop();
                                possibleAttribute = possibleAttributes.Pop();

                                // the previous-1 '<' has been validated by the '>>', so back to the previous-2 '<' ( if exists )
                                if (lessers.Count > 0)
                                {
                                    lastLesser = lessers.Pop();
                                    possibleAttribute = possibleAttributes.Pop();
                                }
                                else
                                {
                                    lastLesser = null;
                                    possibleAttribute = 0;
                                }
                            }

                            possibleGeneric = convertIt;
                            possibleNullableType = convertIt;
						}

                        curCol++;
					}
					else
					{
                        if (tokens.Last == lastQuestion)
                        {
                            closePossibleNullableType = true;
                        }

                        tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));
                        // greater is authorized in generics
                        possibleGeneric = true;
                        possibleNullableType = true;

                        // if the previous '<' has not been set to false
                        // this is a generic declaration

                        // still '<' on the stack ? 
                        if (lessers.Count > 0)
                        {
                            lastLesser = lessers.Pop();
                            possibleAttribute = possibleAttributes.Pop();
                        }
                        else
                        {
                            lastLesser = null;
                            possibleAttribute = 0;
                        }
					}

                    curCol++;
					break;
				}
				case (int)'?':
				{
                    c = src.Read();
                    
					if (c == (int)'?')
					{
                        tokens.AddLast(new Token(TokenID.QuestionQuestion, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.Question, curLine, curCol));
                        possibleNullableType = true;
                        if (lastQuestion != null)
                        {
                            oldQuestions.Push(lastQuestion);
                            oldOpenedBracket.Push(openedBracket);
                            oldOpenedParent.Push(openedParent);
                        }
                        openedBracket = 0;
                        openedParent = 0;
                        lastQuestion = tokens.Last;
					}

                    curCol++;
					break;
				}

				case (int)'[':
				{
                    if (lastQuestion != null)
                    {
                        openedBracket++;
                    }

                    tokens.AddLast(new Token(TokenID.LBracket, curLine, curCol));
                    possibleNullableType = true;
                    possibleAttribute++;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'\\':
				{
                    tokens.AddLast(new Token(TokenID.BSlash, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)']':
				{
                    if (lastQuestion != null)
                    {
                        if (openedBracket == 0)
                        {
                            closePossibleNullableType = true;
                        }
                        else
                        {
                            openedBracket--;
                        }
                    }

                    tokens.AddLast(new Token(TokenID.RBracket, curLine, curCol));
                    possibleGeneric = true;
                    possibleAttribute--;
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'^':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.BXorEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.BXor, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'`':
				{
                    tokens.AddLast(new Token(TokenID.BSQuote, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'{':
				{
                    tokens.AddLast(new Token(TokenID.LCurly, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'|':
				{
                    c = src.Read();
                    
					if (c == (int)'=')
					{
                        tokens.AddLast(new Token(TokenID.BOrEqual, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else if (c == (int)'|')
					{
                        tokens.AddLast(new Token(TokenID.Or, curLine, curCol));
                        c = src.Read();
                        curCol++;
					}
					else
					{
                        tokens.AddLast(new Token(TokenID.BOr, curLine, curCol));
					}

                    curCol++;
					break;
				}
				case (int)'}':
				{
                    tokens.AddLast(new Token(TokenID.RCurly, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				case (int)'~':
				{
                    tokens.AddLast(new Token(TokenID.Tilde, curLine, curCol));
                    c = src.Read();
                    curCol++;
					break;
				}
				#endregion

				#region NUMBERS
				case (int)'0': case (int)'1': case (int)'2': case (int)'3': case (int)'4':
				case (int)'5': case (int)'6': case (int)'7': case (int)'8': case (int)'9':
				case (int)'.':
				{
                    int startCol = curCol;
					sb.Length = 0;
					TokenID numKind = TokenID.IntLiteral; // default
					bool isReal = false;

					// special case dot
					if (c == (int)'.')
					{
                        c = src.Read();
                        curCol++;

                        if (c < '0' || c > '9')
						{
                            if (tokens.Last == lastQuestion)
                            {
                                closePossibleNullableType = true;
                            }

                            tokens.AddLast(new Token(TokenID.Dot, curLine, curCol-1));
                            possibleNullableType = true;
                            possibleGeneric = true;
							break;
						}
						else
						{
							sb.Append('.'); 
							numKind = TokenID.RealLiteral;
							isReal = true;
						}
					}
					bool isNum = true;
					if (c == (int)'0')
					{
						sb.Append((char)c);
                        c = src.Read();
                        curCol++;
						if (c == (int)'x' || c == (int)'X')
						{
							sb.Append((char)c);
							isNum = true;
							while (isNum && c != -1)
							{
                                c = src.Read();
                                curCol++;
								switch (c)
								{
									case (int)'0': case (int)'1': case (int)'2': case (int)'3':
									case (int)'4': case (int)'5': case (int)'6': case (int)'7':
									case (int)'8': case (int)'9': case (int)'A': case (int)'B':
									case (int)'C': case (int)'D': case (int)'E': case (int)'F':
									case (int)'a': case (int)'b': case (int)'c': case (int)'d':
									case (int)'e': case (int)'f':
									{
										sb.Append( (char)c );
										break;
									}
									default:
									{
										isNum = false;
										break;
									}
								}
							}
							// find possible U and Ls
							if (c == (int)'l' || c == (int)'L')
							{
								sb.Append((char)c);
                                c = src.Read();
                                curCol++;
								numKind = TokenID.LongLiteral;
								if (c == (int)'u' || c == (int)'U')
								{
									sb.Append((char)c);
									numKind = TokenID.ULongLiteral;
                                    c = src.Read();
                                    curCol++;
								}
							}
							else if (c == (int)'u' || c == (int)'U')
							{
								sb.Append((char)c);
								numKind = TokenID.UIntLiteral;
                                c = src.Read();
                                curCol++;
								if (c == (int)'l' || c == (int)'L')
								{
									sb.Append((char)c);
									numKind = TokenID.ULongLiteral;
                                    c = src.Read();
                                    curCol++;
								}
							}
							//numKind = TokenID.HexLiteral;
							loc = this.strings.Count;
							this.strings.Add(sb.ToString());
                            tokens.AddLast(new Token(numKind, loc, curLine, startCol));
							break; // done number, exits
						}
					}

					// if we get here, it is non hex, but it might be just zero

					// read number part
					isNum = true;
					while (isNum && c != -1)
					{
						switch (c)
						{
							case (int)'0': case (int)'1': case (int)'2': case (int)'3':
							case (int)'4': case (int)'5': case (int)'6': case (int)'7':
							case (int)'8': case (int)'9': 
							{
								sb.Append((char)c);
                                c = src.Read();
                                curCol++;
								break;
							}
							case (int)'.':
							{
								if (isReal) // only one dot allowed in numbers
								{
									numKind = TokenID.RealLiteral;
									loc = this.strings.Count;
									this.strings.Add(sb.ToString());
                                    tokens.AddLast(new Token(numKind, loc, curLine, startCol));
									goto readLoop;
								}

								// might have 77.toString() construct
                                c = src.Read();
                                curCol++;
								if (c < (int)'0' || c > (int)'9')
								{
									loc = this.strings.Count;
									this.strings.Add(sb.ToString());
                                    tokens.AddLast(new Token(numKind, loc, curLine, startCol));
									goto readLoop;
								}
								else
								{
									sb.Append('.');
									sb.Append((char)c);
									numKind = TokenID.RealLiteral;
									isReal = true;
								}
                                c = src.Read();
                                curCol++;
								break;
							}
							default:
							{
								isNum = false;
								break;
							}
						}
					}
					// now test for letter endings

					// first exponent
					if (c == (int)'e' || c == (int)'E')
					{
						numKind = TokenID.RealLiteral;
						sb.Append((char)c);
                        c = src.Read();
                        curCol++;
						if (c == '+' || c == '-')
						{
							sb.Append((char)c);
                            c = src.Read();
                            curCol++;
						}

						isNum = true;
                        while (isNum && c != -1)
						{
							switch (c)
							{
								case (int)'0': case (int)'1': case (int)'2': case (int)'3':
								case (int)'4': case (int)'5': case (int)'6': case (int)'7':
								case (int)'8': case (int)'9': 
								{
									sb.Append((char)c);
                                    c = src.Read();
                                    curCol++;
									break;
								}
								default:
								{
									isNum = false;
									break;
								}
							}
						}
					}
					else if (	c == (int)'d' || c == (int)'D' || 
								c == (int)'f' || c == (int)'F' ||
								c == (int)'m' || c == (int)'M')
					{
						numKind = TokenID.RealLiteral;
						sb.Append((char)c);
                        c = src.Read();
                        curCol++;
					}
					// or find possible U and Ls
					else if (c == (int)'l' || c == (int)'L')
					{
						sb.Append((char)c);
						numKind = TokenID.LongLiteral;
                        c = src.Read();
                        curCol++;
						if (c == (int)'u' || c == (int)'U')
						{
							sb.Append((char)c);
							numKind = TokenID.ULongLiteral;
                            c = src.Read();
                            curCol++;
						}
					}
					else if (c == (int)'u' || c == (int)'U')
					{
						sb.Append((char)c);
						numKind = TokenID.UIntLiteral;
                        c = src.Read();
                        curCol++;
						if (c == (int)'l' || c == (int)'L')
						{
							sb.Append((char)c);
							numKind = TokenID.ULongLiteral;
                            c = src.Read();
                            curCol++;
						}
					}

					loc = this.strings.Count;
					this.strings.Add(sb.ToString());
                    tokens.AddLast(new Token(numKind, loc, curLine, startCol));
					isNum = false;
					break;
				}
				#endregion

				#region IDENTIFIERS/KEYWORDS
				default:
				{
                    int startCol = curCol;
					// todo: deal with unicode chars
					// check if this is an identifier char

                        char convertedChar = (char)c;
                        //do not use code like "if c =='a'" because it might be not latin caharacter
                        if (Char.IsLetter(convertedChar) || convertedChar == '_')
                        {
                            sb.Length = 0;
                            if (prefixedWithAt)
                            {
                                sb.Append("@");
                                prefixedWithAt = false;
                            }
                            sb.Append((char)c);
                            c = src.Read();
                            curCol++;
                            bool endIdent = false;
                            bool possibleKeyword = true;

                            while (c != -1 && !endIdent)
                            {
                                convertedChar = (char)c;

                                if (!Char.IsLetterOrDigit(convertedChar) && convertedChar != '_')
                                {
                                    endIdent = true;
                                }
                                else
                                {    
                                    //do not use code like "if c =='a'" because it might be not latin caharacter
                                    if (!Char.IsLetter(convertedChar) || Char.IsUpper(convertedChar))
                                    {
                                        possibleKeyword = false;
                                    }

                                    sb.Append(convertedChar);
                                    c = src.Read();
                                    curCol++;
                                }
                            }

                            string identText = sb.ToString();
                            bool isKeyword = possibleKeyword ? keywords.ContainsKey(identText) : false;
                            Token tk;
                            if (isKeyword)
                            {
                                tk = new Token((TokenID)keywords[identText], curLine, startCol);
                            }
                            else
                            {
                                loc = this.strings.Count;
                                this.strings.Add(identText);
                                tk = new Token(TokenID.Ident, loc, curLine, startCol);
                            }


                            if (tk.ID == TokenID.Ident
                                    || tk.ID >= TokenID.Byte && tk.ID <= TokenID.UInt)
                            {
                                possibleGeneric = true;
                                possibleNullableType = true;
                            }

                            tokens.AddLast(tk);
                        }
                        else{
                             tokens.AddLast(new Token(TokenID.Invalid, curLine, curCol));
                                c = src.Read();
                                curCol++;
                        }
					break;
				}
				#endregion
				}

                // the lexer encountered an invalid character for a generic declaration
                // and this is not a possible attribute declaration for a type parameter
                // so the previous '<' was not a generic open declaration
                if (lastLesser != null)
                {
                    if (!possibleGeneric 
                        && possibleAttribute == 0 
                        && ( lastQuestion == null || !possibleNullableType))
                    {
                        Token tk = lastLesser.Value;
                        tk.GenericStart = false;
                        lastLesser.Value = tk;
                        possibleAttribute = 0;

                        // if this is not a generic, it invalidate all previous '<'
                        while (lessers.Count > 0)
                        {
                            lastLesser = lessers.Pop();
                            tk = lastLesser.Value;
                            tk.GenericStart = false;
                            lastLesser.Value = tk;
                            // pop possible attributes stack                             
                            possibleAttribute = possibleAttributes.Pop();
                        }

                        lastLesser = null;
                        possibleAttribute = 0;
                    }
                }

                if (lastQuestion != null)
                {
                    if (!possibleNullableType)
                    {
                        if (oldQuestions.Count > 0 )
                        {
                            lastQuestion = oldQuestions.Pop();
                            openedBracket = oldOpenedBracket.Pop();
                            openedParent = oldOpenedParent.Pop();
                        }

                        Token tk = lastQuestion.Value;
                        tk.NullableDeclaration = false;
                        lastQuestion.Value = tk;
                        lastQuestion = null;
                    }

                    if (closePossibleNullableType)
                    {
                        lastQuestion = null;

                        if (oldQuestions.Count > 0)
                        {
                            lastQuestion = oldQuestions.Pop();
                            openedBracket = oldOpenedBracket.Pop();
                            openedParent = oldOpenedParent.Pop();
                        }
                    }
                }
			}
			return tokens;
		}

		#region STATIC CTOR
		static Lexer( )
		{
			keywords.Add( "byte", TokenID.Byte );
			keywords.Add( "bool", TokenID.Bool );
			keywords.Add( "char", TokenID.Char );
			keywords.Add( "double", TokenID.Double );
			keywords.Add( "decimal", TokenID.Decimal );
			keywords.Add( "float", TokenID.Float );
			keywords.Add( "int", TokenID.Int );
			keywords.Add( "long", TokenID.Long );
			keywords.Add( "object", TokenID.Object );
			keywords.Add( "sbyte", TokenID.SByte );
			keywords.Add( "string", TokenID.String );
			keywords.Add( "short", TokenID.Short );
			keywords.Add( "ushort", TokenID.UShort );
			keywords.Add( "ulong", TokenID.ULong );
			keywords.Add( "uint", TokenID.UInt );
			keywords.Add( "abstract", TokenID.Abstract );
			keywords.Add( "const", TokenID.Const );
			keywords.Add( "extern", TokenID.Extern );
            keywords.Add("alias", TokenID.Alias);
			keywords.Add( "explicit", TokenID.Explicit );
			keywords.Add( "implicit", TokenID.Implicit );
			keywords.Add( "internal", TokenID.Internal );
			keywords.Add( "new", TokenID.New );
			keywords.Add( "out", TokenID.Out );
			keywords.Add( "override", TokenID.Override );
			keywords.Add( "private", TokenID.Private );
			keywords.Add( "public", TokenID.Public );
			keywords.Add( "protected", TokenID.Protected );
			keywords.Add( "ref", TokenID.Ref );
			keywords.Add( "readonly", TokenID.Readonly );
			keywords.Add( "static", TokenID.Static );
			keywords.Add( "sealed", TokenID.Sealed );
			keywords.Add( "volatile", TokenID.Volatile );
			keywords.Add( "virtual", TokenID.Virtual );
			keywords.Add( "class", TokenID.Class );
			keywords.Add( "delegate", TokenID.Delegate );
			keywords.Add( "enum", TokenID.Enum );
			keywords.Add( "interface", TokenID.Interface );
			keywords.Add( "struct", TokenID.Struct );
			keywords.Add( "as", TokenID.As );
			keywords.Add( "base", TokenID.Base );
			keywords.Add( "break", TokenID.Break );
			keywords.Add( "catch", TokenID.Catch );
			keywords.Add( "continue", TokenID.Continue );
			keywords.Add( "case", TokenID.Case );
			keywords.Add( "do", TokenID.Do );
			keywords.Add( "default", TokenID.Default );
			keywords.Add( "else", TokenID.Else );
			keywords.Add( "for", TokenID.For );
			keywords.Add( "foreach", TokenID.Foreach );
			keywords.Add( "finally", TokenID.Finally );
			keywords.Add( "fixed", TokenID.Fixed );
			keywords.Add( "goto", TokenID.Goto );
			keywords.Add( "if", TokenID.If );
			keywords.Add( "in", TokenID.In );
			keywords.Add( "is", TokenID.Is );
			keywords.Add( "lock", TokenID.Lock );
			keywords.Add( "return", TokenID.Return );
			keywords.Add( "stackalloc", TokenID.Stackalloc );
			keywords.Add( "switch", TokenID.Switch );
			keywords.Add( "sizeof", TokenID.Sizeof );
			keywords.Add( "throw", TokenID.Throw );
			keywords.Add( "try", TokenID.Try );
			keywords.Add( "typeof", TokenID.Typeof );
			keywords.Add( "this", TokenID.This );
			keywords.Add( "void", TokenID.Void );
			keywords.Add( "while", TokenID.While );
			keywords.Add( "checked", TokenID.Checked );
			keywords.Add( "event", TokenID.Event );
			keywords.Add( "namespace", TokenID.Namespace );
			keywords.Add( "operator", TokenID.Operator );
			keywords.Add( "params", TokenID.Params );
			keywords.Add( "unsafe", TokenID.Unsafe );
			keywords.Add( "unchecked", TokenID.Unchecked );
			keywords.Add( "using", TokenID.Using );
            keywords.Add("where", TokenID.Where);
            keywords.Add("partial", TokenID.Partial);
            keywords.Add("yield", TokenID.Yield);

			//keywords.Add( "assembly", TokenID.Assembly );
			//keywords.Add( "property", TokenID.Property );
			//keywords.Add( "method", TokenID.Method );
			//keywords.Add( "field", TokenID.Field );
			//keywords.Add( "param", TokenID.Param);
			//keywords.Add( "type", TokenID.Type);

			keywords.Add("true", TokenID.TrueLiteral);
			keywords.Add("false", TokenID.FalseLiteral);
			keywords.Add("null", TokenID.NullLiteral);

		}
		#endregion

	}

}
