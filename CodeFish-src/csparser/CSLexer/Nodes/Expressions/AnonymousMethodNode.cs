using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class AnonymousMethodNode : ExpressionNode
	{
        public AnonymousMethodNode(Token relatedtoken)
            : base(relatedtoken)
		{
            statementBlock = new BlockStatement(relatedtoken);
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
		}

		public override void ToSource(StringBuilder sb)
		{			
			sb.Append("delegate ");

			if (parameters != null)
			{
                sb.Append("( ");
				string comma = "";
				for (int i = 0; i < parameters.Count; i++)
				{
					sb.Append(comma);
					comma = ", ";
					parameters[i].ToSource(sb);
				}

                sb.Append(") ");
			}

			this.NewLine(sb);

			statementBlock.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAnonymousMethodExpression(this, data);
        }

	}
}
