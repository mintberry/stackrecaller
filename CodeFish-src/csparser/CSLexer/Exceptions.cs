using System;

namespace DDW
{
    [Serializable]
    public class UnresolvableIdentifierException : Exception
    {
        private string identifier;

        public UnresolvableIdentifierException(string identifier)
        {
            this.identifier = identifier;
        }

        public string Identifier
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.identifier;
            }
        }
    }
}
