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
    public interface IShader
    {
        void ComputeShader(ref Vertex vertex, Camera camera);
    }


    public struct Light
    {
        public Vector3 Pos;
        public float Intensivity;
        public Light(Vector3 pos , float intensivity)
        {
            Pos = pos;
            Intensivity = intensivity;
        }
    }
}









