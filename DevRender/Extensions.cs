using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Media;

namespace DevRender
{
    public static class BitmapExtensions
    {
        public static int ConvertColor(Color color)
        {
            var col = 0;

            if (color.A != 0)
            {
                var a = color.A + 1;
                col = (color.A << 24)
                      | ((byte)((color.R * a) >> 8) << 16)
                      | ((byte)((color.G * a) >> 8) << 8)
                      | (byte)((color.B * a) >> 8);
            }

            return col;
        }

        public static BitmapContext GetBitmapContext(this WriteableBitmap bmp)
        {
            return new BitmapContext(bmp, ReadWriteMode.ReadWrite);
        }

        public static BitmapContext GetBitmapContext(this WriteableBitmap bmp, ReadWriteMode mode)
        {
            return new BitmapContext(bmp, mode);
        }
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = ConvertColor(color);
            }
        }

        

        public static Color GetPixel(this WriteableBitmap bmp, int x, int y)
        {
            using (var context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var c = context.Pixels[y * context.Width + x];
                var a = (byte)(c >> 24);

                // Prevent division by zero
                int ai = a;
                if (ai == 0)
                {
                    ai = 1;
                }

                // Scale inverse alpha to use cheap integer mul bit shift
                ai = ((255 << 8) / ai);
                return Color.FromArgb(a,
                    (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                    (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                    (byte)(((c & 0xFF) * ai) >> 8));
            }
        }
    }
}
