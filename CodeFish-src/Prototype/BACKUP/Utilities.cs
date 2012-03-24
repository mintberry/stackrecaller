using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDW;
using DDW.Collections;

namespace Prototype
{
    class Utilities
    {
        public static bool[] CreateSkipList(string[] document)
        {
            bool[] skips = new bool[document.Length];
            for (int i = 0; i < document.Length; i++)
            {
                bool hasLetterOrDigit = false;
                char[] chars = document[i].ToCharArray();
                foreach (char c in chars)
                {
                    if (!Char.IsWhiteSpace(c))
                    //if (Char.IsLetterOrDigit(c))
                    {
                        hasLetterOrDigit = true;
                        break;
                    }
                }
                skips[i] = !hasLetterOrDigit;
            }
            return skips;
        }

        public static CompilationUnitNode ParseDocument(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, true);
            Lexer l = new Lexer(sr);
            TokenCollection toks = l.Lex();
            Parser p = new Parser(fileName);
            CompilationUnitNode cu = p.Parse(toks, l.StringLiterals);
            return cu;
        }

        public static TokenCollection LexDocument(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, true);
            Lexer l = new Lexer(sr);
            TokenCollection toks = l.Lex();
            return toks;
        }

        public static string[] FileToStringArray(string filename)
        {
            StreamReader sr = File.OpenText(filename);

            List<string> lines = new List<string>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line.Replace("\t", "    "));
            }
            sr.Close();

            return lines.ToArray();
        }



        private static void AddToNameTable(Dictionary<string, List<int>> nameTable, MemberNode idn)
        {

            string name = idn.Names[0].GenericIdentifier;
            int line = idn.RelatedToken.Line - 1;

            if (nameTable.ContainsKey(name))
                nameTable[name].Add(line);
            else
            {
                List<int> lines = new List<int>();
                lines.Add(line);
                nameTable.Add(name, lines);
            }
        }

        public static float[][] CreateSemanticWeights(int gain)
        {
            // definition of array
            float[][] _semanticWeights = new float[Model.Default.Length][];
            
            // initialization of array
            for (int i = 0; i < _semanticWeights.Length; i++)
            {
                _semanticWeights[i] = new float[Model.Default.Length];
                for (int j = 0; j < _semanticWeights[i].Length; j++)
                {
                    _semanticWeights[i][j] = 0;
                }
            }

            // define nametable, name as key, list of ints as value
            Dictionary<string, List<int>> nameTable = new Dictionary<string, List<int>>();

            // fill nametable dictionary with method, field and property definitions
            foreach (NamespaceNode nsn in Model.Default.ParsedDocument.Namespaces)
            {
                foreach (ClassNode cn in nsn.Classes)
                {
                    foreach (MethodNode mn in cn.Methods)
                        AddToNameTable(nameTable, mn);

                    foreach (FieldNode fn in cn.Fields)
                        AddToNameTable(nameTable, fn);

                    foreach (PropertyNode pn in cn.Properties)
                        AddToNameTable(nameTable, pn);

                }
            }


            Dictionary<string, List<int>> referenceTable = new Dictionary<string, List<int>>();

            foreach (KeyValuePair<string, List<int>> d in nameTable)
            {
                referenceTable.Add(d.Key, new List<int>());
            }

            LinkedListNode<DDW.Token> token = Model.Default.LexedDocument.First;

            List<string> par = new List<string>(); //For debugging
            while (token != null)
            {
                if (token.Value.ID == TokenID.Ident)
                {
                    int count;
                    if (token.Next == null || token.Next.Value.Line != token.Value.Line)
                        count = Model.Default.Document[token.Value.Line - 1].Length - (token.Value.Col);
                    else
                        count = token.Next.Value.Col - token.Value.Col;

                    string part = Model.Default.Document[token.Value.Line - 1].Substring(token.Value.Col, count).Trim();
                    par.Add(part);

                    if (referenceTable.ContainsKey(part) && !referenceTable[part].Contains(token.Value.Line - 1))
                        referenceTable[part].Add(token.Value.Line - 1);
                }
                token = token.Next;
            }

            foreach (KeyValuePair<string, List<int>> kv in referenceTable)
            {
                foreach (int j in kv.Value)
                {
                    for (int f = -Model.Default.FocusAreaRadius; f <= Model.Default.FocusAreaRadius; f++)
                    {
                        if (j + f < 0 || j + f >= Model.Default.Length)
                            continue;

                        foreach (int i in nameTable[kv.Key])
                            _semanticWeights[j + f][i] = gain;
                    }
                }
            }
            return _semanticWeights;
        }

        
    }


}
