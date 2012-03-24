using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class CompilationUnitNode : BaseNode
	{
		private NodeCollection<AttributeNode> globalAttributes;
		private NodeCollection<NamespaceNode> namespaces;
		private NamespaceNode defaultNamespace;
        private Names.NameResolutionTable nameTable;

		public CompilationUnitNode(): base(new Token( TokenID.Invalid, 0, 0) )
		{
			this.globalAttributes = new NodeCollection<AttributeNode>();
            this.namespaces = new NodeCollection<NamespaceNode>();

            this.defaultNamespace = new NamespaceNode(RelatedToken);
		}

		public NodeCollection<NamespaceNode> Namespaces
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return this.namespaces; }
            [System.Diagnostics.DebuggerStepThrough]
            set { this.namespaces = value; }
		}

		public NamespaceNode DefaultNamespace
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return this.defaultNamespace; }
		}


        public override void ToSource(StringBuilder sb)
		{
            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
                sb.Append(Environment.NewLine);
			}

            defaultNamespace.ToSource(sb);

            if (namespaces.Count > 0)
            {
                foreach (NamespaceNode node in namespaces)
                {
                    node.ToSource(sb);
                }
                sb.Append(Environment.NewLine);
            }
        }

        public Names.NameResolutionTable NameTable
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.nameTable;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.nameTable = value;
            }
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            this.DefaultNamespace.Parent = this;
            this.DefaultNamespace.Resolve(resolver);

            foreach (NamespaceNode namespaceNode in this.Namespaces)
            {
                namespaceNode.Parent = this;
                namespaceNode.Resolve(resolver);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCompilationUnit(this, data);
        }

    }
}
