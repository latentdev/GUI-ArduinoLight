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
        /// <summary>
        /// port to be used for writing color data
        /// </summary>
        SerialPort port = null;
        /// <summary>
        /// state of the main loop. true = running. false = stopped
        /// </summary>
        bool state { get; set; }
        /// <summary>
        /// String containing COM port to use
        /// </summary>
        public string COM { get; set; }
        /// <summary>
        /// an array of the # of points used on each side (left, top, right and bottom)
        /// </summary>
        public int[] points = new int[4];
        /// <summary>
        /// coordinates sampled along the top of the screen
        /// </summary>
        public Point[] top_Coords = new Point[5];
        /// <summary>
        /// coordinates sampled along the bottom of the screen
        /// </summary>
        public Point[] bottom_Coords = new Point[5];
        /// <summary>
        /// string containing error to display on GUI
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// eventhandler for coordinates updating
        /// </summary>
        public event EventHandler<StringEventArgs> CoordsReceived;
        /// <summary>
        /// eventhandler for console output changing
        /// </summary>
        public event EventHandler<ConsoleEventArgs> ConsoleOutput;
        /// <summary>
        /// token used for cancelling main loop
        /// </summary>
        CancellationTokenSource cts = null;
        /// <summary>
        /// default constructor. sets needed values to defaults.
        /// </summary>
        public Screen()
        {
            state = false;
            COM = "";
            error = "";
            points[0] = 5;
            points[1] = 5;
            points[2] = 5;
            points[3] = 5;
        }
        /// <summary>
        /// grabs a bitmap of the screen based off the size of the given rectangle
        /// </summary>
        /// <param name="rect">rectangle containing an origin point and size of the area of the screen you want to capture</param>
        /// <returns>bitmap containing the section of the screen defined by the rectangle passed in</returns>
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
        /// <summary>
        /// samples a bitmap for pixel colors, storing them in a list and returning it.
        /// </summary>
        /// <param name="p">Origin point for the rectangle that will be the bitmap</param>
        /// <param name="x">Width of the rectangle that will be the bitmap</param>
        /// <param name="y">Height of the rectangle that will be the bitmap</param>
        /// <param name="points">Number of points to sample from the bitmap</param>
        /// <param name="coords">Array that stores sampled pixel coordinates</param>
        /// <returns>A list of sampled pixel colors</returns>
        static public List<Color> horizontalColors(Point p, int x, int y, int points, Point[] coords)
        {

            List<Color> colors = new List<Color>();
            int offset;
            if (points != 0)
                offset = x / points;
            else
                offset = 0;

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

        /// <summary>
        /// sets the number of points used on each side of screen (left,top,right and bottom). Array passed in should always be 4 long (unless your monitor has more than 4 sides?)
        /// </summary>
        /// <param name="in_points">an integer array containing the number of points to use on each side</param>
        public void setPoints(int [] in_points)
        {
            points = in_points;
        }

        /// <summary>
        /// return COM port
        /// </summary>
        /// <returns></returns>
        static public string getCOM()
        {
            string blah = null;
            return blah;
        }
        /// <summary>
        /// Start event for main loop
        /// </summary>
        public void start()
        {
            if(cts==null|| cts.IsCancellationRequested)
                cts = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(new WaitCallback(loop), cts.Token);
        }

        /// <summary>
        /// stop event for main loop
        /// </summary>
        public void stop()
        {
            state = false;
            if (cts != null)
            {
                if (!cts.IsCancellationRequested)
                {

                    cts.Cancel();
                    try
                    {
                        Task.WaitAll();
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        cts.Dispose();
                    }

                }
            }
            serialWriteStop();
        }
        /// <summary>
        /// get the state of the main loop. true = running. false = stopped.
        /// </summary>
        /// <returns>state of the main loop</returns>
        public bool getState()
        {
            return state;
        }
        /// <summary>
        /// get the port.
        /// </summary>
        /// <returns>returns the current port</returns>
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
            try
            {
                port = new SerialPort(COM, 9600);

                try
                {
                    state = true;
                    port.Open();
                    List<Color>[] horizontals = new List<Color>[2];
                    while (state)
                    {
                        horizontals[0] = horizontalColors(new Point(0, 50), (int)SystemParameters.PrimaryScreenWidth, 4, points[1], top_Coords);
                        horizontals[1] = horizontalColors(new Point(0, (int)SystemParameters.PrimaryScreenHeight - 50), (int)System.Windows.SystemParameters.PrimaryScreenWidth, 4, points[3], bottom_Coords);
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
                    //writeClear();
                    //port.Close();
                }
                catch (Exception e)
                {
                    if (ConsoleOutput != null)
                        error = e.Message;
                    ConsoleOutput(this, new ConsoleEventArgs()
                    {
                        console = error
                    });
                    state = false;
                    Console.WriteLine(e);
                }
                //port.Dispose();
            }
            catch (Exception e)
            {
                if (ConsoleOutput != null)
                    error = "You must choose a port";
                ConsoleOutput(this, new ConsoleEventArgs()
            {
            console = error
            });
            }
}
        /// <summary>
        /// writes to serial a clear state to turn leds off.
        /// </summary>
        public void serialWriteStop()
        {
            char letter = 'E';
            int count = points[1] + points[3];
            byte[] buffer = new byte[] { Convert.ToByte(letter) };
            port.Write(buffer, 0 ,1);
            buffer = new byte[] { Convert.ToByte(count) };
            port.Write(buffer, 0, 1);
            for (int i=0;i<count;i++)
            {
                buffer = new byte[] { Convert.ToByte(0) };
                port.Write(buffer, 0, 1);
                buffer = new byte[] { Convert.ToByte(0) };
                port.Write(buffer, 0, 1);
                buffer = new byte[] { Convert.ToByte(0) };
                port.Write(buffer, 0, 1);
            }
            port.Close();
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
