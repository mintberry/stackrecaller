using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

using System.CodeDom;

namespace DDW
{
    [System.Diagnostics.DebuggerDisplay("Identifier = {GenericIdentifier}")]
    public class QualifiedIdentifierExpression : PrimaryExpression, IEquatable<QualifiedIdentifierExpression>, IGeneric
	{
        private bool isNamespaceAliasQualifier = false;
        public bool IsNamespaceAliasQualifier
        {
            get
            {
                return isNamespaceAliasQualifier;
            }
            set
            {
                isNamespaceAliasQualifier = value;
            }
        }

        ExpressionList expressions = new ExpressionList();

        public ExpressionList Expressions
        {
            get
            {
                return expressions;
            }
        }

        public QualifiedIdentifierExpression(Token relatedToken)
            : base(relatedToken)
        {
        }

        public string QualifiedIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < expressions.Expressions.Count; ++i)
                {
                    expressions.Expressions[i].ToSource(sb);

                    if (i==0 && isNamespaceAliasQualifier)
                    {
                        sb.Append("::");
                    }
                    else
                    {
                        sb.Append(".");
                    }
                }

                if ( sb.Length > 0 )
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public bool Equals(QualifiedIdentifierExpression other)
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
                    // does not use QualifiedIdentifier ( like QualifiedIdentifier == other.QualifiedIdentifier ) 
                    // because the following form may be faster : it can stop before reaching the last identifier
                    if (expressions.Expressions.Count != other.expressions.Expressions.Count)
                    {
                        ret = true;

                        for (int i = 0 ; i < expressions.Expressions.Count; ++i)
                        {
                            if (!expressions.Expressions[i].Equals(other.expressions.Expressions[i]))
                            {
                                ret = false;
                                break;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public GenericNode Generic
        {
            get
            {
                ExpressionNode last= expressions.Last;

                if (last != null && last is IGeneric)
                {
                    return ((IGeneric)last).Generic;
                }

                return null;
            }
            set
            {
                ExpressionNode last = expressions.Last;

                if (last != null && last is IGeneric)
                {
                    ((IGeneric)last).Generic = value;
                }
            }
        }

        public bool IsGeneric
        {
            get
            {
                ExpressionNode last= expressions.Last;

                if (last != null && last is IGeneric)
                {
                    return ((IGeneric)last).Generic != null;
                }

                return false;
            }
        }


        public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < expressions.Expressions.Count; ++i)
                {
                    if (expressions.Expressions[i] is IGeneric)
                    {
                        sb.Append(((IGeneric)expressions.Expressions[i]).GenericIdentifier);
                    }
                    else
                    {
                        expressions.Expressions[i].ToSource(sb);
                    }

                    if (i==0 && isNamespaceAliasQualifier)
                    {
                        sb.Append("::");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public string GenericIndependentIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < expressions.Expressions.Count; ++i)
                {
                    if (expressions.Expressions[i] is IGeneric)
                    {
                        sb.Append(((IGeneric)expressions.Expressions[i]).GenericIndependentIdentifier);
                    }
                    else
                    {
                        expressions.Expressions[i].ToSource(sb);
                    }

                    if (i==0 && isNamespaceAliasQualifier)
                    {
                        sb.Append("::");
                    }
                    else
                    {
                        sb.Append(".");
                    }

                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }


        /// <summary>
        /// only check that the last expression is a TypeNode
        /// </summary>
        public bool IsType
        {
            get
            {
                int lastidx = expressions.Expressions.Count-1;

                return lastidx > 0 && expressions.Expressions[lastidx] is IType;
            }
        }

		public override void ToSource(StringBuilder sb)
		{
            sb.Append(QualifiedIdentifier);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitQualifiedIdentifierExpression(this, data);
        }

	}
}
