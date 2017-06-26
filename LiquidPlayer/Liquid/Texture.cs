using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Texture : Bitmap
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        private int seed;

        protected int w;
        protected int h;

        public static int NewTexture(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Texture);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Texture(id);

            return id;
        }

        public Texture(int id)
            : base(id)
        {
            this.width = 256;
            this.height = 256;
            this.size = width * height;

            this.topX = width - 1;
            this.topY = height - 1;

            this.data = new uint[size];
            this.inSync = false;
            this.doubleBuffered = false;

            this.handle = Sprockets.Graphics.GenTexture();
            Sprockets.Graphics.BindTexture(handle, width, height, data);
            this.inSync = true;

            this.seed = 0;

            this.w = width - 1;
            this.h = height - 1;
        }

        public override string ToString()
        {
            return $"Texture (Resolution: {width}x{height})";
        }

        private int getRandom()
        {
            seed *= 0x015A4E35;

            return seed >> 16;
        }

        private uint cosineInterpolate(Sprockets.Color.RGBA[] o, double x, double y)
        {
            var mf1 = (1d - Math.Cos(x * Math.PI)) * 0.5d;
            var mf2 = (1d - Math.Cos(y * Math.PI)) * 0.5d;

            var f1 = 1d - mf1;
            var f2 = 1d - mf2;

            var g0 = f1 * f2;
            var g1 = mf1 * f2;
            var g2 = f1 * mf2;
            var g3 = mf1 * mf2;

            var r = Math.Round(o[0].R * g0 + o[1].R * g1 + o[2].R * g2 + o[3].R * g3);
            var g = Math.Round(o[0].G * g0 + o[1].G * g1 + o[2].G * g2 + o[3].G * g3);
            var b = Math.Round(o[0].B * g0 + o[1].B * g1 + o[2].B * g2 + o[3].B * g3);
            var a = 255;

            return (uint)r | (uint)g << 8 | (uint)b << 16 | (uint)a << 24;
        }

        private void subPlasma(int dist, int seed, int amplitude, bool rgbFlag)
        {
            var corner = new Sprockets.Color.RGBA[4];

            data.Fill(0u);

            if (seed != 0)
            {
                this.seed = seed;
            }

            var pixel = new Sprockets.Color.RGBA();

            for (var y = 0; y < height; y += dist)
            { 
                for (var x = 0; x < width; x += dist)
                {
                    var offset = y * width + x;

                    pixel.R = (byte)(getRandom() & (amplitude - 1));

                    if (rgbFlag)
                    {
                        pixel.G = (byte)(getRandom() & (amplitude - 1));
                        pixel.B = (byte)(getRandom() & (amplitude - 1));
                    }
                    else
                    {
                        pixel.G = pixel.R;
                        pixel.B = pixel.R;
                    }

                    pixel.A = 255;

                    data[offset] = pixel.Uint;
                }
            }

            if (dist <= 1)
            {
                return;
            }

            var oodist = 1d / dist;

            for (var y = 0; y < height; y += dist)
            {
                var offset = y * width;
                var offset2 = ((y + dist) & h) * width;

                for (var x = 0; x < width; x += dist)
                {
                    corner[0].Uint = data[x + offset];
                    corner[1].Uint = data[((x + dist) & w) + offset];
                    corner[2].Uint = data[x + offset2];
                    corner[3].Uint = data[((x + dist) & w) + offset2];

                    for (var b = 0; b < dist; b++)
                    {
                        var offsetb = ((y + b) & h) * width;
                        var bdist = b * oodist;

                        for (var a = 0; a < dist; a++)
                        {
                            data[((x + a) & w) + offsetb] = cosineInterpolate(corner, a * oodist, bdist);
                        }
                    }
                }
            }
        }

        public void CellMachine(int seed, int rule)
        {
            data.Fill(0u);

            this.seed = seed;

            for (var x = 0; x < width; x++)
            {
                var c = getRandom() >> 10;

                if (c == 0)
                {
                    data[x] = 0xFFFFFFFF;
                }
            }

            var offset = 0;

            for (var y = 1; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var m = 0;

                    if (data[((x - 1) & w) + offset] != 0)
                    {
                        m = 1;
                    }

                    if (data[x + offset] != 0)
                    {
                        m |= 2;
                    }

                    if (data[((x + 1) & w) + offset] != 0)
                    {
                        m |= 4;
                    }

                    var n = 1 << m;

                    if ((n & rule) != 0)
                    {
                        data[offset + width + x] = 0xFFFFFFFF;
                    }
                }

                offset += width;
            }

            inSync = false;
        }

        public void Checkerboard(int dx, int dy, uint color1, uint color2)
        {
            var flipColor = ((256 / dx) & 1) == 0 && (256 % dx) != 0;

            var index = 0;

            for (var y = 0; y < 256; y++)
            {
                if (y % dy == 0)
                {
                    Util.Swap(ref color1, ref color2);
                }

                for (var x = 0; x < 256; x++)
                {
                    if (x % dx == 0)
                    {
                        Util.Swap(ref color1, ref color2);
                    }

                    FastPoke(index++, color1);
                }

                if (flipColor)
                {
                    Util.Swap(ref color1, ref color2);
                }
            }

            inSync = false;
        }

        public void Particle(double size)
        {
            var oo1sxd2 = 1d / (width / 2);
            var oo1syd2 = 1d / (height / 2);

            var f = (100 / size) * 180;

            var index = 0;

            for (var y = 0; y < 256; y++)
            {
                var ny = (y - height / 2) * oo1syd2;

                for (var x = 0; x < 256; x++)
                {
                    var nx = (x - width / 2) * oo1sxd2;

                    var r = 255 - f * Math.Sqrt(nx * nx + ny * ny);

                    var s = (byte)Util.Clamp(r, 0, 255);

                    FastPoke(index++, new Sprockets.Color.RGBA(s, s, s).Uint);
                }
            }

            inSync = false;
        }

        public void PerlinNoise(int dist, int seed, int amplitude, int octaves, int persistence, bool rgbFlag)
        {
            subPlasma(dist, seed, 1, rgbFlag);

            var tempTextureId = NewTexture(objectId);
            var tempTexture = objectManager[tempTextureId].LiquidObject as Texture;

            for (var i = 0; i < octaves - 2; i++)
            {
                amplitude = (amplitude * persistence) >> 8;

                if (amplitude <= 0)
                {
                    break;
                }

                dist >>= 1;

                if (dist <= 0)
                {
                    break;
                }

                tempTexture.subPlasma(dist, (int)this.seed, amplitude, rgbFlag);

                var pixel = new Sprockets.Color.RGBA();
                var tempPixel = new Sprockets.Color.RGBA();

                for (var v = 0; v < width * height; v++)
                {
                    pixel.Uint = data[v];
                    tempPixel.Uint = tempTexture.data[v];

                    pixel.R = (byte)Util.Min(pixel.R + tempPixel.R, 255);
                    pixel.G = (byte)Util.Min(pixel.G + tempPixel.G, 255);
                    pixel.B = (byte)Util.Min(pixel.B + tempPixel.B, 255);
                    pixel.A = 255;

                    data[v] = pixel.Uint;
                }
            }

            objectManager.Mark(tempTextureId);

            inSync = false;
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}