using System;
using System.Collections.Generic;
using System.Text;

using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// type-parameter-list:
    ///     <   type-parameters   >
    /// type-parameters:
    ///     attributesopt   type-parameter
    ///     type-parameters   ,   attributesopt   type-parameter
    /// type-parameter:
    ///     identifier
    /// 
    /// TODO : the typeParameter must be a simple name ... check that there is no namespace as namespae1.namespace2.typeparameter
    /// </summary>    /// 
    public sealed class TypeParameterNode : BaseNode, IEquatable<TypeParameterNode>
	{
        /// <summary>
        /// the type parameter is only an identifier
        /// </summary>
        IdentifierExpression _identifier = null;
        TypeNode _type = null;

        public IdentifierExpression Identifier
        {
            get
            {
                return _identifier;
            }
        }

        public TypeNode Type
        {
            get
            {
                return _type;
            }
        }

        public bool IsEmpty
        {
            get
            {
                bool identEmpty = _identifier == null 
                                    || _identifier.Identifier == null
                                    || _identifier.Identifier == string.Empty;

                bool typeEmpty = _type == null
                                    || _type.Identifier == null
                                    || _type.Identifier.QualifiedIdentifier == string.Empty;;

                return identEmpty && typeEmpty;
            }
        }

        public TypeParameterNode(IdentifierExpression identifier)
            : base(identifier.RelatedToken)
		{
            _identifier = identifier;
		}

        public TypeParameterNode(string identifier, Token relatedToken)
            : base(relatedToken)
		{
            _identifier = new IdentifierExpression(identifier, relatedToken);
		}

        public TypeParameterNode(TypeNode type)
            : base(type.RelatedToken)
        {
            _type = type;
        }

        public TypeParameterNode(Token relatedToken)
            : base(relatedToken)
        {
        }

        public bool Equals(TypeParameterNode other)
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
                    if ( UniqueIdentifier == other.UniqueIdentifier )
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj is TypeParameterNode)
            {
                ret = Equals(obj as TypeParameterNode);
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

        public string UniqueIdentifier
        {
            get
            {
                string ret = string.Empty;

                if (_identifier != null)
                {
                    ret = _identifier.Identifier;
                }
                else
                {
                    if (_type != null)
                    {
                        ret = _type.GenericIndependentIdentifier;
                    }
                }

                return ret;
            }
        }

        public override void ToSource( System.Text.StringBuilder sb )
        {
            if (attributes != null && attributes.Count > 0)
            {
                attributes.ToSource(sb);
            }

            if (_identifier != null)
            {
                _identifier.ToSource(sb);
            }

            if ( _type != null)
            {
                _type.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypeParameter(this, data);
        }
	}
}
