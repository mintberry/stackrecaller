using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class BlockStatement : StatementNode, IUnsafe
	{
		private bool hasBraces = true;
		public bool HasBraces
		{
			get { if(statements.Count != 1) hasBraces = true; return hasBraces; }
			set { hasBraces = value; }
		}

        private bool isUnsafe = false;
        public bool IsUnsafe
        {
            get
            {
                return isUnsafe;
            }
            set
            {
                isUnsafe = value;
            }
        }

        private bool isUnsafeDeclared = false;
        public bool IsUnsafeDeclared
        {
            get
            {
                return isUnsafeDeclared;
            }
            set
            {
                isUnsafeDeclared = value;
            }
        }

		private NodeCollection<StatementNode> statements = new NodeCollection<StatementNode>();
		public NodeCollection<StatementNode> Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public BlockStatement(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        public BlockStatement(bool isUnsafe, Token relatedtoken) : base(relatedtoken)
        {
            this.isUnsafe = isUnsafe;
        }


        public override void ToSource(StringBuilder sb)
        {
            if (IsUnsafeDeclared)
            {
                sb.Append("unsafe ");
            }

			if (hasBraces)
			{
				sb.Append("{");
				indent++;
				this.NewLine(sb);
			}
			else if(statements.Count == 1)
			{
				// only a case stmt can have more than one stmt without braces, and it special cases this
				AddTab(sb);
			}

			if (statements != null)
			{
				statements.ToSource(sb);
			}

			if (hasBraces)
			{
				indent--;
				this.NewLine(sb);
				sb.Append("}");
			}
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBlockStatement(this, data);
        }

	}
	
}
