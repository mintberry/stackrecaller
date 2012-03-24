using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class InterfaceMethodNode : MemberNode, IGeneric
	{
        public InterfaceMethodNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private NodeCollection<ParamDeclNode> parameters;
		public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

        GenericNode generic = null;
        public GenericNode Generic
        {
            get
            {
                return generic;
            }
            set
            {
                generic = value;
            }
        }

        public bool IsGeneric
        {
            get
            {
                return this.Generic != null;
            }
        }

        public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                this.Type.ToSource(sb);

                sb.Append(" ");

                this.names[0].ToSource(sb);

                if (IsGeneric)
                {
                    sb.Append("<");

                    foreach ( TypeParameterNode item in generic.TypeParameters)
                    {
                        item.ToSource(sb);
                        sb.Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);

                    sb.Append(">");
                }

                return sb.ToString();
            }
        }

        public virtual string GenericIndependentIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                this.Type.ToSource(sb);

                sb.Append(" ");

                this.names[0].ToSource(sb);

                if (IsGeneric)
                {
                    sb.Append("<");

                    if (generic.TypeParameters.Count > 1)
                    {
                        sb.Append(',', generic.TypeParameters.Count - 1);
                    }

                    sb.Append(">");
                }

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
			this.TraceModifiers(this.Modifiers, sb);

			this.type.ToSource(sb);
			sb.Append(" ");

			this.names[0].ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			sb.Append("(");

			if (parameters != null)
			{
				string comma = "";
				for (int i = 0; i < parameters.Count; i++)
				{
					sb.Append(comma);
					comma = ", ";
					parameters[i].ToSource(sb);
				}
			}

			sb.Append(")");

            if (IsGeneric)
            {
                this.NewLine(sb);
                Generic.ConstraintsToSource(sb);
            }

            sb.Append(";");

		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceMethodNode(this, data);
        }

	}
}
