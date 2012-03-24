using System;
using System.Collections.Generic;
using System.Text;
using UMD.HCIL.Piccolo;
using System.Drawing;
using UMD.HCIL.Piccolo.Util;
using System.IO;
using System.Data;
using System.Diagnostics;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.PiccoloX.Nodes;
using System.Collections;
using UMD.HCIL.Piccolo.Activities;

namespace SemanticZoomSheet
{
    public enum AnimationType
    {
        Null = 0,
        Decrease = -1,
        Increase = 1,
        ContractCellToRight = -7,
        ExpandCellFromRight = 7,
        MoveArea = 8
    };

    public enum BehaviourType
    {
        BeingCovered = -10,
        BeingExposed = 10,
        SetRoughHide = 0,
        SetRoughShow = 9
    }

    class Sheet : PNode
    {
        #region Sheet Singleton
        public static Sheet Instance
        {
            get
            {
                return NestedSheet.instance;
            }
        }

        class NestedSheet
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedSheet()
            {
            }

            internal static readonly Sheet instance = new Sheet();
        }
        #endregion

        #region Fields
        public const float SCALE_CHANGE_EVENT_STEPS = 0.01f;
        public const int SCALE_CHANGE_EVENT_STEPS_DIV_ONE = 100;

        private float previousDiscreetScaleUsedWhenScaling = SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE;
        private float averageOfRowDOIs = 0f;

        private int _ColumnCount = 0;
        private int _RowCount = 0;

        List<int> CollapsedColumns = new List<int>();
        List<Dictionary<float, List<ScaleRepresentation>>> RepresentationCollection = new List<Dictionary<float, List<ScaleRepresentation>>>();
        #endregion

        private const float LIMIT_0 = float.MaxValue;
        private const float LIMIT_1 = 0.9f;
        private const float LIMIT_2 = 0.85f;
        private const float LIMIT_3 = 0.80f;
        private const float LIMIT_4 = float.MinValue;

        public int ColumnCount
        {
            get { return _ColumnCount; }
            set { _ColumnCount = value; }
        }
        public int RowCount
        {
            get { return _RowCount; }
            set { _RowCount = value; }
        }

        #region Ctor
        public Sheet()
        {
            this.BoundsChanged += new PPropertyEventHandler(Sheet_BoundsChanged);

        }
        #endregion

        #region Bounds managment
        void Sheet_BoundsChanged(object sender, PPropertyEventArgs e)
        {
            MyLayoutChildren();
        }
        #endregion

        #region Init. of scale representations
        private void InitRepresentationCollection()
        {
            for (int r = 0; r < _RowCount; r++)
            {
                Dictionary<float, List<ScaleRepresentation>> sd = new Dictionary<float, List<ScaleRepresentation>>();
                for (float s = SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE; s <= SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE; s += SCALE_CHANGE_EVENT_STEPS)
                {
                    float scale = GetDiscreetScale(s);
                    List<ScaleRepresentation> srl = new List<ScaleRepresentation>();
                    for (int c = 0; c < _ColumnCount; c++)
                    {
                        srl.Add(null);
                    }
                    sd.Add(scale, srl);
                }
                RepresentationCollection.Add(sd);
            }
        }
        public void InitScaleRepresentations()
        {

            InitRepresentationCollection();

            float t = 0f;
            foreach (Row row in this.ChildrenReference)
            {
                ArrayList nums = new ArrayList();
                foreach (Cell cell in row.ChildrenReference)
                {
                    nums.Add(Math.Abs(cell.Value));
                }
                float a = SZSUtils.Average(nums);
                row.DOI = a;
                t += a;
            }

            ArrayList dois = new ArrayList();
            foreach (Row row in this.ChildrenReference)
            {
                row.DOI /= t;
                dois.Add(row.DOI);
            }
            averageOfRowDOIs = SZSUtils.StandardDeviation(dois);

            foreach (PNode row in this.ChildrenReference)
            {
                ArrayList avgs = new ArrayList();
                for (float s = SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE; s < SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE; s += SCALE_CHANGE_EVENT_STEPS)
                {
                    ArrayList nums = new ArrayList();
                    foreach (PNode cell in row.ChildrenReference)
                    {
                        Cell c = (Cell)cell;
                        c.DOI = (Math.Abs((c.Value - c.Average(1, 1))) / c.Average(1, 1)) * (Math.Sign(c.ChangeFromLeft) == Math.Sign(c.ChangeFromRight) ? 1f : 1.1f);

                        nums.Add((float)Math.Pow(2, c.DOI * (1 / s)));
                    }
                    avgs.Add(SZSUtils.Average(nums));
                }
                ((Row)row).DOIThreshold = (float)Math.Pow(4, SZSUtils.StandardDeviation(avgs) * 2);
            }

            for (float s = SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE; s <= SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE; s += SCALE_CHANGE_EVENT_STEPS)
            {
                float scale = GetDiscreetScale(s);

                if (scale == SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE)
                {
                    foreach (Row row in this.ChildrenReference)
                    {
                        int rowIndex = ChildrenReference.IndexOf(row);
                        foreach (Cell cell in row.ChildrenReference)
                        {
                            int cellIndex = row.ChildrenReference.IndexOf(cell);
                            RepresentationCollection[rowIndex][scale][cellIndex] = cell.GetRepresentationAtScale(scale, 1);
                        }
                    }
                }
                else
                {

                    foreach (PNode row in this.ChildrenReference)
                    {

                        int startIndex = 0;
                        int startChange = int.MaxValue;
                        for (int i = 1; i < _ColumnCount; i++)
                        {

                            int rowIndex = ChildrenReference.IndexOf(row);
                            Cell currentCell = (Cell)row.ChildrenReference[i];
                            Cell startIndexCell = ((Cell)row.ChildrenReference[startIndex]);
                            int currentChange = Math.Sign(currentCell.ChangeFromLeft);

                            if (startChange == int.MaxValue)
                                startChange = currentChange;

                            int endIndex = i - 1;
                            int colSpan = (endIndex - startIndex) + 1;

                            //-------------
                            if ((
                                    (currentChange != 0)
                                    &&
                                    (currentChange != startChange)
                                    &&
                                    (Math.Pow(2, currentCell.DOI * (1 / scale)) >= ((Row)row).DOIThreshold)
                                ) || (
                                    (colSpan >= (Math.Round(Math.Pow(1 / s, 8) * 2, 0)) - 1)
                                    &&
                                    (scale > (SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE + SCALE_CHANGE_EVENT_STEPS))
                                ))
                            {

                                RepresentationCollection[rowIndex][scale][startIndex] = startIndexCell.GetRepresentationAtScale(scale, (endIndex - startIndex) + 1);
                                startIndex = i;
                                startChange = int.MaxValue;
                            }
                            //---------------
                            else
                            {
                                //last cell
                                if (i + 1 >= row.ChildrenCount)
                                {
                                    ScaleRepresentation sdf = startIndexCell.GetRepresentationAtScale(scale, (i + 1) - startIndex);
                                    RepresentationCollection[rowIndex][scale][startIndex] = sdf;
                                }
                            }
                            if (startIndex + 1 == row.ChildrenCount)
                                RepresentationCollection[rowIndex][scale][startIndex] = ((Cell)row.ChildrenReference[startIndex]).GetRepresentationAtScale(scale, 1);
                        }
                    }
                }
            }
            InitRowCollapseRepresentations();
            //Animation_PrintAllRepresentations();
        }

        private Dictionary<float, RowCollapseOrder> CollapseOrderCollection = new Dictionary<float, RowCollapseOrder>();

        private void InitRowCollapseRepresentations()
        {

            for (float s = SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE; s <= SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE; s += SCALE_CHANGE_EVENT_STEPS)
            {
                float scale = GetDiscreetScale(s);
                CollapseOrderCollection.Add(scale, new RowCollapseOrder());

                double top = ((scale * scale)) / 1;
                int topCountToShow = (int)Math.Ceiling(top * _RowCount);
                List<Row> topRows = new List<Row>();
                for (int i = 0; i < topCountToShow; i++)
                {
                    Row vip = GetMostImportant(topRows);
                    if (vip != null)
                        topRows.Add(vip);
                }
                List<Row> bottomRows = new List<Row>();
                foreach (Row r in ChildrenReference)
                {
                    if (!topRows.Contains(r))
                        bottomRows.Add(r);
                }
                CollapseOrderCollection[scale].Shown = topRows;
                CollapseOrderCollection[scale].Hidden = bottomRows;
            }
        }

        private Row GetMostImportant(List<Row> doNotChoose)
        {
            Row largest = null;
            foreach (Row r in ChildrenReference)
            {
                if (!doNotChoose.Contains(r))
                {
                    if (largest == null)
                        largest = r;
                    else
                        largest = (r.DOI > largest.DOI ? r : largest);
                }
            }
            return largest;
        }
        #endregion

        #region Updating help text
        private bool IsBetween(float a, float b, float num)
        {
            if (a > b)
                return (num <= a && num > b);
            else
                return (num > a && num <= b);
        }

        private void UpdateHelpText(float s)
        {
            string t = "";
            if (s == SemanticZoomSheetZoomEventHandler.DEFAULT_MAX_SCALE)
                t = "Oprindelige værdier (mio. kr)";
            else
            {

                if (IsBetween(LIMIT_0, LIMIT_1, s))
                    t = "Oprindelige værdier (mio. kr) og intervaller (mia. kr)";
                else if (IsBetween(LIMIT_1, LIMIT_2, s))
                    t = "Interval (start - slut, mia. kr)";
                else if (IsBetween(LIMIT_2, LIMIT_3, s))
                    t = "Gennemsnitlig årlig vækstrate, median (mia. kr)";
                else if (IsBetween(LIMIT_3, LIMIT_4, s))
                    t = "Gennemsnitlig årlig vækstrate";
            }

            MainForm.Instance.UpdateHelp(t);
        }
        #endregion

        #region ScaleBy
        public override void ScaleBy(float scale, float x, float y)
        {
            float newDiscreetScale = GetDiscreetScale(this.Scale * scale);
            if (newDiscreetScale != previousDiscreetScaleUsedWhenScaling)
            {
                MainForm.Instance.UpdateZoom(newDiscreetScale);
                CollapseColumns(newDiscreetScale);
                UpdateHelpText(newDiscreetScale);

                foreach (Row show in CollapseOrderCollection[newDiscreetScale].Shown)
                {
                    int indexOfRow = ChildrenReference.IndexOf(show);
                    if (!show.Visible)
                    {
                        QuickLoadRow(indexOfRow, previousDiscreetScaleUsedWhenScaling);
                        show.Show();
                    }
                    else
                    {
                        for (int j = 0; j < _ColumnCount; j++)
                        {
                            ScaleRepresentation sr = RepresentationCollection[indexOfRow][newDiscreetScale][j];
                            Cell currentCell = ((Cell)show.ChildrenReference[j]);
                            if (sr != null)
                            {
                                if (!sr.Equals(currentCell.CurrentRepresentation) || RepresentationCollection[indexOfRow][previousDiscreetScaleUsedWhenScaling][j] == null)
                                    currentCell.SetRepresentationAndWidth(sr, GetRealWidth(j, sr.ColSpan));
                            }
                            else
                                currentCell.Hide();
                        }
                    }
                }

                foreach (Row hideSelected in CollapseOrderCollection[newDiscreetScale].Hidden)
                {
                    if (hideSelected.Selected)
                    {
                        int indexOfRow = ChildrenReference.IndexOf(hideSelected);
                        if (!hideSelected.Visible)
                        {
                            QuickLoadRow(indexOfRow, previousDiscreetScaleUsedWhenScaling);
                            hideSelected.Show();
                        }
                        else
                        {
                            for (int j = 0; j < _ColumnCount; j++)
                            {
                                ScaleRepresentation sr = RepresentationCollection[indexOfRow][newDiscreetScale][j];
                                Cell currentCell = ((Cell)hideSelected.ChildrenReference[j]);
                                if (sr != null)
                                {
                                    if (!sr.Equals(currentCell.CurrentRepresentation) || RepresentationCollection[indexOfRow][previousDiscreetScaleUsedWhenScaling][j] == null)
                                        currentCell.SetRepresentationAndWidth(sr, GetRealWidth(j, sr.ColSpan));
                                }
                                else
                                    currentCell.Hide();
                            }
                        }
                    }
                }

                foreach (Row hide in CollapseOrderCollection[newDiscreetScale].Hidden)
                {
                    if (hide.Visible && !hide.Selected)
                        hide.Hide();
                }

                previousDiscreetScaleUsedWhenScaling = newDiscreetScale;
                MyLayoutChildren();
                foreach (Row r in ChildrenReference)
                {
                    r.DoLayout();
                }
            }

            base.ScaleBy(scale, x, y);
        }
        #endregion

        public void Clear()
        {
            Matrix = new PMatrix();
            RemoveAllChildren();
            CollapseOrderCollection.Clear();
            RepresentationCollection.Clear();
            CollapsedColumns.Clear();
        }

        private void CollapseColumns(float scale)
        {
            List<ColumnCollapseOrder> collapsableColumns = new List<ColumnCollapseOrder>();
            for (int i = 0; i < _ColumnCount; i++)
            {
                bool isCollapse = true;
                for (int j = 0; j < _RowCount; j++)
                {
                    if (ChildrenReference[j].Visible)
                    {
                        for (float s = scale; s >= SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE; s -= SCALE_CHANGE_EVENT_STEPS)
                        {
                            isCollapse &= (RepresentationCollection[j][GetDiscreetScale(s)][i] == null);
                            if (!isCollapse)
                                break;
                        }
                    }
                    if (!isCollapse)
                        break;
                }

                ColumnCollapseOrder cco = null;
                if (!isCollapse)
                {
                    if (CollapsedColumns.Contains(i))
                    {
                        for (int k = 0; k < _RowCount; k++)
                        {
                            ScaleRepresentation sr = null;
                            int r;
                            for (r = 0; sr == null; r++, sr = RepresentationCollection[k][scale][i - r]) ;
                            if (cco == null)
                                cco = new ColumnCollapseOrder(i);
                            cco.AddRowOrder(GetCell(k, i - r), Cell.STD_CELL_WIDTH);
                        }

                        CollapsedColumns.Remove(i);
                    }
                }
                else
                {
                    if (!CollapsedColumns.Contains(i))
                    {
                        for (int k = 0; k < _RowCount; k++)
                        {
                            ScaleRepresentation sr = null;
                            int r;
                            for (r = 0; sr == null; r++, sr = RepresentationCollection[k][scale][i - r]) ;
                            if (cco == null)
                                cco = new ColumnCollapseOrder(i);
                            cco.AddRowOrder(GetCell(k, i - r), -Cell.STD_CELL_WIDTH);
                        }
                        CollapsedColumns.Add(i);
                    }
                }

                if (cco != null)
                    collapsableColumns.Add(cco);
            }


            foreach (ColumnCollapseOrder ccoi in collapsableColumns)
            {
                ((HeaderCell)RowBar.Instance[ccoi.ColumnIndex]).Collapse();
                ccoi.Execute();
            }
        }

        #region Layout
        public void MyLayoutChildren()
        {
            float xOffset = Cell.STD_CELL_TITLE_WIDTH;
            float yOffset = Cell.STD_CELL_HEIGHT;

            foreach (Row each in ChildrenReference)
            {
                if (each.Visible)
                {
                    each.SetOffset(xOffset, yOffset - each.Y);
                    yOffset += each.Height;
                }
            }
            ColBar.Instance.MyLayoutChildren();
            RowBar.Instance.MyLayoutChildren();
        }
        #endregion

        #region Utils
        private Cell GetCell(int row, int column)
        {
            return (Cell)ChildrenReference[row].ChildrenReference[column];
        }

        public static float GetDiscreetScale(float scale)
        {
            return (float)Math.Round(scale * SCALE_CHANGE_EVENT_STEPS_DIV_ONE) / SCALE_CHANGE_EVENT_STEPS_DIV_ONE;
        }
        public void Reset()
        {
            this.ChildrenReference.Clear();

        }

        public override string ToString()
        {
            string seperator = string.Concat(new string('-', (this.ChildrenReference[0].ChildrenCount + 1) * 5), "\n");
            string s = seperator;
            foreach (PNode pn in this.ChildrenReference)
            {
                s += ((Row)pn).ToString() + "\n";
            }
            return s + seperator;
        }

        private float GetRealWidth(int start, int length)
        {
            float w = Cell.STD_CELL_WIDTH;
            for (int i = 1; i < length; i++)
            {
                if (!CollapsedColumns.Contains(start + i))
                    w += Cell.STD_CELL_WIDTH;
            }
            return w;
        }

        private void QuickLoadRow(int row, float newDiscreetScale)
        {
            for (int i = 0; i < _ColumnCount; i++)
            {
                ScaleRepresentation sr = RepresentationCollection[row][newDiscreetScale][i];
                Cell currentCell = (Cell)ChildrenReference[row][i];
                if (sr != null)
                    currentCell.SetRepresentationAndWidth(sr, GetRealWidth(i, sr.ColSpan));
                else
                    currentCell.Hide();

            }
        }
        #endregion

        #region Printning
        private void Animation_PrintAllRepresentations()
        {
            foreach (Dictionary<float, List<ScaleRepresentation>> arrayOfRepresentations in RepresentationCollection)
            {
                Debug.WriteLine("New row");
                Debug.Indent();
                foreach (KeyValuePair<float, List<ScaleRepresentation>> kv in arrayOfRepresentations)
                {
                    string t = kv.Key.ToString();
                    t = new string(' ', 4 - t.Length) + t;
                    t += ": ";
                    foreach (ScaleRepresentation sr in kv.Value)
                    {
                        string x = (sr == null ? "*" : sr.ColSpan.ToString());

                        t += new string(' ', 2 - x.Length) + x + ", ";
                    }
                    Debug.WriteLine(t);
                }

                Debug.Unindent();
            }
        }
        #endregion
    }

    #region RowCollapseOrder
    public class RowCollapseOrder
    {
        public List<Row> Shown = new List<Row>();
        public List<Row> Hidden = new List<Row>();
    }
    #endregion

    #region ColumnCollapseOrder
    public class ColumnCollapseOrder
    {

        List<ColumnCollapseSubOrder> SubOrderList = new List<ColumnCollapseSubOrder>();
        public int ColumnIndex;

        public ColumnCollapseOrder(int c)
        {
            ColumnIndex = c;
        }

        internal void AddRowOrder(Cell cell, float widthChange)
        {
            bool existed = false;
            foreach (ColumnCollapseSubOrder ccso in SubOrderList)
            {
                if (ccso.Victim == cell)
                {
                    ccso.ChangeToWidth += widthChange;
                    existed = true;
                    break;
                }
            }

            if (!existed)
                SubOrderList.Add(new ColumnCollapseSubOrder(cell, widthChange));
        }

        internal void Execute()
        {
            foreach (ColumnCollapseSubOrder ccso in SubOrderList)
                ccso.Execute();
        }

        internal bool HasItems()
        {
            return SubOrderList.Count > 0;
        }
    }
    #endregion

    #region ColumnCollapseSubOrder
    class ColumnCollapseSubOrder
    {
        public float ChangeToWidth = 0f;
        public Cell Victim = null;

        public ColumnCollapseSubOrder(Cell c, float f)
        {
            ChangeToWidth = f;
            Victim = c;
        }

        internal void Execute()
        {
            if (ChangeToWidth != 0)
            {
                Victim.Width += ChangeToWidth;
            }
        }
    }
    #endregion
}