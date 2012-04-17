using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.VCCodeModel;

namespace TestMargin.Utils
{
    enum CodeLineType{
        Normal,
        Blank,
        PreCompile,
        Comment,                       //maybe later use
        BlockStart,                    //
        BlockEnd,                      //
        Special                        //may fo the questioning indentation code lines
    }

    internal sealed class emuParser
    {
        //Regexs
        private const string rx_tab = @"\t";
        private const string rx_space = @" ";
        private const string rx_commentslash = @"//";
        private const string rx_commentstart = @"/\*";
        private const string rx_commentend = @"\*/";
        private const string rx_nonwhite = @"\S";
        private const string rx_blockstart = @"\s*{\s*\r";
        private const string rx_blockend = @"\s*}\s*\r";
        private const string rx_precompile = @"^#";

        private int tab_count = 4;   //how many space a tab
        private int threshold = -3;  //DOI threshold
        private const int const_threshold = -2;

        ITextSnapshot _ts { get; set; }

        EditorActor _ea { get; set; }

        List<LineEntity> Roots { set; get; }                  //a tree
        public LineEntity [] consLineEntity { get; set; }                                      //for consecutive access
        int LineCount { get; set; }

        public emuParser(ITextSnapshot ts, EditorActor ea) 
        {
            this._ts = ts;
            this._ea = ea;
            this.LineCount = _ts.LineCount;

            Roots = new List<LineEntity>();
            consLineEntity = new LineEntity[this.LineCount];
        }

        public void BuildTrees()     //build single tree
        {
            
            LineEntity root = new LineEntity(0, null, CodeLineType.Normal);        //is 0 origin, comply to textsnapshot
            this.Add2TreeandArray(root);

            LineEntity lastLE = root;
            LineEntity currentParent = root;                  //current LineEntity
            foreach (ITextSnapshotLine tsl in _ts.Lines)
            {
                if(tsl.LineNumber == 0)
                {
                    continue;
                    //Root = new LineEntity(tsl.LineNumber, null);        //is 0 origin, comply to textsnapshot
                }
                CodeLineType linetype = CurrentLineType(tsl);
                int iIndent = GetIndentation(tsl);
                int lastDepth = currentParent.LineDepth;
                //LineEntity thisline = new LineEntity(tsl.LineNumber, currentParent, linetype);
                //
                //if this line is blank, regonized as root line,
                //but will not be parent, parent will be last one
                if (linetype == CodeLineType.Blank)          
                {
                    LineEntity blankline = new LineEntity(tsl.LineNumber, null, linetype);
                    blankline.DisT = DisplayType.Dismiss;       //do not display code line
                    this.Add2TreeandArray(blankline);
                    continue;
                }
                if(iIndent == 0)                               //start a new root line, will be parent
                {
                    LineEntity newroot = new LineEntity(tsl.LineNumber, null, linetype);
                    this.Add2TreeandArray(newroot);
                    currentParent = consLineEntity[newroot.LineNumber];
                }
                else
                {
                    if(iIndent == lastDepth)
                    {
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent.Parent, linetype);
                        this.Add2TreeandArray(newchild);
                        //change current parent here?
                    }
                    else if(iIndent - lastDepth == 1)                   //new children level
                    {
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent, linetype);
                        this.Add2TreeandArray(newchild);
                        currentParent = consLineEntity[newchild.LineNumber];
                    }
                    else if(iIndent < lastDepth)
                    {
                        int temp = iIndent;
                        while (temp != lastDepth)
                        {
                            currentParent = currentParent.Parent;
                            ++temp;
                        }
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent.Parent, linetype);
                        this.Add2TreeandArray(newchild);
                        currentParent = consLineEntity[newchild.LineNumber];
                    }
                    //things we do not wish to see, a hard code work around for now
                    //treated as first level child
                    else if(iIndent - lastDepth > 1) 
                    {
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent, linetype);
                        this.Add2TreeandArray(newchild);
                        currentParent = consLineEntity[newchild.LineNumber];
                    }

                }
                
            }
        }

        int GetIndentation(ITextSnapshotLine tsl)                  //find how many tabs are there
        {
            if(tsl == null)
            {
                throw new NullReferenceException();
            }
            string originStr = tsl.GetText();
            //position of first non-white
            int firstNonWhite = Regex.Match(originStr, rx_nonwhite).Index;
            string whiteString = originStr.Substring(0, firstNonWhite);

            int tabCount = Regex.Matches(whiteString, rx_tab).Count;
            int spaceCount = Regex.Matches(whiteString, rx_space).Count;
            tabCount += spaceCount / tab_count;
            return tabCount;
        }

        
        /// <summary>
        /// generate display type for each code line
        /// </summary>
        /// <param name="focusPoint">the central point of textview</param>
        public void GenDispType(int focusPoint) 
        {
            foreach(LineEntity le in Roots)
            {
                Traverse2SetDispType(le);
            }
        }

        CodeLineType CurrentLineType(ITextSnapshotLine tsl)
        {
            if(tsl == null)
            {
                throw new NullReferenceException();
            }
            string originStr = tsl.GetText();
            //more types to be added, currently only normal or blank
            if(Regex.IsMatch(originStr, rx_nonwhite)) 
            {
                //add more types here
                return CodeLineType.Normal;
            }
            else return CodeLineType.Blank;
        }

        #region Helpers
        /// <summary>
        /// get the distance from current focus(central line) to dest line
        /// </summary>
        /// <param name="cur">focus line</param>
        /// <param name="dest">dest line</param>
        /// <returns></returns>
        int GetDistInAST(LineEntity cur, LineEntity dest) 
        {
            if(cur.LineDepth == 0)
            {
                return dest.LineDepth;
            }
            else if(dest.LineDepth == 0)
            {
                return cur.LineDepth;
            }

            //if neither is root
            LineEntity curAnc = cur;                         //!may use deep clone
            LineEntity destAnc = dest;
            int dist = 0;
            while(curAnc.Parent != null && destAnc != null)
            {
                curAnc = curAnc.Parent;
                destAnc = destAnc.Parent;
                if (curAnc.Equals(destAnc))
                {
                    dist = 2 * (cur.LineDepth - curAnc.LineDepth);
                    return dist;
                }
            }
            dist = cur.LineDepth + dest.LineDepth;
            return dist;
        }


        void Traverse2SetDispType(LineEntity root) 
        {
            int thisDOI = makeDOI(consLineEntity[_ea.GetCentralLine()], root);
            //experiment adjustive threshold
            threshold = -consLineEntity[_ea.CentralLine].LineDepth + const_threshold ;
            if (thisDOI < threshold)
            {
                root.DisT = DisplayType.Dismiss;
            }
            else 
            {
                root.DisT = DisplayType.Origin;
            }
            
            //if not leaf
            if(root.Children.Count != 0)
            {
                foreach(LineEntity child in root.Children)
                {
                    Traverse2SetDispType(child);
                }
            }
        }


        /// <summary>
        /// make a more adjustive DOI algorithm
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        int makeDOI(LineEntity cur, LineEntity dest) 
        {
            //another hard code fix, a line 549 bug, 
            //if (cur == null || dest == null) return -1;
            if (cur.Equals(dest)) return 0;
            int dist = GetDistInAST(cur, dest);
            return (-dest.LineDepth - dist);                              //here can be more complext formular
        }

        void Add2TreeandArray(LineEntity tobeadded) 
        {
            consLineEntity[tobeadded.LineNumber] = tobeadded;
            if (tobeadded.Parent == null)
                Roots.Add(tobeadded);
            else
                tobeadded.Add2Parent();
        }

        /// <summary>
        /// currently deprecated
        /// </summary>
        /// <param name="successor"></param>
        /// <returns></returns>
        LineEntity GetAncestor(LineEntity successor) 
        {
            LineEntity ancestor = successor;                              //deep clone
            while(ancestor.Parent != null)
            {
                ancestor = ancestor.Parent;
            }
            return ancestor;
        }

        /// <summary>
        /// get text extent of specific line, excluding the space & tabs
        /// </summary>
        /// <param name="View"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public static SnapshotSpan? GetTextSpanFromLineNumber(ITextView view, int lineNumber)
        {
            SnapshotSpan? originSpan = view.TextSnapshot.GetLineFromLineNumber(lineNumber).Extent;
            if (originSpan.HasValue)
            {
                int firstNonWhite = Regex.Match(originSpan.Value.GetText(), rx_nonwhite).Index;
                return new SnapshotSpan(originSpan.Value.Start + firstNonWhite, originSpan.Value.End);
            }
            else
                return null;
            
        }
        #endregion
    }
}
