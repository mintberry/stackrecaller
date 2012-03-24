using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class DelegateNode : ConstructedTypeNode
	{
		private IType type;
        public IType Type
		{
			get { return type; }
			set { type = value; }
		}
		
		private NodeCollection<ParamDeclNode> parameters;
		public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

        public DelegateNode(Token relatedToken)
            : base(relatedToken)
        {
            kind = ConstructedTypeNode.KindEnum.Delegate;
        }

        public override string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Type.ToSource(sb);

                sb.Append(" ");

                sb.Append(base.GenericIdentifier);

                return sb.ToString();
            }
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

			sb.Append("delegate ");
			type.ToSource(sb);
			sb.Append(" ");
			name.ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			sb.Append("(");
			if (parameters != null)
			{
				string comma = "";
				foreach (ParamDeclNode pdn in parameters)
				{
					sb.Append(comma);
					comma = ", ";
					pdn.ToSource(sb);
				}
			}
			sb.Append(")");

            if (IsGeneric)
            {
                this.NewLine(sb);
                Generic.ConstraintsToSource(sb);
            }

            sb.Append(";");

			this.NewLine(sb);

		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDelegateDeclaration(this, data);
        }

	}
}