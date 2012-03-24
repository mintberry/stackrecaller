using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class CatchNode : StatementNode
	{
        public CatchNode(Token relatedtoken)
            : base(relatedtoken)
        {
            catchBlock = new BlockStatement(relatedtoken);
        }
        private IType classType;
        public IType ClassType
		{
			get { return classType; }
			set { classType = value; }
		}

		private IdentifierExpression identifier;
		public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

		private BlockStatement catchBlock;
		public BlockStatement CatchBlock
		{
			get { return catchBlock; }
			set { catchBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("catch");
			if (classType != null)
			{
				sb.Append("(");
				classType.ToSource(sb);
				sb.Append(")");
			}
			this.NewLine(sb);
			catchBlock.ToSource(sb);            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCatchClause(this, data);
        }

	}
}
