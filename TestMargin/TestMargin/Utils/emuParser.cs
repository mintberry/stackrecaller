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
using System.Threading;
using TestMargin.Taggers;

namespace TestMargin.Utils
{
    enum CodeLineType{
        Normal,                        //0
        Blank,
        PreCompile,
        Comment,                       //maybe later use
        BlockStart,                    //
        BlockEnd,                      //
        Special,                        //may fo the questioning indentation code lines
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

        public static int central_offset = 5;                                //for focus area to use. should larger than 0

        public static Semaphore ViewSem = new Semaphore(1, 1);

        ITextSnapshot _ts { get; set; }

        EditorActor _ea { get; set; }

        List<LineEntity> Roots { set; get; }                  //a tree
        public LineEntity [] consLineEntity { get; set; }                                      //for consecutive access
        int LineCount { get; set; }

        int LastFocus { get; set; }                           //last focus line

        public emuParser(ITextSnapshot ts, EditorActor ea) 
        {
            this._ts = ts;
            this._ea = ea;
            this.LineCount = _ts.LineCount;
            this.LastFocus = -1;                       //init with -1

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

        /// <summary>
        /// build tree based on brackets
        /// </summary>
        public void BuildBracketsTree() 
        {

        }

        /// <summary>
        /// find how many tabs are there
        /// </summary>
        /// <param name="tsl"></param>
        /// <returns></returns>
        int GetIndentation(ITextSnapshotLine tsl)
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
        public void GenDispType(int focusPoint, bool bSimplyAgthm = false)  //is not simply method by default
        {
            if(LastFocus == -1)                  //first time gen
            {
                if (bSimplyAgthm)
                    LastFocus = focusPoint;                   //record lastfocus point
                foreach (LineEntity le in Roots)
                {
                    Traverse2SetDispType(le);
                }
            }
            else// for a better performance
            {
                LineEntity commonac = GetCommonAncestor(focusPoint);
                if (commonac != null)
                {
                    int distChange = consLineEntity[focusPoint].LineDepth - consLineEntity[LastFocus].LineDepth;
                    foreach (LineEntity le in consLineEntity)
                    {
                        le.DOI += distChange;
                        // set disptype here also
                        SetDispT(le);
                    }
                    //only need to traverse one 'root' now!
                    Traverse2SetDispType(commonac);
                }
                else //
                {
                    int distChange = consLineEntity[focusPoint].LineDepth - consLineEntity[LastFocus].LineDepth;
                    foreach (LineEntity le in consLineEntity)
                    {
                        le.DOI += distChange;               //the root level nodes dist is unable to be applied
                        // set disptype here also
                        SetDispT(le);
                    }
                }
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
        int GetDistInAST(LineEntity cur, LineEntity dest, bool rootdist = false)//root level nodes dist is 0 for default 
        {
            bool curDeeper = cur.LineDepth > dest.LineDepth;
            int depthdiff = Math.Abs(cur.LineDepth - dest.LineDepth);
            LineEntity curAnc = cur;
            LineEntity destAnc = dest;
            if (curDeeper)
            {
                for (int i = depthdiff; i > 0; --i)
                    curAnc = curAnc.Parent;
            }
            else
            {
                for (int i = depthdiff; i > 0; --i)
                    destAnc = destAnc.Parent;
            }
            if(cur.LineDepth == 0)
            {
                return dest.LineDepth + (rootdist ? 2 : 0);
            }
            else if(dest.LineDepth == 0)
            {
                return cur.LineDepth + (rootdist ? 2 : 0);
            }
            //if neither is root
            int dist = 0;
            //may leave a condition here that two nodes are in a path
            while(curAnc != null && destAnc != null)
            {
                if (curAnc.Equals(destAnc))
                {
                    dist = cur.LineDepth - curAnc.LineDepth + dest.LineDepth - destAnc.LineDepth;
                    return dist;
                }
                curAnc = curAnc.Parent;
                destAnc = destAnc.Parent;
            }
            dist = cur.LineDepth + dest.LineDepth;
            return dist;
        }


        void Traverse2SetDispType(LineEntity root) 
        {
            int thisDOI = makeDOI(consLineEntity[_ea.CentralLine], root);                        //marked for reducing GetCentralLine
            root.DOI = thisDOI;                                                //store root DOI
            //experiment adjustive threshold
            SetDispT(root);
            //threshold = -consLineEntity[_ea.CentralLine].LineDepth + const_threshold ;
            //if (thisDOI < threshold)
            //{
            //    root.DisT = DisplayType.Dismiss;
            //}
            //else 
            //{
            //    root.DisT = DisplayType.Origin;
            //}
            //a simple approach to FOCUS area, better add color background
            if (Math.Abs(_ea.CentralLine - root.LineNumber) < central_offset)
                root.DisT = DisplayType.Focus;
            //for the blank line
            if (root.Type == CodeLineType.Blank)
                root.DisT = DisplayType.Dismiss;

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
        /// renewed method
        /// </summary>
        /// <param name="successor"></param>
        /// <returns></returns>
        LineEntity GetCommonAncestor(int curFocus) 
        {
            LineEntity curfocus = consLineEntity[curFocus];                              //deep clone
            LineEntity lastfocus = consLineEntity[LastFocus];
            // eliminate the situation that two nodes are same
            bool curDeeper = curfocus.LineDepth > lastfocus.LineDepth;
            int depthdiff = Math.Abs(curfocus.LineDepth - lastfocus.LineDepth);
            if (curDeeper)
            {
                for (int i = depthdiff; i > 0; --i)
                    curfocus = curfocus.Parent;
            }
            else 
            {
                for (int i = depthdiff; i > 0; --i)
                    lastfocus = lastfocus.Parent;
            }
            while (lastfocus != null && curfocus != null)
            {
                if (curfocus.Equals(lastfocus))
                {
                    return curfocus;
                }
                lastfocus = lastfocus.Parent;
                curfocus = curfocus.Parent;
            }
            return null; //stands for the virtual root
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

        LineEntity IsAncestor(LineEntity subject, LineEntity ac) 
        {
            if (subject == null)
            {
                return null;
            }
            else 
            {
                if (subject.LineNumber == ac.LineNumber)
                    return subject;
                return IsAncestor(subject.Parent, ac);
            }
        }

        void SetDispT(LineEntity sub) 
        {
            threshold = -consLineEntity[_ea.CentralLine].LineDepth + const_threshold;
            if (sub.DOI < threshold)
            {
                sub.DisT = DisplayType.Dismiss;
            }
            else
            {
                sub.DisT = DisplayType.Origin;
            }
        }

        /// <summary>
        /// reset the parser, for edit support
        /// </summary>
        void ResetParser() 
        {

        }

        public IEnumerable<Region> AggregateRegions(DisplayType aggType) 
        {
            List<Region> dismissRegions = new List<Region>();
            
            LineEntity dismissStart = null;
            LineEntity dismissLast = null;
            foreach (LineEntity le in consLineEntity )
            {
                if (le.DisT == aggType)
                {
                    if (dismissStart == null)
                        dismissStart = le;
                    dismissLast = le;
                }
                else
                {
                    if (dismissStart != null)
                    {
                        Region dismissCont = new Region(dismissStart.LineNumber, dismissLast.LineNumber);
                        dismissRegions.Add(dismissCont);
                        dismissLast = null;
                        dismissStart = null;
                    }

                }
            }
            return dismissRegions;
        }

        public static int ReCalFocusAreaHeight(ITextView tv) 
        {
            if (tv == null)
            {
                return central_offset;
            }
            ITextViewLineCollection tvlc;
            try
            {
                tvlc = tv.TextViewLines;
            }
            catch (InvalidOperationException)
            {
                return central_offset;
            }
            if(tvlc.Count != 0)
            {
                central_offset = tvlc.Count / 4;
            }
            return central_offset;
        }
        #endregion
    }
}
