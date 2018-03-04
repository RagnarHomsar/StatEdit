using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StatEdit
{
    public partial class StatEditForm : Form
    {
        private static string[] Classes;
        private static string[] Stats;
        private static System.Drawing.Color[] ClassLineColors;
        private StatTable st;

        public StatEditForm()
        {
            InitializeComponent();

            Classes = new string[]
            {
                "Princess",
                "Gladiator",
                "Hoplite",
                "Buccaneer",
                "Ninja",
                "Monk",
                "Zodiac",
                "Wildling",
                "Arbalist",
                "Farmer",
                "Shogun",
                "Yggdroid"
            };

            Stats = new string[]
            {
                "HP",
                "TP",
                "STR",
                "TEC",
                "VIT",
                "AGI",
                "LUC"
            };

            ClassLineColors = new System.Drawing.Color[]
            {
                System.Drawing.Color.Black,             // Princess
                System.Drawing.Color.BlueViolet,        // Gladiator
                System.Drawing.Color.Fuchsia,        // Hoplite
                System.Drawing.Color.Crimson,           // Buccaneer
                System.Drawing.Color.DarkOrange,        // Ninja
                System.Drawing.Color.DarkTurquoise,     // Monk
                System.Drawing.Color.DeepPink,          // Zodiac
                System.Drawing.Color.Green,             // Wildling
                System.Drawing.Color.Lime,              // Arbalist
                System.Drawing.Color.OrangeRed,         // Farmer
                System.Drawing.Color.Peru,              // Shogun
                System.Drawing.Color.Sienna,          // Yggdroid
            };

            classList.Items.AddRange(Classes);
            classList.SelectedIndex = 0;

            functionStatSelector.Items.AddRange(Stats);
            functionStatSelector.SelectedIndex = 0;

            classList.SelectedIndexChanged += UpdateStats;
            levelEntry.ValueChanged += UpdateStats;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void graphApply_Click(object sender, EventArgs e)
        {
            ChartCheckOneClassSelected();

            var selectedClasses = chartClassBox.Controls
                .OfType<CheckBox>()
                .Where(x => x.Checked == true);

            var statId = int.Parse(
                chartStatBox.Controls
                .OfType<RadioButton>()
                .FirstOrDefault(x => x.Checked == true)
                .Tag.ToString()
            );

            var chartForm = new Form();
            chartForm.Size = new System.Drawing.Size(1280, 720);
            chartForm.Icon = Icon;
            chartForm.Resize += ChartFormResized;

            var chartObject = new Chart();
            chartObject.Size = chartForm.ClientSize;
            chartObject.Series.Clear();

            var chartArea = new ChartArea();
            chartArea.AxisX.Minimum = 1;
            chartArea.AxisX.Maximum = 99;
            chartArea.AxisX.Interval = 10;
            chartArea.AxisX.Title = "Level";

            for (int i = 5; i < 105; i += 10)
            {
                chartArea.AxisX.CustomLabels.Add((i + 1), (i + 11), (i + 5).ToString());
            }

            // if statId is 2 or greater, then we're dealing with non-HP/TP stats
            if (statId >= 2)
            {
                chartArea.AxisY.Minimum = 0;
                chartArea.AxisY.Maximum = 99;
                chartArea.AxisY.Interval = 10;
            }

            else
            {
                chartArea.AxisY.Minimum = 0;
                chartArea.AxisY.Maximum = 800;
                chartArea.AxisY.Interval = 100;
            }

            chartArea.AxisY.Title = "Stat";

            chartObject.ChartAreas.Add(chartArea);

            chartForm.Text = "StatEdit Chart: " + functionStatSelector.Items[statId];

            var legend = new Legend(functionStatSelector.Items[statId].ToString());

            // this is reversed so the legend follows the normal class order
            foreach (var selectedClass in selectedClasses.Reverse())
            {
                var classId = classList.Items.IndexOf(selectedClass.Text);

                var classSeries = new Series()
                {
                    Color = ClassLineColors[classId],
                    Name = Classes[classId],
                    IsXValueIndexed = true,
                    ChartType = SeriesChartType.Line
                };

                for (int i = 1; i < StatTable.LEVELS_PER_CLASS; i++)
                {
                    var statAtLevel = st.Entries[classId, i].ToByteArray()[statId];

                    classSeries.Points.AddXY(i, statAtLevel);
                }

                classSeries.Legend = functionStatSelector.Items[statId].ToString();
                classSeries.IsVisibleInLegend = true;

                chartObject.Series.Add(classSeries);
            }

            chartObject.Legends.Add(legend);

            chartForm.Controls.Add(chartObject);
            chartObject.Location = new System.Drawing.Point(0, 0);

            chartForm.Show();
        }

        private void ChartFormResized(object sender, EventArgs e)
        {
            var form = (Form) sender;

            // screw it, the chart's going to be the only object in the form
            form.Controls[0].Size = form.ClientSize;
        }

        private void UpdateStats(object sender, EventArgs e)
        {
            if (st != null)
            {
                var classId = classList.SelectedIndex;
                var level = (int) levelEntry.Value;

                hpEntry.Text = st.Entries[classId, level].hp.ToString();
                tpEntry.Text = st.Entries[classId, level].tp.ToString();
                strEntry.Text = st.Entries[classId, level].str.ToString();
                tecEntry.Text = st.Entries[classId, level].tec.ToString();
                vitEntry.Text = st.Entries[classId, level].vit.ToString();
                agiEntry.Text = st.Entries[classId, level].agi.ToString();
                lucEntry.Text = st.Entries[classId, level].luc.ToString();
            }
        }

        private void openStatTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openTableDialog = new OpenFileDialog();
            openTableDialog.Filter = "GrowthTable.tbl|GrowthTable.tbl";

            if (openTableDialog.ShowDialog() == DialogResult.OK)
            {
                st = new StatTable(openTableDialog.FileName);
            }

            UpdateStats(null, null);
        }

        private bool ChartCheckOneClassSelected()
        {
            var firstChecked = chartClassBox.Controls
                .OfType<CheckBox>()
                .FirstOrDefault(x => x.Checked == true);

            // if firstChecked is not null, at least one box is selected
            // otherwise, no boxes are selected
            return (firstChecked != null);
        }
    }
}
