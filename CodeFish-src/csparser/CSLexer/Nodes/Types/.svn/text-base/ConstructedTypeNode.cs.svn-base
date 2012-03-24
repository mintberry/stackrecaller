using System.Collections.Generic;
using System.Text;
using System;

using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// this interface is used by all constructed type.
    /// If a node inherits from this class, it does not means that 
    /// the type is a Generic type. It means that the type could be generic.
    /// To test if the type is generic, lookat the property <see cref="IsGeneric"/>
    /// </summary>
    public abstract class ConstructedTypeNode : BaseNode, IGeneric, IPartial, IUnsafe
	{
        public ConstructedTypeNode(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        // Omer - Shouldn't there be an 'Enum' option here as well?
        public enum KindEnum
        {
            Class,
            Struct, 
            Interface,
            Delegate
        }

        protected Modifier modifiers;
        public Modifier Modifiers
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return modifiers; }
            [System.Diagnostics.DebuggerStepThrough]
            set { modifiers = value; }
        }

        protected IdentifierExpression name;
        public IdentifierExpression Name
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return name; }
            [System.Diagnostics.DebuggerStepThrough]
            set { name = value; }
        }

        protected GenericNode generic = null;
        public GenericNode Generic
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return generic;
            }
            [System.Diagnostics.DebuggerStepThrough]
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

        protected KindEnum kind;
        public KindEnum Kind
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return kind;
            }
        }

        PartialCollection partials = null;
        public PartialCollection Partials
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return partials;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                partials = value;
            }
        }

        bool isPartial = false;
        public bool IsPartial
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return isPartial;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                isPartial = value;
            }
        }

        public bool IsStatic
        {
            get
            {
                return ((this.modifiers & Modifier.Static) != Modifier.Empty);
            }
        }

        bool isUnsafe = false;
        public bool IsUnsafe
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return isUnsafe;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                isUnsafe = value;
            }
        }

        private bool isUnsafeDeclared = false;
        public bool IsUnsafeDeclared
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return isUnsafeDeclared;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                isUnsafeDeclared = value;
            }
        }

        public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(kind.ToString().ToLower());
                sb.Append(" ");

                if ( name != null ) name.ToSource(sb);

                if (IsGeneric)
                {
                    sb.Append("<");

                    foreach ( TypeParameterNode item in generic.TypeParameters)
                    {
                        item.ToSource(sb);
                        sb.Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);


                    sb.Remove( sb.Length-1 ,1);
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

                sb.Append(kind.ToString().ToLower());
                sb.Append(" ");

                name.ToSource(sb);

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

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (this.IsGeneric)
            {
                this.Generic.Parent = this;
                this.Generic.Resolve(resolver, false);
            }
        }

    }
}
