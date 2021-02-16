using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace AvaloniaViewer
{
    class Luz
    {
        public Color Iamb = Color.FromRgb(255, 0, 0);
        public Color Il = Color.FromRgb(255, 255, 0);
        public double Ka = 0.5f;
        public double Ks = 0.6f;
        public int n = 5;
        public Point Kd = new Point(0.5f, 0, 0);
        public Point Od = new Point(0.5f, 0, 0);
        public Point Pl = new Point(0, 0, -500);
    }
}
