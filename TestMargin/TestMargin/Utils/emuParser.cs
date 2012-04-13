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
        private const string rx_space = @"\x020";
        private const string rx_commentslash = @"//";
        private const string rx_commentstart = @"/\*";
        private const string rx_commentend = @"\*/";
        private const string rx_nonwhite = @"\S";
        private const string rx_blockstart = @"\s*{\s*\r";
        private const string rx_blockend = @"\s*}\s*\r";
        private const string rx_precompile = @"^#";

        private int tab_count = 4;   //how many space a tab

        ITextSnapshot _ts { get; set; }

        List<LineEntity> Roots { set; get; }                  //a tree
        int LineCount { get; set; }

        emuParser(ITextSnapshot ts) 
        {
            this._ts = ts;
            this.LineCount = _ts.LineCount;
        }

        public void BuildTrees()     //build single tree
        {
            Roots = new List<LineEntity>();
            LineEntity root = new LineEntity(0, null, CodeLineType.Normal);        //is 0 origin, comply to textsnapshot
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
                    Roots.Add(blankline);
                    continue;
                }
                if(iIndent == 0)                               //start a new root line, will be parent
                {
                    LineEntity newroot = new LineEntity(tsl.LineNumber, null, linetype);
                    Roots.Add(newroot);
                    currentParent = newroot;
                }
                else
                {
                    if(iIndent == lastDepth)
                    {
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent.Parent, linetype);
                        newchild.Add2Parent();
                    }
                    else if(iIndent - lastDepth == 1)                   //new children level
                    {
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent, linetype);
                        newchild.Add2Parent();
                        currentParent = newchild;
                    }
                    else if(iIndent < lastDepth)
                    {
                        int temp = iIndent;
                        while (temp != lastDepth)
                        {
                            currentParent = currentParent.Parent;
                            --temp;
                        }
                        LineEntity newchild = new LineEntity(tsl.LineNumber, currentParent.Parent, linetype);
                        newchild.Add2Parent();
                        currentParent = newchild;
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
        void GenDispType(int focusPoint) 
        {

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

            return 0;
        }
        #endregion
    }
}
