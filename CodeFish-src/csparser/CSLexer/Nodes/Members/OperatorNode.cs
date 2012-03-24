using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    public class OperatorNode : MemberNode, IIterator
	{
        public OperatorNode(Token relatedtoken)
            : base(relatedtoken)
		{
            statements = new BlockStatement(relatedtoken);
		}


		private TokenID op;
		public TokenID Operator
		{
			get { return op; }
			set { op = value; }
		}
		
		private bool isExplicit;
		public bool IsExplicit
		{
			get { return isExplicit; }
			set { isExplicit = value; }
		}
		private bool isImplicit;
		public bool IsImplicit
		{
			get { return isImplicit; }
			set { isImplicit = value; }
		}

		private ParamDeclNode param1;
		public ParamDeclNode Param1
		{
			get { return param1; }
			set { param1 = value; }
		}
		private ParamDeclNode param2;
		public ParamDeclNode Param2
		{
			get { return param2; }
			set { param2 = value; }
		}

		private BlockStatement statements ;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
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
        

        public OperatorNode(bool couldBeIterator, Token relatedtoken) :base(relatedtoken)
        {
            this.couldBeIterator = couldBeIterator;
            statements = new BlockStatement(relatedtoken);
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

			if (isExplicit)
			{
				sb.Append("explicit operator ");
				type.ToSource(sb);
			}
			else if (isImplicit)
			{
				sb.Append("implicit operator ");
				type.ToSource(sb);
			}
			else
			{
				type.ToSource(sb);
				sb.Append("operator " + op + " ");
			}

			sb.Append("(");
			if (param1 != null)
			{
				param1.ToSource(sb);
			}
			if (param2 != null)
			{
				sb.Append(", ");
				param2.ToSource(sb);
			}
			sb.Append(")");
			this.NewLine(sb);

			if (statements != null)
			{
				statements.ToSource(sb);
			}
			else
			{
				sb.Append("{}");
			}

        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitOperatorDeclaration(this, data);
        }
	}
}
