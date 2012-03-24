using System;
using System.Collections.Generic;
using System.Text;

namespace Prototype
{

    enum BlockTypes
    {
        Method,
        Property,
        Struct,
        Enum

    };


    class BlockInfo
    {
        public int StartLine;
        public int EndLine;
        public string Name;
        public BlockTypes Type;

        public override string ToString()
        {
            return Name + ": " + StartLine + " - " + EndLine;
        }

        public bool IsWithin(int focus)
        {
            return focus >= StartLine && focus <= EndLine;
        }
    }
}
