using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Poly = System.Tuple<DevRender.Vertex, DevRender.Vertex, DevRender.Vertex>;

namespace DevRender
{
public class ShadowMappingShader : IShader
    {
        public Enviroment Enviroment { get; set; }
        public Rasterizer Rasterizer { get; set; }
        public Camera Camera => Rasterizer.Camera;
        public Pivot Pivot => Camera.Pivot;
        public Vertex[] ZBuffer => Rasterizer.ZBuffer;
        public float LightIntensivity { get; set; }

        public ShadowMappingShader(Enviroment enviroment , Rasterizer rasterizer, float lightIntensivity)
        {
            Enviroment = enviroment;
            LightIntensivity = lightIntensivity;
            Rasterizer = rasterizer;
            Camera.OnRotate += (an , ax) => UpdateVisible();
            Camera.OnMove += (v) => UpdateVisible();
            Enviroment.OnChange += () => UpdateVisible();
            UpdateVisible();
        }
        public void ComputeShader(ref Vertex vertex, Camera camera)
        {
            //вычисляем глобальные координаты вершины
            var gPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            //дистанция до света
            var lghDir = Pivot.Center - gPos;
            var distance = lghDir.Length();
            var local = Pivot.ToLocalCoords(gPos);
            var proectToLight = Camera.ScreenProection(local).ToPoint();
            if (proectToLight.X >= 0 && proectToLight.X < Camera.ScreenWidth && proectToLight.Y >= 0
                && proectToLight.Y < Camera.ScreenHeight)
            {
                int index = proectToLight.Y * Camera.ScreenWidth + proectToLight.X;
                var n = Vector3.Normalize(vertex.Normal);
                var ld = Vector3.Normalize(lghDir);
                //вычислем сдвиг глубины
                float bias = (float)Math.Max(5 * (1.0 - VectorMath.Cross(n, ld)), 0.05);
                if (ZBuffer[index].Position.Z == 0 || ZBuffer[index].Position.Z + bias >= local.Z)
                {
                    vertex.Color = new TGAColor(vertex.Color.a,
                        (byte)Math.Min(255, vertex.Color.r * LightIntensivity),
                        (byte)Math.Min(255, vertex.Color.g * LightIntensivity),
                        (byte)Math.Min(255, vertex.Color.b * LightIntensivity));
                }
            }
            else
            {
                vertex.Color = new TGAColor(vertex.Color.a,
                    (byte)Math.Min(255, vertex.Color.r * LightIntensivity / 15),
                    (byte)Math.Min(255, vertex.Color.g * LightIntensivity / 15),
                    (byte)Math.Min(255, vertex.Color.b * LightIntensivity / 15));
            }
        }
        public void UpdateVisible()
        {
            Rasterizer.ComputeVisibleVertices(Enviroment.GetPrimitives());
        }
    }
}