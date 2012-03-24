using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace Prototype
{
    public class FisheyeViewOptionsWrapper
    {
        public FisheyeViewOptionsWrapper(CodeviewControl fe)
        {
        }

        #region Colors
        [CategoryAttribute("Colors"),
        DescriptionAttribute("Background color in overview area.")]
        public Color OverviewBackgroundColor
        {
            get { return Settings.Default.OverviewBackgroundColor; }
            set { Settings.Default.OverviewBackgroundColor = value; }
        }

        [CategoryAttribute("Colors"),
        DescriptionAttribute("Background color in focus area.")]
        public Color FocusColor
        {
            get { return Settings.Default.FocusColor; }
            set { Settings.Default.FocusColor = value; }
        }



        [CategoryAttribute("Colors"),
		DescriptionAttribute("Color of selected lines")]
        public Color SelectedColor
        {
            get { return Settings.Default.SelectedColor; }
            set { Settings.Default.SelectedColor = value; }
        }

        [CategoryAttribute("Colors"),
        DescriptionAttribute("Color of focus area center line")]
        public Color FocusCenterColor
        {
            get { return Settings.Default.FocusCenterColor; }
            set { Settings.Default.FocusCenterColor = value; }
        }

        [CategoryAttribute("Colors"),
        DescriptionAttribute("Color of connectors from code view to overview")]
        public Color ConnectorsColor
        {
            get { return Settings.Default.ConnectorsColor; }
            set { Settings.Default.ConnectorsColor = value; }
        }


        [CategoryAttribute("Colors"),
        DescriptionAttribute("Color of referenced lines")]
        public Color SemanticColor
        {
            get { return Settings.Default.SemanticColor; }
            set { Settings.Default.SemanticColor = value; }
        }

        [CategoryAttribute("Colors"),
        DescriptionAttribute("Color of segment seperator indication")]
        public Color SeperatorColor
        {
            get { return Settings.Default.SeperatorColor; }
            set { Settings.Default.SeperatorColor = value; }
        }
        #endregion

        #region Formatting
        [CategoryAttribute("Formatting"),
        DescriptionAttribute("Font")]
        public Font Font
        {
            get { return Settings.Default.Font; }
            set { Settings.Default.Font = value; }
        }

        [CategoryAttribute("Formatting"),
        DescriptionAttribute("Minimum font size")]
        public int MinFontSize
        {
            get { return Settings.Default.MinFontSize; }
            set { Settings.Default.MinFontSize = value; }
        }

        [CategoryAttribute("Formatting"),
        DescriptionAttribute("Span of focus area.")]
        public int FocusAreaRadius
        {
            get { return Model.Default.FocusAreaRadius; }
            set { Model.Default.FocusAreaRadius = value; }
        }

        #endregion

        #region Segments
        [CategoryAttribute("Segments"),
        DescriptionAttribute("Style of seperator between segments of code.")]
        public Prototype.Seperators SegmentSeperatorStyle
        {
            get { return Settings.Default.SegmentSeperatorStyle; }
            set { Settings.Default.SegmentSeperatorStyle = value; }
        }

        [CategoryAttribute("Segments"),
DescriptionAttribute("Seperator between segments of code.")]
        public bool DrawSeperator
        {
            get { return Settings.Default.DrawSeperator; }
            set { Settings.Default.DrawSeperator = value; }
        }

        [CategoryAttribute("Segments"),
        DescriptionAttribute("How many invisible lines are allowed before a segment is considered be finished.")]
        public int SegmentFuzziness
        {
            get { return Settings.Default.SegmentFuzziness; }
            set { Settings.Default.SegmentFuzziness = value; }
        }
        #endregion




        #region Live dynamics


        [CategoryAttribute("Live dynamics"),
        DescriptionAttribute("Length of transition animation between layouts, in seconds.")]
        public float TransitionSeconds
        {
            get { return View.Default.TransitionSeconds; }
            set { View.Default.TransitionSeconds = value; }
        }
        #endregion
    }
}
