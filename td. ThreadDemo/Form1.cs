using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;

namespace Lab_3
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        const int VK_MENU = 0x12; // Alt

        Stream myStream;
        MyThread[] threadLst;
        const int xCell = 30;
        const int yCell = 30;
        const int xMax = 25;
        const int yMax = 20;
        public Random rnd = new Random();

        void MySleep(long count)
        {
            long i, j, k = 0;
            for (i = 0; i < count; i++)
                for (j = 0; j < 64000; j++) k = k + 1;
        }
        void Running(object obj)
        {
            MyThread p = (MyThread)obj;
            Graphics g = this.CreateGraphics();
            Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0));
            int x1, y1;
            int x2, y2;
            int x, y;
            bool kq = true;
            try
            {
                while (p.start)
                {
                    x1 = p.Pos.X; y1 = p.Pos.Y;
                    g.DrawImage(p.Pic, xCell * x1, yCell * y1);
                    Color c = p.Pic.GetPixel(1, 1);
                    int yR, yG, yB;
                    if (c.R > 128) yR = 0; else yR = 255;
                    if (c.G > 128) yG = 0; else yG = 255;
                    if (c.B > 128) yB = 0; else yB = 255;
                    Pen pen = new Pen(Color.FromArgb(yR, yG, yB), 2);
                    if (p.tx >= 0 && p.ty >= 0)
                    {
                        x = xCell * x1 + xCell - 2;
                        y = yCell * y1 + yCell - 2;
                        g.DrawLine(pen, x, y, x - 10, y);
                        g.DrawLine(pen, x, y, x, y - 10);
                    }
                    else if (p.tx >= 0 && p.ty < 0)
                    {
                        x = xCell * x1 + xCell - 2;
                        y = yCell * y1 + 2;
                        g.DrawLine(pen, x, y, x - 10, y);
                        g.DrawLine(pen, x, y, x, y + 10);
                    }
                    else if (p.tx < 0 && p.ty >= 0)
                    {
                        x = xCell * x1 + 2;
                        y = yCell * y1 + yCell - 2;
                        g.DrawLine(pen, x, y, x + 10, y);
                        g.DrawLine(pen, x, y, x, y - 10);
                    }
                    else
                    {
                        x = xCell * x1 + 2;
                        y = yCell * y1 + 2;
                        g.DrawLine(pen, x, y, x + 10, y);
                        g.DrawLine(pen, x, y, x, y + 10);
                    }
                    MySleep(500);
                    p.HieuchinhVitri();
                    x2 = p.Pos.X; y2 = p.Pos.Y;
                    g.FillRectangle(brush, xCell * x1, yCell * y1, xCell, yCell);
                    if (kq == false && p.start == false)
                    {
                        p.t.Abort();
                        p.stop = true;
                        return;
                    }
                }
            }
            catch (Exception e) { p.t.Abort(); }
            x1 = p.Pos.X; y1 = p.Pos.Y;
            g.FillRectangle(brush, xCell * x1, yCell * y1, xCell, yCell);
            p.stop = true;
            p.t.Abort();
        }
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            threadLst = new MyThread[26];
            for (int i = 0; i < 26; i++)
            {
                threadLst[i] = new MyThread(rnd, xMax, yMax);
                threadLst[i].stop = threadLst[i].suspend = threadLst[i].start = false;
                myStream = myAssembly.GetManifestResourceStream("td.ThreadDemo.Resources." + (i + 1) + ".bmp");
                threadLst[i].Pic = new Bitmap(myStream);
                threadLst[i].Xmax = 25;
                threadLst[i].Ymax = 20;
            }
            ClientSize = new Size(25 * 30, 20 * 30);
            this.Location = new Point(0, 0);
            this.BackColor = Color.Black;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int newch = e.KeyValue;
            if (newch < 0x41 || newch > 0x5a) return;
            int index = newch - 65;
            if (e.Control && e.Shift)
            {
                threadLst[index].start = false;
            }

            else if (e.Control && e.Alt)
            {
                if (threadLst[index].t != null &&
                    threadLst[index].start &&
                    !threadLst[index].suspend)
                {
                    threadLst[index].t.Suspend();
                    threadLst[index].suspend = true;
                }
            }

            else if (e.Control)
            {
                threadLst[index].t.Priority = ThreadPriority.Lowest;
                MessageBox.Show("Lowest");
            }

            else if (e.Alt)
            {
                if (threadLst[index].t != null &&
                    threadLst[index].start &&
                    threadLst[index].suspend)
                {
                    threadLst[index].t.Resume();
                    threadLst[index].suspend = false;
                }
            }

            else if (e.Shift)
            {
                threadLst[index].t.Priority = ThreadPriority.Highest;
                MessageBox.Show("Highest");
            }

            else
            {
                if (!threadLst[index].start)
                {
                    threadLst[index].start = true;
                    threadLst[index].suspend = false;

                    threadLst[index].t = new Thread(
                        new ParameterizedThreadStart(Running));

                    if (index == 0)
                        threadLst[index].t.Priority = ThreadPriority.Highest;
                    else
                        threadLst[index].t.Priority = ThreadPriority.Lowest;

                    threadLst[index].t.Start(threadLst[index]);
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            int i;
            for (i = 0; i < 26; i++)
                if (threadLst[i].start)
                {
                    threadLst[i].start = false;
                    while (!threadLst[i].stop) ;
                }
        }


    }
}
