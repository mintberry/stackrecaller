using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMargin.Utils
{
    enum BlockTypes
    {
        Method,
        Property,
        Struct,
        Enum

    };

    class CodeBlock
    {
        public int startLine;
        public int endLine;
        public string name;
        public BlockTypes type;

        public override string ToString()
        {
            return name + ": " + startLine + " - " + endLine;
        }

        public bool IsWithin(int focus)
        {
            return focus >= startLine && focus <= endLine;
        }
    }
}
