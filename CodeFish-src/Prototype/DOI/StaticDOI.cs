using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDW;
using DDW.Collections;
using DDW.Names;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Prototype
{
    class StaticDOI : IDOIStrategy
    {
        private const int SEMANTIC_GAIN = 100;
        private const int NAMESPACE_GAIN = 30;
        private const int TYPE_GAIN = 20;
        private const int CLASS_GAIN = 30;
        private const int METHOD_GAIN = 20;
        private const int BLOCK_GAIN = 20;
        private const int RCURLY_GAIN = 5;
        private const int METHOD_DECLARATION_GAIN = 150;

        private float[][] _weights;


        // Base interests - classes = 30, methods = 20, ...
        private float[] _baseInterests;

        //Block indendation filtered per line
        private float[][] _syntacticInterests;

        //Semantic weights
        private float[][] _semanticWeights;

        public StaticDOI()
        {
            Settings.Default.DrawSeperator = false;
            Settings.Default.MinFontSize = (int)Settings.Default.Font.Size;
        }

        public void OnModelChanged()
        {
            CreateDOIData();
        }

        [Browsable(false)]
        DOIValue[] IDOIStrategy.Weights
        {
            get
            {
                if (Model.Default.Focus == null)
                    return WrapWeights(_weights[0]);
                else
                    return WrapWeights(_weights[Model.Default.Focus.Center]);
            }
        }

        private DOIValue[] WrapWeights(float[] weights)
        {
            DOIValue[] result = new DOIValue[weights.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                // 23-04-07: weight is saved but only importance is used to color the connectors
                result[i].weight = weights[i];
                result[i].importance = weights[i];
                if (_semanticWeights[Model.Default.FocusCenter][i] > 0f)
                    result[i].semantic = 1f;
                else
                    result[i].semantic = 0f;
            }

            return result;
        }

        private float API(int i)
        {
            return _baseInterests[i];// -(float)Math.Sqrt(2 * Model.Default.Indentation[i]);
        }

        //Same indentation
        private float Syntactic(int focus, int i)
        {
            int distanceToFocusCenter = Math.Abs(focus - i);
            bool inFocusArea = distanceToFocusCenter <= Model.Default.FocusAreaRadius;

            if (inFocusArea || Model.Default.Skips[i])
                return 0;
            else
            {
                float distanceToFocus = distanceToFocusCenter - Model.Default.FocusAreaRadius;
                float bonus = 2*BLOCK_GAIN/ (float)distanceToFocus;
                return _syntacticInterests[focus][i] + bonus;
            }

        }

        private bool WithinMethod(int focus, int i)
        {
            foreach (BlockInfo mi in Model.Default.BlockInfos)
            {
                if (mi.IsWithin(focus) && mi.IsWithin(i))
                        return true;
            }
            return false;
        }

        //References
        private float Semantic(int focus, int i)
        {
            return _semanticWeights[focus][i];
        }

        public void OnFocusChanged()
        {
        }

        private void CreateDOIData()
        {
            // API
            CreateBaseInterests();

            CreateSyntacticWeights();

            // references
            _semanticWeights = Utilities.CreateSemanticWeights(SEMANTIC_GAIN);
            
            CreateWeights();
        }

        private void CreateWeights()
        {
            _weights = new float[Model.Default.Length][];

            for (int x = 0; x < Model.Default.Length; x++)
            {
                _weights[x] = new float[Model.Default.Length];

                float max = 0.0f;
                for (int i = 0; i < _weights[x].Length; i++)
                {
                    float weight;
                    if (Model.Default.Skips[i])
                        weight = 0f;
                    else
                    {
                        weight =
                            //API(i)
                            Syntactic(x, i) //Not distance, but bonus
                            + Semantic(x, i); //Not distance, but bonus
                    }

                    weight = Math.Max(weight, 0f);

                    max = Math.Max(max, weight);

                    Debug.Assert(weight != float.NaN && !float.IsInfinity(weight));
                    _weights[x][i] = weight;

                }

                for (int i = 0; i < _weights[x].Length; i++)
                {
                    _weights[x][i] = _weights[x][i] / max;
                }
            }
        }

        private void CreateSyntacticWeights()
        {
            _syntacticInterests = new float[Model.Default.Length][];
            for (int i = 0; i < Model.Default.Length; i++)
            {
                _syntacticInterests[i] = new float[Model.Default.Length];

                int min;
                int max;
                BlockInfo mi = Model.Default.BlockInfoMapping[i];
                if (mi != null)
                {
                    min = mi.StartLine;
                    max = mi.EndLine;
                }
                else
                {
                    min = 0;
                    max = Model.Default.Length;

                }
                float maxIndentation = Model.Default.Indentation[i];
                //Before current line
                for (int j = i - 1; j >= 0; j--)
                {
                    if (j == min)
                        _syntacticInterests[i][j] = METHOD_DECLARATION_GAIN;
                    else if (Model.Default.Indentation[j] > maxIndentation || j < min || Model.Default.Skips[j])
                        _syntacticInterests[i][j] = 0;
                    else if (Model.Default.Indentation[j] == maxIndentation)
                        _syntacticInterests[i][j] = API(j)/2f;
                    else
                    {
                        maxIndentation = (float)Math.Min(maxIndentation, Model.Default.Indentation[j]);
                        float api = 0;
                        if (WithinMethod(i, j))
                            api = API(i);
                        _syntacticInterests[i][j] = (4 * maxIndentation) + api;
                    }
                }

                maxIndentation = Model.Default.Indentation[i];
                //After current line
                for (int j = i + 1; j < Model.Default.Length; j++)
                {
                    if (Model.Default.Indentation[j] > maxIndentation || j > max || Model.Default.Skips[j])
                        _syntacticInterests[i][j] = 0;
                    else if (Model.Default.Indentation[j] == maxIndentation)
                        _syntacticInterests[i][j] = API(j)/2f;
                    else
                    {
                        maxIndentation = (float)Math.Min(maxIndentation, Model.Default.Indentation[j]);
                        float api = 0;
                        if (WithinMethod(i, j))
                            api = API(i);
                        _syntacticInterests[i][j] = (4 * maxIndentation) + api;
                    }
                }
            }
        }

        private void CreateBaseInterests()
        {
            _baseInterests = new float[Model.Default.Length];


            foreach (DDW.Token tok in Model.Default.LexedDocument)
            {
                int add = 0;
                switch (tok.ID)
                {
                    #region Old crap
                    case TokenID.Public:
                    case TokenID.Private:
                    case TokenID.Delegate:
                        //add = METHOD_GAIN;
                        break;

                    case TokenID.Class:
                        //add = CLASS_GAIN;
                        break;

                    case TokenID.Enum:
                    case TokenID.Struct:
                        //add = TYPE_GAIN;
                        break;

                    case TokenID.Namespace:
                        //add = NAMESPACE_GAIN;
                        break;

                    case TokenID.RCurly:
                        //add = RCURLY_GAIN;
                        break;
                    #endregion

                    case TokenID.For:
                    case TokenID.Foreach:
                    case TokenID.While:
                    case TokenID.If:
                    case TokenID.Else:
                    case TokenID.Case:
                    case TokenID.Switch:
                    case TokenID.Try:
                    case TokenID.Catch:
                    case TokenID.Finally:
                        add = BLOCK_GAIN;
                        break;
                        
                    default:
                        break;
                }
                _baseInterests[tok.Line - 1] += add;
            }
        }

        public void Reset()
        {
            CreateDOIData();
        }

        public override string ToString()
        {
            return "Semantic DOI";
        }
    }
}

