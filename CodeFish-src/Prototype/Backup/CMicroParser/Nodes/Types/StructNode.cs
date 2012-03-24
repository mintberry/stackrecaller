using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class StructNode : ClassNode
	{
        public StructNode(Token relatedToken)
            : base(relatedToken)
        {
            kind = ConstructedTypeNode.KindEnum.Struct;
        }


        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStructNode(this, data);
        }

	}
}
