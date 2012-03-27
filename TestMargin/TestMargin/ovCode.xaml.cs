using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace TestMargin
{
    /// <summary>
    /// Interaction logic for ovCode.xaml
    /// </summary>
    public partial class ovCode : UserControl
    {
        private IWpfTextView _wtv = null;
        private ITextViewLine []_tvls;

        public ovCode()
        {
            InitializeComponent();
        }

        public ovCode(IWpfTextView wtv)
        {
            _wtv = wtv;
            _tvls = _wtv.TextViewLines.ToArray();
            InitializeComponent();
            
        }

        public void UpdateOV() 
        {
            
        }
    }
}
