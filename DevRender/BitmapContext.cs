using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using System.IO;

namespace DevRender
{
    public unsafe class BitmapContext : IDisposable
    {
        private readonly WriteableBitmap _writeableBitmap;
        private readonly ReadWriteMode _mode;

        private readonly int _pixelWidth;
        private readonly int _pixelHeight;

        private readonly static IDictionary<WriteableBitmap, int> UpdateCountByBmp = new ConcurrentDictionary<WriteableBitmap, int>();
        private readonly static IDictionary<WriteableBitmap, int[]> PixelCacheByBmp = new ConcurrentDictionary<WriteableBitmap, int[]>();
        private int length;
        private int[] pixels;

        /// <summary>
        /// The Bitmap
        /// </summary>
        public WriteableBitmap WriteableBitmap { get { return _writeableBitmap; } }

        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width { get { return _writeableBitmap.PixelSize.Width; } }

        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height { get { return _writeableBitmap.PixelSize.Height; } }
        /// <summary>
        /// Creates an instance of a BitmapContext, with default mode = ReadWrite
        /// </summary>
        /// <param name="writeableBitmap"></param>
        public BitmapContext(WriteableBitmap writeableBitmap)
            : this(writeableBitmap, ReadWriteMode.ReadWrite)
        {
        }

        /// <summary>
        /// Creates an instance of a BitmapContext, with specified ReadWriteMode
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="mode"></param>
        public BitmapContext(WriteableBitmap writeableBitmap, ReadWriteMode mode)
        {
            _writeableBitmap = writeableBitmap;
            _mode = mode;

            _pixelWidth = _writeableBitmap.PixelSize.Width;
            _pixelHeight = _writeableBitmap.PixelSize.Height;

            // Ensure the bitmap is in the dictionary of mapped Instances
            if (!UpdateCountByBmp.ContainsKey(_writeableBitmap))
            {
                // Set UpdateCount to 1 for this bitmap
                UpdateCountByBmp.Add(_writeableBitmap, 1);
                length = _writeableBitmap.PixelSize.Width * _writeableBitmap.PixelSize.Height;
                pixels = new int[length];
                CopyPixels();
                PixelCacheByBmp.Add(_writeableBitmap, pixels);
            }
            else
            {
                // For previously contextualized bitmaps increment the update count
                IncrementRefCount(_writeableBitmap);
                pixels = PixelCacheByBmp[_writeableBitmap];
                length = pixels.Length;
            }
        }

        private void CopyPixels()
        {
            using (var bmp = _writeableBitmap.Lock())
            {
                byte[] data = new byte[length * 4];
                Marshal.Copy(bmp.Address, data, 0, length * 4);
                fixed (byte* srcPtr = data)
                {
                    fixed (int* dstPtr = pixels)
                    {
                        for (var i = 0; i < length; i++)
                        {
                            dstPtr[i] = (srcPtr[i * 4 + 3] << 24)
                                        | (srcPtr[i * 4 + 2] << 16)
                                        | (srcPtr[i * 4 + 1] << 8)
                                        | srcPtr[i * 4 + 0];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Pixels array
        /// </summary>
        public int[] Pixels { get { return pixels; } }

        /// <summary>
        /// Gets the length of the Pixels array
        /// </summary>
        public int Length { get { return length; } }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(BitmapContext src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src.Pixels, srcOffset, dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source Array to destination BitmapContext
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(Array src, int srcOffset, BitmapContext dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest.Pixels, destOffset, count);
        }

        /// <summary>
        /// Performs a Copy operation from source BitmapContext to destination Array
        /// </summary>
        /// <remarks>Equivalent to calling Buffer.BlockCopy in Silverlight, or native memcpy in WPF</remarks>
        public static void BlockCopy(BitmapContext src, int srcOffset, Array dest, int destOffset, int count)
        {
            Buffer.BlockCopy(src.Pixels, srcOffset, dest, destOffset, count);
        }

        /// <summary>
        /// Clears the BitmapContext, filling the underlying bitmap with zeros
        /// </summary>
        public void Clear()
        {
            var pixels = Pixels;
            Array.Clear(pixels, 0, pixels.Length);
        }

        /// <summary>
        /// Disposes this instance if the underlying platform needs that.
        /// </summary>
        public void Dispose()
        {
            // Decrement the update count. If it hits zero
            if (DecrementRefCount(_writeableBitmap) == 0)
            {
                // Remove this bitmap from the update map
                UpdateCountByBmp.Remove(_writeableBitmap);
                PixelCacheByBmp.Remove(_writeableBitmap);

                // Copy data back
                if (_mode == ReadWriteMode.ReadWrite)
                {
                    using var data = _writeableBitmap.Lock();
                    using var stream = new UnmanagedMemoryStream((byte*)data.Address, length, length * 4, FileAccess.ReadWrite);
                    var buffer = new byte[length * 4];
                    fixed (int* srcPtr = pixels)
                    {
                        var b = 0;
                        for (var i = 0; i < length; i++, b += 4)
                        {
                            var p = srcPtr[i];
                            buffer[b + 3] = (byte)((p >> 24) & 0xff);
                            buffer[b + 2] = (byte)((p >> 16) & 0xff);
                            buffer[b + 1] = (byte)((p >> 8) & 0xff);
                            buffer[b + 0] = (byte)(p & 0xff);
                        }

                        stream.Write(buffer, 0, length * 4);
                    }
                }
            }
        }

        private static void IncrementRefCount(WriteableBitmap target)
        {
            UpdateCountByBmp[target]++;
        }

        private static int DecrementRefCount(WriteableBitmap target)
        {
            int current;
            if (!UpdateCountByBmp.TryGetValue(target, out current))
            {
                return -1;
            }
            current--;
            UpdateCountByBmp[target] = current;
            return current;
        }
    }

}
