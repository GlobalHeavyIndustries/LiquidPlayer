using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Filter : Object
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        private int seed;

        protected int w;
        protected int h;

        public static int NewFilter(int bitmapId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Filter);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Filter(id, bitmapId);

            return id;
        }

        public Filter(int id, int bitmapId)
            : base(id)
        {
            if (bitmapId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            this.seed = 0;

            this.w = bitmap.Width - 1;
            this.h = bitmap.Height - 1;
        }

        public override string ToString()
        {
            return $"Filter";
        }

        private int getRandom()
        {
            seed *= 0x015A4E35;

            return seed >> 16;
        }

        private void median(byte[] v, byte a)
        {
            if (a <= v[4])
            {
                return;
            }

            if (a <= v[3])
            {
                v[4] = a;
                return;
            }

            for (var t = 0; t <= 2; t++)
            {
                if (a > v[t])
                {
                    for (var i = 3; i >= t; i--)
                    {
                        v[i + 1] = v[i];
                    }

                    v[t] = a;
                    return;
                }
            }

            v[4] = v[3];
            v[3] = a;
        }

        private void mirrorCorner(Bitmap bitmap, int c0)
        {
            switch (c0)
            {
                case 0:
                    for (var yc = 0; yc < bitmap.Height / 2; yc++)
                    {
                        var offset = yc * bitmap.Width;

                        for (var xc = 0; xc < bitmap.Width / 2; xc++)
                        {
                            var color = bitmap.FastPeek(offset + xc);

                            bitmap.FastPoke(offset + ((bitmap.Width - 1) - xc), color);
                        }
                    }
                    break;
                case 1:
                    for (var yc = 0; yc < bitmap.Height / 2; yc++)
                    {
                        var destination = ((bitmap.Height - 1) - yc) * bitmap.Width + (bitmap.Width / 2);
                        var source = yc * bitmap.Width + (bitmap.Width / 2);
                        var length = bitmap.Width / 2;

                        for (var index = 0; index < length; index++)
                        {
                            var color = bitmap.FastPeek(source++);

                            bitmap.FastPoke(destination++, color);
                        }
                    }
                    break;
                case 2:
                    for (var yc = 0; yc < bitmap.Height / 2; yc++)
                    {
                        var offset = (yc + (bitmap.Height / 2)) * bitmap.Width;

                        for (var xc = 0; xc < bitmap.Width / 2; xc++)
                        {
                            var color = bitmap.FastPeek(offset + ((bitmap.Width - 1) - xc));

                            bitmap.FastPoke(offset + xc, color);
                        }
                    }
                    break;
                case 3:
                    for (var yc = 0; yc < bitmap.Height / 2; yc++)
                    {
                        var destination = yc * bitmap.Width;
                        var source = ((bitmap.Height - 1) - yc) * bitmap.Width;
                        var length = bitmap.Width / 2;

                        for (var index = 0; index < length; index++)
                        {
                            var color = bitmap.FastPeek(source++);

                            bitmap.FastPoke(destination++, color);
                        }
                    }
                    break;
            }
        }

        public void Dilate()
        {
            var tempData = bitmap.CastToRGBA();

            var index = 0;

            var offset = 0;

            for (var y = 0; y < bitmap.Height; y++)
            {
                offset = y * bitmap.Width;

                var offsetym1 = ((y - 1) & h) * bitmap.Width;
                var offsetyp1 = ((y + 1) & h) * bitmap.Width;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    var offsetxm1 = (x - 1) & w;
                    var offsetxp1 = (x + 1) & w;


                    var r = Util.Max(tempData[offsetym1 + offsetxm1].R,
                                     tempData[offsetym1 + x].R,
                                     tempData[offsetym1 + offsetxp1].R,
                                     tempData[offset + offsetxm1].R,
                                     tempData[offset + x].R,
                                     tempData[offset + offsetxp1].R,
                                     tempData[offsetyp1 + offsetxm1].R,
                                     tempData[offsetyp1 + x].R,
                                     tempData[offsetyp1 + offsetxp1].R);

                    var g = Util.Max(tempData[offsetym1 + offsetxm1].G,
                                     tempData[offsetym1 + x].G,
                                     tempData[offsetym1 + offsetxp1].G,
                                     tempData[offset + offsetxm1].G,
                                     tempData[offset + x].G,
                                     tempData[offset + offsetxp1].G,
                                     tempData[offsetyp1 + offsetxm1].G,
                                     tempData[offsetyp1 + x].G,
                                     tempData[offsetyp1 + offsetxp1].G);

                    var b = Util.Max(tempData[offsetym1 + offsetxm1].B,
                                     tempData[offsetym1 + x].B,
                                     tempData[offsetym1 + offsetxp1].B,
                                     tempData[offset + offsetxm1].B,
                                     tempData[offset + x].B,
                                     tempData[offset + offsetxp1].B,
                                     tempData[offsetyp1 + offsetxm1].B,
                                     tempData[offsetyp1 + x].B,
                                     tempData[offsetyp1 + offsetxp1].B);

                    bitmap.FastPoke(index++, Sprockets.Color.GetColor(r, g, b));
                }
            }
        }

        public void GrayScale()
        {
            for (var index = 0; index < bitmap.Size; index++)
            {
                var pixel = bitmap.FastPeek(index);

                bitmap.FastPoke(index, Sprockets.Color.GrayScale(pixel));
            }
        }

        public void Kaleidoscope(int corner)
        {
            corner--;

            for (var y = 0; y < bitmap.Height / 2; y++)
            {
                mirrorCorner(bitmap, corner);
                mirrorCorner(bitmap, (corner + 1) % 4);
                mirrorCorner(bitmap, (corner + 2) % 4);
            }
        }

        public void MakeTileable(int strength)
        {
            var tempData = bitmap.CastToRGBA();

            var w = bitmap.Width - 1;
            var h = bitmap.Height - 1;

            var index = 0;

            var pixel = new Sprockets.Color.RGBA();

            strength = bitmap.Width / 2 - strength;

            var sq = (float)(strength * strength);

            var sd = (float)((bitmap.Width / 2) * (bitmap.Height / 2) - sq);

            if (sd != 0)
            {
                sd = 0.75f / sd;
            }
            else
            {
                sd = 0.75f / 0.00001f;
            }

            for (var y = 0; y < bitmap.Height; y++)
            {
                var offset = y * bitmap.Width;
                var offset2 = (h - y) * bitmap.Width;

                var sy = y - bitmap.Height / 2;
                sy = sy * sy;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    var sx = x - bitmap.Width / 2;
                    var sr = sx * sx + sy - sq;

                    if (sr > 0)
                    {
                        sr = sr * sd;
                        var srm = 0f;

                        if (sr > 0.75f)
                        {
                            sr = 0.25f;
                            srm = 0.25f;
                        }
                        else
                        {
                            srm = 1f - sr;
                            sr = sr * 0.333333f;
                        }

                        pixel.R = (byte)Math.Round(tempData[offset + x].R * srm + (tempData[offset + w - x].R + tempData[offset2 + w - x].R + tempData[offset2 + x].R) * sr);
                        pixel.G = (byte)Math.Round(tempData[offset + x].G * srm + (tempData[offset + w - x].G + tempData[offset2 + w - x].G + tempData[offset2 + x].G) * sr);
                        pixel.B = (byte)Math.Round(tempData[offset + x].B * srm + (tempData[offset + w - x].B + tempData[offset2 + w - x].B + tempData[offset2 + x].B) * sr);
                    }
                    else
                    {
                        pixel.R = tempData[offset + x].R;
                        pixel.G = tempData[offset + x].G;
                        pixel.B = tempData[offset + x].B;
                    }

                    pixel.A = 255;

                    bitmap.FastPoke(index++, pixel.Uint);
                }
            }
        }

        public void Median()
        {
            var tempData = bitmap.CastToRGBA();

            var index = 0;

            var v = new byte[5];
            
            var pixel = new Sprockets.Color.RGBA();

            for (var y = 0; y < bitmap.Height; y++)
            {
                var offset = y * bitmap.Width;
                var offsetym1 = ((y - 1) & h) * bitmap.Width;
                var offsetyp1 = ((y + 1) & h) * bitmap.Width;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    var offsetxm1 = (x - 1) & w;
                    var offsetxp1 = (x + 1) & w;

                    v.Fill((byte)0);
                    v[0] = tempData[offsetym1 + offsetxm1].R;
                    median(v, tempData[offsetym1 + x].R);
                    median(v, tempData[offsetym1 + offsetxp1].R);
                    median(v, tempData[offset + offsetxm1].R);
                    median(v, tempData[offset + x].R);
                    median(v, tempData[offset + offsetxp1].R);
                    median(v, tempData[offsetyp1 + offsetxm1].R);
                    median(v, tempData[offsetyp1 + x].R);
                    median(v, tempData[offsetyp1 + offsetxp1].R);
                    pixel.R = v[4];

                    v.Fill((byte)0);
                    v[0] = tempData[offsetym1 + offsetxm1].G;
                    median(v, tempData[offsetym1 + x].G);
                    median(v, tempData[offsetym1 + offsetxp1].G);
                    median(v, tempData[offset + offsetxm1].G);
                    median(v, tempData[offset + x].G);
                    median(v, tempData[offset + offsetxp1].G);
                    median(v, tempData[offsetyp1 + offsetxm1].G);
                    median(v, tempData[offsetyp1 + x].G);
                    median(v, tempData[offsetyp1 + offsetxp1].G);
                    pixel.G = v[4];

                    v.Fill((byte)0);
                    v[0] = tempData[offsetym1 + offsetxm1].B;
                    median(v, tempData[offsetym1 + x].B);
                    median(v, tempData[offsetym1 + offsetxp1].B);
                    median(v, tempData[offset + offsetxm1].B);
                    median(v, tempData[offset + x].B);
                    median(v, tempData[offset + offsetxp1].B);
                    median(v, tempData[offsetyp1 + offsetxm1].B);
                    median(v, tempData[offsetyp1 + x].B);
                    median(v, tempData[offsetyp1 + offsetxp1].B);
                    pixel.B = v[4];

                    pixel.A = tempData[offset + x].A;

                    bitmap.FastPoke(index++, pixel.Uint);
                }
            }
        }

        public void Noise(int seed, int radius)
        {
            if (radius < 1 || radius > 8)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            var data = bitmap.Save();

            this.seed = seed;

            radius = 16 - radius;

            var index = 0;

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var dx = getRandom() - 32767 >> radius;
                    var dy = getRandom() - 32767 >> radius;

                    var pixel = data[((x + dx) & (bitmap.Width - 1)) + ((y + dy) & (bitmap.Height - 1)) * bitmap.Width];

                    bitmap.FastPoke(index++, pixel);
                }
            }
        }

        public void ReplaceAlpha(uint color, byte alpha)
        {
            for (var index = 0; index < bitmap.Size; index++)
            {
                var pixel = bitmap.FastPeek(index);

                if (pixel == color)
                {
                    bitmap.FastPoke(index, pixel & 0x00FFFFFF | (uint)(alpha << 24));
                }
            }
        }

        public void ReplaceColor(uint oldColor, uint newColor)
        {
            for (var index = 0; index < bitmap.Size; index++)
            {
                if (bitmap.FastPeek(index) == oldColor)
                {
                    bitmap.FastPoke(index, newColor);
                }
            }
        }

        public void Sepia()
        {
            for (var index = 0; index < bitmap.Size; index++)
            {
                var pixel = bitmap.FastPeek(index);

                bitmap.FastPoke(index, Sprockets.Color.Sepia(pixel));
            }
        }

        public void Sharpen()
        {
            var tempData = bitmap.CastToRGBA();

            var index = 0;

            var offset = 0;

            for (var y = 0; y < bitmap.Height; y++)
            {
                offset = y * bitmap.Width;

                var offsetym1 = ((y - 1) & h) * bitmap.Width;
                var offsetyp1 = ((y + 1) & h) * bitmap.Width;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    var offsetxm1 = (x - 1) & w;
                    var offsetxp1 = (x + 1) & w;

                    var r = (5 * tempData[offset + x].R >> 1)
                        - ((tempData[offsetym1 + offsetxm1].R
                        + tempData[offsetyp1 + offsetxm1].R
                        + tempData[offsetym1 + offsetxp1].R
                        + tempData[offsetyp1 + offsetxp1].R) >> 3)
                        - ((tempData[offset + offsetxm1].R
                        + tempData[offset + offsetxp1].R
                        + tempData[offsetym1 + x].R
                        + tempData[offsetyp1 + x].R) >> 2);

                    var g = (5 * tempData[offset + x].G >> 1)
                        - ((tempData[offsetym1 + offsetxm1].G
                        + tempData[offsetyp1 + offsetxm1].G
                        + tempData[offsetym1 + offsetxp1].G
                        + tempData[offsetyp1 + offsetxp1].G) >> 3)
                        - ((tempData[offset + offsetxm1].G
                        + tempData[offset + offsetxp1].G
                        + tempData[offsetym1 + x].G
                        + tempData[offsetyp1 + x].G) >> 2);

                    var b = (5 * tempData[offset + x].B >> 1)
                        - ((tempData[offsetym1 + offsetxm1].B
                        + tempData[offsetyp1 + offsetxm1].B
                        + tempData[offsetym1 + offsetxp1].B
                        + tempData[offsetyp1 + offsetxp1].B) >> 3)
                        - ((tempData[offset + offsetxm1].B
                        + tempData[offset + offsetxp1].B
                        + tempData[offsetym1 + x].B
                        + tempData[offsetyp1 + x].B) >> 2);

                    r = Util.Clamp(r, 0, 255);
                    g = Util.Clamp(g, 0, 255);
                    b = Util.Clamp(b, 0, 255);

                    bitmap.FastPoke(index++, Sprockets.Color.GetColor((byte)r, (byte)g, (byte)b));
                }
            }
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            base.shutdown();
        }
    }
}