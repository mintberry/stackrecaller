using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMargin.Utils
{
    enum DisplayType {
        Origin,
        Dismiss,
        Minimum,
        HighLit,
        Focus
    }
    class LineEntity: ICloneable
    {
        public int LineNumber { set; get; }
        public int LineDepth { set; get; }                   //root depth is 0

        public CodeLineType Type { set; get; }
        public DisplayType DisT { set; get; }
        public LineEntity Parent { set; get; }               //a tree structure

        public List<LineEntity> Children { get; set; }
        
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
            this.DisT = DisplayType.Origin;
            this.Children = new List<LineEntity>();
        }

        //use this
        public void Add2Parent() 
        {
            if (Parent != null)
                Parent.Children.Add(this);
        }

        #region ICloneable Members


        /// <summary>
        /// this is problematic
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            LineEntity ret = new LineEntity(this.LineNumber, this.Parent, this.Type);
            
            return ret;
            //throw new NotImplementedException();
        }

        #endregion
    }
}
