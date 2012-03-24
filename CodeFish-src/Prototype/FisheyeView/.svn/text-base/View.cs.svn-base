using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Prototype
{
    class View
    {
        public delegate void ViewRefreshDelegate();
        public event ViewRefreshDelegate OnViewRefresh;

        public void Refresh()
        {
            RefreshLayout();

            if (OnViewRefresh != null)
                OnViewRefresh();
        }

        Size _size;

        public Size Size
        {
            get { return _size; }
            set
            {
                _size = value;
                UpdateLayout(false);
            }
        }

        private Layout _oldLayout;
        private Layout _newLayout;
        private Layout _layout;

        private DateTime _layoutUpdateTime = DateTime.MinValue;

        private bool _animateTransition;
        private float _transitionSeconds = 0.25f;

        public float TransitionSeconds
        {
            get { return _transitionSeconds; }
            set { _transitionSeconds = value; }
        }

        public Layout Layout
        {
            get { return _layout; }
        }

        public void ClearLayout()
        {
            _layout = null;
            _oldLayout = null;
            _newLayout = null;
        }

        public void UpdateLayout(bool animateTransition)
        {
            _oldLayout = _layout;

            _layoutUpdateTime = DateTime.Now;

            if (Model.Default.DOIStrategy != null)
                _newLayout = Model.Default.RenderStrategy.GenerateLayout();
            
            _animateTransition = animateTransition;
        }

        private void RefreshLayout()
        {
            float a = (float)((TimeSpan)(DateTime.Now - _layoutUpdateTime)).TotalSeconds / _transitionSeconds;

            if (_oldLayout != null && _newLayout != null && a < 1f && _animateTransition)
            {
                _layout = Layout.Blend(_oldLayout, _newLayout, a);
            }
            else
            {
                _layout = _newLayout;
            }
        }

        #region Singleton
        static private View view = null;
        //Timer updateTimer = new Timer();

        protected View()
        {
            //depreciationTimer.Tick += new EventHandler(depreciationTimer_Tick);
            //depreciationTimer.Interval = 1;
            //depreciationTimer.Start();
        }

        public static View Default
        {
            get
            {
                if (view == null)
                    view = new View();

                return view;
            }
        }
        #endregion
    }
}
