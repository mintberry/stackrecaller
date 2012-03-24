using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class AccessorNode : MemberNode, IIterator
	{
		private string kind;

		public string Kind
		{
			get { return kind; }
			set { kind = value; }
		}

		private bool isAbstractOrInterface = false;
		public bool IsAbstractOrInterface
		{
			get { return isAbstractOrInterface; }
			set { isAbstractOrInterface = value; }
		}

		private BlockStatement statementBlock;
		public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
		}

        private bool couldBeIterator = false;
        public bool CouldBeIterator
        {
            get
            {
                return couldBeIterator;
            }
        }

        bool isIterator = false;
        public bool IsIterator
        {
            get
            {
                return isIterator;
            }

            set
            {
                isIterator = value;
            }
        }

        public AccessorNode(bool couldBeIterator, Token relatedtoken): base(relatedtoken)
        {
            this.couldBeIterator = couldBeIterator;
            statementBlock = new BlockStatement(relatedtoken);
        }



        public override void ToSource(StringBuilder sb)
		{
            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
			}

            TraceModifiers(modifiers, sb);

			sb.Append(Kind);
			if (IsAbstractOrInterface)
			{
				sb.Append(";");
			}
			else
			{
				this.NewLine(sb);
				// statements
				this.StatementBlock.ToSource(sb);
			}
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAccessorNode(this, data);
        }
	}
}
