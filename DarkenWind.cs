using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Runtime.InteropServices;

namespace DarkenWind
{
    class Win32
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }

    struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    class dust
    {
        public double x;
        public double y;
        public double dx;
        public double dy;
        public double alpha;
        public Color color;
        public Pen pen;
    }

    class wav
    {
        public static int headerSize = 36;
        public static int fmtSize = 16;
        public static short tag = 1;
        public static short channels= 1;
        public static int sampleRate = 44100;
        public static short bitsPerSample = 16;
        public static short blockAlign = 1 * 2;
        public static int averageBytesPerSecond = sampleRate * blockAlign;
        public static short extraSize = 0;
    }

    class shotSound
    {
        static int length = (int)(wav.averageBytesPerSecond * 0.2);
        SoundPlayer player = new SoundPlayer();
        MemoryStream stream = new MemoryStream(new byte[wav.headerSize + length + 8]);
        Random rand = new Random();

        public shotSound()
        {
            writeHeader(stream, length);
            writeBody(stream, length);
            stream.Position = 0;
            player.Stream = stream;
        }

        public void Play()
        {
            player.Play();
        }

        void writeHeader(Stream st, int len)
        {
            using (var b = new BinaryWriter(st, Encoding.UTF8, true))
            {
                b.Write(new char[4] { 'R', 'I', 'F', 'F' });
                b.Write(wav.headerSize + len);
                b.Write(new char[4] { 'W', 'A', 'V', 'E' });
                b.Write(new char[4] { 'f', 'm', 't', ' ' });
                b.Write(wav.fmtSize);
                b.Write(wav.tag);
                b.Write(wav.channels);
                b.Write(wav.sampleRate);
                b.Write(wav.averageBytesPerSecond);
                b.Write(wav.blockAlign);
                b.Write(wav.bitsPerSample);
                b.Write(new char[4] { 'd', 'a', 't', 'a' });
                b.Write(len);
            }
        }

        void writeBody(Stream st, int len)
        {
            using (var b = new BinaryWriter(st, Encoding.UTF8, true))
            {
                var m = 2;
                var w = 8;
                var l = m + w + m;
                var n = len / 2 / l;
                for (var i = 0; i < n; i++)
                {
                    var s = rand.Next(3) * short.MaxValue / 16;
                    var t = s * (n - i) / n;
                    for (var j = 0; j < m; j++)
                        b.Write((short)(t / w * j));
                    for (var j = 0; j < w; j++)
                        b.Write((short)(t));
                    for (var j = 0; j < m; j++)
                        b.Write((short)(t / w * (w - j)));
                }
            }
        }
    }
 
    public class Layer : Form
    {
        const int MAX_DUSTS = 500;
        Tuple<int, int> DUST_NUM_RANGE = new Tuple<int, int>(5, 12);
        double DUST_GRAVITYY = 0.075;
        double DUST_FADEOUT = 0.96;
        Tuple<double, double> DUST_V_X_RANGE = new Tuple<double, double>(-1.0, 1.0);
        Tuple<double, double> DUST_V_Y_RANGE = new Tuple<double, double>(-3.5, -1.5);
        Timer timer = new Timer();
        Color bgColor = Color.Black;
        dust[] dusts = new dust[MAX_DUSTS];
        int index = 0;
        bool has_frame = false;
        Random rand = new Random();
        shotSound shot = new shotSound();

        public Layer(string handle, string show_frame)
        {
            var hwnd = (IntPtr)int.Parse(handle);
            if (show_frame == "0")
              this.FormBorderStyle = FormBorderStyle.None;
            else
              has_frame = true;

            this.Text = "DarkenWind";
            this.StartPosition = FormStartPosition.Manual;
            this.TransparencyKey = this.BackColor;
            this.Shown += (EventHandler)((s,e) => {
                this.TopMost = true;
                Win32.SetForegroundWindow(hwnd);
            });
            this.timer.Interval = 30;
            this.timer.Enabled = true;
            this.timer.Tick += dusts_tick;
            windowSetting(hwnd);
            Task.Run(() =>
            {
                var run = true;
                while(run)
                {
                    try
                    {
                        var q = Console.ReadLine().Split(' ');
                        switch (q[0])
                        {
                            case "shot":
                                putout(Int32.Parse(q[1]), Int32.Parse(q[2]), Int32.Parse(q[3]));
                                shot.Play();
                                break;
                            case "bg":
                                this.bgColor = Color.FromArgb(Int32.Parse(q[1]));
                                break;
                            case "set":
                                windowSetting((IntPtr)int.Parse(q[1]));
                                break;
                            case "exit":
                                run = false;
                                break;
                            default:
                                putout(60, 60, 55555);
                                shot.Play();
                                break;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                Application.Exit();
            });
        }
 

        void windowSetting(IntPtr hwnd)
        {
            RECT rect;
            var result = Win32.GetWindowRect(hwnd, out rect);
            if (!result)
            {
                Console.WriteLine("Tell me window handle");
                Application.Exit();
            }
            var x = rect.Left;
            var y = rect.Top;
            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;
            this.SetBounds(x, y, w, h);
        }

        void putout(int x, int y, int color)
        {
            var num = rand.Next(DUST_NUM_RANGE.Item1, DUST_NUM_RANGE.Item2);
            var vx1 = DUST_V_X_RANGE.Item1;
            var vx2 = DUST_V_X_RANGE.Item2;
            var vy1 = DUST_V_Y_RANGE.Item1;
            var vy2 = DUST_V_Y_RANGE.Item2;
            for(var i = 0; i < num; i++)
            {
                var col = Color.FromArgb(color);
                var dust = new dust {
                    x = x + rand.Next(8),
                    y = y + rand.Next(8),
                    dx = vx1 + rand.NextDouble() * (vx2 - vx1),
                    dy = vy1 + rand.NextDouble() * (vy2 - vy1),
                    alpha = 1,
                    color = col,
                    pen = new Pen(col)
                };
                dusts[index] = dust;
                index = (index + 1) % MAX_DUSTS;
            }
        }

        void dusts_tick(object sender, EventArgs e)
        {
            foreach(var dust in dusts)
            {
                if (dust == null || dust.alpha < 0.1)
                    continue;
                dust.dy += DUST_GRAVITYY;
                dust.x += dust.dx;
                dust.y += dust.dy;
                dust.alpha *= DUST_FADEOUT;
                var a = dust.alpha;
                var c = dust.color;
                var d = this.bgColor;
                var r = c.R * a + d.R * (1 - a);
                var g = c.G * a + d.G * (1 - a);
                var b = c.B * a + d.B * (1 - a);
                dust.pen = new Pen(Color.FromArgb((int)r, (int)g, (int)b));
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var title = 0;
            if (has_frame) title = 32;

            foreach (var dust in dusts)
            {
                if (dust == null || dust.alpha < 0.1)
                    continue;
                var x = (int)dust.x;
                var y = (int)dust.y - title;
                e.Graphics.DrawLine(dust.pen, x, y, x + 1, y + 1);
            }
        }
    }
}
