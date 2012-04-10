using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMargin.Utils
{
    class LineEntity
    {
        int LineNumber { set; get; }
        int LineDepth { set; get; }                   //root depth is 0

        CodeLineType Type { set; get; }
        LineEntity Parent { set; get; }               //a tree structure

        List<LineEntity> Children { get; set; }
        
        public LineEntity(int lineNumber, LineEntity parent, CodeLineType type)
        {
            this.LineNumber = lineNumber;
            if (parent != null)
            {
                this.Parent = parent;
                this.LineDepth = parent.LineDepth + 1;
            }
            else                                                  //is root
            {
                this.Parent = null;
                this.LineDepth = 0;
            }
            this.Type = type;
            this.Children = new List<LineEntity>();
        }

        void Add2Parent() 
        {
            if (Parent != null)
                Parent.Children.Add(this);
        }
    }
}
