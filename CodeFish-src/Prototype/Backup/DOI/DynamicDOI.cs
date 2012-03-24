using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using DDW;

namespace Prototype
{
    class DynamicDOI : IDOIStrategy
    {
        #region Fields

        DateTime[] _history;
        float[] _weights;
        
        // timeMultiplier = Math.Pow(timePassed, timePower)
        // pow(x, 0.5) = sqrt(x)
        float timePower = 0.5f;

        private DateTime _lastChange;
        private FocusArea _lastFocus;

        // time spent before a move is considered a transition and not just scrolling
        private float _timeout = 1f;

        //Semantic weights
        private float[][] _semanticWeights;

        #endregion

        #region Properties

        //[CategoryAttribute("DOI settings"),
        //DescriptionAttribute("Weight factor for distance component of DOI function. Should be between 1 and 0.")]
        //public float WeightToTimeRatio
        //{
        //    get
        //    {
        //        return _timeToWeightRatio;
        //    }
        //    set
        //    {
        //        _timeToWeightRatio = value;
        //    }
        //}

        #endregion

        public DynamicDOI()
        {
            Settings.Default.MinFontSize = 7;
            Settings.Default.DrawSeperator = false;
        }

        DOIValue[] IDOIStrategy.Weights
        {
            get
            {
                DOIValue[] weights = new DOIValue[Model.Default.Length];

                for (int i = 0; i < weights.Length; i++)
                {
                    if (Model.Default.Skips[i])
                    {
                        weights[i].weight = 0f;
                        weights[i].importance = 0f;
                        weights[i].semantic = 0f;
                    }
                    else
                    {
                        weights[i].weight = LineDistance(i) * 0.0001f + TimeUsage(i) + TimeDistance(i) * 2f;
                        weights[i].importance = TimeDistance(i);
                        if (_semanticWeights[Model.Default.FocusCenter][i] > 0f)
                            weights[i].semantic = 1f;
                        else
                            weights[i].semantic = 0f;
                    }
                    weights[i].weight += _semanticWeights[Model.Default.FocusCenter][i];
                }


                Normalize(weights);

                return weights;
            }
        }

        private float TimeUsage(int i)
        {
            return _weights[i];
        }

        private float TimeDistance(int i)
        {
            double weight;

            if (_history[i] != DateTime.MinValue)
            {
                float timePassed = (float)(_lastChange - _history[i]).TotalSeconds;
                weight = Math.Min(1f / timePassed, 1f);
            }
            else
            {
                weight = 1f / (Model.Default.Focus.Distance(i) + 1);
            }

            return (float)Math.Pow(weight, timePower);
        }

        private float LineDistance(int i)
        {
            return 1f / (Model.Default.Focus.Distance(i) + 1);
        }

        #region Initialization / ModelChange / Reset

        public void OnModelChanged()
        {
            InitDOI();
        }
        
        public virtual void Reset()
        {
            InitDOI();
        }

        private void InitDOI()
        {
            //_maxWeight = 0.1f;
            _lastChange = DateTime.Now;
            _lastFocus = Model.Default.Focus;

            _semanticWeights = Utilities.CreateSemanticWeights(50);

            _weights = new float[Model.Default.Length];
            _history = new DateTime[Model.Default.Length];

            for (int i = 0; i < Model.Default.Length; i++)
            {
                _weights[i] = 1f;
                _history[i] = _lastChange;
            }
        }

        public override string ToString()
        {
            return "Dynamic DOI";
        }
        #endregion

        #region History and weights updating

        public virtual void OnFocusChanged()
        {
            float timePassed = (float)(DateTime.Now - _lastChange).TotalSeconds;

            if (timePassed > _timeout && _lastFocus != null)
            {
                InflateWeights(_lastFocus, 0.05f, timePassed, timePower);
                UpdateHistory(_lastFocus);
            }

            _lastChange = DateTime.Now;
            _lastFocus = Model.Default.Focus;
        }

        private void UpdateHistory(FocusArea focus)
        {
            for (int i = focus.Start; i < focus.End; i++)
                _history[i] = DateTime.Now;
        }

        private void InflateWeights(FocusArea focus, float multiplier, float timePassed, float power)
        {
            for (int i = focus.Start; i < focus.End; i++)
            {
                float distanceWeight = 1.0f - (float)focus.DistanceToCenter(i) / focus.Lines;
                _weights[i] += multiplier * (float)Math.Pow(timePassed, power) * distanceWeight;
            }

            Normalize(_weights);
        }

        private void Normalize(float[] values)
        {
            float max = float.MinValue;

            for (int i = 0; i < _weights.Length; i++)
            {
                max = Math.Max(values[i], max);
            }

            for (int i = 0; i < _weights.Length; i++)
            {
                values[i] /= max;
            }
        }

        private void Normalize(DOIValue[] values)
        {
            float maxWeight = float.MinValue;
            float maxImportance = float.MinValue;

            for (int i = 0; i < _weights.Length; i++)
            {
                maxWeight = Math.Max(values[i].weight, maxWeight);
                maxImportance = Math.Max(values[i].importance, maxImportance);
            }

            for (int i = 0; i < _weights.Length; i++)
            {
                values[i].importance /= maxImportance;
                values[i].weight /= maxWeight;
            }
        }

        #endregion


    }
}
