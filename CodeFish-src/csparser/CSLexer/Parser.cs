using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using DDW.Collections;
using DDW.Enums;
using DDW.Names;

namespace DDW
{
	public class Parser
	{
        /// <summary>
        /// Error message struct
        /// </summary>
        [DebuggerDisplay("Message = {Message}, Line = {Line}, Column={Column} \n File={FileName}")]
        public struct Error
        {
            public readonly string Message;
            public readonly Token Token;
            public readonly int Line;
            public readonly int Column;
            public readonly string FileName;

            public Error(string message, Token token, int line, int column, string fileName)
            {
                Message = message;
                Token = token;
                Line = line;
                Column = column;
                FileName = fileName;
            }
        }

        public static int no_name_index = 0;

        /// <summary>
        /// all parsed errors are stored in this list
        /// </summary>
        public readonly List<Error> Errors = new List<Error>();
		public static readonly Token EOF = new Token(TokenID.Eof);
		private static SortedList<TokenID, Modifier> modMap;
		private static SortedList<string, PreprocessorID> preprocessor;
		private static SortedList<string, PreprocessorID> ppDefs = new SortedList<string,PreprocessorID>();
		private static byte[] precedence;

		private CompilationUnitNode cu;
		private TokenCollection tokens;
		private List<string> strings;

        /// <summary>
        /// stores all parsed generic type
        /// </summary>
        private List<IGeneric> genericList;
        /// <summary>
        /// this class store all parsed cosntructed type.
        /// The key is the full qualified namespace plus the unique identifier of the type
        /// ( <see cref="ConstructedTypeNode.GenericIdentifier"/> ).
        /// I.e. : namespace1.namespace2.type
        /// 
        /// This field has two objectives :
        ///     1. control the unicity of each type between all parsed files.
        ///     2. grabs together partial class defined in different files.
        /// </summary>
        private Dictionary<string, ConstructedTypeNode> constructedTypes;
		private Stack<NamespaceNode> namespaceStack;
		private Stack<ClassNode> typeStack;
        /// <summary>
        /// many syntax check need to access method parameter list
        /// </summary>
        private MethodNode curMethod = null;
        private OperatorNode curOperator = null;

        /// <summary>
        /// used by ParseYieldStatement to check that an iterator is really iterator : 
        /// A class which inherits from the IITerator interface could be iterator ad it could be not iterator.
        /// It depends on it return type.
        /// </summary>
        private IIterator curIterator = null;

		private Stack<ExpressionNode> exprStack;
		private ParseStateCollection curState;
		private InterfaceNode curInterface;
		private Token curtok;
        /// <summary>
        /// this is always the following token of the curtok
        /// </summary>
        private LinkedListNode<Token> curTokNode = null;
		private Modifier curmods;
        private bool nextIsPartial = false;
		private NodeCollection<AttributeNode> curAttributes;

        /// <summary>
        /// can not be Dictionary<string, TypeParameterNode> because in the case
        /// class test : i<X,X>, the keys 'X' will be duplicated
        /// </summary>
        private List<TypeParameterNode> curTypeParameters;

        private Dictionary<string,Constraint> curTypeParameterConstraints;

        /// <summary>
        /// accessed by <see cref="ParseLocalDeclaration"/>
        /// </summary>
        private Stack<BlockStatement> blockStack;

        ///// <summary>
        ///// used to handle the case A<B<X>> that ends with ShiftRight operator ... 
        ///// </summary>
        //private int countOpenedGeneric = 0; 

        //private int index = 0;
		private bool isLocalConst = false;
		private int lineCount = 1;
		private bool ppCondition = false;
		private bool inPPDirective = false;
        /// <summary>
        /// this flag is set when we enter in an anonymous delegate declaration
        /// this is an integer because we can declare anonymous delegate in anonymous delegate ... 
        /// </summary>
        private int isAnonynous = 0;

        private bool isNewStatement = false;
        /// <summary>
        /// this flag is set when we enter in an unsafe block
        /// this is an integer because we can declare unsafe block in unsafe block ... 
        /// 
        /// It is usefull to travel the unsafe state in child block : 
        /// for example, a block declared in an unsafe block, i unsafe, so 
        /// it inherits the unsafe state of his parent block
        /// </summary>
        private int isUnsafe = 0;

        /// <summary>
        /// this flag is set when we enter in an try, catch or finally block
        /// this is an integer because we can declare try another try ... 
        /// </summary>
        private int isTry = 0;
        private int isCatch = 0;
        private int isFinally = 0;

        /// <summary>
        /// to travel information from the try to catch to notify it that the try has a 'yiel return'
        /// </summary>
        private bool hasYieldReturnInTry = false;


        private string currentFileName = string.Empty;

        //iterator constant
        /// <summary>
        /// this field is used to keep a list of interface that made a method an iterator
        /// </summary>
        private StringCollection iteratorsClass = null;

        public ParseStateCollection CurrentState
		{
			get { return curState; }
		}


        public Parser(string currentFileName)
		{
            this.currentFileName = currentFileName;
		}

        
		public CompilationUnitNode Parse(TokenCollection tokens, List<string> strings)
		{			
			this.tokens = tokens;
			this.strings = strings;
			curmods = Modifier.Empty;
			curAttributes = new NodeCollection<AttributeNode>();
            curTypeParameters = new List<TypeParameterNode>();
            curTypeParameterConstraints = new Dictionary<string, Constraint>();

            blockStack = new Stack<BlockStatement>();

			curState = new ParseStateCollection();

			cu = new CompilationUnitNode();
			namespaceStack = new Stack<NamespaceNode>();
			namespaceStack.Push(cu.DefaultNamespace);
			typeStack = new Stack<ClassNode>();
            genericList = new List<IGeneric>();
            constructedTypes = new Dictionary<string, ConstructedTypeNode>();

            iteratorsClass = new StringCollection();

            iteratorsClass.Add( "IEnumerator" );
            iteratorsClass.Add( "IEnumerable" );
            iteratorsClass.Add("Collections.IEnumerator");
            iteratorsClass.Add( "Collections.IEnumerable" );
            iteratorsClass.Add("System.Collections.IEnumerator");
            iteratorsClass.Add("System.Collections.IEnumerable");

            iteratorsClass.Add( "IEnumerator<>" );
            iteratorsClass.Add( "IEnumerable<>" );
            iteratorsClass.Add("Generic.IEnumerator<>");
            iteratorsClass.Add( "Generic.IEnumerable<>" );
            iteratorsClass.Add("Collections.Generic.IEnumerator<>");
            iteratorsClass.Add( "Collections.Generic.IEnumerable<>" );
            iteratorsClass.Add("System.Collections.Generic.IEnumerator<>");
            iteratorsClass.Add( "System.Collections.Generic.IEnumerable<>" );

			exprStack = new Stack<ExpressionNode>();

			// begin parse
            curTokNode = tokens.First;
			Advance();
			ParseNamespaceOrTypes();

            this.cu.NameTable = this.nameTable;

			return cu;
		}

		private void ParseNamespaceOrTypes()								
		{	
			while(!curtok.Equals(EOF))
			{
				// todo: account for assembly attributes
				ParsePossibleAttributes(true);
				if (curAttributes.Count > 0)
				{
                    for( int i = 0; i < curAttributes.Count; ++ i)
                    {
                        AttributeNode an = curAttributes[i];
                        if (an.Modifiers == Modifier.Assembly)
                        {
                            cu.Attributes.Add(an);
                            curAttributes.RemoveAt(i);
                            i--;
                        }					
					}
					curAttributes.Clear();
				}

				// can be usingDirectives, globalAttribs, or NamespaceMembersDecls
				// NamespaceMembersDecls include namespaces, class, struct, interface, enum, delegate
				switch (curtok.ID)
				{
					case TokenID.Using:
						// using directive
						ParseUsingDirectives();
						break;

					case TokenID.New:
					case TokenID.Public:
					case TokenID.Protected:
					case TokenID.Internal:
					case TokenID.Private:
					case TokenID.Abstract:
					case TokenID.Sealed:
                    case TokenID.Static:
                    case TokenID.Unsafe:
						//parseTypeModifier();
						curmods |= modMap[curtok.ID];
						Advance();
						break;

                    case TokenID.Partial:
                        
                        Advance();

                        if (curtok.ID != TokenID.Class
                                && curtok.ID != TokenID.Interface
                                && curtok.ID != TokenID.Struct)
                        {
                            RecoverFromError("Only class, struct and interface may be declared partial.", curtok.ID);
                        }
                        else
                        {
                            nextIsPartial = true;
                        }

                        break;

					case TokenID.Namespace:
						ParseNamespace();
						break;

					case TokenID.Class:
						ParseClass();
						break;

					case TokenID.Struct:
						ParseStruct();
						break;

					case TokenID.Interface:
						ParseInterface();
						break;

					case TokenID.Enum:
						ParseEnum();
						break;
						
					case TokenID.Delegate:
						ParseDelegate();
						break;

					case TokenID.Semi:
						Advance();
						break;
                    case TokenID.Extern:
                        ParseExternAlias();
                        break;

					default:
						return;
				}
			}
		}

        private void ParseExternAlias()
        {
            AssertAndAdvance(TokenID.Extern);
            AssertAndAdvance(TokenID.Alias);

            ExternAliasDirectiveNode node = new ExternAliasDirectiveNode(curtok);
            node.ExternAliasName = ParseIdentifierOrKeyword(false, false, false, false);

            namespaceStack.Peek().ExternAliases.Add(node);
        }

		private void ParseUsingDirectives()									
		{
			do
			{
				Advance();
                UsingDirectiveNode node = new UsingDirectiveNode(curtok);

				QualifiedIdentifierExpression nameOrAlias = ParseQualifiedIdentifier(true, false, false);
				if (curtok.ID == TokenID.Equal)
				{
					Advance();

                    // an alias could be write like 
                    // using alias = path.identifier
                    //
                    // or like
                    // using alias = path.identifier<object>
                    //
                    // is not possible to know before if this a type or an qualified identifier
                    // so we always parse it as a Qualified expression, and if the result is not a type
                    // we keep only the identifier part

                    QualifiedIdentifierExpression target = ParseQualifiedIdentifier(true, false, false);

                    if (target.IsType)
                    {
                        node.Target = new TypeNode( target );
                    }
                    else
                    {
                        // it does not mean that this is not a type.
                        // it only means that actually we can not resolve the type
                        // but in a next stage, we wiil probably resolve it as a type.
                        node.Target = target;
                    }
                    

					node.AliasName = nameOrAlias;
				}
				else
				{
					node.Target = nameOrAlias;
				}

				AssertAndAdvance(TokenID.Semi);

                namespaceStack.Peek().UsingDirectives.Add(node);

                this.currentContext.AddUsingDirective(node);

			} while (curtok.ID == TokenID.Using);
		}

		private PPNode ParsePreprocessorDirective()							
		{
			PPNode result = null;
			int startLine = lineCount;

			inPPDirective = true;
			Advance(); // over hash

            IdentifierExpression ie = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
			string ppKind = ie.Identifier;

			PreprocessorID id = PreprocessorID.Empty;
			if (preprocessor.ContainsKey(ppKind))
			{
				id = preprocessor[ppKind];
			}
			else
			{
				ReportError("Preprocessor directive must be valid identifier, rather than \"" + ppKind + "\".");
			}

			switch (id)
			{
				case PreprocessorID.Define:
					// conditional-symbol pp-newline
                    IdentifierExpression def = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
					if (!ppDefs.ContainsKey(def.Identifier))
					{
						ppDefs.Add(def.Identifier, PreprocessorID.Empty);
					}
					result = new PPDefineNode(def );
					break;
				case PreprocessorID.Undef:
					// conditional-symbol pp-newline
                    IdentifierExpression undef = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
					if(ppDefs.ContainsKey(undef.Identifier))
					{
						ppDefs.Remove(undef.Identifier);
					}
					result = new PPDefineNode(undef );
					break;
				case PreprocessorID.If:
					// pp-expression pp-newline conditional-section(opt)
					if (curtok.ID == TokenID.LParen)
					{
						Advance();
					}
					//int startCount = lineCount;
					ppCondition = false;
					
					// todo: account for true, false, ||, &&, ==, !=, ! 
                    //IdentifierExpression ifexpr = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
                    ExpressionNode ifexpr = ParseExpression(true);

                    // TODO : Parse and interpret identifier : mono file test-345.cs -> #if (!TEST && !DUNNO && !DUNNO)

					if ( (ifexpr is IdentifierExpression)
                            && ppDefs.ContainsKey(((IdentifierExpression)ifexpr).Identifier))
					{
						ppCondition = true;
					}
					//result = new PPIfNode(ParseExpressionToNewline());
					if (curtok.ID == TokenID.RParen)
					{
						Advance();
					}
					if (ppCondition == false)
					{
						// skip this block
						SkipToElseOrEndIf();
					}
					break;
				case PreprocessorID.Elif:
					// pp-expression pp-newline conditional-section(opt)
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Else:
					// pp-newline conditional-section(opt)
					if (ppCondition)
					{
						// skip this block
						SkipToElseOrEndIf();
					}
					break;
				case PreprocessorID.Endif:
					// pp-newline
					result = new PPEndIfNode(curtok);
					ppCondition = false;
					break;
				case PreprocessorID.Line:
					// line-indicator pp-newline
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Error:
					// pp-message
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Warning:
					// pp-message
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Region:
					// pp-message
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Endregion:
					// pp-message
					SkipToEOL(startLine);
					break;
				case PreprocessorID.Pragma:
					// pp-message
                    //pragma-warning-body:
                    //  warning   whitespace   warning-action
                    //  warning   whitespace   warning-action   whitespace   warning-list
                    int start_line = curtok.Line;

                    result = new PPPragmaNode(curtok);

                    if (curtok.Line == start_line)
                    {
                        ((PPPragmaNode)result).Identifier = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
                    }

                    if (curtok.Line == start_line)
                    {
                        string paction = ((IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false)).Identifier;
                        ((PPPragmaNode)result).Action = (PragmaAction)Enum.Parse(typeof(PragmaAction), paction);
                    }

                    while (curtok.Line == start_line)
                    {
                        if (curtok.ID != TokenID.Comma)
                        {
                            if (curtok.ID == TokenID.IntLiteral)
                            {
                                ((PPPragmaNode)result).Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.Int, curtok) ));
                            }
                            else
                            {
                                if (curtok.ID == TokenID.UIntLiteral)
                                {
                                    ((PPPragmaNode)result).Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.UInt,curtok) ));
                                }
                                else
                                {
                                    if (curtok.ID == TokenID.LongLiteral)
                                    {
                                        ((PPPragmaNode)result).Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.Long, curtok) ));
                                    }
                                    else
                                    {
                                        if (curtok.ID == TokenID.ULongLiteral)
                                        {
                                            ((PPPragmaNode)result).Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.ULong, curtok) ));
                                        }
                                        else
                                        {
                                            RecoverFromError(TokenID.IntLiteral);
                                        }
                                    }
                                }
                            }
                        }

                        Advance();
                    }

                    ppCondition = true;
					break;
				default:
					break;
			}
			inPPDirective = false;
			return result;
		}
		private void ParsePossibleAttributes(bool isGlobal)					
		{
			while (curtok.ID == TokenID.LBracket)
			{
                // is it neccessary to save current typeparameter
                // because an attribute can be declared for a type parameter
                // so if we do not do that, the typeparameter will be considered
                // as the type parameter of his attribute declaration ...
                // i.e. : 
                // class cons <[GenPar] A, [GenPar] B>{}
                //
                // without backup, A is considered as the type parameter of GenPar, 
                // and the parser will generate the wrong outpu :  class cons <[GenPar<A>]...

                List<TypeParameterNode> backupTypeParameters = curTypeParameters;
                curTypeParameters = new List<TypeParameterNode>();
                Dictionary<string, Constraint> backupConstraints = curTypeParameterConstraints;
                curTypeParameterConstraints = new Dictionary<string, Constraint>();

				Advance(); // advance over LBracket token
				curmods = ParseAttributeModifiers();

				if (isGlobal && curmods == Modifier.GlobalAttributeMods)
				{
					// nothing to check, globally positioned attributes can still apply to namespaces etc
				}
				else
				{
					uint attribMask = ~(uint)Modifier.AttributeMods;
					if (((uint)curmods & attribMask) != (uint)Modifier.Empty)
						ReportError("Attribute contains illegal modifiers.");
				}

                AttributeNode node = new AttributeNode(curtok);

				Modifier curAttribMods = curmods;
				curmods = Modifier.Empty;

				if (curAttribMods != Modifier.Empty)
				{
					AssertAndAdvance(TokenID.Colon);
				}

                curAttributes.Add(node);
				node.Modifiers = curAttribMods;

				while (curtok.ID != TokenID.RBracket && curtok.ID != TokenID.Eof)
				{
                    node.Name = ParseQualifiedIdentifier(true, false, false);

					if (curtok.ID == TokenID.LParen)
					{
                        // named argument
                        //gtest-286, line 16
                        // [Test(typeof(C<string>))]
                        // public static void Foo()
                        // {
                        // }
                        //
                        // the attribute is applied to the type parameter, so we back up it.
                        NodeCollection<AttributeNode> backupAttributes = curAttributes;
                        curAttributes = new NodeCollection<AttributeNode>();

						// has attribute arguments
						Advance(); // over lparen

						// named args are ident = expr
						// positional args are just expr
						while (curtok.ID != TokenID.RParen && curtok.ID != TokenID.Eof)
						{
							AttributeArgumentNode aNode = new AttributeArgumentNode(curtok);

							if ( curTokNode != null 
                                    && curTokNode.Next != null && curTokNode.Next.Next != null &&
								curtok.ID == TokenID.Ident &&
								curTokNode.Value.ID == TokenID.Equal)
							{   
                                aNode.ArgumentName = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
								Advance(); // over '='
							}
							aNode.Expression = ParseExpression();
							node.Arguments.Add(aNode);

							if (curtok.ID == TokenID.Comma)
							{
								Advance(); // over comma
							}
						}
						AssertAndAdvance(TokenID.RParen);  // over rparen

                        // restore the backup
                        curAttributes = backupAttributes;

						if ( curTokNode != null && curTokNode.Next != null && curTokNode.Next.Next != null &&
							curtok.ID == TokenID.Comma &&
							curTokNode.Value.ID != TokenID.RBracket)
						{
							Advance(); // over comma
							node = new AttributeNode(curtok);
							curAttributes.Add(node);
							node.Modifiers = curAttribMods;
						}
					}
					if (curtok.ID == TokenID.Comma)
					{
						// comma can hang a t end like enums
						Advance();
					}
				}
				AssertAndAdvance(TokenID.RBracket); // over rbracket

                curTypeParameters = backupTypeParameters;
                curTypeParameterConstraints = backupConstraints;
			}
		}
		private void ParseNamespace()										
		{
            Advance(); // advance over Namespace token

            NamespaceNode node = new NamespaceNode(curtok);

			if (curmods != Modifier.Empty)
				ReportError("Namespace can not contain modifiers");

            if (namespaceStack.Count == 1 /*one -> default namespace only*/)
            {

                cu.Namespaces.Add(node);
            }
            else
            {
                namespaceStack.Peek().Namespaces.Add(node);
            }

			namespaceStack.Push(node);


            node.Name = ParseQualifiedIdentifier(false, false, false);

			AssertAndAdvance(TokenID.LCurly);

            this.EnterNamespace(node.Name.QualifiedIdentifier);

			ParseNamespaceOrTypes();

            this.LeaveNamespace(node.Name.QualifiedIdentifier);

			AssertAndAdvance(TokenID.RCurly);
			namespaceStack.Pop();
		}

        /// <summary>
        /// it returns the current qualified name space plus classes path 
        /// </summary>
        /// <returns></returns>
        private string GetQualifiedNameSpace()
        {
            string ret = string.Empty;

            // first parse namespaces
            foreach (NamespaceNode ns in namespaceStack)
            {
                if (ns.Name != null)
                {
                    ret += ns.Name.QualifiedIdentifier;
                    ret += ".";
                }
            }

            // now parse the "type path"
            foreach (ClassNode cn in typeStack)
            {
                ret += cn.GenericIdentifier;
                ret += ".";
            }
            

            return ret.TrimEnd('.');
        }


		// types
        /// <summary>
        /// this method is used to check that a type name is not already 
        /// declared in the same namespace. 
        /// It also do the link between partials types
        /// </summary>
        /// <param name="node"></param>
        private void CheckTypeUnicityAndAdd(ConstructedTypeNode node )
        {
            string key = GetQualifiedNameSpace() + "." + node.GenericIdentifier;

            //checks that this type does not exist in this namespace ( from this file or another parsed file )
            if (constructedTypes.ContainsKey(key))
            {
                ConstructedTypeNode otherType = constructedTypes[key];

                if ( otherType.Kind == node.Kind
                    || otherType.IsPartial
                    || node.IsPartial)
                {
                    if (otherType.IsPartial && !node.IsPartial
                        || node.IsPartial && !otherType.IsPartial
                        )
                    {
                        //one of the two types is declared as partial.
                        //so maybe is missing the partial modifier for one of these types
                        ReportError("The " + node.Kind.ToString().ToLower() + " "
                                            + node.GenericIdentifier 
                                            + " is already declared in the namespace. Is missing partial modifier?");
                    }
                    else
                    {
                        //the first declared partial type become the partial container
                        if (otherType.Partials == null)
                        {
                            otherType.Partials = new PartialCollection();
                        }

                        //all partial classes must have the same modifiers set ( public, protected, internal and private )
                        if ( otherType.Modifiers != Modifier.Empty && node.Modifiers != Modifier.Empty
                                && (otherType.Modifiers & Modifier.Accessibility) != (node.Modifiers & Modifier.Accessibility))
                        {
                            ReportError("All partial types declarations must have the same modifiers set");
                        }

                        if (otherType.IsGeneric)
                        {
                            //all type parameter must have the same name
                            foreach( TypeParameterNode item in otherType.Generic.TypeParameters )
                            {
                                if (!node.Generic.TypeParameters.Contains(item))
                                {
                                    ReportError("Type parameter must have the same name in all partial declarations.");
                                    break;
                                }
                            }

                            // partial declaration with constrainst must have the same constraint declarations.
                            // a partial declaration can have no constraint declaration.

                            ConstructedTypeNode typeWithConstraint = otherType;

                            //if the main partial container does not have any constraint, 
                            //we look for another partial declaration with constraint
                            foreach( IPartial p in typeWithConstraint.Partials )
                            {
                                if (((IGeneric)p).Generic.Constraints.Count > 0)
                                {
                                    typeWithConstraint = (ConstructedTypeNode)p;
                                    break;
                                }
                            }

                            if (typeWithConstraint != null 
                                && typeWithConstraint.Generic.Constraints.Count > 0)
                            {
                                if (typeWithConstraint.Generic.Constraints.Count == node.Generic.Constraints.Count)
                                {
                                    foreach (Constraint cst in typeWithConstraint.Generic.Constraints)
                                    {
                                        //TODO : to resolve : see mono file gtest-129.cs.
                                        // it should not generate errors ... 
                                        if (!node.Generic.Constraints.Contains(cst))
                                        {
                                            ReportError("All partial declarations must the same constraint declaration.");
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if ( typeWithConstraint.Generic.Constraints.Count != 0
                                            && node.Generic.Constraints.Count != 0)
                                    {
                                    ReportError("All partial declarations must have the same constraint declaration.");
                                    }
                                }
                            }

                            //same base class
                            

                        }

                        otherType.Partials.Add(node);
                    }
                }
                else
                {
                    // one another type has the same name.
                    ReportError("The identifier " + node.GenericIdentifier + " is already declared in the namespace.");
                }
            }
            else
            {
                constructedTypes.Add(key, node);
            }
        }

		private void ParseClass()											
		{
			uint classMask = ~(uint)Modifier.ClassMods;
			if ( ((uint)curmods & classMask) != (uint)Modifier.Empty)
				ReportError("Class contains illegal modifiers.");

			ClassNode node = new ClassNode(curtok);

            if (curAttributes.Count > 0)
            {
                node.Attributes = curAttributes;
                curAttributes = new NodeCollection<AttributeNode>();
            }

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek(); //retrieve the parent class if existing;

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the class is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            if (cl != null)
            {
                CheckStaticClass(cl, node.Modifiers, false);
            }

            if ( node.IsStatic  )
            {
                if (( node.Modifiers  & (Modifier.Sealed | Modifier.Abstract)) != Modifier.Empty)
                {
                    ReportError("A class delared as 'static' can not be declared nor 'sealed', nor 'abstract'.");
                }
            }

			Advance(); // advance over Class token

            if (curtok.ID != TokenID.Ident)
            {
                string msg = "Error: Expected " + TokenID.Ident + " found: " + curtok.ID;

                ReportError(msg);

                //to stay coherent with the rest of the parser
                //it generate a random class name 

                node.Name = new IdentifierExpression("no_name_" + (no_name_index++).ToString(), node.RelatedToken );

            }
            else
            {
                node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true);
            }

            ParsePossibleTypeParameterNode(true, true, false);
            ApplyTypeParameters(node);

			if (curtok.ID == TokenID.Colon) // for base members
			{
                if (node.IsStatic)
                {
                    ReportError("Static class can not have nor base classes nor base interface.");
                }

				Advance();
				node.BaseClasses.Add(ParseType());
				while (curtok.ID == TokenID.Comma)
				{
					Advance();
					node.BaseClasses.Add(ParseType());
				}
			}

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                this.nameTable.AddIdentifier(new ClassName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll<string>(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    this.currentContext));
            }
            else
            {
                this.nameTable.AddIdentifier(new ClassName(node.Name.Identifier, 
                    ToVisibilityRestriction(node.Modifiers), 
                    this.currentContext));
            }

            typeStack.Push(node);

            if (cl == null)
            {
                namespaceStack.Peek().Classes.Add(node);
            }
            else
            {
                cl.Classes.Add(node);
            }

			AssertAndAdvance(TokenID.LCurly);

            this.currentContext.Enter(node.Name.Identifier, false);

            while (curtok.ID != TokenID.RCurly && curtok.ID != TokenID.Eof) // guard for empty
			{
				ParseClassMember();
			}

			AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                isUnsafe--;
            }

            this.currentContext.Leave();
			typeStack.Pop();
		}

		private void ParseInterface()										
		{
			InterfaceNode node = new InterfaceNode(curtok);
			curInterface = node;

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

			uint interfaceMask = ~(uint)Modifier.InterfaceMods;
			if (((uint)curmods & interfaceMask) != (uint)Modifier.Empty)
				ReportError("Interface contains illegal modifiers");
			
			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
                //the interface is declared in an unsafe type ?
                node.IsUnsafe = isUnsafe > 0;

			Advance(); // advance over Interface token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true);

            ParsePossibleTypeParameterNode(true, true, false);
            ApplyTypeParameters(node);

			if (curtok.ID == TokenID.Colon) // for base members
			{
				Advance();
				node.BaseClasses.Add(ParseType());
				while (curtok.ID == TokenID.Comma)
				{
					Advance();
					node.BaseClasses.Add(ParseType());
				}
			}

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                this.nameTable.AddIdentifier(new InterfaceName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers), 
                    node.Generic.TypeParameters.ConvertAll<string>(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    this.currentContext));
            }
            else
            {
                this.nameTable.AddIdentifier(new InterfaceName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    this.currentContext));
            }

            this.currentContext.Enter(node.Name.Identifier, false);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Interfaces.Add(node);
            }
            else
            {
                cl.Interfaces.Add(node);
            }

			AssertAndAdvance(TokenID.LCurly);

			while (curtok.ID != TokenID.RCurly) // guard for empty
			{
				ParseInterfaceMember();
			}

			AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            this.currentContext.Leave();

			curInterface = null;

		}
		private void ParseStruct()											
		{
			StructNode node = new StructNode(curtok);

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

			typeStack.Push(node);

			uint structMask = ~(uint)Modifier.StructMods;
			if (((uint)curmods & structMask) != (uint)Modifier.Empty)
				ReportError("Struct contains illegal modifiers");

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
                //the struct is declared in an unsafe type ?
                node.IsUnsafe = isUnsafe > 0;

			Advance(); // advance over Struct token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true);

            ParsePossibleTypeParameterNode(true, true, false);
            ApplyTypeParameters(node);

			if (curtok.ID == TokenID.Colon) // for base members
			{
				Advance();
				node.BaseClasses.Add(ParseType());
				while (curtok.ID == TokenID.Comma)
				{
					Advance();
					node.BaseClasses.Add(ParseType());
				}
			}

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                this.nameTable.AddIdentifier(new StructName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll<string>(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    this.currentContext));
            }
            else
            {
                this.nameTable.AddIdentifier(new StructName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    this.currentContext));
            }

            this.currentContext.Enter(node.Name.Identifier, false);

            if (cl == null)
            {
                namespaceStack.Peek().Structs.Add(node);
            }
            else
            {
                cl.Structs.Add(node);
            }

			AssertAndAdvance(TokenID.LCurly);

			while (curtok.ID != TokenID.RCurly) // guard for empty
			{
				ParseClassMember();
			}

			AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            this.currentContext.Leave();

			typeStack.Pop();
		}
		private void ParseEnum()											
		{
			EnumNode node = new EnumNode(curtok);
			// todo: this needs to have any nested class info, or go in potential container class

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			uint enumMask = ~(uint)Modifier.EnumMods;
			if (((uint)curmods & enumMask) != (uint)Modifier.Empty)
				ReportError("Enum contains illegal modifiers");

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

			Advance(); // advance over Enum token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true);

			if (curtok.ID == TokenID.Colon) // for base type
			{
				Advance();
				node.BaseClass = ParseType();
			}

            CheckTypeUnicityAndAdd(node);

            this.nameTable.AddIdentifier(new EnumName(node.Name.Identifier,
                ToVisibilityRestriction(node.Modifiers),
                this.currentContext));

            this.currentContext.Enter(node.Name.Identifier, false);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Enums.Add(node);
            }
            else
            {
                cl.Enums.Add(node);
            }

			AssertAndAdvance(TokenID.LCurly);

            DDW.Collections.NodeCollection<EnumNode> list = new DDW.Collections.NodeCollection<EnumNode>();
            node.Value = list;

			while (curtok.ID != TokenID.RCurly) // guard for empty
			{
				list.Add( ParseEnumMember() );
			}

			AssertAndAdvance(TokenID.RCurly);

			if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}

            this.currentContext.Leave();
		}
		private void ParseDelegate()										
		{
			DelegateNode node = new DelegateNode(curtok);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			uint delegateMask = ~(uint)Modifier.DelegateMods;
			if (((uint)curmods & delegateMask) != (uint)Modifier.Empty)
				ReportError("Delegate contains illegal modifiers");

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
                //the delegate is declared in an unsafe type ?
                node.IsUnsafe = isUnsafe > 0;
 
			Advance(); // advance over delegate token
			node.Type = ParseType();
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true);

            ParsePossibleTypeParameterNode(true, true, false);
            ApplyTypeParameters(node);

            CheckTypeUnicityAndAdd(node);


			node.Params = ParseParamList();

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Delegates.Add(node);
            }
            else
            {
                cl.Delegates.Add(node);
            }

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

			AssertAndAdvance(TokenID.Semi);

            if (node.IsGeneric)
            {
                this.nameTable.AddIdentifier(new DelegateName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll<string>(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    this.currentContext));
            }
            else
            {
                this.nameTable.AddIdentifier(new DelegateName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    this.currentContext));
            }
        }

        private void CheckStaticClass( ClassNode cl, Modifier nodeModifier, bool checkStaticModifier )
        {
            if (cl.IsStatic)
            {
                if (checkStaticModifier
                    && ((nodeModifier & Modifier.Static) == Modifier.Empty))
                {
                    ReportError("Static class accepts only static members.");
                }

                if (((nodeModifier & (Modifier.Protected | Modifier.Internal)) == (Modifier.Protected | Modifier.Internal)))
                {
                    ReportError("Static class does not accept protected internal member.");
                }
                else
                {
                    //do it in the else condition of the previous if or the message
                    //will be duplicate
                    if (((nodeModifier & Modifier.Protected) == Modifier.Protected))
                    {
                        ReportError("Static class does not accept protected member.");
                    }
                }

            }

        }

		// members
		private void ParseClassMember()										
		{
			// const field method property event indexer operator ctor ~ctor cctor typedecl
			ParsePossibleAttributes(false);
			ParseModifiers();
			switch (curtok.ID)
			{
				case TokenID.Class:
					ParseClass();
					break;

				case TokenID.Interface:
					ParseInterface();
					break;

				case TokenID.Struct:
					ParseStruct();
					break;

				case TokenID.Enum:
					ParseEnum();
					break;

				case TokenID.Delegate:
					ParseDelegate();
					break;

				case TokenID.Const:
					ParseConst();
					break;

				case TokenID.Event:
					ParseEvent();
					break;

				case TokenID.Tilde:
					ParseDestructor();
					break;

				case TokenID.Explicit:
				case TokenID.Implicit:
					ParseOperatorDecl(null);
					break;

				default:
					IType type = ParseType();
					if (type == null)
					{
						ReportError("Expected type or ident in member definition");
					}
					switch (curtok.ID)
					{
						case TokenID.Operator:
							ParseOperatorDecl(type);
							break;
						case TokenID.LParen:
							ParseCtor((TypeNode)type);
							break;
						case TokenID.This: // can be iface.this too, see below
							ParseIndexer(type, null);
							break;
						default:
                            // this is qualified name because it could be
                            // an explicit interface member declaration
                            //HACK : ParseQualifiedIdentifier call PArseTypeParameter
                            //and PArseTypeParameter consume modifiers ... so we backup modifiers

                            Modifier backupMod = curmods;
                            curmods = Modifier.Empty;

                            QualifiedIdentifierExpression name2 = ParseQualifiedIdentifier(true, true, true);

							if (name2 == null || name2.Expressions.Expressions.Count == 0)
							{
								ReportError("Expected name or ident in member definition");
							}

                            curmods = backupMod;

							switch (curtok.ID)
							{
								case TokenID.This:
									ParseIndexer(type, name2);
									break;
								case TokenID.Comma:
								case TokenID.Equal:
								case TokenID.Semi:
                                    ParseField(type, name2);
                                    break;
                                case TokenID.LBracket:
									ParseFixedBuffer(type, name2);
									break;
                                case TokenID.Less:
								case TokenID.LParen:
									ParseMethod(type, name2);
									break;
								case TokenID.LCurly:
									ParseProperty(type, name2);
									break;
								default:
									ReportError("Invalid member syntax");
									break;
							}
							break;
					}
					break;
			}
		}
		private void ParseInterfaceMember()									
		{
			ParsePossibleAttributes(false);
			ParseModifiers();

			switch (curtok.ID)
			{
				case TokenID.Event:
					// event
					InterfaceEventNode node = new InterfaceEventNode(curtok);
					curInterface.Events.Add(node);

					if (curAttributes.Count > 0)
					{
						node.Attributes = curAttributes;
						curAttributes = new NodeCollection<AttributeNode>();
					}

					node.Modifiers = curmods;
					curmods = Modifier.Empty;
					AssertAndAdvance(TokenID.Event);
					node.Type = ParseType();
                    node.Names.Add(ParseQualifiedIdentifier(false, true, true));
					AssertAndAdvance(TokenID.Semi);

					break;
				default:
					IType type = ParseType();
					if (type == null)
					{
						ReportError("Expected type or ident in interface member definition.");
					}
					switch (curtok.ID)
					{
						case TokenID.This:
							// interface indexer
							InterfaceIndexerNode iiNode = new InterfaceIndexerNode(curtok);
							if (curAttributes.Count > 0)
							{
								iiNode.Attributes = curAttributes;
								curAttributes = new NodeCollection<AttributeNode>();
							}
							iiNode.Type = type;
							Advance(); // over 'this'
							iiNode.Params = ParseParamList(TokenID.LBracket, TokenID.RBracket);

							bool hasGetter = false;
							bool hasSetter = false;
							ParseInterfaceAccessors(ref hasGetter, ref hasSetter);
							iiNode.HasGetter = hasGetter;
							iiNode.HasSetter = hasSetter;
							break;

						default:
                            QualifiedIdentifierExpression name = ParseQualifiedIdentifier(false, true, true);
							if (name == null || name.Expressions.Expressions.Count == 0)
							{
								ReportError("Expected name or ident in member definition.");
							}
							switch (curtok.ID)
							{
                                case TokenID.Less:
								case TokenID.LParen:
									// method
									InterfaceMethodNode mnode = new InterfaceMethodNode(curtok);
									curInterface.Methods.Add(mnode);

									if (curAttributes.Count > 0)
									{
										mnode.Attributes = curAttributes;
										curAttributes = new NodeCollection<AttributeNode>();
									}

									mnode.Modifiers = curmods;
									curmods = Modifier.Empty;

									mnode.Names.Add(name);
									mnode.Type = type;

                                    ParsePossibleTypeParameterNode(true, true, false);
                                    //if generic applies type parameter collection to the node
                                    ApplyTypeParameters(mnode);

                                    // starts at LParen

									mnode.Params = ParseParamList();

                                    ParsePossibleTypeParameterConstraintNode(mnode);
                                    ApplyTypeParameterConstraints(mnode);


									AssertAndAdvance(TokenID.Semi);
									break;

								case TokenID.LCurly:
									// property
									InterfacePropertyNode pnode = new InterfacePropertyNode(curtok);
									curInterface.Properties.Add(pnode);									

									// these are the prop nodes
									if (curAttributes.Count > 0)
									{
										pnode.Attributes = curAttributes;
										curAttributes = new NodeCollection<AttributeNode>();
									}
									
									pnode.Modifiers = curmods;
									curmods = Modifier.Empty;

									pnode.Names.Add(name);
									pnode.Type = type;

									bool pHasGetter = false;
									bool pHasSetter = false;
									ParseInterfaceAccessors(ref pHasGetter, ref pHasSetter);
									pnode.HasGetter = pHasGetter;
									pnode.HasSetter = pHasSetter;

									if (curtok.ID == TokenID.Semi)
									{
										AssertAndAdvance(TokenID.Semi);
									}
									break;

								default:
									ReportError("Invalid interface member syntax.");
									break;
							}
							break;
					}
					break;
			}
		}

		private void ParseCtor(TypeNode type)								
		{
			ConstructorNode node = new ConstructorNode(curtok);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			if ((curmods & Modifier.Static) != Modifier.Empty)
			{
				node.IsStaticConstructor = true;
				curmods = curmods & ~Modifier.Static;
			}
			uint mask = ~(uint)Modifier.ConstructorMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("constructor declaration contains illegal modifiers");

			ClassNode cl = typeStack.Peek();
            cl.Constructors.Add(node);

			//node.Attributes.Add(curAttributes);
			//curAttributes.Clear();
			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the constructor is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            if (node.IsStaticConstructor)
            {
                CheckStaticClass(cl, node.Modifiers | Modifier.Static, true);
            }
            else
            {
                CheckStaticClass(cl, node.Modifiers, true);
            }

            node.Type = type;
            QualifiedIdentifierExpression nameCtor = new QualifiedIdentifierExpression(node.RelatedToken);
            nameCtor.Expressions.Add(typeStack.Peek().Name);
			node.Names.Add( nameCtor );

			// starts at LParen
			node.Params = ParseParamList();

			if (curtok.ID == TokenID.Colon)
			{
				Advance();
				if (curtok.ID == TokenID.Base)
				{
					Advance();
					node.HasBase = true;
					node.ThisBaseArgs = ParseArgs();
				}
				else if (curtok.ID == TokenID.This)
				{
					Advance();
					node.HasThis = true;
					node.ThisBaseArgs = ParseArgs();
				}
				else
				{
					RecoverFromError("constructor requires this or base calls after colon", TokenID.Base);
				}
			}
			ParseBlock(node.StatementBlock);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }
		}
		private void ParseDestructor()										
		{
			Advance(); // over tilde

			DestructorNode node = new DestructorNode(curtok);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}
			uint mask = ~(uint)Modifier.DestructorMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("destructor declaration contains illegal modifiers");

			 ClassNode cl = typeStack.Peek();
             cl.Destructors.Add(node);

            node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the destructor is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true);

			if (curtok.ID == TokenID.Ident)
			{
                node.Names.Add(ParseQualifiedIdentifier(false, false, true));
			}
			else
			{
				ReportError("Destructor requires identifier as name.");
			}
			// no args in dtor
			AssertAndAdvance(TokenID.LParen);
			AssertAndAdvance(TokenID.RParen);

			ParseBlock(node.StatementBlock);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }
		}
		private void ParseOperatorDecl(IType type)						
		{			
			OperatorNode node;

            //we check the return type of the method.
            // -> it determines if the method is an iterator.

            if ( type != null //implicit/explicit operator 
                && type is TypeNode
                && iteratorsClass.Contains(((TypeNode)type).GenericIndependentIdentifier))
            {
                node = new OperatorNode(true, curtok);
                curIterator = node;
            }
            else
            {
                node = new OperatorNode(false, curtok);
            }

            curOperator = node;

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			uint mask = ~(uint)Modifier.OperatorMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("operator declaration contains illegal modifiers");

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the operator is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            ClassNode cl = typeStack.Peek();
            cl.Operators.Add(node);

            CheckStaticClass(cl, node.Modifiers, true);
            
			if (type == null && curtok.ID == TokenID.Explicit)
			{
				Advance();
				node.IsExplicit = true;
				AssertAndAdvance(TokenID.Operator);
				type = ParseType();
			}
			else if (type == null && curtok.ID == TokenID.Implicit)
			{
				Advance();
				node.IsImplicit = true;
				AssertAndAdvance(TokenID.Operator);
				type = ParseType();
			}
			else
			{
				AssertAndAdvance(TokenID.Operator);
				node.Operator = curtok.ID;
				Advance();
			}
            node.Type = type;

			NodeCollection<ParamDeclNode> paramList = ParseParamList();
			if (paramList.Count == 0 || paramList.Count > 2)
			{
				ReportError("Operator declarations must only have one or two parameters.");
			}
			node.Param1 = paramList[0];
			if (paramList.Count == 2)
			{
				node.Param2 = paramList[1];
			}
			ParseBlock(node.Statements);

            if (node.IsIterator)
            {
                if ((node.Param1.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                {
                    ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                }

                if ((node.Param2.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                {
                    ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                }
            }

            curIterator = null;
            curOperator = null;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            // Doubt this will be in use.
            if (node.IsImplicit)
                this.nameTable.AddIdentifier(new OperatorName(TokenID.Implicit, this.currentContext));
            else if (node.IsExplicit)
                this.nameTable.AddIdentifier(new OperatorName(TokenID.Explicit, this.currentContext));
            else
                this.nameTable.AddIdentifier(new OperatorName(node.Operator, this.currentContext));
		}

		private void ParseIndexer(IType type, QualifiedIdentifierExpression interfaceType)
		{
			IndexerNode node = new IndexerNode(curtok);
			ClassNode cl = typeStack.Peek();
            cl.Indexers.Add(node);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			uint mask = ~(uint)Modifier.IndexerMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("indexer declaration contains illegal modifiers");


			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the indexer is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

			node.Type = type;
			if (interfaceType != null)
			{
                node.InterfaceType = new TypeNode(interfaceType );
			}

			AssertAndAdvance(TokenID.This);
			node.Params = ParseParamList(TokenID.LBracket, TokenID.RBracket);

			// parse accessor part
			AssertAndAdvance(TokenID.LCurly);

            ParseModifiers();

			if (curtok.ID != TokenID.Ident)
			{
				RecoverFromError("At least one get or set required in accessor", curtok.ID);
			}
			bool parsedGet = false;
			if (strings[curtok.Data] == "get")
			{
				node.Getter = ParseAccessor(type);
				parsedGet = true;
			}

            ParseModifiers();

			if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
			{
				node.Setter = ParseAccessor(type);
			}

            ParseModifiers();
			// get might follow set
			if (!parsedGet && curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
			{
				node.Getter = ParseAccessor(type);
			}

            if (node.Setter != null
                && node.Getter != null)
            {
                if (node.Getter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Modifiers is permitted only for one of the acessors.");
                }
            }
            else
            {
                if (node.Setter == null && node.Getter.Modifiers != Modifier.Empty
                    || node.Getter == null && node.Setter.Modifiers != Modifier.Empty)
                {
                   ReportError("Accessor modifier is authorized only if the 'get' and the 'set' are declared.");
                }

            }

            switch (node.Modifiers)
            {
                case Modifier.Public:
                    break;
                case (Modifier.Protected | Modifier.Internal):
                    if ( node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty
                        && (node.Getter.Modifiers != Modifier.Protected
                                || node.Getter.Modifiers != Modifier.Private
                                || node.Getter.Modifiers != Modifier.Internal
                            )
                        )
                    {
                        ReportError("The property is protected internal, so the accessor can be only protected, private or internal.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty
                        && (node.Setter.Modifiers != Modifier.Protected
                                || node.Setter.Modifiers != Modifier.Private
                                || node.Setter.Modifiers != Modifier.Internal
                            )
                        )
                    {
                        ReportError("The property is protected internal, so the accessor can be only protected, private or internal.");
                    }
                    break;
                case Modifier.Protected:
                case Modifier.Internal:
                    if (node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty
                        && node.Getter.Modifiers != Modifier.Private)
                    {
                        ReportError("The property is protected or internal, so the accessor can be only private.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Private)
                    {
                        ReportError("The property is protected or internal, so the accessor can be only private.");
                    }
                    break;
                case Modifier.Private:
                    if (node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty)
                    {
                        ReportError("The property is private or internal, so the accessor can not have any modifier.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty)
                    {
                        ReportError("The property is private or internal, so the accessor can not have any modifier.");
                    }

                    break;
            }

			AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            #region Save name in the name table
            if (node.Getter != null)
            {
                if (node.Setter != null)
                {
                    if (node.Getter.Modifiers != Modifier.Empty)
                    {
                        this.nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                    else if (node.Setter.Modifiers != Modifier.Empty)
                    {
                        this.nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                    else
                    {
                        this.nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                }
                else
                {
                    this.nameTable.AddIdentifier(new IndexerName(
                        ToVisibilityRestriction(node.Modifiers),
                        NameVisibilityRestriction.Self,
                        ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                        PropertyAccessors.Get,
                        this.currentContext));
                }
            }
            else
            {
                this.nameTable.AddIdentifier(new IndexerName(
                    NameVisibilityRestriction.Self,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                    PropertyAccessors.Set,
                    this.currentContext));
            }
            #endregion
		}

        private void ApplyAttributes( BaseNode node )
        {
            if (curAttributes.Count > 0)
            {
                node.Attributes = curAttributes;
                curAttributes = new NodeCollection<AttributeNode>();
            }
        }

        private void ApplyTypeParameters(IGeneric node)
        {
            if (curTypeParameters.Count > 0)
            {
                if (node.Generic == null)
                {
                    node.Generic = new GenericNode(curtok);
                    genericList.Add(node);
                }

                node.Generic.TypeParameters.AddRange( curTypeParameters );
                curTypeParameters = new List<TypeParameterNode>();
            }
        }

        private void ApplyTypeParameterConstraints(IGeneric node)
        {
            if (curTypeParameterConstraints.Count > 0)
            {
                if (node.IsGeneric)
                {
                    node.Generic.Constraints.AddRange( curTypeParameterConstraints.Values );
                    curTypeParameterConstraints = new Dictionary<string, Constraint>();
                }
                else
                {
                    ReportError("Type parameter constraint applies only on generic type or generic method");
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowAttributes"> Attributes are authorized in type parameter declaration. 
        /// In closed type and in constraints, attributes are not authorized </param>
        /// <param name="inDeclaration"> in generic type declaration, type parameter are only identifier, and type name are not authorized.
        /// So it is used to reject string like "object", "string", etc ... </param>
        /// <param name="allowEmpty"> In the case of the expression typeof(), empty type parameter are allowed </param>
        private void ParseTypeParameterNode(bool allowAttributes, bool inDeclaration, bool allowEmpty )
        {
            if (curmods != Modifier.Empty)
                ReportError("Type parameter can not contain modifiers");

            ParsePossibleAttributes(false);

            if ( !allowAttributes
                && curAttributes.Count > 0)
            {
                RecoverFromError("Attributes not allowed for the type Parameter", TokenID.Ident);
            }

            TypeParameterNode typeParameter = null;

            // inDeclaration is only used to force the type parameter to be an identifier
            // but, due to developer write errors, it might be a TypeNode ... 
            if (inDeclaration)
            {
                //parse ident only
                // parses all type of type parameter to handle wrong user's syntax 
                // i.e. : the user add a type name in a method generic type parameters declaration.
                // -> it avoid crash
                // one another case is :
                //
                // class kv <k,v> {}
                //
                // class m <k,v> : c <k,v>,
                // {
                //         void a <kv <k,v>>.x () {} // a<t>.x ()
                // }
                // 
                ExpressionNode name = ParseIdentifierOrKeyword(true, true, true, false);
                bool empty = true;

                if ( name != null )
                {
                    if ( name is IdentifierExpression )
                    {
                        IdentifierExpression id = name as IdentifierExpression;
                        empty = id.Identifier == string.Empty; // TODO: ?
                        typeParameter = new TypeParameterNode(id );

                        empty = false;
                    }
                    else
                    {
                        if (name is TypeNode)
                        {
                            TypeNode ty = name as TypeNode;
                            empty = ty.Identifier.QualifiedIdentifier == string.Empty;
                            typeParameter = new TypeParameterNode(ty );

                            empty = false;
                        }
                    }
                }

                if ( empty )
                {
                    ReportError("type parameter must be simple name");
                    typeParameter = new TypeParameterNode(name.RelatedToken);
                }
            }
            else
            {
                //ParseType ...WARNING : it could be a type parameter

                switch (curtok.ID)
                {
                    case TokenID.Comma:
                    case TokenID.Greater:
                    case TokenID.RParen:
                        // we are in the case of empty parameter
                        typeParameter = new TypeParameterNode(curtok);
                        break;
                    default:
                        IType t = ParseType();
                        if (t is TypePointerNode)
                        {
                            ReportError("pointer type are not authorized in generic's type parameters.");
                        }
                        // if t is TypePointerNode, it creates an empty type parameter
                        typeParameter = new TypeParameterNode( t as TypeNode );
                        break;
                }

            }

            ApplyAttributes(typeParameter);

            if (!allowEmpty 
                && typeParameter.IsEmpty )
            {
                ReportError("Empty type parameter not allowed");
            }

            if (!typeParameter.IsEmpty
                    && curTypeParameters.Contains(typeParameter))
            {
                ReportError("duplicated type parameter name");
            }
            else
            {
                curTypeParameters.Add(typeParameter);
            }
        }

        private void ParseGenericTypeNodeExpression()
        {
            if (ParsePossibleTypeParameterNode(false, false, true))
            {
                IdentifierExpression leftSide = (IdentifierExpression)exprStack.Pop();
                TypeNode node = new TypeNode(leftSide );

                ApplyTypeParameters(node);

                ParsePossibleTypeParameterConstraintNode(node); // over lparen

                if (curTypeParameterConstraints.Count > 0)
                {
                    ReportError("Constraints are not allowed in generic closed type declaration.");
                }

                //set constraint only to preserve file source 
                // when ToSource is called
                ApplyTypeParameterConstraints(node);

                exprStack.Push(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowAttributes"> Attributes are authorized in type parameter declaration. 
        /// In closed type and in constraints, attributes are not authorized </param>
        /// <param name="inDeclaration"> in generic type declaration, type parameter are only identifier, and type name are not authorized.
        /// So it is used to reject string like "object", "string", etc ... </param>
        /// <param name="allowEmpty"> In the case of the expression typeof(), empty type parameter are allowed </param>
        /// <returns> <c>true</c> if it parsed a type parameter</returns>
        private bool ParsePossibleTypeParameterNode(bool allowAttributes, bool inDeclaration, bool allowEmpty)
        {
            // if the current token is "<", the source code is starting 
            // a type parameter list
            if (curtok.ID == TokenID.Less && curtok.GenericStart)
            {
                    List<TypeParameterNode> backupTyparameters;
                    // use of AssertAndAdvance to valid the previous state restore
                    AssertAndAdvance(TokenID.Less); // advance over Less token

                    //countOpenedGeneric++;

                    //parse it at least one time out of the next while loop
                    // to be sure that at least one type parameter is defined 
                    //( for example -> "< >" is invalid ( except in typeof expression )
                    ParseTypeParameterNode(allowAttributes, inDeclaration, allowEmpty);

                    // back up the previous curTypeParameters or following type parameter are parsed as type node
                    // i.e. : 
                    //
                    // class TOTO<X,Y> is parsed like class TOTO<Y<X>>
                    backupTyparameters = curTypeParameters;

                    while (curtok.ID != TokenID.Greater
                            //&& curtok.ID != TokenID.ShiftRight//we handle the case X<Y<object>> which ends with >>
                            && curtok.ID != TokenID.Eof)
                    {
                        curTypeParameters = new List<TypeParameterNode>();

                        //we need a comma
                        AssertAndAdvance(TokenID.Comma);

                        ParseTypeParameterNode(allowAttributes, inDeclaration, allowEmpty);

                        //test if type parameter already exists ? 
                        backupTyparameters.AddRange(curTypeParameters);
                    }

                    //restor curtype parameter
                    curTypeParameters = backupTyparameters;

                    //check if the end of file is reached
                    AssertAndAdvance(TokenID.Greater); // over ">"

                return true;
            }

            return false;
        }

        private void ParseConstraintNode(Constraint node)
        {
            switch (curtok.ID)
            {
                //type constraint
                case TokenID.Object:
                case TokenID.String:
                    {
                        node.ConstraintExpressions.Add(new ConstraintExpressionNode((PrimaryExpression)ParseType()));
                        break;
                    }
                case TokenID.Class:
                case TokenID.Struct:
                    node.ConstraintExpressions.Add(new ConstraintExpressionNode(curtok));
                    Advance();
                    break;
                case TokenID.Ident:
                    {
                        node.ConstraintExpressions.Add(new ConstraintExpressionNode((PrimaryExpression)ParseType()));
                        break;
                    }

                //constructor constrains?
                case TokenID.New:
                    {
                        if (node.ConstructorConstraint != null)
                        {
                            ReportError("Only one constructor constraint allowed by constraint");
                        }

                        node.ConstructorConstraint = new ConstructorConstraint();

                        Advance();
                        AssertAndAdvance(TokenID.LParen);
                        AssertAndAdvance(TokenID.RParen);
                        break;
                    }
                default:
                    {
                        ReportError("Invalid constraint definition. " + TokenID.Ident.ToString().ToLower() + " expected");
                        break;
                    }
            }
        }

        private void ParseTypeParameterConstraintNode(IGeneric genericType)
        {
            if (curmods != Modifier.Empty)
                ReportError("Type parameter constraint can not contain modifiers");

            ParseTypeParameterNode(false, true, false);

            Constraint node = new Constraint(curTypeParameters[0]);

            curTypeParameters = new List<TypeParameterNode>();

            AssertAndAdvance(TokenID.Colon);

            //parse the first constraint
            ParseConstraintNode(node);

            while( curtok.ID == TokenID.Comma && curtok.ID != TokenID.Eof )
            {
                Advance();
                ParseConstraintNode(node);
            }

            if (!node.TypeParameter.IsEmpty)
            {
                if (!genericType.Generic.TypeParameters.Contains(node.TypeParameter))
                {
                    ReportError("unknow type parameter : " + node.TypeParameter.UniqueIdentifier);
                }
            }

            string key = node.TypeParameter.Identifier.Identifier;

            if (curTypeParameterConstraints.ContainsKey(key))
            {
                ReportError("duplicated constraint for the type parameter '" + key + "'.");
            }

            curTypeParameterConstraints.Add(key, node);
        }

        private void ParsePossibleTypeParameterConstraintNode(IGeneric node)
        {
            // if the current token is "where", the source code is starting 
            // a type parameter constraint
            if (curtok.ID == TokenID.Where)
            {
                Advance(); // advance over Where token

                //parse it at least one time out of the next while loop
                // to be sure that at least one type parameter constraint is defined 
                //( for example -> "where " is invalid
                ParseTypeParameterConstraintNode(node);

                while (curtok.ID == TokenID.Where && curtok.ID != TokenID.Eof)
                {
                    Advance(); //over the TokenID.Where

                    ParseTypeParameterConstraintNode(node);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the method</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
		private void ParseMethod(IType type, QualifiedIdentifierExpression name)	
		{
			uint mask = ~(uint)Modifier.MethodMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("method declaration contains illegal modifiers");

            MethodNode node;

            //we check the return type of the method.
            // -> it dertimes if the method is an iterator.

            if ( type is TypeNode
                    && iteratorsClass.Contains(((TypeNode)type).GenericIndependentIdentifier))
            {
                node = new MethodNode(true, curtok);
                curIterator = node;
            }
            else
            {
                node = new MethodNode(false, curtok);
            }

            curMethod = node;

			ClassNode cl = typeStack.Peek();
            cl.Methods.Add(node);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}
			
			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the method is declared in an unsafe context ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

			node.Type = type;
            QualifiedIdentifierExpression methName = new QualifiedIdentifierExpression(name.RelatedToken);
            methName.Expressions.Add(name);
			node.Names.Add(methName);

            if (methName.IsGeneric)
            {
                //move generic from identifier to method
                node.Generic = methName.Generic;
                methName.Generic = null;
            }

            ParsePossibleTypeParameterNode(true, true, false);
            //if generic applies type parameter collection to the node
            ApplyTypeParameters(node);

			// starts at LParen
			node.Params = ParseParamList();

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

			ParseBlock(node.StatementBlock);

            //we parse all parameter, if only one is ref or out, we raise an exception
            if (node.IsIterator)
            {
                foreach (ParamDeclNode param in node.Params)
                {
                    if ((param.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                    {
                        ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                        break;
                    }
                }
            }

            curMethod = null;
            curIterator = null;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            this.nameTable.AddIdentifier(new MethodName(name.QualifiedIdentifier,
                ToVisibilityRestriction(node.Modifiers),
                ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                this.currentContext));
		}

        private void ParseFixedBuffer(StructNode st, FixedBufferNode node)
        {
            if (st == null)
            {
                ReportError("fixed buffer authorized only in structure declaration.");
            }

            if (!node.IsUnsafe)
            {
                ReportError("fixed buffer authorized only in unsafe context.");
            }

            if ((node.Modifiers & Modifier.Static) != Modifier.Empty)
            {
                ReportError("fixed buffer can not be declared as static.");
            }

            if ( node.Type is TypePointerNode )
            {
                ReportError("fixed buffer can not be pointer.");
            }
            else
            {
                StringCollection strColl = new StringCollection();
                string type_str = ((TypeNode)node.Type).Identifier.QualifiedIdentifier.ToLower();

                strColl.AddRange(new string[] { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "bool" });

                if (!strColl.Contains(type_str))
                {
                    ReportError("fixed buffer element type must be of type sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double or bool.");    
                }
            }
            AssertAndAdvance(TokenID.LBracket);

            ConstantExpression expr = new ConstantExpression(curtok);
            expr.Value = ParseExpression(TokenID.RBracket);
            node.FixedBufferConstants.Add(  expr );

            AssertAndAdvance(TokenID.RBracket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the field</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
		private void ParseFixedBuffer(IType type, QualifiedIdentifierExpression name)	
		{
			uint mask = ~(uint)Modifier.FxedBufferdMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("field declaration contains illegal modifiers");

            FixedBufferNode node = new FixedBufferNode(curtok);
			StructNode st = (StructNode)typeStack.Peek();
            st.FixedBuffers.Add(node);

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(st, node.Modifiers, true);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}
			
			node.Type = type;
            QualifiedIdentifierExpression fieldName = new QualifiedIdentifierExpression(name.RelatedToken);
            fieldName.Expressions.Add(name);
			node.Names.Add(fieldName);

            //fixed buffer
            // if the current type is not a structure, it is invalid
            ParseFixedBuffer(st, node);

			while (curtok.ID == TokenID.Comma)
			{
				Advance(); // over comma
				QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(false, false, true);
				node.Names.Add(ident);

                ParseFixedBuffer(st, node);
			}

			 if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}

		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the field</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
		private void ParseField(IType type, QualifiedIdentifierExpression name)	
		{
			uint mask = ~(uint)Modifier.FieldMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("field declaration contains illegal modifiers");

			FieldNode node = new FieldNode(curtok);
			ClassNode cl = typeStack.Peek();
            cl.Fields.Add(node);

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}
			
			node.Type = type;
            QualifiedIdentifierExpression fieldName = new QualifiedIdentifierExpression(name.RelatedToken);
            fieldName.Expressions.Add(name);
			node.Names.Add(fieldName);

            this.nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                ToVisibilityRestriction(node.Modifiers),
                ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                this.currentContext));

			//eg: int ok = 0, error, xx = 0;
			if (curtok.ID == TokenID.Equal)
			{
				Advance();
				node.Value = ParseConstExpr();
				if (curtok.ID == TokenID.Comma)
				{
					node = new FieldNode(curtok);
					typeStack.Peek().Fields.Add(node);
					node.Modifiers = curmods;
					node.Type = type;
				}
			}

			while (curtok.ID == TokenID.Comma)
			{
				Advance(); // over comma
				QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(false, false, true);
				node.Names.Add(ident);

				if (curtok.ID == TokenID.Equal)
				{
					Advance();
					node.Value = ParseConstExpr();

					if (curtok.ID == TokenID.Comma)
					{
                        node = new FieldNode(curtok);
						typeStack.Peek().Fields.Add(node);
						node.Modifiers = curmods;
						node.Type = type;
					}
				}

                this.nameTable.AddIdentifier(new FieldName(ident.QualifiedIdentifier,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Empty ? Scope.Instance : Scope.Static),
                    this.currentContext));
            }

			 if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}
			


		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the property</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
		private void ParseProperty(IType type, QualifiedIdentifierExpression name)
		{
			uint mask = ~(uint)Modifier.PropertyMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("field declaration contains illegal modifiers");

            PropertyNode node = new PropertyNode(curtok);
			ClassNode cl = typeStack.Peek();
            cl.Properties.Add(node);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the property is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

			node.Type = type;
            QualifiedIdentifierExpression propName = new QualifiedIdentifierExpression(name.RelatedToken);
            propName.Expressions.Add(name);
			node.Names.Add(propName);

			// opens on lcurly
			AssertAndAdvance(TokenID.LCurly);

			// todo: AddNode attributes to get and setters
			ParsePossibleAttributes(false);

            ParseModifiers();

			if (curAttributes.Count > 0)
			{
				//node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			if (curtok.ID != TokenID.Ident)
			{
				RecoverFromError("At least one get or set required in accessor", curtok.ID);
			}

			bool parsedGet = false;
			if (strings[curtok.Data] == "get")
			{
				node.Getter = ParseAccessor(type);
				parsedGet = true;
			}

			// todo: AddNode attributes to get and setters
			ParsePossibleAttributes(false);

            ParseModifiers();

			if (curAttributes.Count > 0)
			{
				//node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
			{
				node.Setter = ParseAccessor(type);
			}

            if (!parsedGet)
            {
                // todo: AddNode attributes to get and setters
                ParsePossibleAttributes(false);

                ParseModifiers();

                if (curAttributes.Count > 0)
                {
                    //node.Attributes = curAttributes;
                    curAttributes = new NodeCollection<AttributeNode>();
                }

                // get might follow set
                if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
                {
                    node.Getter = ParseAccessor(type);
                }
            }

            if (node.Setter != null
                && node.Getter != null)
            {
                if (node.Getter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Modifiers is permitted only for one of the acessors.");
                }
            }
            else
            {
                if (node.Setter == null && node.Getter != null && node.Getter.Modifiers != Modifier.Empty
                    || node.Getter == null && node.Setter != null && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Accessor modifier is authorized only if the 'get' and the 'set' are declared.");
                }

            }

			AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            #region Save name in the name table
            if (node.Getter != null)
            {
                if (node.Setter != null)
                {
                    if (node.Getter.Modifiers != Modifier.Empty)
                    {
                        this.nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                    else if (node.Setter.Modifiers != Modifier.Empty)
                    {
                        this.nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                    else
                    {
                        this.nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            this.currentContext));
                    }
                }
                else
                {
                    this.nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                        ToVisibilityRestriction(node.Modifiers),
                        NameVisibilityRestriction.Self,
                        ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                        PropertyAccessors.Get,
                        this.currentContext));
                }
            }
            else
            {
                this.nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                    NameVisibilityRestriction.Self,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                    PropertyAccessors.Set,
                    this.currentContext));
            }
            #endregion
		}

		private void ParseEvent()											
		{
			uint mask = ~(uint)Modifier.EventMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("Event contains illegal modifiers");

            EventNode node = new EventNode(curtok);

            ClassNode cl = typeStack.Peek();

			cl.Events.Add(node);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the event is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

			Advance(); // advance over event keyword

			node.Type = ParseType();

			if (curtok.ID != TokenID.Ident)
				ReportError("Expected event member name.");

			while (curtok.ID == TokenID.Ident)
			{
                node.Names.Add(ParseQualifiedIdentifier(true, false, true));
			}

            //TODO: Omer - Add all of the events in that line to the name table once Robin fixes the code.

            switch (curtok.ID)
            {
                case TokenID.Semi:
                    AssertAndAdvance(TokenID.Semi);
                    break;
                case TokenID.Equal:
                    Advance();
                    node.Value = ParseConstExpr();
                    AssertAndAdvance(TokenID.Semi);
                    break;
                case TokenID.LCurly:
                    Advance(); // over lcurly

                    ParsePossibleAttributes(false);

                    if (curtok.ID != TokenID.Ident)
                    {
                        ReportError("Event accessor requires add or remove clause.");
                    }

                    string curAccessor = strings[curtok.Data];
                    Advance(); // over ident
                    if (curAccessor == "add")
                    {
                        node.AddBlock = new AccessorNode(false, curtok);
                        node.AddBlock.Kind = "add";
                        node.AddBlock.IsUnsafe = isUnsafe > 0;
                        ApplyAttributes( node.AddBlock );
                        ParseBlock( node.AddBlock.StatementBlock );

                        ParsePossibleAttributes(false);
                        if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "remove")
                        {
                            node.RemoveBlock = new AccessorNode( false, curtok );
                            node.RemoveBlock.IsUnsafe = isUnsafe > 0;
                            node.RemoveBlock.Kind = "remove";
                            ApplyAttributes( node.RemoveBlock );
                            Advance(); // over ident
                            ParseBlock( node.RemoveBlock.StatementBlock );
                        }
                        else
                        {
                            ReportError("Event accessor expected remove clause.");
                        }
                    }
                    else if (curAccessor == "remove")
                    {
                        node.RemoveBlock = new AccessorNode(false, curtok);
                        node.RemoveBlock.IsUnsafe = isUnsafe > 0;
                        node.RemoveBlock.Kind = "remove";
                        ApplyAttributes( node.RemoveBlock );
                        ParseBlock( node.RemoveBlock.StatementBlock );

                        ParsePossibleAttributes(false);
                        if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "add")
                        {
                            node.AddBlock = new AccessorNode(false, curtok);
                            node.AddBlock.IsUnsafe = isUnsafe > 0;
                            node.AddBlock.Kind = "remove";
                            Advance(); // over ident
                            ApplyAttributes(node.AddBlock);
                            ParseBlock(node.AddBlock.StatementBlock);
                        }
                        else
                        {
                            ReportError("Event accessor expected add clause.");
                        }
                    }
                    else
                    {
                        ReportError("Event accessor requires add or remove clause.");
                    }

                    AssertAndAdvance(TokenID.RCurly);

                    break;
            }

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }


		}
		private void ParseConst()											
		{
			uint mask = ~(uint)Modifier.ConstantMods;
			if (((uint)curmods & mask) != (uint)Modifier.Empty)
				ReportError("const declaration contains illegal modifiers");

            ConstantNode node = new ConstantNode(curtok);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();
            cl.Constants.Add(node); // TODO: If cl is null, then this will throw an exception... Better throw a specialized one.

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Modifiers = curmods;
			curmods = Modifier.Empty;

            if (cl != null)
            {
                CheckStaticClass(cl, node.Modifiers, false);
            }

			Advance(); // advance over const keyword

			node.Type = ParseType();

			bool hasEqual = false;
            QualifiedIdentifierExpression name = ParseQualifiedIdentifier(false, false, true);
            node.Names.Add(name);
			
            if (curtok.ID == TokenID.Equal)
			{
				Advance();
				hasEqual = true;
			}

            this.nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                ToVisibilityRestriction(node.Modifiers),
                Scope.Static, 
                this.currentContext));

			while (curtok.ID == TokenID.Comma)
			{
				Advance();

                name = ParseQualifiedIdentifier(false, false, true);
                node.Names.Add(name);
				
                if (curtok.ID == TokenID.Equal)
				{
					Advance();
					hasEqual = true;
				}
				else
				{
					hasEqual = false;
				}

                // TODO: Robin - make sure when you fix the constants that this is still relevant.
                this.nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                    ToVisibilityRestriction(node.Modifiers),
                    Scope.Static, 
                    this.currentContext));
			}

			if (hasEqual)
			{
				node.Value = ParseConstExpr();
			}

			AssertAndAdvance(TokenID.Semi);
		}

		private EnumNode ParseEnumMember()									
		{
            EnumNode result = new EnumNode(curtok);

			ParsePossibleAttributes(false);

			if (curAttributes.Count > 0)
			{
				result.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			if (curtok.ID != TokenID.Ident)
			{
				ReportError("Enum members must be legal identifiers.");
			}

            result.Name = new IdentifierExpression(strings[curtok.Data], curtok);
			Advance();

			if (curtok.ID == TokenID.Equal)
			{
				Advance();
				result.Value = ParseExpression();
			}

			if (curtok.ID == TokenID.Comma)
			{
				Advance();
			}

            this.nameTable.AddIdentifier(new FieldName(result.Name.Identifier,
                NameVisibilityRestriction.Everyone,
                Scope.Static,
                this.currentContext));

			return result;
		}

		// member helpers
		private NodeCollection<ParamDeclNode> ParseParamList()				
		{
			// default is parens, however things like indexers use square brackets
			return ParseParamList(TokenID.LParen, TokenID.RParen);
		}

		private NodeCollection<ParamDeclNode> ParseParamList(TokenID openToken, TokenID closeToken)
		{
			AssertAndAdvance(openToken);
			if (curtok.ID == closeToken)
			{
				Advance();
				return null;
			}
			NodeCollection<ParamDeclNode> result = new NodeCollection<ParamDeclNode>();
			bool isParams;
			bool hasComma;
			do
			{
				ParamDeclNode node = new ParamDeclNode(curtok);
				result.Add(node);
				isParams = false;

				ParsePossibleAttributes(false);

                if (isAnonynous > 0
                    && curAttributes.Count > 0)
                {
                    ReportError("Attributes are not allowed for anonymous delegate's parameters.");
                }

				if (curtok.ID == TokenID.Ref)
				{
					node.Modifiers |= Modifier.Ref;
					Advance();
				}
				else if (curtok.ID == TokenID.Out)
				{
					node.Modifiers |= Modifier.Out;
					Advance();
				}
				else if (curtok.ID == TokenID.Params)
				{
                    if (isAnonynous > 0)
                    {
                        ReportError("Params parameter are not allowed for anonymous delegate.");
                    }

					isParams = true;
					node.Modifiers |= Modifier.Params;
					Advance();
				}

				node.Type = ParseType();

				if (isParams)
				{
					// ensure is array type
				}

				if (curtok.ID == TokenID.Ident)
				{
                    node.Name = ((IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false)).Identifier;//strings[curtok.Data];
				}

				hasComma = false;
				if (curtok.ID == TokenID.Comma)
				{
					Advance();
					hasComma = true;
				}
			}
			while (!isParams && hasComma);

			AssertAndAdvance(closeToken);

			return result;
		}
		private ParamDeclNode ParseParamDecl()								
		{

			ParamDeclNode node = new ParamDeclNode(curtok);

			ParsePossibleAttributes(false);

			if (curAttributes.Count > 0)
			{
				node.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			node.Type = ParseType();

			if (curtok.ID == TokenID.Ident)
			{
				node.Name = strings[curtok.Data];
				Advance();
			}
			else
			{
				RecoverFromError("Expected arg name.", TokenID.Ident);
			}
			return node;
		}
		private NodeCollection<ArgumentNode> ParseArgs()					
		{
			AssertAndAdvance(TokenID.LParen);
			if (curtok.ID == TokenID.RParen)
			{
				Advance();
				return null;
			}
			bool hasComma;
			NodeCollection<ArgumentNode> result = new NodeCollection<ArgumentNode>();
			do
			{
				ArgumentNode node = new ArgumentNode(curtok);
				result.Add(node);

				if (curtok.ID == TokenID.Ref)		
				{
					node.IsRef = true;
					Advance();
				}
				else if (curtok.ID == TokenID.Out)
				{
					node.IsOut = true;
					Advance();
				}
				node.Expression = ParseExpression();

				hasComma = false;
				if (curtok.ID == TokenID.Comma)
				{
					Advance();
					hasComma = true;
				}
			}
			while (hasComma);

			AssertAndAdvance(TokenID.RParen);

			return result;
		}

		private AccessorNode ParseAccessor(IType type)								
		{
			AccessorNode result;


            if ( type is TypeNode
                    && iteratorsClass.Contains(((TypeNode)type).GenericIndependentIdentifier))
            {
                result = new AccessorNode(true, curtok);
                curIterator = result;
            }
            else
            {
                result = new AccessorNode(false, curtok);
            }

            result.Modifiers = curmods;
            curmods = Modifier.Empty;

			ParsePossibleAttributes(false);

			if (curAttributes.Count > 0)
			{
				result.Attributes = curAttributes;
				curAttributes = new NodeCollection<AttributeNode>();
			}

			string kind = "";
			if (curtok.ID == TokenID.Ident)
			{
				kind = strings[curtok.Data];
			}
			else
			{
				RecoverFromError("Must specify accessor kind in accessor.", curtok.ID);
			}

            if (result.Modifiers != Modifier.Empty )
            {
                if (curInterface != null)
                {
                    ReportError("Property accessor modifiers are not authorized in interface declaration");
                }
                
                Modifier id = (result.Modifiers & ( Modifier.Private | Modifier.Protected | Modifier.Internal) );

                switch (id)
                {
                    case Modifier.Private:
                    case Modifier.Protected:
                    case Modifier.Internal:
                    case (Modifier.Internal | Modifier.Protected):
                        break;
                    default:
                        ReportError("Invalid modifiers set for the accessor " + kind + "."); 
                        break;
                }
            }


			result.Kind = kind;
			Advance();
			if (curtok.ID == TokenID.Semi)
			{
				result.IsAbstractOrInterface = true;
				Advance(); // over semi
			}
			else
			{
				ParseBlock(result.StatementBlock);
			}

            curIterator = null;
			return result;
		}
		private ConstantExpression ParseConstExpr()							
		{
			ConstantExpression node = new ConstantExpression(curtok);
			node.Value = ParseExpression();

			return node;
		}
		private void ParseModifiers()										
		{
			while(!curtok.Equals(EOF))
			{
				switch (curtok.ID)
				{
					case TokenID.New:
					case TokenID.Public:
					case TokenID.Protected:
					case TokenID.Internal:
					case TokenID.Private:
					case TokenID.Abstract:
					case TokenID.Sealed:
					case TokenID.Static:
					case TokenID.Virtual:
					case TokenID.Override:
					case TokenID.Extern:
					case TokenID.Readonly:
					case TokenID.Volatile:
                    case TokenID.Unsafe:
                    case TokenID.Fixed:
                    case TokenID.Partial:
					case TokenID.Ref:
					case TokenID.Out:

					//case TokenID.Assembly:
					//case TokenID.Field:
					//case TokenID.Event:
					//case TokenID.Method:
					//case TokenID.Param:
					//case TokenID.Property:
					//case TokenID.Return:
					//case TokenID.Type:

						uint mod = (uint)modMap[curtok.ID];
						if (((uint)curmods & mod) > 0)
						{
							ReportError("Duplicate modifier.");
						}
						curmods |= (Modifier)mod;
						Advance();
						break;


					default:
						return;
				}
			}
		}
		private Modifier ParseAttributeModifiers()							
		{
			Modifier result = Modifier.Empty;
		    bool isMod = true;

			while (isMod)
			{
				switch (curtok.ID)
				{
					case TokenID.Ident:
				        string curIdent;
				        curIdent = strings[curtok.Data];
						switch (curIdent)
						{
							case "field":
								result |= Modifier.Field;
								Advance();
								break;
							case "method":
								result |= Modifier.Method;
								Advance();
								break;
							case "param":
								result |= Modifier.Param;
								Advance();
								break;
							case "property":
								result |= Modifier.Property;
								Advance();
								break;
							case "type":
								result |= Modifier.Type;
								Advance();
								break;
							case "module":
								result |= Modifier.Module;
								Advance();
								break;
							case "assembly":
								result |= Modifier.Assembly;
								Advance();
								break;
							default:
								isMod = false;
								break;
						}
						break;

					case TokenID.Return:
						result |= Modifier.Return;
						Advance();
						break;

					case TokenID.Event:
						result |= Modifier.Event;
						Advance();
						break;

					default:
						isMod = false;
						break;

				}
			}
			return result;
		}

		private IType ParseType()										
		{
            //backup modifiers because ParseQualifiedIdentifier will consume ParseTypeParameter
            // and ParseTypeParameter consume modifiers ... 
            Modifier backupModifiers = curmods;
            curmods = Modifier.Empty;
            IType result = new TypeNode(curtok);

            QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(true, false, false);

            // if the ParseQualifiedIdentifier is can resolve the identifier to a type, 
            // the parser does not need to wrappe the type in another typenode ... 
            if ( ident.Expressions.Last is PredefinedTypeNode )
            {
                result = (IType)ident.Expressions.Last;
            }
            else
            {
                ((TypeNode)result).Identifier = ident;

                // move the rank specifier
                if (((TypeNode)result).Identifier.IsType)
                {
                    ((IType)result).RankSpecifiers = ((IType)((TypeNode)result).Identifier.Expressions.Last).RankSpecifiers;
                    ((IType)((TypeNode)result).Identifier.Expressions.Last).RankSpecifiers = new List<int>();
                }
            }

            if (curtok.ID == TokenID.Star)
            {
                result = new TypePointerNode( (ExpressionNode)result ); 
                Advance();
            }

            if (curtok.ID == TokenID.Question)
            {
                ((INullableType)result).IsNullableType = true;
                Advance();
            }

            curmods = backupModifiers;

            return result;
		}

        /// <summary>
        /// it parse expressions like
        /// 
        /// A.B.type1<int>.type2
        /// 
        /// </summary>
        /// <returns></returns>
        private QualifiedIdentifierExpression ParseQualifiedIdentifier(bool consumeTypeParameter, bool allowTypeParameterAttributes, bool inDeclaration)
        {
            QualifiedIdentifierExpression result = new QualifiedIdentifierExpression(curtok);

            result.Expressions.Add(ParseIdentifierOrKeyword(true, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration));
            while (curtok.ID == TokenID.Dot || curtok.ID == TokenID.ColonColon )
            {
                if (curtok.ID == TokenID.ColonColon)
                {
                    // 'global' is not a kw so it is treated as an identifier
                    if (result.IsNamespaceAliasQualifier)
                    {
                        ReportError("Qualified name can not have more than one qualificator alias name.");
                    }
                    result.IsNamespaceAliasQualifier = true;
                }

                Advance();
                if (curtok.ID == TokenID.This)
                {
                    // this is an indexer with a prepended interface, do nothing (but consume dot)
                }
                else
                {
                    result.Expressions.Add(ParseIdentifierOrKeyword(true, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration));                    
                }
            }

            return result;
        }

        private void CheckRankSpecifier(ExpressionNode node)
        {
            // now any 'rank only' specifiers (without size decls)
            while (curtok.ID == TokenID.LBracket)
            {
                if ( curTokNode != null &&
                    curTokNode.Value.ID != TokenID.RBracket &&
                    curTokNode.Value.ID != TokenID.Comma)
                {
                    // anything with size or accessor decls has own node type
                    break;
                }

                if (!(node is IType))
                {
                    node = new TypeNode( node  );
                }

                Advance(); // over lbracket
                int commaCount = 0;
                while (curtok.ID == TokenID.Comma)
                {
                    commaCount++;
                    Advance();
                }
                ((IType)node).RankSpecifiers.Add(commaCount);
                AssertAndAdvance(TokenID.RBracket);
            }
        }

        private ExpressionNode CheckIdentifierIsType(ExpressionNode expr, bool consumeTypeParameter, bool allowTypeParameterAttributes, bool inDeclaration)
        {
            ExpressionNode result = expr;

            if (consumeTypeParameter)
            {
                //check if it is a generic type
                ParsePossibleTypeParameterNode(allowTypeParameterAttributes, inDeclaration, true);
                if (curTypeParameters != null
                        && curTypeParameters.Count != 0
                        //&& curtok.ID != TokenID.LParen
                    )
                {
                    TypeNode type = result as TypeNode;

                    if (type == null)
                    {
                       type = new TypeNode(result.RelatedToken);
                       type.Identifier.Expressions.Add(result);
                       result = type;
                    }
                    
                    ApplyTypeParameters(type);
                }
            }

            switch (curtok.ID)
            {
                case TokenID.LBracket:
                    if (!isNewStatement)
                    {
                        CheckRankSpecifier(result);
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// this method parse the next token.
        /// </summary>
        /// <param name="checkIsType">
        /// if set to <c>true</c>, it will check that the identifier is a type. If it is a type, it converts it
        /// to a TypeNode.
        /// if set to <c>false</c>, it will not check that the identifier is a type.
        /// 
        /// Set it to <c>false</c> in the case of a type declaration while your are parsing the type's name
        /// 
        /// </param>
        /// <returns>
        /// Generally it returns identifier expression
        /// But in some cases ,it is possible to resolve the kind the expression, 
        /// so it might returns a TypeNode object.
        /// 
        /// For exemple in this case : System.Collections.Generic.List<int>
        /// 
        /// The first identifiers are really identifier.
        /// And the last identifier is a type, and now that because of the generic declaration
        /// 
        /// in the next case : 
        /// 
        /// System.Collections.ArrayList
        /// 
        /// All are returned as Identifier, because in the first stage of the parser, 
        /// we are not able to pronostic what is it the kind of the expression. The ArrayList type will
        /// be resolved in another parser stage
        /// </returns>
        private ExpressionNode ParseIdentifierOrKeyword(bool checkIsType, bool consumeTypeParameter, bool allowTypeParameterAttributes, bool inDeclaration)
        {
            ExpressionNode result = null;
            switch (curtok.ID)
            {
                case TokenID.Ident:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        result = new IdentifierExpression(curtok.ID.ToString().ToLower(), curtok);
                    }
                    else
                    {
                        result = new IdentifierExpression(strings[curtok.Data], curtok);
                    }
                    Advance();

                    while (curtok.ID == TokenID.BSlash)
                    {
                        Advance();
                        string c = char.ConvertFromUtf32( int.Parse(strings[curtok.Data].TrimStart('\\').TrimStart('u'), NumberStyles.HexNumber ) );
                        ((IdentifierExpression)result).Identifier += c ;
                        Advance();
                    }

                    break;


                case TokenID.Bool:
                case TokenID.Byte:
                case TokenID.Char:
                case TokenID.Decimal:
                case TokenID.Double:
                case TokenID.Float:
                case TokenID.Int:
                case TokenID.Long:
                case TokenID.Object:
                case TokenID.SByte:
                case TokenID.Short:
                case TokenID.String:
                case TokenID.UInt:
                case TokenID.ULong:
                case TokenID.UShort:
                case TokenID.Void:
                    result = new PredefinedTypeNode(curtok.ID, curtok);
                    Advance();
                    break;
                case TokenID.If:
                case TokenID.Else:
                case TokenID.This:
                case TokenID.Base:
                    string predef = Enum.GetName(TokenID.Invalid.GetType(), curtok.ID).ToLower();
                    result = new IdentifierExpression(predef, curtok);
                    ((IdentifierExpression)result).StartingPredefinedType = curtok.ID;
                    Advance();
                    break;

                default:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        goto case TokenID.Ident;
                    }
                    else
                    {
                        RecoverFromError(TokenID.Ident);
                    }
                    break;
            }

            if (checkIsType)
            {
                result = CheckIdentifierIsType(result, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration);
            }

            //check if it is an array access
            // TODO

            return result;
        }

		private void ParseInterfaceAccessors(ref bool hasGetter, ref bool hasSetter)
		{
			AssertAndAdvance(TokenID.LCurly); // LCurly

			// the get and set can also have attributes
			ParsePossibleAttributes(false);

			if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
			{
				if (curAttributes.Count > 0)
				{
					// todo: store get/set attributes on InterfacePropertyNode
					// pnode.getAttributes = curAttributes;
					curAttributes = new NodeCollection<AttributeNode>();
				}

				hasGetter = true;
				Advance();
				AssertAndAdvance(TokenID.Semi);
				if (curtok.ID == TokenID.Ident)
				{
					if (strings[curtok.Data] == "set")
					{
						hasSetter = true;
						Advance();
						AssertAndAdvance(TokenID.Semi);
					}
					else
					{
						RecoverFromError("Expected set in interface property def.", curtok.ID);
					}
				}
			}
			else if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
			{
				if (curAttributes.Count > 0)
				{
					// todo: store get/set attributes on InterfacePropertyNode
					// pnode.setAttributes = curAttributes;
					curAttributes = new NodeCollection<AttributeNode>();
				}
				hasSetter = true;
				Advance();
				AssertAndAdvance(TokenID.Semi);
				if (curtok.ID == TokenID.Ident)
				{
					if (strings[curtok.Data] == "get")
					{
						hasGetter = true;
						Advance();
						AssertAndAdvance(TokenID.Semi);
					}
					else
					{
						RecoverFromError("Expected get in interface property def.", curtok.ID);
					}
				}
			}
			else
			{
				RecoverFromError("Expected get or set in interface property def.", curtok.ID);
			}

			AssertAndAdvance(TokenID.RCurly);
		}

        private void PopLocalDeclaration(ExpressionNode node)
        {
            ExpressionNode expr = node;
            // local declaration case
            BlockStatement curBlock = blockStack.Peek();

            do
            {
                curBlock.Statements.Add(new ExpressionStatement(expr ));

                expr = (exprStack.Count > 0) ? exprStack.Pop() : null;
            }
            while (expr is LocalDeclarationStatement);
        }
	
		// statements		
		private void ParseStatement(NodeCollection<StatementNode> node)		
		{
			// label		ident	: colon
			// localDecl	type	: ident
			// block		LCurly
			// empty		Semi
			// expression	
			//	-invoke		pexpr	: LParen
			//	-objCre		new		: type
			//	-assign		uexpr	: assignOp
			//	-postInc	pexpr	: ++
			//	-postDec	pexpr	: --
			//	-preInc		++		: uexpr
			//	-preDec		--		: uexpr
			//
			// selection	if		: LParen
			//				switch	: LParen
			//
			// iteration	while	: LParen
			//				do		: LParen
			//				for		: LParen
			//				foreach	: LParen
			//
			// jump			break	: Semi
			//				continue: Semi
			//				goto	: ident | case | default
			//				return	: expr
			//				throw	: expr
			//
			// try			try		: block
			// checked		checked	: block
			// unchecked	unchecked : block
			// lock			lock	: LParen
			// using		using	: LParen
			switch (curtok.ID)
			{
				case TokenID.LCurly:	// block
                    BlockStatement newBlock = new BlockStatement(isUnsafe > 0, curtok);
                    newBlock.IsUnsafe = isUnsafe > 0;
					node.Add(newBlock);
					ParseBlock(newBlock);
					break;
				case TokenID.Semi:		// empty statement
					node.Add(new StatementNode(curtok));
                    Advance();
					break;
				case TokenID.If:		// If statement
					node.Add(ParseIf());
					break;
				case TokenID.Switch:	// Switch statement
					node.Add(ParseSwitch());
					break;
				case TokenID.While:		// While statement
					node.Add(ParseWhile());
					break;
				case TokenID.Do:		// Do statement
					node.Add(ParseDo());
					break;
				case TokenID.For:		// For statement
					node.Add(ParseFor());
					break;
				case TokenID.Foreach:	// Foreach statement
					node.Add(ParseForEach());
					break;
				case TokenID.Break:		// Break statement
					node.Add(ParseBreak());
					break;
				case TokenID.Continue:	// Continue statement
					node.Add(ParseContinue());
					break;
				case TokenID.Goto:		// Goto statement
					node.Add(ParseGoto());
					break;
				case TokenID.Return:	// Return statement
					node.Add(ParseReturn());
					break;
				case TokenID.Throw:		// Throw statement
					node.Add(ParseThrow());
					break;
				case TokenID.Try:		// Try statement
					node.Add(ParseTry());
					break;
				case TokenID.Checked:	// Checked statement
					node.Add(ParseChecked());
					break;
				case TokenID.Unchecked:	// Unchecked statement
					node.Add(ParseUnchecked());
					break;
				case TokenID.Lock:		// Lock statement
					node.Add(ParseLock());
					break;
				case TokenID.Using:		// Using statement
					node.Add(ParseUsing());
					break;

				case TokenID.Const:
					isLocalConst = true;
					Advance();
					break;

                case TokenID.Yield:
                    node.Add(ParseYieldStatement());
                    break;

                case TokenID.Void:
				case TokenID.Bool:
				case TokenID.Byte:
				case TokenID.Char:
				case TokenID.Decimal:
				case TokenID.Double:
				case TokenID.Float:
				case TokenID.Int:
				case TokenID.Long:
				case TokenID.Object:
				case TokenID.SByte:
				case TokenID.Short:
				case TokenID.String:
				case TokenID.UInt:
				case TokenID.ULong:
				case TokenID.UShort:

				case TokenID.StringLiteral:
				case TokenID.HexLiteral:
				case TokenID.IntLiteral:
				case TokenID.UIntLiteral:
				case TokenID.LongLiteral:
				case TokenID.ULongLiteral:
				case TokenID.TrueLiteral:
				case TokenID.FalseLiteral:
				case TokenID.NullLiteral:
				case TokenID.LParen:
				case TokenID.DecimalLiteral:
				case TokenID.RealLiteral:
				case TokenID.CharLiteral:
				case TokenID.PlusPlus:	// PreInc statement
				case TokenID.MinusMinus:// PreDec statement
				case TokenID.This:
				case TokenID.Base:
				case TokenID.New:		// creation statement

                    ExpressionNode rexpr = ParseExpression();

                    if (rexpr is LocalDeclarationStatement )
                    {
                        PopLocalDeclaration(rexpr);
                    }
                    else
                    {
                        ExpressionStatement enode = new ExpressionStatement(rexpr );
                        node.Add(enode);
                    }

					if (curtok.ID == TokenID.Semi)
					{
						Advance();
					}
					break;

				case TokenID.Ident:
                    if (curTokNode != null && curTokNode.Next != null && curTokNode.Value.ID == TokenID.Colon)
					{
						LabeledStatement lsnode = new LabeledStatement(curtok);
                        lsnode.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, true, false, false);
						AssertAndAdvance(TokenID.Colon);
						ParseStatement(lsnode.Statements);
						node.Add(lsnode);
					}
					else
					{
                        rexpr = ParseExpression();

                        if (rexpr is LocalDeclarationStatement)
                        {
                            PopLocalDeclaration(rexpr);
                        }
                        else
                        {
                            ExpressionStatement enode = new ExpressionStatement(rexpr );
                            node.Add(enode);
                        }
                    }
					if (curtok.ID == TokenID.Semi)
					{
						Advance();
					}
					break;

				case TokenID.Unsafe:
					// preprocessor directives
                    node.Add(ParseUnsafeCode());
					break;

                case TokenID.Fixed:
                    // preprocessor directives
                    node.Add(ParseFixedStatement());
                    break;


                case TokenID.Star:
                    // dereference variable 
                    // introduced because of the mono file test-406.cs
                    //private static uint DoOp2 (uint *u) {
                    //    *(u) += 100;
                    //    return *u;
                    //}
                    node.Add(new ExpressionStatement(ParseExpression()));
                    break;

				default:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        goto case TokenID.Ident;
                    }
                    else
                    {
                        Console.WriteLine("Unhandled case in statement parsing: \"" + curtok.ID + "\" in line: " + lineCount);
                        // this is almost always an expression
                        ExpressionStatement dnode = new ExpressionStatement(ParseExpression());
                        node.Add(dnode);
                        if (curtok.ID == TokenID.Semi)
                        {
                            Advance();
                        }
                    }
					break;
			}
		}
		private void ParseBlock(BlockStatement node)						
		{
			ParseBlock(node, false);
		}
		private void ParseBlock(BlockStatement node, bool isCase)			
		{
            blockStack.Push(node);

            node.IsUnsafe = isUnsafe > 0;

			if (curtok.ID == TokenID.LCurly)
			{
				Advance(); // over lcurly
				while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.RCurly)
				{
					ParseStatement(node.Statements);
				}
				AssertAndAdvance(TokenID.RCurly);
			}
			else if(isCase)
			{
				// case stmts can have multiple lines without curlies, ugh
				// break can be omitted if it is unreachable code, double ugh
				// this becomes impossible to trace without code analysis of course, so look for 'case' or '}'

				while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.Case && curtok.ID != TokenID.Default && curtok.ID != TokenID.RCurly)
				{
					ParseStatement(node.Statements);
				}
				//bool endsOnReturn = false;
				//while (curtok.ID != TokenID.Eof && !endsOnReturn)
				//{
				//    TokenID startTok = curtok.ID;
				//    if (startTok == TokenID.Return	|| 
				//        startTok == TokenID.Goto	|| 
				//        startTok == TokenID.Throw	|| 
				//        startTok == TokenID.Break)
				//    {
				//        endsOnReturn = true;
				//    }

				//    ParseStatement(node.Statements);

				//    // doesn't have to end on return or goto
				//    if (endsOnReturn && (startTok == TokenID.Return	|| startTok == TokenID.Goto	|| startTok == TokenID.Throw))
				//    {
				//        if (curtok.ID == TokenID.Break)
				//        {
				//            ParseStatement(node.Statements);
				//        }
				//    }
				//}
			}
			else
			{
				ParseStatement(node.Statements);
			}

            blockStack.Pop();

		}
		private IfStatement ParseIf()										
		{
			IfStatement node = new IfStatement(curtok);
			Advance(); // advance over IF

			AssertAndAdvance(TokenID.LParen);
			node.Test = ParseExpression();
			AssertAndAdvance(TokenID.RParen);

			ParseBlock(node.Statements);

			if (curtok.ID == TokenID.Else)
			{
				Advance(); // advance of else
				ParseBlock(node.ElseStatements);
			}
			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private SwitchStatement ParseSwitch()								
		{
            SwitchStatement node = new SwitchStatement(curtok);
			Advance(); // advance over SWITCH

			AssertAndAdvance(TokenID.LParen);
			node.Test = ParseExpression();
			AssertAndAdvance(TokenID.RParen);

			AssertAndAdvance(TokenID.LCurly);
			while (curtok.ID == TokenID.Case || curtok.ID == TokenID.Default)
			{
				node.Cases.Add(ParseCase());
			}

			AssertAndAdvance(TokenID.RCurly);

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private CaseNode ParseCase()										
		{
            CaseNode node = new CaseNode(curtok);
			bool isDefault = (curtok.ID == TokenID.Default);
			Advance(); // advance over CASE or DEFAULT

			if (!isDefault)
			{
				node.Ranges.Add(ParseExpression());
			}
			else
			{
				node.IsDefaultCase = true;
			}
			AssertAndAdvance(TokenID.Colon);

			// may be multiple cases, but must be at least one
			while (curtok.ID == TokenID.Case || curtok.ID == TokenID.Default)
			{
				isDefault = (curtok.ID == TokenID.Default);
				Advance(); // advance over CASE or DEFAULT
				if (!isDefault)
				{
					node.Ranges.Add(ParseExpression());
				}
				else
				{
					node.IsDefaultCase = true;
				}
				AssertAndAdvance(TokenID.Colon);
			}
			if (curtok.ID != TokenID.LCurly)
			{
				node.StatementBlock.HasBraces = false;
			}
			ParseBlock(node.StatementBlock, true);
			return node;
		}
		private WhileStatement ParseWhile()									
		{
            WhileStatement node = new WhileStatement(curtok);
			Advance(); // advance over While

			AssertAndAdvance(TokenID.LParen);
			node.Test = ParseExpression();
			AssertAndAdvance(TokenID.RParen);

			ParseBlock(node.Statements);
			
			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private DoStatement ParseDo()										
		{
            DoStatement node = new DoStatement(curtok);
			Advance(); // advance over DO

			ParseBlock(node.Statements);

			AssertAndAdvance(TokenID.While); // advance over While

			AssertAndAdvance(TokenID.LParen);
			node.Test = ParseExpression();
			AssertAndAdvance(TokenID.RParen);

			AssertAndAdvance(TokenID.Semi); // not optional on DO

			return node;
		}
		private ForStatement ParseFor()										
		{
            ForStatement node = new ForStatement(curtok);
			Advance(); // advance over FOR

			AssertAndAdvance(TokenID.LParen);

			if (curtok.ID != TokenID.Semi)
			{
                ExpressionNode expr = ParseExpression();
				node.Init.Add(expr);
				while (curtok.ID == TokenID.Comma)
				{
					AssertAndAdvance(TokenID.Comma);
                    expr = ParseExpression();
                    node.Init.Add(expr);
				}
			} 
			AssertAndAdvance(TokenID.Semi);

			if (curtok.ID != TokenID.Semi)
			{
				node.Test.Add(ParseExpression());
				while (curtok.ID == TokenID.Comma)
				{
					AssertAndAdvance(TokenID.Comma);
					node.Test.Add(ParseExpression());
				}
			}
			AssertAndAdvance(TokenID.Semi);

			if (curtok.ID != TokenID.RParen)
			{
				node.Inc.Add(ParseExpression());
				while (curtok.ID == TokenID.Comma)
				{
					AssertAndAdvance(TokenID.Comma);
					node.Inc.Add(ParseExpression());
				}
			}
			AssertAndAdvance(TokenID.RParen);
			ParseBlock(node.Statements);

			if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}
			return node;
		}
		private ForEachStatement ParseForEach()								
		{
            ForEachStatement node = new ForEachStatement(curtok);
			Advance(); // advance over FOREACH

			AssertAndAdvance(TokenID.LParen);
			node.Iterator = ParseParamDecl();
			AssertAndAdvance(TokenID.In);
			node.Collection = ParseExpression();
			AssertAndAdvance(TokenID.RParen);

			//node.Statements = ParseBlock().Statements;

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private BreakStatement ParseBreak()									
		{
            BreakStatement node = new BreakStatement(curtok);
			Advance(); // advance over BREAK

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private ContinueStatement ParseContinue()							
		{
            ContinueStatement node = new ContinueStatement(curtok);
			Advance(); // advance over Continue

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private GotoStatement ParseGoto()									
		{
			Advance();
            GotoStatement gn = new GotoStatement(curtok);
			if (curtok.ID == TokenID.Case)
			{
				Advance();
				gn.IsCase = true;
			}
			else if (curtok.ID == TokenID.Default)
			{
				Advance();
				gn.IsDefaultCase = true;
			}
			if (!gn.IsDefaultCase)
			{
				gn.Target = ParseExpression();
			}
			AssertAndAdvance(TokenID.Semi);
			return gn;
		}
		private ReturnStatement ParseReturn()								
		{
            ReturnStatement node = new ReturnStatement(curtok);
			Advance(); // advance over Return

            if (curIterator!= null
                    && curIterator.IsIterator)
            {
                ReportError("return unauthorized in iterator.");
            }

			if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}
			else
			{
				node.ReturnValue = ParseExpression();
				AssertAndAdvance(TokenID.Semi);
			}
			return node;
		}
		private ThrowNode ParseThrow()										
		{
            ThrowNode node = new ThrowNode(curtok);
			Advance(); // advance over Throw

			if (curtok.ID != TokenID.Semi)
			{
				node.ThrowExpression = ParseExpression();
			}

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private TryStatement ParseTry()										
		{
            TryStatement node = new TryStatement(curtok);
			Advance(); // advance over Try

            isTry++;

			ParseBlock(node.TryBlock);

            isTry--;

			while (curtok.ID == TokenID.Catch)
			{
                isCatch++;

                CatchNode cn = new CatchNode(curtok);
				node.CatchBlocks.Add(cn);

				Advance(); // over catch
				if (curtok.ID == TokenID.LParen)
				{
					Advance(); // over lparen
					cn.ClassType = ParseType();

					if (curtok.ID == TokenID.Ident)
					{
                        cn.Identifier = new IdentifierExpression(strings[curtok.Data], curtok);
						Advance();
					}
					AssertAndAdvance(TokenID.RParen);
					ParseBlock(cn.CatchBlock);
				}
				else
				{
					ParseBlock(cn.CatchBlock);
                    isCatch--;
					break; // must be last catch block if not a specific catch clause
				}

                isCatch--;
			}
			if (curtok.ID == TokenID.Finally)
			{
				Advance(); // over finally
                FinallyNode fn = new FinallyNode(curtok);
				node.FinallyBlock = fn;

                isFinally++;

				ParseBlock(fn.FinallyBlock);

                isFinally--;
			}

			if (curtok.ID == TokenID.Semi)
			{
				Advance();
			}

            hasYieldReturnInTry = false;

			return node;
		}
		private CheckedStatement ParseChecked()								
		{
            CheckedStatement node = new CheckedStatement(curtok);
            AssertAndAdvance(TokenID.Checked); // advance over Checked

            if (curtok.ID == TokenID.LParen)
            {
                Advance();
                node.CheckedExpression = ParseExpression();
                AssertAndAdvance(TokenID.RParen);
            }
            else
            {
                node.CheckedBlock = new BlockStatement(curtok);
                ParseBlock(node.CheckedBlock);
            }

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private UncheckedStatement ParseUnchecked()							
		{
            UncheckedStatement node = new UncheckedStatement(curtok);
            AssertAndAdvance(TokenID.Unchecked);  // advance over Uncecked

            if (curtok.ID == TokenID.LParen)
            {
                Advance();
                node.UncheckedExpression = ParseExpression();
                AssertAndAdvance(TokenID.RParen);
            }
            else
            {
                node.UncheckedBlock = new BlockStatement(curtok);
                ParseBlock(node.UncheckedBlock);
            }

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private LockStatement ParseLock()									
		{
            LockStatement node = new LockStatement(curtok);
			Advance(); // advance over Lock

			AssertAndAdvance(TokenID.LParen);
			node.Target = ParseExpression();
			AssertAndAdvance(TokenID.RParen);
			ParseBlock(node.Statements);

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}
		private UsingStatement ParseUsing()									
		{
            UsingStatement node = new UsingStatement(curtok);
			Advance(); // advance over Using

			AssertAndAdvance(TokenID.LParen);
			node.Resource = ParseExpression();
			AssertAndAdvance(TokenID.RParen);
			ParseBlock(node.Statements);

			if (curtok.ID == TokenID.Semi)
				Advance();
			return node;
		}

        private YieldStatement ParseYieldStatement()
        {
            YieldStatement node = null;

            if (isAnonynous > 0)
            {
                ReportError("yield not be authorized in anonymous method.");
            }

            if (curIterator != null)
            {
                //double check ...
                if (!curIterator.CouldBeIterator)
                {
                    ReportError("yield is permitted only in iterator's body.");
                }
                else
                {
                    curIterator.IsIterator = true;
                }
            }
            else
            {
                ReportError("yield is permitted only in iterator's body.");
            }

            AssertAndAdvance(TokenID.Yield);

            switch (curtok.ID)
            {
                case TokenID.Return:
                    node = new YieldStatement(false, true, curtok);
                    Advance();
                    node.ReturnValue = ParseExpression();
                    break;
                case TokenID.Break:
                    node = new YieldStatement(true, false, curtok);
                    Advance();
                    break;
                default:
                    //TODO: Wait, but 'node' will still be null!
                    ReportError("Expected return or break. Found '" + curtok.ID.ToString().ToLower() + "'.");
                    Advance();
                    break;
            }

            //try .. catch .. finally checks
            if (isTry>0
                && node.IsReturn )
            {
                hasYieldReturnInTry = true;
            }

            if(isCatch>0)
            {
                ReportError("'yield return' and 'yield break' are not permitted in catch block.");

                if (hasYieldReturnInTry)
                {
                    ReportError("'yield return' is not permitted in try block with catch block.");
                }
            }

            if(isFinally>0)
            {
                ReportError("'yield return' and 'yield break' are not permitted in finally block.");
            }
            
            AssertAndAdvance(TokenID.Semi);

            return node;
        }
        private BlockStatement ParseUnsafeCode()										
		{
            isUnsafe++;

            BlockStatement ret = new BlockStatement(isUnsafe > 0, curtok);
            ret.IsUnsafeDeclared = true;
            ret.IsUnsafe = isUnsafe > 0;

			// todo: fully parse unsafe code
            AssertAndAdvance(TokenID.Unsafe); // over 'unsafe'

            if (curIterator != null )
            {
                if (curIterator.IsIterator)
                {
                    ReportError("Unsafe block not authorized in iterator");
                }
                else
                {
                    //at this point the parser did not parsed any yield statement
                    // so the block is not really an iterator, but it could be ;)
                    if (curIterator.CouldBeIterator)
                    {
                        ReportError("Warning : unsafe block in potential iterator");
                    }
                }
            }

            if (curtok.ID != TokenID.RCurly)
            {
                ParseBlock(ret);
            }
            else
            {
                Advance();
            }

            isUnsafe--;

            return ret;
		}

        private void ParseAddressOfIdentifier()
        {
            AssertAndAdvance(TokenID.BAnd);
            exprStack.Push(new AddressOfExpression(ParseExpression()));
        }

        private FixedStatementStatement ParseFixedStatement()
		{
            FixedStatementStatement ret = new FixedStatementStatement(curtok);

            AssertAndAdvance(TokenID.Fixed);
            AssertAndAdvance(TokenID.LParen);

            // parse  pointer-type   fixed-pointer-declarators
            ret.LocalPointers.Type = ParseType();

            //parse 
            //  fixed-pointer-declarators:
            //      fixed-pointer-declarator
            //      fixed-pointer-declarators   ,   fixed-pointer-declarator
            //
            //  fixed-pointer-declarator:
            //      identifier   =   fixed-pointer-initializer
            //
            //  fixed-pointer-initializer:
            //      &   variable-reference
            //      expression

            while (curtok.ID != TokenID.RParen)
            {
                ret.LocalPointers.Identifiers.Expressions.Add( (IdentifierExpression)ParseIdentifierOrKeyword( false, false, false, false ) );

                AssertAndAdvance(TokenID.Equal);

                ret.LocalPointers.RightSide.Expressions.Add(ParseExpression(TokenID.Comma));

                if (curtok.ID == TokenID.Comma)
                {
                    Advance();
                }
            }

            AssertAndAdvance(TokenID.RParen);

            if (curtok.ID != TokenID.RCurly)
            {
                ParseBlock(ret.Statements);
            }
            else
            {
                Advance();
            }

            return ret;
		}


		// expressions
        /// <summary>
        /// this version of ParseExpression will stop when the parser increment the current file line
        /// It used by the ParsePreprocessorDirective 
        /// </summary>
        private ExpressionNode ParseExpression(bool stopWhenLineChange)
        {
            TokenID id = curtok.ID;
            int oldLine = curtok.Line;
            bool lineChanged = false;

            while ( !lineChanged && id != TokenID.Eof &&
                    id != TokenID.Semi && id != TokenID.RParen &&
                    id != TokenID.Comma && id != TokenID.Colon)
            {
                ParseExpressionSegment();
                id = curtok.ID;
                if (stopWhenLineChange && curtok.Line != oldLine)
                {
                    lineChanged = true;
                }
            }
            return exprStack.Pop();
        }
		private ExpressionNode ParseExpression(TokenID endToken)			
		{
			TokenID id = curtok.ID;
			while (	id != endToken		&& id != TokenID.Eof	&& 
					id != TokenID.Semi	&& id != TokenID.RParen &&
					id != TokenID.Comma && id != TokenID.Colon)
			{
				ParseExpressionSegment();
				id = curtok.ID;	
			}
			return exprStack.Pop();
		}
		private ExpressionNode ParseExpression()							
		{
			TokenID id = curtok.ID;
			while ( id != TokenID.Eof && id != TokenID.RCurly &&
					id != TokenID.Semi	&& id != TokenID.RParen &&
					id != TokenID.Comma && id != TokenID.Colon &&
                    id != TokenID.Comma && id != TokenID.RBracket )
			{
				ParseExpressionSegment();
				id = curtok.ID;	
			}
			return exprStack.Pop();
		}



        public bool TokenIsAssignement(TokenID id)
        {
            return
                (id == TokenID.Equal || id == TokenID.PlusEqual || id == TokenID.MinusEqual || id == TokenID.StarEqual ||
                 id == TokenID.SlashEqual || id == TokenID.PercentEqual || id == TokenID.BAndEqual ||
                 id == TokenID.BOrEqual || id == TokenID.BXorEqual || id == TokenID.ShiftLeftEqual ||
                 id == TokenID.ShiftRightEqual);
        }

        /// <summary>
        /// tag a type as nullable.
        /// If node is not a type, the method converts it to a type
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static ExpressionNode TagAsNullableType(ExpressionNode node)
        {
            // this is not a type declaration
            // the parser convert it to inullable type
            if ( !(node is INullableType) )
            {
               node = new TypeNode(node );
            }

            ((INullableType)node).IsNullableType = true;

            return node;
        }

        /// <summary>
        /// tag a type as pointer.
        /// If node is not a type pointer, the method converts it to a type pointer
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static ExpressionNode TagAsPointerType(ExpressionNode node)
        {
            if (!(node is IPointer))
            {
                node = new TypeNode(node );
            }
            return new TypePointerNode(node );
        }


		private void ParseExpressionSegment()
		{
			#region Chart
			// arraycre		new			: type : [{
			// literal		(lit)
			// simpleName	ident
			// parenExpr	LParen		: expr
			// memAccess	pexpr		: Dot
			//				pdefType	: Dot
			// invoke		pexpr		: LParen
			// elemAccess	noArrCreExpr: LBracket
			// thisAccess	this
			// baseAccess	base		: Dot
			//				base		: LBracket
			// postInc		pexpr		: ++
			// postDec		pexpr		: --
			// objCre		new			: type : LParen
			// delgCre		new			: delgType : LParen
			// typeof		typeof		: LParen
			// checked		checked		: LParen
			// unchecked	unchecked	: LParen
			#endregion
			ExpressionNode tempNode = null;
			TokenID startToken = curtok.ID;
			switch (curtok.ID)
			{
				#region Literals
				case TokenID.NullLiteral:
                    exprStack.Push(new NullPrimitive(curtok));
					Advance();
					break;

				case TokenID.TrueLiteral:
                    exprStack.Push(new BooleanPrimitive(true, curtok));
					Advance();
					ParseContinuingPrimary();
					break;

				case TokenID.FalseLiteral:
                    exprStack.Push(new BooleanPrimitive(false, curtok));
					Advance();
					ParseContinuingPrimary();
					break;

				case TokenID.IntLiteral:
                    exprStack.Push(new IntegralPrimitive(strings[curtok.Data], IntegralType.Int, curtok));
					Advance();
					ParseContinuingPrimary();
					break;
				case TokenID.UIntLiteral:
                    exprStack.Push(new IntegralPrimitive(strings[curtok.Data], IntegralType.UInt, curtok));
					Advance();
					ParseContinuingPrimary();
					break;
				case TokenID.LongLiteral:
                    exprStack.Push(new IntegralPrimitive(strings[curtok.Data], IntegralType.Long, curtok));
					Advance();
					ParseContinuingPrimary();
					break;
				case TokenID.ULongLiteral:
                    exprStack.Push(new IntegralPrimitive(strings[curtok.Data], IntegralType.ULong, curtok));
					Advance();
					ParseContinuingPrimary();
					break;

				case TokenID.RealLiteral:
                    exprStack.Push(new RealPrimitive(strings[curtok.Data], curtok));
					Advance();
					ParseContinuingPrimary();
					break;

				case TokenID.CharLiteral:
                    exprStack.Push(new CharPrimitive(strings[curtok.Data], curtok));
					Advance();
					ParseContinuingPrimary();
					break;

				case TokenID.StringLiteral:
					string sval = strings[curtok.Data];
                    exprStack.Push(new StringPrimitive(sval, curtok));
					Advance();
					ParseContinuingPrimary();
					break;
				#endregion
				#region Predefined Types

				case TokenID.Bool:
				case TokenID.Byte:
				case TokenID.Char:
				case TokenID.Decimal:
				case TokenID.Double:
				case TokenID.Float:
				case TokenID.Int:
				case TokenID.Long:
				case TokenID.Object:
				case TokenID.SByte:
				case TokenID.Short:
				case TokenID.String:
				case TokenID.UInt:
				case TokenID.ULong:
				case TokenID.UShort:
                    exprStack.Push(ParseIdentifierOrKeyword(false, true, false, false));

                    if (curtok.ID == TokenID.Question)
                    {
                        Advance();
                        ((INullableType)exprStack.Peek()).IsNullableType = true;
                    }
                    else
                    {
                        if (curtok.ID == TokenID.Star)
                        {
                            Advance();
                            exprStack.Push(new TypePointerNode(exprStack.Pop()));
                        }   
                    }

					ParseContinuingPrimary();
					break;
				#endregion
				#region Binary Ops
					
				case TokenID.Plus:
					tempNode = ConsumeBinary(startToken);
					if (tempNode != null)
					{
                        exprStack.Push(new UnaryExpression(startToken, tempNode, tempNode.RelatedToken)); // unary
					}
					break;
				case TokenID.Minus:
                    if (exprStack.Count != 0 && exprStack.Peek() is PrimaryExpression)
                    {
                        tempNode = ConsumeBinary(startToken);
                    }
                    else
                    {
                        ConsumeUnary(TokenID.Minus);
                    }
					if (tempNode != null)
					{
						exprStack.Push(new UnaryExpression(startToken, tempNode, tempNode.RelatedToken)); // unary
					}
					break;

				case TokenID.Star:
                    // for this token, we have to control it is not a
                    // pointer type declaration
                    bool isPointer = false;

                    if ( exprStack.Count == 0
                        || !(exprStack.Peek() is IPointer) &&
                            (curTokNode != null && curTokNode.Value.ID == TokenID.Star || curTokNode.Previous.Previous.Value.ID == TokenID.Star))
                    {
                        // case : 
                        // int* p;
                        // *p = x;
                        Advance();//over the '*';

                        ParseExpressionSegment();
                        exprStack.Push(new DereferenceExpression(exprStack.Pop(), true ));
                    }
                    else
                    {
                        if (isLocalConst // if const kw -> local/member declaration
                            || exprStack.Peek() is IPointer)
                        {
                            //the expression inherits from IPointer, so this is a type expression
                            //and it might be a pointer
                            isPointer = true;
                        }
                        else
                        {
                            // if the next segment can not be an expression
                            TokenID id = (curTokNode != null) ? curTokNode.Value.ID : TokenID.Eof;
                            if (id == TokenID.Eof || id == TokenID.RCurly ||
                                id == TokenID.Semi || id == TokenID.RParen ||
                                id == TokenID.Comma || id == TokenID.Colon)
                            {
                                isPointer = true;
                            }
                        }

                        if (isPointer)
                        {
                            Advance();
                            ExpressionNode e = TagAsPointerType(exprStack.Pop());
                            exprStack.Push(e);
                        }
                        else
                        {
                            //the next segment is an expression. 
                            //at this sage it is impossible to determine if this is a pointer expression
                            ConsumeBinary(startToken);

                            if (curtok.ID == TokenID.Equal)
                            {
                                // the assignment means that the previous expression is not 
                                // a binary expression but a local pointer declaration

                                Advance(); // over equal

                                BinaryExpression bin = (BinaryExpression)exprStack.Pop();
                                LocalDeclarationStatement ldecl = new LocalDeclarationStatement((IType)TagAsPointerType(bin.Left), (IdentifierExpression)bin.Right, ParseExpression(TokenID.Comma) );

                                exprStack.Push(ldecl);

                                if (curtok.ID == TokenID.Comma)// multiple local declaration
                                {
                                    exprStack.Push((ExpressionNode)ldecl.Type);
                                    ParseLocalDeclaration();
                                }
                            }
                        }
                    }
                    break;
                case TokenID.Is:
                case TokenID.As:
				case TokenID.Slash:
				case TokenID.Percent:
				case TokenID.ShiftLeft:
				case TokenID.ShiftRight:
				case TokenID.Less:
				case TokenID.Greater:
				case TokenID.LessEqual:
				case TokenID.GreaterEqual:
				case TokenID.EqualEqual:
				case TokenID.NotEqual:
				case TokenID.BXor:
				case TokenID.BOr:
				case TokenID.And:
				case TokenID.Or:
					ConsumeBinary(startToken);
					break;

                case TokenID.BAnd:
                    if (exprStack.Count == 0)
                    {
                        //the expression stack is empty, so this is n identifier dereference
                        ParseAddressOfIdentifier();
                    }
                    else
                    {
                        ConsumeBinary(startToken);
                    }
                    break;

				#endregion
				#region Unary Ops

				case TokenID.Not:
				case TokenID.Tilde:
				case TokenID.PlusPlus:
				case TokenID.MinusMinus:
					ConsumeUnary(startToken);
					break;

				#endregion
				#region Conditional
				case TokenID.Question:
                    if (curtok.NullableDeclaration)
                    {
                        Advance();

                        ExpressionNode expr = TagAsNullableType(exprStack.Pop());
                        CheckRankSpecifier(expr);

                        exprStack.Push(expr);
                    }
                    else
                    {
                        Advance();

                        ConditionalExpression conditionalExpression = new ConditionalExpression(exprStack.Pop(), null, null);

                        exprStack.Push(conditionalExpression);

                        ExpressionNode cond1 = ParseExpression(TokenID.Equal);
                        AssertAndAdvance(TokenID.Colon);
                        ExpressionNode cond2 = ParseExpression();

                        conditionalExpression.Left = cond1;
                        conditionalExpression.Right = cond2;
                    }                    

					break;
                //case TokenID.ColonColon:

                //    Advance();
                    //break;
                case TokenID.QuestionQuestion:
                    ExpressionNode left = exprStack.Pop();
                    Advance();
                    ExpressionNode right = ParseExpression(TokenID.Semi);
                    exprStack.Push(new NullCoalescingExpression(left, right ));
                    break;
				#endregion
				#region Keywords
				// keywords
				case TokenID.Ref:
					Advance();
					ParseExpressionSegment();
                    exprStack.Push(new RefNode(exprStack.Pop()));
					break;

				case TokenID.Out:
					Advance();
					ParseExpressionSegment();
                    exprStack.Push(new OutNode(exprStack.Pop()));
					break;

				case TokenID.This:
                    exprStack.Push(ParseIdentifierOrKeyword(false, true, false, false));
					ParseContinuingPrimary();
					break;

				case TokenID.Void:
					// this can happen in typeof(void), nothing can follow
                    exprStack.Push(new VoidPrimitive(curtok)); 
                    Advance();
					break;

				case TokenID.Base:
					Advance();
					TokenID newToken = curtok.ID;
					if (newToken == TokenID.Dot)
					{
						Advance();// advance over dot
						exprStack.Push( new IdentifierExpression(strings[curtok.Data], curtok) );
						Advance();//advance over ident
					}
					else if (newToken == TokenID.LBracket)
					{
						Advance();//advance over left bracket
						exprStack.Push( ParseExpressionList(TokenID.RBracket) );
					}
                   
					ParseContinuingPrimary();

                    exprStack.Push(new BaseAccessExpression(exprStack.Pop()));
					break;

				case TokenID.Typeof:
					Advance();
					AssertAndAdvance(TokenID.LParen);
                    exprStack.Push(new TypeOfExpression(ParseExpression(TokenID.RParen)));
					AssertAndAdvance(TokenID.RParen);
					ParseContinuingPrimary();
					break;

				case TokenID.Checked:
					Advance();
					AssertAndAdvance(TokenID.LParen);
					//ParseExpressionSegment();
                    exprStack.Push(new CheckedExpression(ParseExpression(TokenID.RParen)));
					AssertAndAdvance(TokenID.RParen);
					ParseContinuingPrimary();
					break;

				case TokenID.Unchecked:
					Advance();
					AssertAndAdvance(TokenID.LParen);
					ParseExpressionSegment();
                    exprStack.Push(new UncheckedExpression(exprStack.Pop()));
					AssertAndAdvance(TokenID.RParen);
					ParseContinuingPrimary();
					break;
				#endregion
				#region Assignment

				case TokenID.Equal:
				case TokenID.PlusEqual:
				case TokenID.MinusEqual:
				case TokenID.StarEqual:
				case TokenID.SlashEqual:
				case TokenID.PercentEqual:
				case TokenID.BAndEqual:
				case TokenID.BOrEqual:
				case TokenID.BXorEqual:
				case TokenID.ShiftLeftEqual:
				case TokenID.ShiftRightEqual:
					TokenID op = curtok.ID;
					Advance();
					if (exprStack.Count > 0 && !(exprStack.Peek() is PrimaryExpression) && !(exprStack.Peek() is UnaryCastExpression))
					{
						ReportError("Left hand side of assignment must be a variable.");
					}
					ExpressionNode assignVar = exprStack.Pop();
					ExpressionNode rightSide = ParseExpression();
					exprStack.Push(new AssignmentExpression(op, assignVar, rightSide));
					break;

				#endregion
                #region Generic
                case TokenID.Default:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    ParseTypeParameterNode(false, false, false);
                    AssertAndAdvance(TokenID.RParen);
                    exprStack.Push(new DefaultConstantExpression(curTypeParameters[0] ));

                    curTypeParameters = new List<TypeParameterNode>();
                    ParseContinuingPrimary();
                    break;
                #endregion
                case TokenID.LCurly:
                    ArrayInitializerExpression aie = new ArrayInitializerExpression(curtok);
                    Advance();
					exprStack.Push(aie);
					aie.Expressions = ParseExpressionList(TokenID.RCurly);
					break;

				case TokenID.New:
					Advance();

                    isNewStatement = true;

                    // TODO : new int*() is allowed ? 
					IType newType = ParseType();

                    //ApplyTypeParameters(newType); -> not sure, but a type pointer can not be generic!

					if (curtok.ID == TokenID.LParen)
					{
						Advance();
						ExpressionList newList = ParseExpressionList(TokenID.RParen);
						exprStack.Push(new ObjectCreationExpression(newType, newList, newList.RelatedToken));
					}
					else if (curtok.ID == TokenID.LBracket)
					{
						ParseArrayCreation(newType);
					}
					ParseContinuingPrimary();

                    isNewStatement = false;
					break;

				case TokenID.Ident:

					//test for local decl
					bool isDecl = isAfterType();
					if (isDecl)
					{
						ParseLocalDeclaration();
					}
					else
					{
                        ExpressionNode expr = ParseIdentifierOrKeyword(false, true, false, false);

                        exprStack.Push(expr);
						ParseContinuingPrimary();
					}
					break;

				case TokenID.LParen:
					Advance();
					ParseCastOrGroup();
					break;

                case TokenID.Delegate://anonymous method
                    Advance();
                    ParseAnonymousMethod();
                    break;

                case TokenID.Sizeof:
                    ParseSizeOf();
                    break;

                case TokenID.Stackalloc:
                    ParseStackalloc();
                    break;

                case TokenID.MinusGreater:
                    // pointer member access
                    ParseContinuingPrimary();
                    break;

				default:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        goto case TokenID.Ident;
                    }
                    else
                    {
                        RecoverFromError("Unhandled case in ParseExpressionSegment ", curtok.ID); // todo: fill out error report
                    }
					break;
			}
		}

		private void ConsumeUnary(TokenID startOp)							
		{
			Advance();
			ParseExpressionSegment();
			while (precedence[(int)curtok.ID] > precedence[(int)startOp])
			{
				ParseExpressionSegment();
			}
            ExpressionNode node = exprStack.Pop();
			UnaryExpression uNode = new UnaryExpression(startOp, node.RelatedToken);
			uNode.Child = node;
			exprStack.Push(uNode);
		}
		private ExpressionNode ConsumeBinary(TokenID startOp)				
		{
			ExpressionNode result = null;

            if ((exprStack.Count == 0 || precedence[(int)curTokNode.Previous.Previous.Value.ID] > 0))
			{
				// assert +,-,!,~,++,--,cast
				Advance();
				ParseExpressionSegment();
				while (precedence[(int)curtok.ID] > precedence[(int)startOp])
				{
					ParseExpressionSegment();
				}
				result = exprStack.Pop(); // this signals it was a unary operation
			}
			else
			{
                BinaryExpression bNode = new BinaryExpression(startOp, curtok);
				Advance();
				bNode.Left = exprStack.Pop();
				exprStack.Push(bNode); // push node
				ParseExpressionSegment(); // right side
				// consume now or let next op consume?
				while (precedence[(int)curtok.ID] > precedence[(int)startOp])
				{
					ParseExpressionSegment();
				}
                
                
                bNode.Right = exprStack.Pop();
           }
			return result;
		}


		private bool isAfterType()											
		{
			bool result = false;
            if ( exprStack.Count > 0)
			{
                ExpressionNode node = exprStack.Peek();

                if (node is QualifiedIdentifierExpression)
                {
                    QualifiedIdentifierExpression qie = (QualifiedIdentifierExpression)exprStack.Pop();
                    TypeNode t = new TypeNode(qie );
                    exprStack.Push(t);
                    result = true;
                }
                else
                {
                    if (node is IdentifierExpression)
                    {
                        IdentifierExpression ie = (IdentifierExpression)exprStack.Pop();
                        TypeNode t = new TypeNode(ie );
                        exprStack.Push(t);
                        result = true;
                    }
                    else
                    {
                        if (node is TypeNode
                            || node is TypePointerNode
                            || node is MemberAccessExpression)
                        {
                            result = true;
                        }
                    }
                }
			}
			return result;
		}
		private ExpressionList ParseExpressionList(TokenID termChar)		
		{
			ExpressionList list = new ExpressionList();
			TokenID id = curtok.ID;
			while (id != TokenID.Eof && id != termChar)
			{
				while (id != TokenID.Eof && id != termChar && id != TokenID.Comma)
				{
					ParseExpressionSegment();
					id = curtok.ID;
				}

				if (curtok.ID == TokenID.Comma)
				{
					Advance(); // over comma
				}
				list.Expressions.Add(exprStack.Pop());
				id = curtok.ID;
			}
			if (curtok.ID == termChar)
			{
				Advance();
			}
			return list;
		}
		private void ParseLocalDeclaration()								
		{
            IdentifierExpression declIdentifier = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false);
			IType type = (IType)exprStack.Pop();
			LocalDeclarationStatement lnode = new LocalDeclarationStatement(curtok);
			lnode.Identifiers.Expressions.Add(declIdentifier);

			if (isLocalConst)
			{
				lnode.IsConstant = true;
			}
			isLocalConst = false;
			lnode.Type = type;

			// a using statement can hold a local decl without a semi, thus the rparen
			while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.Semi && curtok.ID != TokenID.RParen)
			{
				while (curtok.ID == TokenID.Comma)
				{
					Advance(); // over comma
                    declIdentifier = (IdentifierExpression)ParseIdentifierOrKeyword(false, true, false, false);
                    lnode.Identifiers.Expressions.Add(declIdentifier);
				}
				if (curtok.ID == TokenID.Equal)
				{
					Advance(); // over equal
					lnode.RightSide = ParseExpression(TokenID.Comma);

					if (curtok.ID == TokenID.Comma)
					{
						exprStack.Push(lnode);
						lnode = new LocalDeclarationStatement(curtok);
						lnode.Type = type;
					}
				}
			}
			exprStack.Push(lnode);
		}

        private void ParseAnonymousMethod()
        {
            AnonymousMethodNode ret = new AnonymousMethodNode(curtok);

            isAnonynous++;

            if (curtok.ID == TokenID.LParen)
            {
                ret.Params = ParseParamList();
            }

            ParseBlock(ret.StatementBlock);

            isAnonynous--;

            exprStack.Push(ret);
        }

        private void ParseSizeOf()
        {
            AssertAndAdvance(TokenID.Sizeof);
            AssertAndAdvance(TokenID.LParen);

            SizeOfExpression expr = new SizeOfExpression(ParseExpression(TokenID.RParen));
            
            AssertAndAdvance(TokenID.RParen);

            exprStack.Push(expr);
        }

        private void ParseStackalloc()
        {
            if (isUnsafe <= 0)
            {
                ReportError("stackalloc authorized only in unsafe context.");
            }

            if (isFinally > 0 || isCatch > 0)
            {
                ReportError("stackalloc not authorized in catch/finally block.");
            }

            AssertAndAdvance(TokenID.Stackalloc);
            
            IType type = ParseType();

            AssertAndAdvance(TokenID.LBracket);

            ExpressionNode n = ParseExpression(TokenID.RBracket);

            StackallocExpression expr = new StackallocExpression(type, n, n.RelatedToken);
            
            AssertAndAdvance(TokenID.RBracket);

            exprStack.Push(expr);
        }


		private void ParseCastOrGroup()										
		{
			ExpressionNode interior = ParseExpression();
			AssertAndAdvance(TokenID.RParen);
			TokenID rightTok = curtok.ID;

			// check if this is terminating - need better algorithm here :(
			// todo: this can probably be simplified (and correctified!) with new expression parsing style
			if (!(interior is IType) ||
				rightTok == TokenID.Semi ||
				rightTok == TokenID.RParen ||
				rightTok == TokenID.RCurly ||
				rightTok == TokenID.RBracket ||
				rightTok == TokenID.Comma)
			{
				// was group for sure
				exprStack.Push(new ParenthesizedExpression(interior ));
				ParseContinuingPrimary();
			}
			else
			{
				// push a pe just in case upcoming is binary expr
				ParenthesizedExpression pe = new ParenthesizedExpression(curtok);
				exprStack.Push(pe);

				// find out what is on right
				ParseExpressionSegment();
				ExpressionNode peek = exprStack.Peek();

				if (peek is PrimaryExpression || peek is UnaryExpression)
				{
					// cast
					UnaryCastExpression castNode = new UnaryCastExpression(curtok);
					castNode.Type = (IType)interior;
					castNode.Child = exprStack.Pop();
					// need to pop off the 'just in case' pe
					exprStack.Pop();
					exprStack.Push(castNode);
				}
				else
				{
					// group
					pe.Expression = interior;
					ParseContinuingPrimary();
				}
			}
		}
		private void ParseArrayCreation(IType type)						
		{
			ArrayCreationExpression arNode = new ArrayCreationExpression(curtok);
			

			arNode.Type = type;

            if (curtok.ID != TokenID.LCurly)
            {
                Advance(); // over lbracket
            }

            if (curtok.ID == TokenID.Comma)
            {
                int comma = 0;
                // comma specifier
                do
                {
                    ++comma;
                    Advance();
                } while (curtok.ID == TokenID.Comma);
                arNode.AdditionalRankSpecifiers.Add(comma);
            }
            else
            {
                if (curtok.ID == TokenID.RBracket)
                {
                    arNode.AdditionalRankSpecifiers.Add(0);
                }
                else
                {
                    // this tests for literal size declarations on first rank specifiers
                    if (curtok.ID != TokenID.Invalid
                        && curtok.ID != TokenID.Comma
                        && curtok.ID != TokenID.RBracket
                        && curtok.ID != TokenID.LCurly)
                    {
                        //Advance(); // over lbracket
                        arNode.RankSpecifier = ParseExpressionList(TokenID.RBracket);
                    }
                }
            }

            if (curtok.ID == TokenID.RBracket)
            {
                Advance();
            }

			// now any 'rank only' specifiers (without size decls)
			while (curtok.ID == TokenID.LBracket)
			{
				Advance(); // over lbracket
				int commaCount = 0;
				while (curtok.ID == TokenID.Comma)
				{
					commaCount++;
					Advance();
				}
				arNode.AdditionalRankSpecifiers.Add(commaCount);
				AssertAndAdvance(TokenID.RBracket);
			}
			if (curtok.ID == TokenID.LCurly)
			{
                arNode.Initializer = new ArrayInitializerExpression(curtok);

				Advance();
               
				arNode.Initializer.Expressions = ParseExpressionList(TokenID.RCurly);
			}

            exprStack.Push(arNode);
		}

		private void ParseContinuingPrimary()								
		{
			bool isContinuing = curtok.ID == TokenID.LBracket 
                                || curtok.ID == TokenID.Dot
                                || curtok.ID == TokenID.ColonColon
                                || curtok.ID == TokenID.MinusGreater 
                                || curtok.ID == TokenID.LParen 
                                || curtok.ID == TokenID.Less;

			while (isContinuing)
			{
				switch (curtok.ID)
				{
					case TokenID.Dot:
                    case TokenID.ColonColon:
                        ParseMemberAccess();
                        break;
                    case TokenID.MinusGreater:
                        exprStack.Push(new DereferenceExpression(exprStack.Pop(), false ));
						ParseMemberAccess();
						break;
					case TokenID.LParen:
						ParseInvocation();
						break;
					case TokenID.LBracket:
						isContinuing = ParseElementAccess();
						break;
                    case TokenID.Less:
                        ParseGenericTypeNodeExpression();
                        break;
					default:
						isContinuing = false;
						break;
				}
				if (isContinuing)
				{
                    isContinuing = curtok.ID == TokenID.LBracket || curtok.ID == TokenID.Dot || curtok.ID == TokenID.LParen;
				}
			}
			// can only be one at end
			if (curtok.ID == TokenID.PlusPlus)
			{
				Advance();
                exprStack.Push(new PostIncrementExpression(exprStack.Pop()));
			}
			else if(curtok.ID == TokenID.MinusMinus)
			{
				Advance();
                exprStack.Push(new PostDecrementExpression(exprStack.Pop()));
			}
		}
		private void ParseMemberAccess()									
		{
            bool qualifierAlias = curtok.ID == TokenID.ColonColon;

			Advance(); // over dot
			if (curtok.ID != TokenID.Ident)
			{
				ReportError("Right side of member access must be identifier");
			}
            // the member acces could be a type
            ExpressionNode identifier = ParseIdentifierOrKeyword(true, true, false, false);
			if (exprStack.Count > 0 && exprStack.Peek() is IMemberAccessible)
			{
				IMemberAccessible ima = (IMemberAccessible)exprStack.Pop();

                MemberAccessExpression mba = new MemberAccessExpression(ima, identifier );
                mba.IsNamespaceAliasQualifier = qualifierAlias;

				exprStack.Push( mba );
			}
			else
			{
				ReportError("Left side of member access must be PrimaryExpression or PredefinedType.");
			}
		}
		private void ParseInvocation()										
		{
			Advance(); // over lparen

			PrimaryExpression leftSide = (PrimaryExpression)exprStack.Pop();
			ExpressionList list = ParseExpressionList(TokenID.RParen);
			exprStack.Push(new InvocationExpression(leftSide, list ));
		}
		private bool ParseElementAccess()									
		{
			bool isElementAccess = true;
			Advance(); // over lbracket
			ExpressionNode type = exprStack.Pop(); // the caller pushed, so must have at least one element

			// case one is actually a type decl (like T[,,]), not element access (like T[2,4])
			// in this case we need to push the type, and abort parsing the continuing
			if (curtok.ID == TokenID.Comma || curtok.ID == TokenID.RBracket)
			{
				isElementAccess = false;

                if (!(type is IType))
                {
                    type = new TypeNode(type );
                }

				exprStack.Push(type);
				ParseArrayRank((IType)type);
			}
			else
			{
				// element access case
				if (type is PrimaryExpression)
				{
					PrimaryExpression tp = (PrimaryExpression)type;
					ExpressionList el = ParseExpressionList(TokenID.RBracket);
					exprStack.Push(new ElementAccessExpression(tp, el ));
				}
				else
				{
					ReportError("Left side of Element Access must be primary expression.");
				}
			}

			return isElementAccess;
		}
		private void ParseArrayRank(IType type)							
		{
			// now any 'rank only' specifiers (without size decls)
			bool firstTime = true;
			while (curtok.ID == TokenID.LBracket || firstTime)
			{
				if (!firstTime)
				{
					Advance();
				}
				firstTime = false;
				int commaCount = 0;
				while (curtok.ID == TokenID.Comma)
				{
					commaCount++;
					Advance();
				}
				type.RankSpecifiers.Add(commaCount);
				AssertAndAdvance(TokenID.RBracket);
			}
		}

		// utility
		private void RecoverFromError(TokenID id)							
		{
			RecoverFromError("", id);
		}
		private void RecoverFromError(string message, TokenID id)			
		{
			string msg = "Error: Expected " + id + " found: " + curtok.ID;
			if (message != null)
				msg = message + msg;

			ReportError(msg);

            // the recover from try to recover a correct state from the erroned token
            // flush all stacks 

            //typeStack;
            //curMethod = null;
            //curOperator = null;
            //curIterator = null;

            //exprStack;
            //curState;
            //curInterface;
            //curtok;
            //curTokNode = null;

            //curmods;
            //nextIsPartial = false;
            //curAttributes;

            //curTypeParameters;

            //curTypeParameterConstraints;

            //blockStack;

            //inPPDirective = false;
            //isAnonynous = 0;

            //isNewStatement = false;
            //isUnsafe = 0;

            //isTry = 0;
            //isCatch = 0;
            //isFinally = 0;

			Advance();
		}
		private void ReportError(string message)							
		{
            ReportError(message, curtok);
		}

        private void ReportError(string message, Token tok)
        {
            Errors.Add(new Error(message, curtok, tok.Line, tok.Col, currentFileName));
        }

		private void AssertAndAdvance(TokenID id)							
		{
			if (curtok.ID != id)
			{
				RecoverFromError(id);
			}
			Advance();
		}
		private void Advance()												
		{
			bool skipping = true;
			do
			{
				if (curTokNode != null)
				{
					curtok = curTokNode.Value;
                    curTokNode = curTokNode.Next;
				}
				else
				{
					curtok = EOF;
				}

				switch (curtok.ID)
				{
					case TokenID.SingleComment:
						break;
					case TokenID.MultiComment:
						string[] s = strings[curtok.Data].Split('\n');
						lineCount += s.Length - 1;
						break;

					case TokenID.Newline:
						lineCount++;
						break;

					case TokenID.Hash:
						// preprocessor directives
						if (!inPPDirective)
						{
							ParsePreprocessorDirective();
							if (curtok.ID != TokenID.Newline &&
								curtok.ID != TokenID.SingleComment &&
								curtok.ID != TokenID.MultiComment && 
								curtok.ID != TokenID.Hash )
							{
								skipping = false;
							}
							else if(curtok.ID == TokenID.Hash)
							{
								//index--;
                                curTokNode = curTokNode.Previous;
							}
						}
						else
						{
							skipping = false;
						}
						break;

					default:
						skipping = false;
						break;
				}
			} while (skipping);
		}
		private void SkipToEOL(int startLine)								
		{
			if (lineCount > startLine)
			{
				return;
			}
			bool skipping = true;
			do
			{
                if (curTokNode != null)
                {
                    curtok = curTokNode.Value;
                    curTokNode = curTokNode.Next;
                }
                else
                {
                    curtok = EOF;
                    skipping = false;
                }
                
				if(curtok.ID == TokenID.Newline)
				{
					lineCount++;
					skipping = false;
				}
			} while (skipping);
		}
		private void SkipToNextHash()										
		{
			bool skipping = true;
			do
			{

                if (curTokNode != null)
                {
                    curtok = curTokNode.Value;
                    curTokNode = curTokNode.Next;
                }
                else
                {
                    curtok = EOF;
                    skipping = false;
                }
                
				if (curtok.ID == TokenID.Hash)
				{
					skipping = false;
				}
				else if (curtok.ID == TokenID.Newline)
				{
					lineCount++;
				}
			} while (skipping);
		}
		private void SkipToElseOrEndIf()									
		{
			// advance to elif, else, or endif
			int endCount = 1;
			bool firstPassHash = curtok.ID == TokenID.Hash;
			while (endCount > 0)
			{
				if (!firstPassHash)
				{
					SkipToNextHash();
				}
				firstPassHash = false;

				if ( curTokNode == tokens.Last )
				{
					break;
                }

                Token tk = curTokNode.Value;

                if (tk.ID == TokenID.Ident)
				{
                    string sKind = strings[tk.Data];
					if (sKind == "endif")
					{
						endCount--;
					}
					else if (sKind == "elif")
					{
						if (endCount == 1)
						{
							break;
						}
					}
				}
                else if (tk.ID == TokenID.If)
				{
					endCount++;
				}
                else if (tk.ID == TokenID.Else)
				{
					if (endCount == 1)
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
		}


		static Parser()														
		{
			modMap = new SortedList<TokenID, Modifier>();
			modMap.Add(TokenID.New, Modifier.New);
			modMap.Add(TokenID.Public, Modifier.Public);
			modMap.Add(TokenID.Protected, Modifier.Protected);
			modMap.Add(TokenID.Internal, Modifier.Internal);
			modMap.Add(TokenID.Private, Modifier.Private);
			modMap.Add(TokenID.Abstract, Modifier.Abstract);
			modMap.Add(TokenID.Sealed, Modifier.Sealed);
			modMap.Add(TokenID.Static, Modifier.Static);
			modMap.Add(TokenID.Virtual, Modifier.Virtual);
			modMap.Add(TokenID.Override, Modifier.Override);
			modMap.Add(TokenID.Extern, Modifier.Extern);
			modMap.Add(TokenID.Readonly, Modifier.Readonly);
			modMap.Add(TokenID.Volatile, Modifier.Volatile);
			modMap.Add(TokenID.Ref, Modifier.Ref);
			modMap.Add(TokenID.Out, Modifier.Out);
			modMap.Add(TokenID.Assembly, Modifier.Assembly);
			modMap.Add(TokenID.Field, Modifier.Field);
			modMap.Add(TokenID.Event, Modifier.Event);
			modMap.Add(TokenID.Method, Modifier.Method);
			modMap.Add(TokenID.Param, Modifier.Param);
			modMap.Add(TokenID.Property, Modifier.Property);
			modMap.Add(TokenID.Return, Modifier.Return);
			modMap.Add(TokenID.Type, Modifier.Type);
            modMap.Add(TokenID.Partial, Modifier.Partial);
            modMap.Add(TokenID.Unsafe, Modifier.Unsafe);
            modMap.Add(TokenID.Fixed, Modifier.Fixed);

			// all default to zero
			precedence = new byte[0xFF];

			// these start at 80 for no paticular reason
			precedence[ (int)TokenID.LBracket]		= 0x90;

			precedence[ (int)TokenID.LParen]		= 0x80;
			precedence[ (int)TokenID.Star ]		 	= 0x7F; 
			precedence[ (int)TokenID.Slash ]	 	= 0x7F;
			precedence[ (int)TokenID.Percent ]	 	= 0x7F;
			precedence[ (int)TokenID.Plus ]		 	= 0x7E;
			precedence[ (int)TokenID.Minus ]	 	= 0x7E;
			precedence[ (int)TokenID.ShiftLeft ] 	= 0x7D;
			precedence[ (int)TokenID.ShiftRight] 	= 0x7D;
			precedence[ (int)TokenID.Less ]		 	= 0x7C;
			precedence[ (int)TokenID.Greater ]	 	= 0x7C;
			precedence[ (int)TokenID.LessEqual ] 	= 0x7C;
			precedence[ (int)TokenID.GreaterEqual ]	= 0x7C;
			precedence[ (int)TokenID.EqualEqual ]	= 0x7B;
			precedence[ (int)TokenID.NotEqual ]	 	= 0x7B;
			precedence[ (int)TokenID.BAnd ]		 	= 0x7A;
			precedence[ (int)TokenID.BXor ]		 	= 0x79;
			precedence[ (int)TokenID.BOr ]		 	= 0x78;
			precedence[ (int)TokenID.And]			= 0x77;
			precedence[ (int)TokenID.Or]			= 0x76;


			preprocessor = new SortedList<string, PreprocessorID>();

			preprocessor.Add("define", PreprocessorID.Define);
			preprocessor.Add("undef", PreprocessorID.Undef);
			preprocessor.Add("if", PreprocessorID.If);
			preprocessor.Add("elif", PreprocessorID.Elif);
			preprocessor.Add("else", PreprocessorID.Else);
			preprocessor.Add("endif", PreprocessorID.Endif);
			preprocessor.Add("line", PreprocessorID.Line);
			preprocessor.Add("error", PreprocessorID.Error);
			preprocessor.Add("warning", PreprocessorID.Warning);
			preprocessor.Add("region", PreprocessorID.Region);
			preprocessor.Add("endregion", PreprocessorID.Endregion);
			preprocessor.Add("pragma", PreprocessorID.Pragma);

		}

        #region Name Resolution
        private NameResolutionTable nameTable = new NameResolutionTable();
        private Context currentContext = new Context();

        private void EnterNamespace(string qualifiedName)
        {
            string[] nameParts = qualifiedName.Split(Type.Delimiter);

            for (int i = 0; i < nameParts.Length; i++)
            {
                this.nameTable.AddIdentifier(new NamespaceName(nameParts[i], this.currentContext));
                this.currentContext.Enter(nameParts[i], true);
            }
        }

        private void LeaveNamespace(string qualifiedName)
        {
            int index = -1;

            while ((index = qualifiedName.IndexOf(Type.Delimiter, index + 1)) != -1)
            {
                this.currentContext.Leave();
            }

            this.currentContext.Leave();
        }

        private static NameVisibilityRestriction ToVisibilityRestriction(Modifier modifier)
        {
            Modifier relevantMods = modifier & Modifier.Accessibility;
            NameVisibilityRestriction restriction = NameVisibilityRestriction.Self;

            if ((relevantMods & Modifier.Protected) != Modifier.Empty)
            {
                restriction = NameVisibilityRestriction.Family;
            }

            if (((relevantMods & Modifier.Internal) != Modifier.Empty) ||
                ((relevantMods & Modifier.Public) != Modifier.Empty))
            {
                restriction = NameVisibilityRestriction.Everyone;
            }

            return restriction;
        }
        #endregion
    }
}
