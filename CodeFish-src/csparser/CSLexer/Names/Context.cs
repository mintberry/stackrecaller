using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace DDW.Names
{
    public class Context
    {
        #region Internal implementation
        private abstract class Level
        {
            private string name;

            protected Level(string name)
            {
                this.name = name;
            }

            public string Name
            {
                [DebuggerStepThrough]
                get
                {
                    return this.name;
                }
            }
        }

        private class TypeLevel : Level
        {
            public TypeLevel(string name)
                : base(name)
            {
            }
        }

        private class NamespaceLevel : Level
        {
            public static readonly NamespaceLevel Root = new NamespaceLevel(string.Empty);

            private List<UsingDirectiveNode> usingDirectives;

            public NamespaceLevel(string name)
                : base(name)
            {
            }

            public List<UsingDirectiveNode> UsingDirectives
            {
                [DebuggerStepThrough]
                get
                {
                    if (this.usingDirectives == null)
                    {
                        usingDirectives = new List<UsingDirectiveNode>();
                    }

                    return this.usingDirectives;
                }
            }
        }
        #endregion

        private Stack<Level> levels = new Stack<Level>();

        /// <summary>
        /// Initializes a new instance of the Context class.
        /// </summary>
        public Context()
        {
            this.levels.Push(NamespaceLevel.Root);
        }

        public void Enter(string name, bool isNamespace)
        {
            if (isNamespace)
            {
                this.levels.Push(new NamespaceLevel(name));
            }
            else
            {
                this.levels.Push(new TypeLevel(name));
            }
        }

        /// <summary>
        /// Adds a using directive for the current context.
        /// </summary>
        public void AddUsingDirective(UsingDirectiveNode node)
        {
            NamespaceLevel currentLevel = this.levels.Peek() as NamespaceLevel;

            if (currentLevel == null)
                throw new InvalidOperationException("Can not add a using directive in a type.");

            currentLevel.UsingDirectives.Add(node);
        }

        public void Leave()
        {
            this.levels.Pop();
        }

        public string[] GetContext()
        {
            return this.GetContext(false);
        }

        public string[] GetContext(bool ignoreTypes)
        {
            Level[] levelsArray = this.levels.ToArray();
            string[] names = null;

            if (ignoreTypes)
            {
                for (int i = levelsArray.Length - 1; i >= 0; i--)
                {
                    if (levelsArray[i] is NamespaceLevel)
                    {
                        names = new string[i + 1];
                        break;
                    }
                }

                if (names == null)
                    names = new string[0];
            }
            else
            {
                names = new string[levelsArray.Length - 1];
            }

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = levelsArray[i].Name;
            }

            return names;
        }

        public IEnumerable<UsingDirectiveNode> GetAllUsingDirectives()
        {
            Stack<Level> backup = new Stack<Level>();

            while (this.levels.Count > 0)
            {
                Level currentLevel = this.levels.Pop();
                backup.Push(currentLevel);

                if (currentLevel is NamespaceLevel)
                {
                    foreach (UsingDirectiveNode node in ((NamespaceLevel)(currentLevel)).UsingDirectives)
                    {
                        yield return node;
                    }
                }
            }

            while (backup.Count > 0)
                this.levels.Push(backup.Pop());
        }
    }
}
