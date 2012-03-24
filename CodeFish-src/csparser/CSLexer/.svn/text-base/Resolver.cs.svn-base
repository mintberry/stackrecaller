using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDW.Names;

namespace DDW
{
    /// <summary>
    /// Resolves all identifiers of a parse tree.
    /// </summary>
    public partial class Resolver : IResolver
    {
        private List<Assembly> referencedAssemblies;
        private List<CompilationUnitNode> sources;
        private NameResolutionTable nameTable = new NameResolutionTable();
        private Context context;

        #region Constructors
        public Resolver(IEnumerable<CompilationUnitNode> sources, IEnumerable<Assembly> referencedAssemblies)
        {
            if (sources == null)
            {
                throw new ArgumentNullException("sources");
            }

            this.sources = new List<CompilationUnitNode>(sources);
            this.referencedAssemblies = new List<Assembly>(referencedAssemblies);
        }

        public Resolver(IEnumerable<CompilationUnitNode> sources)
            : this(sources, new Assembly[0])
        {
        }

        public Resolver(params CompilationUnitNode[] sources)
            : this((IEnumerable<CompilationUnitNode>)sources)
        {
        }

        public Resolver()
            : this(new CompilationUnitNode[0])
        {
        }
        #endregion

        #region Properties
        public List<Assembly> ReferencedAssemblies
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.referencedAssemblies;
            }
        }

        public List<CompilationUnitNode> IncludedSources
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.sources;
            }
        }
        #endregion

        public void Resolve()
        {
            this.MergeNameTables();

            foreach (CompilationUnitNode node in this.sources)
            {
                this.context = new Context();
                node.Resolve(this, true);
            }

            // TODO: Add more passes.
        }

        private void MergeNameTables()
        {
            foreach (CompilationUnitNode source in this.sources)
            {
                this.MergeNameTables(source);
            }
        }

        private void MergeNameTables(CompilationUnitNode source)
        {
            foreach (IdentifierName identifierName in source.NameTable)
            {
                // TODO: Check for duplicates.
                this.nameTable.AddIdentifier(identifierName);
            }
        }

        IdentifiedTypeNode IResolver.IdentifyType(TypeNode node)
        {
            if (node is PredefinedTypeNode)
            {
                return new IdentifiedExternalTypeNode(((PredefinedTypeNode)node).GetRealType(), node.RelatedToken);
            }
            else
            {
                return IdentifyType(node.GenericIdentifier);
            }
        }

        private IdentifiedTypeNode IdentifyType(string identifier)
        {
            // Glossary:
            //
            // Identifier - The text found in the source.
            // Name - The last part of the identifier.
            // Chain - The current context's type/namespace chain.
            string strippedIdentifier = identifier;

            while (strippedIdentifier.EndsWith(">") || strippedIdentifier.EndsWith("]"))
            {
                Stack<char> brackets = new Stack<char>();

                for (int i = strippedIdentifier.Length - 1; i >= 0; i--)
                {
                    switch (strippedIdentifier[i])
                    {
                        case '>': case ']':
                            brackets.Push(strippedIdentifier[i]);
                            break;
                        case '<':
                            if (brackets.Pop() != '>')
                                throw new InvalidOperationException("Invalid identifier name " + identifier);
                            break;
                        case '[':
                            if (brackets.Pop() != ']')
                                throw new InvalidOperationException("Invalid identifier name " + identifier);
                            break;
                        default:
                            break;
                    }

                    if (brackets.Count == 0)
                    {
                        strippedIdentifier = strippedIdentifier.Substring(0, i);
                        break;
                    }
                }
            }

            // Maybe the parent is a type and we'll be able to find it
            IdentifiedTypeNode parentTypeNode;

            if (strippedIdentifier.LastIndexOf(Type.Delimiter) != -1)
            {
                string parent = strippedIdentifier.Substring(0, strippedIdentifier.LastIndexOf(Type.Delimiter));

                parentTypeNode = this.IdentifyType(parent);
            }

            // The possibilities we have here are (by order of trumping):
            // ---------------------------------------------------------
            // 1. The name exists in the current context (and might be prefixed by the levels above the current context).
            // 2. When going up the context levels (all the way to the root):
            //    2.1. The name exists in the context (and might be prefixed by the levels above that context).
            //    2.2. The identifier is only the name and the name exists in a namespace specified by a using clause.
            //    2.3. The identifier is or starts with an alias. Replace the alias with the real deal and start over.
            // 3. The identifier can not be currently resolved.

            // Notes:
            // - Local references are preferrable to external references. Having both is allowed.
            // - Nested classes are preferrable to namespaces.
            // - Handle generics. The generic identifier will give us either Namespace.Type or Namespace.Type<identifier, identifier>.
            //   - If the identifier sans name has <>, it's a nested class.
            // - Handle visibility.

            return null;
        }

        Context IResolver.Context
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.context;
            }
        }

        void IResolver.Collect(IdentifierName name)
        {
            this.nameTable.AddIdentifier(name);
        }
    }

    public interface IResolver
    {
        Context Context { get; }
        IdentifiedTypeNode IdentifyType(TypeNode node);
        void Collect(IdentifierName name);
    }
}