using System;

namespace Prototype
{
    public struct Line
    {
        public string text;
        public float weight;

        public Line(string text, float weight)
        {
            this.text = text;
            this.weight = weight;
        }
    }

    public struct DOIValue
    {
        public float weight;
        public float importance;
        public float semantic;
    }

    public interface IDOIStrategy
    {
        DOIValue[] Weights { get; }

        void OnFocusChanged();

        void OnModelChanged();

        void Reset();
    }
}
