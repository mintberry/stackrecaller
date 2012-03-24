using System;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using DDW;
using DDW.Collections;
using System.Text.RegularExpressions;

namespace Prototype
{
    class Model
    {
        public delegate void ModelChangedDelegate();
        public event ModelChangedDelegate OnModelChanged;
        
        static private Model model = null;

        int _maxNumberLength;

        // Document
        private Token[][] _tokenizedDocument;
        private String[] _document = new String[] { };
        private CompilationUnitNode _parsedDocument;
        private string _documentFile;
        private TokenCollection _lexedDocument;
        private bool[] _emptyLines;
        private int[] _indentation;
        private List<BlockInfo> _blockInfos;
        private BlockInfo[] _blockInfoMapping;

        internal BlockInfo[] BlockInfoMapping
        {
            get { return _blockInfoMapping; }
            set { _blockInfoMapping = value; }
        }
        private bool[] _skips;

        // Focus
        private int _focusRadius = 10;
        private FocusArea _focus = new FocusArea(0, 0, 0);

        //strategies
        private IDOIStrategy _DOIStrategy = null;
        private IRenderStrategy _renderStrategy = new FisheyeRender();

        private int _selectedLine;


        #region Properties

        public bool[] Skips
        {
            get { return _skips; }
            set { _skips = value; }
        }


        public List<BlockInfo> BlockInfos
        {
            get { return _blockInfos; }
            set { _blockInfos = value; }
        }


        public int SelectedLine
        {
            get { return _selectedLine; }
            set { _selectedLine = value; }
        }

        public int[] Indentation
        {
            get { return _indentation; }
        }

        public FocusArea Focus
        {
            get { return _focus; }
        }

        public int FocusCenter
        {
            get { return _focus.Center; }
        }

        public void UpdateFocus(int center, bool animateTransition)
        {
            _focus = new FocusArea(center, center - _focusRadius, center + _focusRadius);
            _focus = _focus.Clip(Model.Default.Document.Length);

            DOIStrategy.OnFocusChanged();
            View.Default.UpdateLayout(animateTransition);
        }

        /*public bool[] Skips
        {
            get { return _skips; }
        }*/

        public TokenCollection LexedDocument
        {
            get { return _lexedDocument; }
        }


        public int Length
        {
            get { return _document.Length; }
        }


        public int FocusAreaRadius
        {
            get { return _focusRadius; }
            set
            {
                _focusRadius = value;
                UpdateFocus(Focus.Center, false);
                ModelChanged();
            }
        }

        public int MaxNumberLength
        {
            get { return _maxNumberLength; }
        }

        public CompilationUnitNode ParsedDocument
        {
            get { return _parsedDocument; }
        }

        public Token[][] TokenizedDocument
        {
            get { return _tokenizedDocument; }
        }

        public String[] Document
        {
            get { return _document; }
        }

        public string DocumentFile
        {
            get { return _documentFile; }
            set
            {
                ResetSearch();

                _documentFile = value;
                _parsedDocument = Utilities.ParseDocument(value);
                _lexedDocument = Utilities.LexDocument(value);
                _document = Utilities.FileToStringArray(value);
                _emptyLines = Utilities.CreateSkipList(_document);


                CreateAlmostSkipList();
                CreateIndentationList();
                CreateBlockInfos();
                CreateBlockInfoMappings();

                _maxNumberLength = (int)Math.Log10((double)_document.Length) + 1;

                _tokenizedDocument = new Token[_document.Length][];
                for (int i = 0; i < _document.Length; i++)
                    _tokenizedDocument[i] = Highlighter.TokenizeLine(_document[i]);

                if (_DOIStrategy == null)
                    DOIStrategy = new StaticDOI();

                ResetSearch();

                DOIStrategy.OnModelChanged();

                View.Default.ClearLayout();

                UpdateFocus(0, false);

                ModelChanged();
            }
        }



        public IDOIStrategy DOIStrategy
        {
            get { return _DOIStrategy; }
            set
            {
                _DOIStrategy = value;
                ModelChanged();
                Model.Default.UpdateFocus(Focus.Center, false);
            }
        }

        public IRenderStrategy RenderStrategy
        {
            get { return _renderStrategy; }
            set
            {
                _renderStrategy = value;
                ModelChanged();
                Model.Default.UpdateFocus(Focus.Center, false);
            }
        }
        #endregion

        private void CreateIndentationList()
        {
            _indentation = new int[Model.Default.Length];
            int prevIndentation = 0;
            for (int i = 0; i < Model.Default.Length; i++)
            {
                if (_emptyLines[i])
                    _indentation[i] = prevIndentation;
                else
                {
                    int indentation = Model.Default.Document[i].Length - Model.Default.Document[i].TrimStart().Length;
                    _indentation[i] = indentation;
                    prevIndentation = indentation;
                }
            }
        }

        private void ModelChanged()
        {
            if (OnModelChanged != null)
            {
                OnModelChanged();
            }

            if (DOIStrategy != null)
                DOIStrategy.OnModelChanged();
        }

        public void Reset()
        {
            DOIStrategy.Reset();
            ModelChanged();
        }

        private BlockInfo FindMethodInfo(int i)
        {
            foreach (BlockInfo mi in Model.Default.BlockInfos)
            {
                if (mi.Type == BlockTypes.Method && mi.IsWithin(i))
                    return mi;
            }
            return null;
        }

        private void CreateBlockInfoMappings()
        {
            _blockInfoMapping = new BlockInfo[Length];

            for (int i = 0; i < Length; i++)
            {
                _blockInfoMapping[i] = FindMethodInfo(i);
            }
        }

        private List<BlockInfo> CreateBlockInfos()
        {
            Dictionary<string, BlockInfo> nameTable = new Dictionary<string, BlockInfo>();

            foreach (NamespaceNode nsn in Model.Default.ParsedDocument.Namespaces)
            {
                foreach (ClassNode cn in nsn.Classes)
                {
                    foreach (MethodNode mn in cn.Methods)
                    {

                        string name = mn.Names[0].GenericIdentifier;
                        int line = mn.RelatedToken.Line;
                        BlockInfo bi = new BlockInfo();
                        bi.StartLine = line;
                        bi.Type = BlockTypes.Method;
                        

                        if (!nameTable.ContainsKey(name))
                            nameTable.Add(name, bi);
                    }
                    foreach (PropertyNode pn in cn.Properties)
                    {
                        string name = pn.Names[0].GenericIdentifier;
                        int line = pn.RelatedToken.Line;
                        BlockInfo bi = new BlockInfo();
                        bi.StartLine = line;
                        bi.Type = BlockTypes.Property;


                        if (!nameTable.ContainsKey(name))
                            nameTable.Add(name, bi);
                    }

                    foreach (StructNode sn in cn.Structs)
                    {
                        string name = sn.Name.Identifier;
                        int line = sn.RelatedToken.Line;
                        BlockInfo bi = new BlockInfo();
                        bi.StartLine = line;
                        bi.Type = BlockTypes.Struct;


                        if (!nameTable.ContainsKey(name))
                            nameTable.Add(name, bi);
                    }

                    foreach (EnumNode en in cn.Enums)
                    {
                        string name = en.Name.Identifier;
                        int line = en.RelatedToken.Line;
                        BlockInfo bi = new BlockInfo();
                        bi.StartLine = line;
                        bi.Type = BlockTypes.Enum;


                        if (!nameTable.ContainsKey(name))
                            nameTable.Add(name, bi);
                    }
                }
            }

            _blockInfos = new List<BlockInfo>();

            foreach (KeyValuePair<string, BlockInfo> kv in nameTable)
            {
                int i = kv.Value.StartLine - 1;
                int indentation = _indentation[i];
                while (true)
                {
                    i++;
                    if (_indentation[i] == indentation
                        && !Regex.IsMatch(Model.Default.Document[i], @"^\s*\{\s*$")
                        && !Regex.IsMatch(Model.Default.Document[i], @"^\s*//.*$")
                        && !Regex.IsMatch(Model.Default.Document[i], @"^\s*$"))
                    {

                        BlockInfo mi = new BlockInfo();
                        mi.Name = kv.Key;
                        mi.StartLine = kv.Value.StartLine - 1;
                        mi.EndLine = i;
                        mi.Type = kv.Value.Type;
                        _blockInfos.Add(mi);
                        break;
                    }
                }
            }
            return _blockInfos;
        }

        private void CreateAlmostSkipList()
        {
            _skips = new bool[Model.Default.Length];
            for (int i = 0; i < Model.Default.Length; i++)
            {
                bool almostSkip =
                    Regex.IsMatch(Model.Default.Document[i], @"^\s*\{\s*$") //Starting brackets
                    //Regex.IsMatch(Model.Default.Document[i], @"^\s*[\{\}]\s*$")
                    || Regex.IsMatch(Model.Default.Document[i], @"^\s*//.*$") //Comments
                    || Regex.IsMatch(Model.Default.Document[i], @"^\s*#.*") //Preprocessor
                    || Regex.IsMatch(Model.Default.Document[i], @"^\s*$"); //Empty lines
                _skips[i] = almostSkip;
            }
        }

        #region Searching

        private int _lastFoundIndex = 0;
        private int _lastFoundIndexEnd = 0;
        private int _lastFoundLine = 0;
        private string _lastSearchText = "";
        private int _lastSearchFocus = 0;

        public int SearchResultLine
        {
            get { return _lastFoundLine; }
        }

        public int SearchResultColStart
        {
            get { return _lastFoundIndex; }
        }
        public int SearchResultColEnd
        {
            get {return _lastFoundIndexEnd; }
        }

        public void ResetSearch()
        {
            _lastFoundIndex = 0;
            _lastFoundIndexEnd = 0;
            _lastFoundLine = 0;
            _lastSearchText = "";
            _lastSearchFocus = 0;
        }

        bool Instance_OnSearchNext(string text)
        {
            if (_lastSearchText != text || _lastSearchFocus != FocusCenter)
            {
                _lastFoundLine = _focus.Center;
                _lastFoundIndex = 0;
                _lastFoundIndexEnd = 0;
                _lastSearchText = "";
            }
            
            _lastSearchText = text;


            if (_lastFoundLine == 0)
                _lastFoundLine = _focus.Center;

            for (int i = _lastFoundLine; i < _document.Length; i++)
            {
                if (i != _lastFoundLine)
                    _lastFoundIndex = 0;

                int index = _document[i].IndexOf(text, _lastFoundIndexEnd, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    _lastFoundIndex = index;
                    _lastFoundIndexEnd = index + text.Length;
                    _lastFoundLine = i;
                    _lastSearchFocus = i;
                    UpdateFocus(i, true);
                    return true;
                }
                _lastFoundIndex = 0;
                _lastFoundIndexEnd = 0;
                _lastSearchFocus = FocusCenter;
            }
            _lastFoundLine = 0;
            _lastFoundIndex = 0;
            _lastFoundIndexEnd = 0;
            _lastSearchText = "";
            return false;
        }

        #endregion

        #region Singleton
        protected Model()
        {
            SearchDialog.Instance.OnSearchNext += new SearchNextDelegate(Instance_OnSearchNext);
        }

        public static Model Default
        {
            get
            {
                if (model == null)
                    model = new Model();

                return model;
            }
        }

        #endregion
    }
}
