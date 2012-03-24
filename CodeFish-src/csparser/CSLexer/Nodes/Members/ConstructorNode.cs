using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ConstructorNode : MemberNode
	{
        public ConstructorNode(Token relatedtoken)
            : base(relatedtoken)
        {
            statementBlock = new BlockStatement(relatedtoken);
        }
		private bool hasThis;
		public bool HasThis
		{
			get { return hasThis; }
			set { hasThis = value; }
		}

		private bool hasBase;
		public bool HasBase
		{
			get { return hasBase; }
			set { hasBase = value; }
		}

		private NodeCollection<ArgumentNode> thisBaseArgs;
		public NodeCollection<ArgumentNode> ThisBaseArgs
		{
			get { return thisBaseArgs; }
			set { thisBaseArgs = value; }
		}

		private NodeCollection<ParamDeclNode> parameters;
		public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

		private BlockStatement statementBlock;
		public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
		}

		private bool isStaticConstructor = false;
		public bool IsStaticConstructor
		{
			get { return isStaticConstructor; }
			set { isStaticConstructor = value; }
		}

        public override void ToSource(StringBuilder sb)
		{
            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
			}
			this.TraceModifiers(this.Modifiers, sb);

			if (isStaticConstructor)
			{
				sb.Append("static ");
			}

			this.Names[0].ToSource(sb);
			sb.Append("(");

			string comma = "";
			if (parameters != null)
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					sb.Append(comma);
					comma = ", ";
					parameters[i].ToSource(sb);
				}
			}
			sb.Append(")");

			// possible :this or :base
			if (hasBase)
			{
				sb.Append(" : base(");
			}
			else if (hasThis)
			{
				sb.Append(" : this(");
			}
			if (hasBase || hasThis)
			{
				if (thisBaseArgs != null)
				{
					comma = "";
					for (int i = 0; i < thisBaseArgs.Count; i++)
					{
						sb.Append(comma);
						comma = ", ";
						thisBaseArgs[i].ToSource(sb);
					}
				}
				sb.Append(")");
			}

			// start block
			this.NewLine(sb);
			
			statementBlock.ToSource(sb);
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstructorDeclaration(this, data);
        }

	
	}
}
