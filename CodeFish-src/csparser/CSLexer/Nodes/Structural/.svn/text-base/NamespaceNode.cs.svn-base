using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class NamespaceNode : BaseNode
	{
        public NamespaceNode(Token relatedToken) : base(relatedToken)
        {
        }

        private NodeCollection<ExternAliasDirectiveNode> externAliases = new NodeCollection<ExternAliasDirectiveNode>();
        public NodeCollection<ExternAliasDirectiveNode> ExternAliases
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return externAliases; }
            [System.Diagnostics.DebuggerStepThrough]
            set { externAliases = value; }
        }

        private NodeCollection<UsingDirectiveNode> usingDirectives = new NodeCollection<UsingDirectiveNode>();
        public NodeCollection<UsingDirectiveNode> UsingDirectives
        {
            get { return usingDirectives; }
            set { usingDirectives = value; }
        }

		private NodeCollection<NamespaceNode> namespaces;
		public NodeCollection<NamespaceNode> Namespaces
		{
			get { if (namespaces == null) namespaces = new NodeCollection<NamespaceNode>(); return namespaces; }
            [System.Diagnostics.DebuggerStepThrough]
            set { namespaces = value; }
		}
		private NodeCollection<ClassNode> classes = new NodeCollection<ClassNode>();
		public NodeCollection<ClassNode> Classes
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return classes; }
            [System.Diagnostics.DebuggerStepThrough]
            set { classes = value; }
		}

		private NodeCollection<EnumNode> enums = new NodeCollection<EnumNode>();
		public NodeCollection<EnumNode> Enums
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return enums; }
            [System.Diagnostics.DebuggerStepThrough]
            set { enums = value; }
		}

		private NodeCollection<DelegateNode> delegates = new NodeCollection<DelegateNode>();
		public NodeCollection<DelegateNode> Delegates
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return delegates; }
            [System.Diagnostics.DebuggerStepThrough]
            set { delegates = value; }
		}

		private NodeCollection<InterfaceNode> interfaces = new NodeCollection<InterfaceNode>();
		public NodeCollection<InterfaceNode> Interfaces
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return interfaces; }
            [System.Diagnostics.DebuggerStepThrough]
            set { interfaces = value; }
		}

		private NodeCollection<StructNode> structs = new NodeCollection<StructNode>();
		public NodeCollection<StructNode> Structs
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return structs; }
            [System.Diagnostics.DebuggerStepThrough]
            set { structs = value; }
		}


		private QualifiedIdentifierExpression name = null;
        public QualifiedIdentifierExpression Name
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return name; }
            [System.Diagnostics.DebuggerStepThrough]
            set { name = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
			}

			if (name != null)
			{
				sb.Append("namespace ");
				name.ToSource(sb);
				NewLine(sb);
				sb.Append("{");
				indent++;
				NewLine(sb);
			}

            if (ExternAliases.Count > 0)
            {
                foreach (ExternAliasDirectiveNode node in ExternAliases)
                {
                    node.ToSource(sb);
                }
                sb.Append(Environment.NewLine);
            }


            if (usingDirectives.Count > 0)
            {
                foreach (UsingDirectiveNode node in usingDirectives)
                {
                    node.ToSource(sb);
                }
                sb.Append(Environment.NewLine);
            }

			if (namespaces != null)
				namespaces.ToSource(sb);

			if(interfaces != null)
				interfaces.ToSource(sb);

			if (classes != null)
				classes.ToSource(sb);

			if (structs != null)
				structs.ToSource(sb);

			if (delegates != null)
				delegates.ToSource(sb);

			if (enums != null)
				enums.ToSource(sb);

			if (name != null)
			{
				indent--;
				NewLine(sb);
				sb.Append("}");
				NewLine(sb);
			}
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (canEnterContext && this.name != null)
                resolver.Context.Enter(this.name.QualifiedIdentifier, true);

            foreach (UsingDirectiveNode node in this.usingDirectives)
            {
                node.Parent = this;
                resolver.Context.AddUsingDirective(node);
            }

            foreach (ExternAliasDirectiveNode aliasDirectiveNode in this.ExternAliases)
            {
                // TODO: Collect
                aliasDirectiveNode.Parent = this;
                aliasDirectiveNode.Resolve(resolver, false);
            }

            foreach (NamespaceNode namespaceNode in this.Namespaces)
            {
                namespaceNode.Parent = this;
                namespaceNode.Resolve(resolver);
            }

            foreach (ClassNode classNode in this.Classes)
            {
                classNode.Parent = this;
                classNode.Resolve(resolver);
            }

            foreach (DelegateNode delegateNode in this.Delegates)
            {
                delegateNode.Parent = this;
                delegateNode.Resolve(resolver);
            }

            foreach (EnumNode enumNode in this.Enums)
            {
                enumNode.Parent = this;
                enumNode.Resolve(resolver);
            }

            foreach (InterfaceNode interfaceNode in this.Interfaces)
            {
                interfaceNode.Parent = this;
                interfaceNode.Resolve(resolver);
            }

            foreach (StructNode structNode in this.Structs)
            {
                structNode.Parent = this;
                structNode.Resolve(resolver);
            }

            if (canEnterContext && this.name != null)
                resolver.Context.Leave();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitNamespaceDeclaration(this, data);
        }
    }
}
