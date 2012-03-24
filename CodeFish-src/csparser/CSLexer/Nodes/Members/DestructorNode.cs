using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class DestructorNode : MemberNode
	{
        public DestructorNode(Token relatedtoken)
            : base(relatedtoken)
		{
            statementBlock = new BlockStatement(relatedtoken);
		}
        private BlockStatement statementBlock;
		public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
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

			sb.Append("~");
			this.Names[0].ToSource(sb);
			sb.Append("()");
			this.NewLine(sb);

			statementBlock.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDestructorDeclaration(this, data);
        }

	}
}
