using System;
using System.Collections.Generic;
using System.Text;

using DDW.Collections;

namespace DDW
{
    public enum PragmaAction { disable, restore }
	public class PPPragmaNode : PPNode
	{
        public PPPragmaNode(Token relatedToken)
            : base(relatedToken)
		{
		}
        public PPPragmaNode(IdentifierExpression identifier, NodeCollection<ConstantExpression> value, PragmaAction action)
            : base(identifier.RelatedToken)
		{
			this.identifier = identifier;
            this.value = value;
            this.action = action;
		}

        private PragmaAction action;
        public PragmaAction Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

		private IdentifierExpression identifier = null;
		public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

        private NodeCollection<ConstantExpression> value = new NodeCollection<ConstantExpression>();
        public NodeCollection<ConstantExpression> Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("#pragma ");
            if (identifier!= null ) identifier.ToSource(sb);
            sb.Append(" ");
            if ( value != null && value.Count > 0) value.ToSource(sb);
            this.NewLine(sb);
        }
	}
}
