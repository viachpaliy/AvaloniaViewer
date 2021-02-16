using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaViewer
{
    class Triangle
    {
        public List<Point> vertices;
        public List<Point> coordenadaVista;
        public List<Point> normaisVertices;
        public List<Point> coordenadaTela;
        public List<Point> verticeTelaOriginal;
        public Point normal;
        public Point baricentro;

        public int[] verticesIndex;
    }
}
