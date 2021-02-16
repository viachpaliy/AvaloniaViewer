using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaViewer
{
    class Camera
    {
        public Point C = new Point(0, -500, 500);
        public Point N = new Point(0, 1, -1);
        public Point V = new Point(0, -1, -1);
        public int d = 5;
        public int hX = 2;
        public int hY = 2;
    }
}
