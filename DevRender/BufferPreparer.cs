﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poly = System.Tuple<System.Numerics.Vector3, System.Numerics.Vector3, System.Numerics.Vector3>;
using Poly2D = System.Tuple<System.Drawing.PointF, System.Drawing.PointF, System.Drawing.PointF>;
using Buffer = System.Drawing.Bitmap;
using System.Numerics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;

namespace DevRender
{

    public class BufferPreparer
    {
        public int MaxBuffersCount { get; set; } = 1000000000;
        public Queue<Buffer> Buffers { get; private set; }
        Enviroment Enviroment { get; set; }
        public Buffer GetBuffer() => Buffers.Count == 0 ? new Buffer(1, 1) : Buffers.Dequeue();
        public Rasterizer CurrentRaterizer => Rasterizers[RasterizerIndex];
        public List<Rasterizer> Rasterizers { get; set; }
        public int RasterizerIndex { get; private set; }
        public BufferPreparer(List<Camera> cameras, Enviroment enviroment)
        {
            Enviroment = enviroment;
            Buffers = new Queue<Buffer>();
            Rasterizers = cameras.Select(c => new Rasterizer(c)).ToList();
        }
        public void MoveNextRasterizer()
        {
            RasterizerIndex = (RasterizerIndex + 1) % Rasterizers.Count;
        }
        public void PrepareNewBuffer()
        { 
            Buffers.Enqueue(Rasterizers[RasterizerIndex].Rasterize(Enviroment.GetPrimitives()));
        }
    }
}
