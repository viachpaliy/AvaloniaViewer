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
public class PhongModelShader : IShader
    {
        public static float DiffuseCoef = 0.5f;
        public static float ReflectCoef = 0.05f;
        public Light[] Lights { get; set; }

        public PhongModelShader(params Light[] lights)
        {
            Lights = lights;
        }
        public void ComputeShader(ref Vertex vertex, Camera camera)
        {
            if (vertex.Normal.X == 0 && vertex.Normal.Y == 0 && vertex.Normal.Z == 0)
            {
                return;
            }
            var gPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            foreach (var light in Lights)
            {
                var ldir = Vector3.Normalize(light.Pos - gPos);
                //Следующие три строчки нужны чтобы найти отраженный от поверхности луч
                var proection = VectorMath.Proection(ldir, vertex.Normal);
                var d = ldir - proection;
                var reflect = proection - d;
                var diffuseVal = Math.Max(VectorMath.Cross(ldir, vertex.Normal), 0) * light.Intensivity;
                //луч от наблюдателя
                var eye = Vector3.Normalize(vertex.Position);
                var reflectVal = Math.Max(VectorMath.Cross(reflect, eye), 0) * light.Intensivity;
                var total = diffuseVal * DiffuseCoef + reflectVal * ReflectCoef;
                vertex.Color = new TGAColor( vertex.Color.a,
                    (byte)Math.Min(255, vertex.Color.r * total),
                    (byte)Math.Min(255, vertex.Color.g * total),
                    (byte)Math.Min(255, vertex.Color.b * total));
            }
        }
    }
}