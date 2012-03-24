using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    [System.Diagnostics.DebuggerDisplay("Identifier = {Identifier}")]
	public class IdentifierExpression : PrimaryExpression, IType, IEquatable<IdentifierExpression>
	{
        public IdentifierExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public IdentifierExpression(string identifier, Token relatedToken)
            : base(relatedToken)
		{
			this.identifier = identifier;
		}

		private TokenID startingPredefinedType = TokenID.Invalid;
		public TokenID StartingPredefinedType
		{
			get { return startingPredefinedType; }
			set { startingPredefinedType = value; }
		}
		public bool StartsWithPredefinedType
		{
			get
			{
				return (startingPredefinedType != TokenID.Invalid);
			}
		}

		protected string identifier;
		public string Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

		private List<int> rankSpecifiers = new List<int>();
		public List<int> RankSpecifiers
		{
			get { return rankSpecifiers; }
			set { rankSpecifiers = value; }
		}

        public bool Equals(IdentifierExpression other)
        {
            bool ret = false;

            if (this == other)
            {
                ret = true;
            }
            else
            {
                if (other != null)
                {
                    if ( startingPredefinedType.Equals(other.startingPredefinedType))
                    {
                        if (identifier == other.identifier)
                        {
                            if (rankSpecifiers.Count == other.rankSpecifiers.Count)
                            {
                                ret = true;

                                for (int i = 0; i < rankSpecifiers.Count; ++i)
                                {
                                    if (rankSpecifiers[i] != other.rankSpecifiers[i])
                                    {
                                        ret = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public override bool Equals( object obj )
        {
            bool ret = false;

            if (obj is IdentifierExpression)
            {
                ret = Equals( obj as IdentifierExpression);
            }
            else
            {
                ret = base.Equals(obj);
            }

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public override void ToSource(StringBuilder sb)
		{
            sb.Append(identifier);

            if (rankSpecifiers.Count > 0)
            {
                foreach (int val in rankSpecifiers)
                {
                    sb.Append("[");
                    for (int i = 0; i < val; i++)
                    {
                        sb.Append(",");
                    }
                    sb.Append("]");
                }
            }
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitIdentifierExpression(this, data);
        }

	}
}
