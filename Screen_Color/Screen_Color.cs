using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace Screen_Color
{
    public class StringEventArgs : EventArgs
    {
        public string top { get; set; }
        public string bottom { get; set; }
    }

    public class ConsoleEventArgs : EventArgs
    {
        public string console { get; set; }
    }

    public class Screen
    {
        SerialPort port = null;
        bool state { get; set; }
        public string COM { get; set; }
        public Point[] top_Coords = new Point[5];
        public Point[] bottom_Coords = new Point[5];
        public string error { get; set; }

        public event EventHandler<StringEventArgs> CoordsReceived;
        public event EventHandler<ConsoleEventArgs> ConsoleOutput;

        CancellationTokenSource cts = null;
        public Screen()
        {
            state = false;
            COM = "COM3";
            error = "";
        }
        static public Bitmap CaptureFromScreen(Rectangle rect)
        {
            Bitmap bmpScreenCapture = null;
            Graphics p = null;
            try
            {
                bmpScreenCapture = new Bitmap(rect.Width, rect.Height);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (bmpScreenCapture != null)
            {
                p = Graphics.FromImage(bmpScreenCapture);
            }

            try
            {
                p.CopyFromScreen(rect.X,
                     rect.Y,
                     0, 0,
                     rect.Size,
                     CopyPixelOperation.SourceCopy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (p != null)
                p.Dispose();

            return bmpScreenCapture;
        }

        static public List<Color> horizontalColors(Point p, int x, int y, int points, Point[] coords)
        {

            List<Color> colors = new List<Color>();
            int offset = x / points;

            Rectangle rect = new Rectangle(p, new Size(x, y));

            Bitmap map = CaptureFromScreen(rect);
            int in_x = 0;
            int in_y = y / 2;
            if (map != null)
            {
                for (int i = 0; i < points; i++)
                {
                    in_x = i * offset;
                    coords[i].X = in_x;
                    coords[i].Y = p.Y;

                    colors.Add(map.GetPixel(in_x, in_y));
                    //Console.WriteLine("{0}: Red: {1}, Green: {2}, Blue: {3}", i, colors[i].R, colors[i].G, colors[i].B);
                }
            }
            return colors;
        }

        static public string getCOM()
        {
            string blah = null;
            return blah;
        }
        public void start()
        {
            state = true;
            cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(loop), cts.Token);
        }
        public void stop()
        {
            state = false;
        }

        public bool getState()
        {
            return state;
        }

        public SerialPort getPort()
        {
            return port;
        }
        /// <summary>
        /// Main loop for controlling the leds
        /// </summary>
        /// <param name="obj"></param>
        public void loop(object obj)
        {


            port = new SerialPort(COM, 9600);
            try
            {
                port.Open();
                List<System.Drawing.Color>[] horizontals = new List<System.Drawing.Color>[2];
                while (state)
                {
                    horizontals[0] = horizontalColors(new Point(0, 50), (int)SystemParameters.PrimaryScreenWidth, 4, 5, top_Coords);
                    horizontals[1] = horizontalColors(new Point(0, (int)SystemParameters.PrimaryScreenHeight - 50), (int)System.Windows.SystemParameters.PrimaryScreenWidth, 4, 5, bottom_Coords);
                    horizontals[0].Reverse();
                    //Helper.averageColor(points, port);
                    if (CoordsReceived != null)
                    {
                        string coords1 = null;
                        string coords2 = null;
                        for (int i = 0; i < top_Coords.Count(); i++)
                            coords1 += top_Coords[i];
                        for (int i = 0; i < bottom_Coords.Count(); i++)
                            coords2 += bottom_Coords[i];

                        CoordsReceived(this, new StringEventArgs()
                        {
                            top = coords1,
                            bottom = coords2,
                        });
                    }
                    ConsoleOutput(this, new ConsoleEventArgs()
                    {
                        console = ""
                    });
                    serialWrite(horizontals, port);
                }
                port.Close();
            }
            catch (Exception e)
            {
                if(ConsoleOutput!=null)
                error = e.Message;
                ConsoleOutput(this, new ConsoleEventArgs()
                {
                    console = error
                });

                Console.WriteLine(e);
            }
            port.Dispose();
        }
        /// <summary>
        /// Write the color arrays to Serial Port
        /// </summary>
        /// <param name="horizontal"> List of color value arrays</param>
        /// <param name="port"> The port to write to</param>
        static public void serialWrite(List<Color>[] horizontal, SerialPort port)
        {
            char letter;
            for (int i = 0; i < horizontal.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        letter = 'T';
                        break;
                    case 1:
                        letter = 'B';
                        break;
                    default:
                        letter = 'T';
                        break;
                }
                byte[] buffer = new byte[] { Convert.ToByte(letter) };
                port.Write(buffer, 0, 1);
                buffer = new byte[] { Convert.ToByte(horizontal[i].Count()) };
                port.Write(buffer, 0, 1);
                for (int m = 0; m < horizontal[i].Count(); m++)
                {

                    buffer = new byte[] { Convert.ToByte(horizontal[i][m].R) };
                    port.Write(buffer, 0, 1);
                    buffer = new byte[] { Convert.ToByte(horizontal[i][m].G) };
                    port.Write(buffer, 0, 1);
                    buffer = new byte[] { Convert.ToByte(horizontal[i][m].B) };
                    port.Write(buffer, 0, 1);
                }
            }
        }
    }
}
