using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class EventNode : MemberNode
	{
        public EventNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        //private BlockStatement addBlock = null;
        //public BlockStatement AddBlock
        //{
        //    get { return addBlock; }
        //    set { addBlock = value; }
        //}
        //private BlockStatement removeBlock = null;
        //public BlockStatement RemoveBlock
        //{
        //    get { return removeBlock; }
        //    set { removeBlock = value; }
        //}

        private AccessorNode addBlock = null;
        public AccessorNode AddBlock
        {
            get { return addBlock; }
            set { addBlock = value; }
        }
        private AccessorNode removeBlock = null;
        public AccessorNode RemoveBlock
        {
            get { return removeBlock; }
            set { removeBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            // todo: eventnode to source

            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
			}

            TraceModifiers(this.Modifiers, sb);

            sb.Append("event ");

            this.Type.ToSource(sb);

            sb.Append(" ");

            foreach (QualifiedIdentifierExpression n in Names)
            {
                n.ToSource(sb);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);

            if (this.AddBlock != null
                || this.RemoveBlock != null)
            {
                sb.Append(Environment.NewLine);

                sb.Append("{");
                sb.Append(Environment.NewLine);

                if (AddBlock != null)
                {
                    AddBlock.ToSource(sb);
                }

                sb.Append(Environment.NewLine);

                if (RemoveBlock != null)
                {
                    RemoveBlock.ToSource(sb);
                }

                sb.Append("}");
                sb.Append(Environment.NewLine);
            }
            else
            {
                if (this.Value != null)
                {
                    sb.Append("= ");
                    Value.ToSource(sb);
                }

                sb.Append(";");
            }
            sb.Append(Environment.NewLine);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitEventDeclaration(this, data);
        }

	}
}
