using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClockApp
{
    public partial class Form1 : Form
    {
        private PictureBox clockPictureBox;
        // エラーを避けるため、明示的にWindows FormsのTimerを指定します
        private System.Windows.Forms.Timer animationTimer;

        public Form1()
        {
            // 既存のコンポーネント初期化（デザイナ用）を呼び出す
            InitializeComponent();
            SetupClock();
        }

        private void SetupClock()
        {
            this.Text = "Analog Clock";
            this.Size = new Size(400, 420);

            clockPictureBox = new PictureBox();
            clockPictureBox.Dock = DockStyle.Fill;
            clockPictureBox.BackColor = Color.White;
            
            // チラつき防止（ダブルバッファリング）
            typeof(PictureBox)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(clockPictureBox, true);

            clockPictureBox.Paint += ClockPictureBox_Paint;
            this.Controls.Add(clockPictureBox);

            // 明示的にFormsのTimerとして生成
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += (s, e) => clockPictureBox.Invalidate();
            animationTimer.Start();

            this.Resize += (s, e) => clockPictureBox.Invalidate();
        }

        private void ClockPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = clockPictureBox.Width;
            int h = clockPictureBox.Height;

            DrawClock(g, w, h);
        }

        private void DrawClock(Graphics g, int w, int h)
        {
            DateTime date = DateTime.Now;
            float radius = Math.Min(w, h) / 2f * 0.9f;
            float cx = w / 2f;
            float cy = h / 2f;

            g.Clear(clockPictureBox.BackColor);

            using (Pen pen = new Pen(Color.FromArgb(0x33, 0x33, 0x33), 5))
            {
                g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            for (int i = 0; i < 60; i++)
            {
                double angle = (i / 60.0) * 2.0 * Math.PI - Math.PI / 2.0;
                float outer = radius * 0.95f;
                float inner = (i % 5 == 0) ? radius * 0.85f : radius * 0.90f;
                float lineWidth = (i % 5 == 0) ? 4f : 2f;

                using (Pen pen = new Pen(Color.Black, lineWidth))
                {
                    float x1 = cx + (float)Math.Cos(angle) * inner;
                    float y1 = cy + (float)Math.Sin(angle) * inner;
                    float x2 = cx + (float)Math.Cos(angle) * outer;
                    float y2 = cy + (float)Math.Sin(angle) * outer;
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            void DrawHand(double value, double max, float len, float width, Color color)
            {
                double angle = (value / max) * 2.0 * Math.PI - Math.PI / 2.0;
                using (Pen pen = new Pen(color, width))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    float targetX = cx + (float)Math.Cos(angle) * len;
                    float targetY = cy + (float)Math.Sin(angle) * len;
                    g.DrawLine(pen, cx, cy, targetX, targetY);
                }
            }

            double sec = date.Second + (date.Millisecond / 1000.0);
            double min = date.Minute + sec / 60.0;
            double hour = (date.Hour % 12) + min / 60.0;

            DrawHand(hour, 12, radius * 0.5f, 6, Color.Black);
            DrawHand(min, 60, radius * 0.75f, 4, Color.FromArgb(0x44, 0x44, 0x44));
            DrawHand(sec, 60, radius * 0.85f, 2, Color.FromArgb(0xD0, 0x00, 0x00));

            using (Brush brush = new SolidBrush(Color.Black))
            {
                float pinRadius = 6f;
                g.FillEllipse(brush, cx - pinRadius, cy - pinRadius, pinRadius * 2, pinRadius * 2);
            }
        }
    }
}
