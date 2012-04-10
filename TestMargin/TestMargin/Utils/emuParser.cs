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
        BlockEnd                       //
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
        private const string rx_precompile = @"";

        ITextSnapshot _ts { get; set; }

        List<LineEntity> Roots { set; get; }                  //a tree
        int LineCount { get; set; }

        emuParser(ITextSnapshot ts) 
        {
            this._ts = ts;
            this.LineCount = _ts.LineCount;
        }

        void BuildTree(out LineEntity root)     //build single tree
        {
            root = new LineEntity(0, null, CodeLineType.Normal);        //is 0 origin, comply to textsnapshot
            LineEntity lastLE = root;
            LineEntity currentParent = root;
            foreach (ITextSnapshotLine tsl in _ts.Lines)
            {
                
                if(tsl.LineNumber == 0)
                {
                    continue;
                    //Root = new LineEntity(tsl.LineNumber, null);        //is 0 origin, comply to textsnapshot
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
            tabCount += spaceCount / 4;
            return tabCount;
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
    }
}
