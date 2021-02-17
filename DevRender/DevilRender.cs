using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Avalonia.Threading;
using System.Windows;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia;

namespace DevRender
{
    public class DevRender
    {
        public DispatcherTimer Timer { get; private set; }
        public BufferPreparer Preparer { get; private set; }
        public List<Camera> Cameras { get; private set; }
        //public Form RenderForm { get; private set; }
        public Enviroment Enviroment { get; private set; }
        public Controller Controller { get; private set; }

        public WriteableBitmap wbitmap;

        int Width;
        int Height;
        public DevRender(Enviroment enviroment, int winWidth, int winHeight , params Camera[] cameras)
        {
            Width = winHeight;
            Height = winHeight;
            Avalonia.Vector dpi = new Avalonia.Vector(96, 96);
            PixelSize wbitmapSize = new PixelSize(Width, Height);
            WriteableBitmap wbitmap = new WriteableBitmap(wbitmapSize, dpi, PixelFormat.Bgra8888);
            Enviroment = enviroment;
            Cameras = cameras.ToList();
            foreach (var cam in Cameras)
            {
                cam.ScreenWidth = winWidth;
                cam.ScreenHeight = winHeight;
            }
            Preparer = new BufferPreparer(Cameras , enviroment);
           // RenderForm = new Form1()
          //  {
          //      BackColor = Color.Black,
           //     Width = winWidth,
           //     Height = winHeight
          //  };
            Controller = new Controller(this);
            

            //RenderForm.KeyDown += (args, e) => Controller.KeyDown(e);
          //  RenderForm.KeyUp += (args, e) => Controller.KeyUp(e);
          //  RenderForm.MouseMove += (args, e) => Controller.HanleMouse(e);
            Timer = new DispatcherTimer();
            Timer.Interval =new System.TimeSpan(0,0,0,0,100);
            Timer.Tick += (args, e) =>
            {
                Controller.ComputeKeys();
                Preparer.PrepareNewBuffer();
                var buffer = Preparer.GetBuffer();
                if (buffer.Width != 1)
                {
                    for (int row = 0; row < Height; row++)
                    {
                        for (int col = 0; col < Width; col++)
                        {
                            System.Drawing.Color colr = buffer.GetPixel(col, row);
                            Avalonia.Media.Color color = Avalonia.Media.Color.FromArgb(colr.A, colr.R, colr.G, colr.B);
                            wbitmap.SetPixel(col, row,color);
                        }
                    }
                            //RenderForm.BackgroundImage = new Bitmap(buffer , RenderForm.Size);
                        }
            };
        }
        public void Start()
        {
            Timer.Start();
            //Application.Run(RenderForm);
        }
    }
}