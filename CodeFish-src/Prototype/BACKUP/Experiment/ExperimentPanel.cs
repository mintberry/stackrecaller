using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Prototype.Experiment
{
    public partial class ExperimentPanel : UserControl
    {
        public ExperimentPanel()
        {
            InitializeComponent();
        }

        private int _lastTask = 0;
        private List<string> _taskList1 = new List<string>();
        private List<string> _taskList2 = new List<string>();
        private DateTime _startTime = DateTime.Now;

        private void ExperimentPanel_Load(object sender, EventArgs e)
        {
            panel1.Visible = false;
            
                string s = Path.GetDirectoryName(Application.ExecutablePath) + @"\Experiment\Introduction.htm";
                if(File.Exists(s))
                    webBrowser1.Url = new Uri(s);
        }

        private void BuildTaskList(TaskSet taskset, Panel p, TabPage tab, EventHandler startHandler, EventHandler buttonHandler, List<string> taskList, bool enableTab)
        {
            Button startB = new Button();
            Label startL = new Label();

            p.Tag = taskList;

            startB.Text = "Start";
            startB.Name = "Start";
            startB.Dock = DockStyle.Top;
            startB.Click += new EventHandler(startHandler);

            startL.Text = "Click the button above to start the experiment." + Environment.NewLine + Environment.NewLine + "When the experiment is started a list of tasks is shown. You'll have to solve each of the tasks individually and as fast as possible. When finished or wanting to skip a task press the next button to activate the next task." + Environment.NewLine + Environment.NewLine + "Once the next button has been clicked you cannot edit your answer.";
            startL.Top = startB.Bottom + 10;
            startL.AutoSize = true;
            startL.MaximumSize = new Size(Width - 50, 5000);

            startB.Enabled = enableTab;

            tab.Controls.Add(startB);
            tab.Controls.Add(startL);

            // open file
            StreamReader sr = File.OpenText("./Experiment/Tasks/" + taskset.tasksfile);
            
            // read source filename
            taskset.sourcefile = sr.ReadLine();
            startB.Tag = taskset;

            // read lines 
            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine();
                taskList.Add(line);
            }

            int buttonWidth = 80;
            int y = 0;
            for (int i = 0; i < taskList.Count; i++)
            {

                Label l = new Label();
                l.Text = "(" + (i + 1).ToString() + ") " + taskList[i];
                l.Top = y;
                l.Font = new Font(l.Font, FontStyle.Bold);
                l.AutoSize = true;
                l.MaximumSize = new Size(panel1.Width - 50, 200);
                l.Name = "Label" + i.ToString();
                l.Visible = false;
                p.Controls.Add(l);

                y += l.Height + 5;

                TextBox t = new TextBox();
                t.Multiline = true;
                t.WordWrap = true;
                t.Height = t.Height * 3;
                t.Width = Width - buttonWidth - 60;
                t.Top = y;
                t.Left = 5;
                if (i != 0)
                    t.Enabled = false;
                t.Name = "Answer" + i.ToString();
                p.Controls.Add(t);

                y += t.Height + 20;

                Button b = new Button();
                b.Left = t.Right + 10;
                b.Width = buttonWidth;
                b.Text = "Next";
                b.Click += new EventHandler(buttonHandler);
                b.Top = t.Top;
                if (i != 0)
                    b.Enabled = false;
                b.Name = "Button" + i.ToString();
                p.Controls.Add(b);

            }
            p.Visible = false;
        }

        void start1_Click(object sender, EventArgs e)
        {
            Model.Default.DocumentFile = ((TaskSet)((Button)sender).Tag).sourcefile;
            Model.Default.DOIStrategy = (((TaskSet)((Button)sender).Tag).strategy == "semantic") ? (IDOIStrategy)new StaticDOI() : (IDOIStrategy)new DynamicDOI();
            _startTime = DateTime.Now;
            foreach (Control c in tasksTab.Controls)
            {
                c.Visible = false;
            }
            panel1.Controls["Label" + 0].Visible = true;
            panel1.Visible = true;
        }

        void start2_Click(object sender, EventArgs e)
        {
            Model.Default.DocumentFile = ((TaskSet)((Button)sender).Tag).sourcefile;
            Model.Default.DOIStrategy = (((TaskSet)((Button)sender).Tag).strategy == "semantic") ? (IDOIStrategy)new StaticDOI() : (IDOIStrategy)new DynamicDOI();
            _startTime = DateTime.Now;
            foreach (Control c in tasksTab2.Controls)
            {
                c.Visible = false;
            }
            panel2.Controls["Label" + 0].Visible = true;
            panel2.Visible = true;
        }

        void b1_Click(object sender, EventArgs e)
        {

            NextClicked(panel1);
        }

        void b2_Click(object sender, EventArgs e)
        {

            NextClicked(panel2);
        }

        private void NextClicked(Panel p)
        {
            Logger.Default.Log(Logger.EntryType.NextQuestion, null);

            p.Controls["Answer" + _lastTask].Tag = DateTime.Now;

            for (int i = 0; i <= _lastTask; i++)
            {
                p.Controls["Answer" + i].Enabled = false;
                p.Controls["Button" + i].Enabled = false;
                p.Controls["Label" + i].Enabled = false;
            }

            _lastTask++;

            if (_lastTask < ((List<string>)p.Tag).Count)
            {
                p.Controls["Answer" + _lastTask].Enabled = true;
                p.Controls["Answer" + _lastTask].Visible = true;

                p.Controls["Button" + _lastTask].Enabled = true;
                p.Controls["Button" + _lastTask].Visible = true;

                p.Controls["Label" + _lastTask].Enabled = true;
                p.Controls["Label" + _lastTask].Visible = true;
            }

            StreamWriter sw = File.CreateText(_startTime.ToString("yyyyMMddHHmm") + " - Answers to " + ExperimentInfo.Instance.CurrentTaskSet + " from " + ExperimentInfo.Instance.ParticipantID + ".txt");
            DateTime lastTime = _startTime;
            for (int i = 0; i < _lastTask; i++)
            {
                string answer = p.Controls["Answer" + i.ToString()].Text;
                DateTime t = ((DateTime)p.Controls["Answer" + i.ToString()].Tag);
                string completionTime = ((TimeSpan)(t - lastTime)).TotalMilliseconds.ToString();
                string line = answer + "|" + completionTime;
                sw.WriteLine(line);
                lastTime = t;
            }
            sw.Close();

            if (p == panel1 && _lastTask == _taskList1.Count)
            {
                MessageBox.Show("Please ask the experiment leader for the questionaire to the tasks you just completed.");
                _lastTask = 0;
                _startTime = DateTime.Now;
                tasksTab2.Controls["Start"].Enabled = true;
                tabControl1.SelectedTab = tasksTab2;
                ExperimentInfo.Instance.CurrentTaskSet = ExperimentInfo.Instance.TaskSet2;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Default.Log(Logger.EntryType.Init, "");

                ExperimentInfo.Instance.ParticipantID = int.Parse(textBox1.Text);

                button1.Enabled = false;
                textBox1.Enabled = false;

                BuildTaskList(ExperimentInfo.Instance.TaskSet1, panel1, tasksTab, start1_Click, b1_Click, _taskList1, true);
                BuildTaskList(ExperimentInfo.Instance.TaskSet2, panel2, tasksTab2, start2_Click, b2_Click, _taskList2, false);
                ExperimentInfo.Instance.CurrentTaskSet = ExperimentInfo.Instance.TaskSet1;
                textBox1.BackColor = Color.White;
            }
            catch (FormatException)
            {
                textBox1.BackColor = Color.Red;
            }


        }      
    }
}
