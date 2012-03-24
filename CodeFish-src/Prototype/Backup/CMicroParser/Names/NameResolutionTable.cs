using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Names
{
    public class NameResolutionTable : IEnumerable<IdentifierName>
    {
        private Dictionary<string, List<IdentifierName>> names =
            new Dictionary<string, List<IdentifierName>>();

        public void AddIdentifier(IdentifierName item)
        {
            if (!this.names.ContainsKey(item.FullyQualifiedName[0]))
            {
                this.names.Add(item.FullyQualifiedName[0], new List<IdentifierName>());
            }

            this.names[item.FullyQualifiedName[0]].Add(item);
        }

        public IEnumerable<T> GetMatches<T>(string name) where T : IdentifierName
        {
            if (this.names.ContainsKey(name))
            {
                foreach (IdentifierName identifierName in this.names[name])
                {
                    if (identifierName is T)
                    {
                        yield return (T)identifierName;
                    }
                }
            }
        }

        IEnumerator<IdentifierName> IEnumerable<IdentifierName>.GetEnumerator()
        {
            foreach (KeyValuePair<string, List<IdentifierName>> pair in this.names)
            {
                foreach (IdentifierName identifierName in pair.Value)
                {
                    yield return identifierName;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IdentifierName>)this).GetEnumerator();
        }
    }
}
