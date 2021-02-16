using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using Avalonia.Platform;
using System.IO;
using System;
using System.Linq;
using System.Windows;
using Avalonia.Media;

namespace AvaloniaViewer
{
    public class MainWindow : Window
    {
        MathHelper math = new MathHelper();
        Camera camera = new Camera();

        Luz luz = new Luz();

        int WIDTH = 500;
        int HEIGHT = 500;
        PixelSize wbitmapSize = new PixelSize(500, 500);
        Vector dpi = new Vector(96, 96);
        WriteableBitmap wbitmap;
        byte[,,] pixels = new byte[500, 500, 4];

        double[,] zBuffer = new double[500, 500];

        List<double[]> vertices = new List<double[]>();
        List<int[]> triangulos = new List<int[]>();
        List<Point> verticesVista = new List<Point>();

        List<Triangle> triangles = new List<Triangle>();

        string filename = "";
        public MainWindow()
        {
            InitializeComponent();
            wbitmap = new WriteableBitmap(wbitmapSize, dpi, PixelFormat.Bgra8888);
#if DEBUG
            this.AttachDevTools();
#endif
            Init("../../../objetos/calice2.byu");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadFile(string name)
        {
            string file = File.ReadAllText(name);

            string[] data = file.Split(new string[2] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var vLen = data[0].Split(' ')[0];
            var tLen = data[0].Split(' ')[1];

            for (var i = 1; i <= int.Parse(vLen); i++)
            {
                string[] arr = data[i].Split(' ');
                double[] nArr = new double[arr.Length];

                for (int j = 0; j < arr.Length; j++)
                    nArr[j] = Convert.ToDouble(arr[j].Replace('.', ','));

                vertices.Add(nArr);
            }

            for (var i = int.Parse(vLen) + 1; i < data.Length; i++)
            {
                string[] arr = data[i].Split(' ');
                int[] nArr = new int[arr.Length];

                for (int j = 0; j < arr.Length; j++)
                    nArr[j] = Convert.ToInt32(arr[j]);

                triangulos.Add(nArr);
            }
        }

        private void DrawPixels()
        {
            // Copy the data into a one-dimensional array.
            byte[] pixels1d = new byte[HEIGHT * WIDTH * 4];
            int index = 0;
            for (int row = 0; row < HEIGHT; row++)
            {
                for (int col = 0; col < WIDTH; col++)
                {
                    pixels[row, col, 3] = 255;

                    for (int i = 0; i < 4; i++)
                        pixels1d[index++] = pixels[row, col, i];
                }
            }

            // Update writeable bitmap with the colorArray to the image.
            //PixelRect rect = new PixelRect(0, 0, WIDTH, HEIGHT);
            //int stride = 4 * WIDTH;
            //wbitmap.WritePixels(rect, pixels1d, stride, 0);
            using (var context = wbitmap.GetBitmapContext())
            {
                BitmapContext.BlockCopy(pixels1d, 0, context, 0, HEIGHT * WIDTH * 4);

            }
                // Create an Image to display the bitmap.
                Image image = new Image();
            image.Stretch = Stretch.None;
            image.Margin = new Thickness(0);

            //grdMain.Children.Add(image);
            this.FindControl<Grid>("grdMain").Children.Add(image);
            //Set the Image source.
            image.Source = wbitmap;
        }

        private void Init(String name)
        {
            triangles.Clear();
            vertices.Clear();
            verticesVista.Clear();
            triangulos.Clear();

            for (int row = 0; row < HEIGHT; row++)
            {
                for (int col = 0; col < WIDTH; col++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        pixels[row, col, i] = 0;
                    }
                }
            }

            for (int i = 0; i < zBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < zBuffer.GetLength(1); j++)
                {
                    zBuffer[i, j] = int.MaxValue;

                }
            }

            LoadFile(name);

            for (var i = 0; i < vertices.Count; i++)
            {
                double[,] vista = coordenadaVista(new Point(vertices[i][0], vertices[i][1], vertices[i][2]));
                verticesVista.Add(new Point(vista[0, 0], vista[1, 0], vista[2, 0]));
            }

            for (var i = 0; i < triangulos.Count; i++)
            {
                Triangle t = new Triangle();
                t.verticesIndex = new int[3] { triangulos[i][0] - 1, triangulos[i][1] - 1, triangulos[i][2] - 1 };

                t.vertices = new List<Point>();
                t.vertices.Add(new Point(vertices[t.verticesIndex[0]][0], vertices[t.verticesIndex[0]][1], vertices[t.verticesIndex[0]][2]));
                t.vertices.Add(new Point(vertices[t.verticesIndex[1]][0], vertices[t.verticesIndex[1]][1], vertices[t.verticesIndex[1]][2]));
                t.vertices.Add(new Point(vertices[t.verticesIndex[2]][0], vertices[t.verticesIndex[2]][1], vertices[t.verticesIndex[2]][2]));

                List<Point> coordenadasVista = new List<Point>();

                for (var j = 0; j < 3; j++)
                {
                    coordenadasVista.Add(verticesVista[t.verticesIndex[j]]);
                }

                t.coordenadaVista = coordenadasVista;

                Point V = math.subtracaoPontos(coordenadasVista[0], coordenadasVista[1]);
                Point W = math.subtracaoPontos(coordenadasVista[0], coordenadasVista[2]);

                t.normal = math.normalizar(math.produtoVetorial(V, W));

                double x = t.coordenadaVista[0].x + t.coordenadaVista[1].x + t.coordenadaVista[2].x;
                double y = t.coordenadaVista[0].y + t.coordenadaVista[1].y + t.coordenadaVista[2].y;
                double z = t.coordenadaVista[0].z + t.coordenadaVista[1].z + t.coordenadaVista[2].z;

                t.baricentro = new Point(x / 3, y / 3, z / 3);

                triangles.Add(t);
            }

            Point[] verticeNormal = new Point[vertices.Count];

            for (int i = 0; i < verticeNormal.Length; i++)
            {
                verticeNormal[i] = new Point(0, 0, 0);
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                verticeNormal[triangles[i].verticesIndex[0]].x += triangles[i].normal.x;
                verticeNormal[triangles[i].verticesIndex[0]].y += triangles[i].normal.y;
                verticeNormal[triangles[i].verticesIndex[0]].z += triangles[i].normal.z;

                verticeNormal[triangles[i].verticesIndex[1]].x += triangles[i].normal.x;
                verticeNormal[triangles[i].verticesIndex[1]].y += triangles[i].normal.y;
                verticeNormal[triangles[i].verticesIndex[1]].z += triangles[i].normal.z;

                verticeNormal[triangles[i].verticesIndex[2]].x += triangles[i].normal.x;
                verticeNormal[triangles[i].verticesIndex[2]].y += triangles[i].normal.y;
                verticeNormal[triangles[i].verticesIndex[2]].z += triangles[i].normal.z;
            }

            for (int i = 0; i < verticeNormal.Length; i++)
            {
                verticeNormal[i] = math.normalizar(verticeNormal[i]);
            }

            triangles = triangles.OrderBy(p => p.baricentro.z).ToList();
            triangles.Reverse();

            for (int i = 0; i < triangles.Count; i++)
            {
                triangles[i].normaisVertices = new List<Point>();
                triangles[i].normaisVertices.Add(verticeNormal[triangles[i].verticesIndex[0]]);
                triangles[i].normaisVertices.Add(verticeNormal[triangles[i].verticesIndex[1]]);
                triangles[i].normaisVertices.Add(verticeNormal[triangles[i].verticesIndex[2]]);

                List<Point> verticeTela = new List<Point>();

                for (var j = 0; j < 3; j++)
                {
                    double Xs = (camera.d / camera.hX) * (triangles[i].coordenadaVista[j].x / triangles[i].coordenadaVista[j].z);
                    double Ys = (camera.d / camera.hY) * (triangles[i].coordenadaVista[j].y / triangles[i].coordenadaVista[j].z);

                    double I = Math.Floor((((Xs + 1) / 2) * WIDTH) + 0.5);
                    double J = Math.Floor((HEIGHT - ((Ys + 1) / 2) * HEIGHT) + 0.5);

                    verticeTela.Add(new Point(I, J, 0));
                }

                Point V = math.subtracaoPontos(verticeTela[0], verticeTela[1]);
                Point W = math.subtracaoPontos(verticeTela[0], verticeTela[2]);

                triangles[i].verticeTelaOriginal = new List<Point>();
                triangles[i].verticeTelaOriginal.Add(new Point(verticeTela[0].x, verticeTela[0].y, verticeTela[0].z));
                triangles[i].verticeTelaOriginal.Add(new Point(verticeTela[1].x, verticeTela[1].y, verticeTela[1].z));
                triangles[i].verticeTelaOriginal.Add(new Point(verticeTela[2].x, verticeTela[2].y, verticeTela[2].z));

                if (verticeTela[0].y > verticeTela[1].y) verticeTela = swap(verticeTela, 0, 1);
                if (verticeTela[0].y > verticeTela[2].y) verticeTela = swap(verticeTela, 0, 2);
                if (verticeTela[1].y > verticeTela[2].y) verticeTela = swap(verticeTela, 1, 2);

                triangles[i].coordenadaTela = verticeTela;

                if ((int)verticeTela[1].y == (int)verticeTela[2].y)
                {
                    drawTopBottom(triangles[i], 0, 1, 2);
                }
                else if ((int)verticeTela[0].y == (int)verticeTela[1].y)
                {
                    drawBottomTop(triangles[i], 0, 1, 2);
                }
                else
                {
                    var v4x = (verticeTela[0].x + ((verticeTela[1].y - verticeTela[0].y) / (verticeTela[2].y - verticeTela[0].y)) * (verticeTela[2].x - verticeTela[0].x));
                    verticeTela.Add(new Point(v4x, verticeTela[1].y, 0));

                    drawTopBottom(triangles[i], 0, 1, 3);
                    drawBottomTop(triangles[i], 1, 3, 2);
                }
            }

            DrawPixels();
        }

        private void drawTopBottom(Triangle triangle, int idx1, int idx2, int idx3)
        {
            Point v1 = triangle.coordenadaTela[idx1];
            Point v2 = triangle.coordenadaTela[idx2];
            Point v3 = triangle.coordenadaTela[idx3];

            double invslope1 = (v2.x - v1.x) / (v2.y - v1.y);
            double invslope2 = (v3.x - v1.x) / (v3.y - v1.y);

            double x1 = v1.x;
            double x2 = v1.x;

            for (var y = (int)v1.y; y <= v2.y; y++)
            {
                var xMin = x1 < x2 ? x1 : x2;
                var xMax = x1 > x2 ? x1 : x2;

                for (int x = (int)xMin; x <= xMax; x++)
                {
                    Point[] PN = originalPoint(triangle, x, y);

                    if (PN != null && PN[0].z <= zBuffer[x, y])
                    {
                        Color I = calcularCor(triangle, PN[0], PN[1]);

                        AddPixel(x, y, I, PN[0].z);
                    }
                }

                x1 += invslope1;
                x2 += invslope2;
            }
        }

        private void drawBottomTop(Triangle triangle, int idx1, int idx2, int idx3)
        {
            Point v1 = triangle.coordenadaTela[idx1];
            Point v2 = triangle.coordenadaTela[idx2];
            Point v3 = triangle.coordenadaTela[idx3];

            double invslope1 = (v3.x - v1.x) / (v3.y - v1.y);
            double invslope2 = (v3.x - v2.x) / (v3.y - v2.y);

            double x1 = v3.x;
            double x2 = v3.x;

            for (int y = (int)v3.y; y >= v1.y; y--)
            {
                var xMin = x1 < x2 ? x1 : x2;
                var xMax = x1 > x2 ? x1 : x2;

                for (int x = (int)xMin; x <= xMax; x++)
                {
                    Point[] PN = originalPoint(triangle, x, y);

                    if (PN != null && PN[0].z <= zBuffer[x, y])
                    {
                        Color I = calcularCor(triangle, PN[0], PN[1]);

                        AddPixel(x, y, I, PN[0].z);
                    }
                }

                x1 -= invslope1;
                x2 -= invslope2;
            }
        }

        private Point[] originalPoint(Triangle triangle, int i, int j)
        {
            double[,] bar = math.coordenadasBaricentricas(new Point(i, j, 0), triangle.verticeTelaOriginal[0], triangle.verticeTelaOriginal[1], triangle.verticeTelaOriginal[2]);

            var alpha = bar[0, 0];
            var beta = bar[0, 1];
            var gama = bar[0, 2];

            Point P = new Point(
                alpha * triangle.coordenadaVista[0].x + beta * triangle.coordenadaVista[1].x + gama * triangle.coordenadaVista[2].x,
                alpha * triangle.coordenadaVista[0].y + beta * triangle.coordenadaVista[1].y + gama * triangle.coordenadaVista[2].y,
                alpha * triangle.coordenadaVista[0].z + beta * triangle.coordenadaVista[1].z + gama * triangle.coordenadaVista[2].z
            );

            Point N = new Point(
                alpha * triangle.normaisVertices[0].x + beta * triangle.normaisVertices[1].x + gama * triangle.normaisVertices[2].x,
                alpha * triangle.normaisVertices[0].y + beta * triangle.normaisVertices[1].y + gama * triangle.normaisVertices[2].y,
                alpha * triangle.normaisVertices[0].z + beta * triangle.normaisVertices[1].z + gama * triangle.normaisVertices[2].z
            );


            return new Point[2] { P, N };
        }

        private Color calcularCor(Triangle triangle, Point P, Point N)
        {
            Point L = math.subtracaoPontos(P, luz.Pl);

            N = math.normalizar(N);
            L = math.normalizar(L);

            double NxL = math.produtoEscalar(N, L);

            Point R = new Point(2 * NxL * N.x - L.x, 2 * NxL * N.y - L.y, 2 * NxL * N.z - L.z);
            Point V = new Point(-P.x, -P.y, -P.z);

            V = math.normalizar(V);

            double RxV = math.produtoEscalar(R, V);
            double RxV2 = Math.Pow(RxV, luz.n);

            if (NxL < 0)
            {
                if (math.produtoEscalar(V, N) < 0)
                {
                    N = new Point(-N.x, -N.y, -N.z);
                    NxL = math.produtoEscalar(N, L);

                    R = new Point(2 * NxL * N.x - L.x, 2 * NxL * N.y - L.y, 2 * NxL * N.z - L.z);

                    RxV = math.produtoEscalar(R, V);
                    RxV2 = Math.Pow(RxV, luz.n);
                }
                else
                {
                    NxL = 0;
                    RxV2 = 0;
                }
            }

            if (math.produtoEscalar(V, R) < 0)
            {
                RxV2 = 0;
            }

            Cor Ia = new Cor(
                (int)(luz.Ka * luz.Iamb.R),
                (int)(luz.Ka * luz.Iamb.G),
                (int)(luz.Ka * luz.Iamb.B));

            Cor Id = new Cor(
                (int)(NxL * luz.Kd.x * luz.Od.x * luz.Il.R),
                (int)(NxL * luz.Kd.y * luz.Od.y * luz.Il.G),
                (int)(NxL * luz.Kd.z * luz.Od.z * luz.Il.B));

            Cor Is = new Cor(
                (int)(RxV2 * luz.Ks * luz.Il.R),
                (int)(RxV2 * luz.Ks * luz.Il.G),
                (int)(RxV2 * luz.Ks * luz.Il.B));

            Cor color = new Cor(
                Ia.R + Id.R + Is.R,
                Ia.G + Id.G + Is.G,
                Ia.B + Id.B + Is.B);

            color.R = color.R > 255 ? 255 : color.R;
            color.G = color.G > 255 ? 255 : color.G;
            color.B = color.B > 255 ? 255 : color.B;

            Color I = Color.FromRgb((byte)color.R, (byte)color.G, (byte)color.B);

            return I;
        }

        private List<Point> swap(List<Point> array, int i, int j)
        {
            var aux = array[i];
            array[i] = array[j];
            array[j] = aux;

            return array;
        }

        private double[,] coordenadaVista(Point ponto)
        {
            // ortogonalizar V
            var prod = math.produtoEscalar(camera.V, camera.N) / math.produtoEscalar(camera.N, camera.N);
            var vLinha = new Point(camera.V.x - prod * camera.N.x, camera.V.y - prod * camera.N.y, camera.V.z - prod * camera.N.z);

            // Normalização
            camera.N = math.normalizar(camera.N);
            vLinha = math.normalizar(vLinha);

            // U = N x V'
            Point U = new Point(
                camera.N.y * vLinha.z - camera.N.z * vLinha.y,
                camera.N.z * vLinha.x - camera.N.x * vLinha.z,
                camera.N.x * vLinha.y - camera.N.y * vLinha.x);

            Point alpha = new Point(math.normalizar(U), vLinha, camera.N);

            double[,] matrizTransformacao = new double[3, 3]
            {
                { alpha.a.x, alpha.a.y, alpha.a.z },
                { alpha.b.x, alpha.b.y, alpha.b.z },
                { alpha.c.x, alpha.c.y, alpha.c.z },
            };

            Point subPonto = math.subtracaoPontos(camera.C, ponto);

            return math.multiplicarMatriz(matrizTransformacao, new double[3, 1]
            {
                { subPonto.x },
                { subPonto.y },
                { subPonto.z }
            });
        }

        private void AddPixel(int x, int y, Color color, double z)
        {
            pixels[y, x, 0] = color.B;
            pixels[y, x, 1] = color.G;
            pixels[y, x, 2] = color.R;
            pixels[y, x, 3] = 255;

            zBuffer[x, y] = z;
        }




    }
}
