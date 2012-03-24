using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ParamDeclNode : BaseNode
	{
        public ParamDeclNode(Token relatedtoken)
            : base(relatedtoken)
		{
		}
		private Modifier modifiers;
		public Modifier Modifiers
		{
			get { return modifiers; }
			set { modifiers = value; }
		}

		private string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

        private IType type;
		public IType Type
		{
			get { return type; }
			set { type = value; }
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

			type.ToSource(sb);
			sb.Append(" ");

			sb.Append(name);
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitParameterDeclarationExpression(this, data);
        }
	}
}
