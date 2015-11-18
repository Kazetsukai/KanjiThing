using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Serialization;

namespace KanjiWriter
{
    public partial class Form1 : Form
    {
        private bool _started = false;
        private bool _drawing = false;
        private List<List<TimePoint>> _segments;
        private List<TimePoint> _points; 
        private DateTime _start;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_started)
            {
                _segments = new List<List<TimePoint>>();
                _start = DateTime.Now;
                _started = true;
            }
            if (!_drawing)
            {
                _points = new List<TimePoint>();
                _segments.Add(_points);
                _drawing = true;
            }

            AddPoint(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _drawing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_drawing)
                AddPoint(e.X, e.Y);
        }

        private void AddPoint(int x, int y)
        {
            _points.Add(new TimePoint
            {
                X = x, 
                Y = y, 
                TimeFromStart = (int)(DateTime.Now - _start).TotalMilliseconds
            });

            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawLines(e.Graphics);
        }

        private void DrawLines(Graphics graphics)
        {
            if (_segments == null)
                return;

            foreach (var segment in _segments)
            {
                if (segment.Count > 1)
                    graphics.DrawLines(Pens.Black, segment.Select(p => new Point(p.X, p.Y)).ToArray());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Save kana/kanji //
            /////////////////////

            if (_segments == null) return;

            if (textBox1.Text.Length > 0 && char.IsSurrogatePair(textBox1.Text, 0))
                throw new NotImplementedException("Welp, looks like I need surrogate pair support now!");

            // This might not strictly be the most correctest unicoding...
            var codePoint = string.Format("{0:X5}", char.ConvertToUtf32(textBox1.Text, 0));
            this.Text = "U+" + codePoint;

            var dir = @"kanji\" + codePoint;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Get the highest existing number, and go one up.
            int maxNum = -1;
            var listing = Directory.EnumerateFiles(dir).Select(f => new FileInfo(f).Name);
            foreach (var file in listing)
            {
                int num = 0;
                if (int.TryParse(file.Split('.').DefaultIfEmpty("").First(), out num) && num > maxNum)
                    maxNum = num;
            }
            maxNum++;

            var filenameData = dir + "\\" + maxNum + ".ji";
            var filenamePng = dir + "\\" + maxNum + ".png";

            using (var stream = File.OpenWrite(filenameData))
            {
                var writer = new StreamWriter(stream);
                
                foreach (var s in _segments)
                {
                    foreach (var p in s)
                    {
                        writer.WriteLine("{0} {1} {2}", p.X, p.Y, p.TimeFromStart);
                    }
                    writer.WriteLine();
                }

                writer.Flush();
                writer.Close();
            }

            Bitmap b = new Bitmap(109, 109);
            var graphics = Graphics.FromImage(b);
            graphics.ScaleTransform(0.5f, 0.5f);
            DrawLines(graphics);
            b.Save(filenamePng);

            _segments = null;
            _started = false;
            pictureBox1.Invalidate();
        }
    }

    public struct TimePoint
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int TimeFromStart { get; set; }
    }
}
