using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;


namespace DevRender
{
    public class Controller
    {
        public HashSet<Key> DownKeys;
        public int Speed { get; set; } = 40;

        public DevRender DevilRender;
        public Point LastMousePos { get; private set; }
        public Camera CurrentCamera => DevilRender.Preparer.CurrentRaterizer.Camera;
        public Controller(DevRender render)
        {
            DownKeys = new HashSet<Key>();
            DevilRender = render;
        }
        public void KeyDown(KeyEventArgs e)
        {
            if (!DownKeys.Contains(e.Key))
            {
                DownKeys.Add(e.Key);
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if (DownKeys.Contains(e.Key))
            {
                DownKeys.Remove(e.Key);
            }
        }


        public void ComputeKeys()
        {
            foreach (var key in DownKeys)
            {
                switch (key)
                {
                    case Key.Q:
                        CurrentCamera.Rotate(-0.05f, Axis.Y);
                        break;
                    case Key.S:
                        var v = -CurrentCamera.Pivot.ZAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Key.E:
                        CurrentCamera.Rotate(0.05f, Axis.Y);
                        break;
                    case Key.W:
                        v = CurrentCamera.Pivot.ZAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Key.D:
                        v = CurrentCamera.Pivot.XAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Key.A:
                        v = -CurrentCamera.Pivot.XAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Key.R:
                        CurrentCamera.Rotate(-0.05f, Axis.X);
                        break;
                    case Key.F:
                        CurrentCamera.Rotate(0.05f, Axis.X);
                        break;
                    case Key.Y:
                        DevilRender.Preparer.MoveNextRasterizer();
                        break;
                    case Key.M:
                        DevilRender.Timer.Stop();
                        var camera = new Camera(CurrentCamera.Pivot.Center , 1 , (float)Math.PI / 2 ,1920 * 7 , 1080 * 7);
                        camera.Pivot.XAxis = CurrentCamera.Pivot.XAxis;
                        camera.Pivot.YAxis = CurrentCamera.Pivot.YAxis;
                        camera.Pivot.ZAxis = CurrentCamera.Pivot.ZAxis;
                        var preparer = new BufferPreparer(new List<Camera>() { camera}, DevilRender.Enviroment);
                        preparer.PrepareNewBuffer();
                        var buffer = preparer.Buffers.Dequeue();
                        buffer.Save(@"D:\VSCode\AvaloniaViewer\DevRender\Images\img.png");
                        DevilRender.Timer.Start();
                        break;
                }
            }
        }
       // public void HandleMouse(MouseEventArgs e)
        //{
            //float deltaX = e.X - LastMousePos.X;
            //float deltaY = e.Y - LastMousePos.Y;
            //CurrentCamera.Rotate(-deltaX / 100, Axis.Y);
            //CurrentCamera.Rotate(-deltaY / 100, Axis.X);
            //LastMousePos = e.Location;
       // }
    }
}
