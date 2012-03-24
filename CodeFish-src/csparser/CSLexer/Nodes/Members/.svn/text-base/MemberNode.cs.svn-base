using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    public abstract class MemberNode : BaseNode, IUnsafe
	{
        public MemberNode(Token relatedToken)
            : base(relatedToken)
        {
        }

		protected Modifier modifiers;
		public Modifier Modifiers
		{
			get { return modifiers; }
			set { modifiers = value; }
		}

        protected List<QualifiedIdentifierExpression> names = new List<QualifiedIdentifierExpression>();
        public List<QualifiedIdentifierExpression> Names
		{
			get { return names; }
			set { names = value; }
		}

		protected IType type;
        public IType Type
		{
			get { return type; }
			set { type = value; }
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

		protected ConstantExpression val;

		public ConstantExpression Value
		{
			get { return val; }
			set { val = value; }
        }
	}
}
