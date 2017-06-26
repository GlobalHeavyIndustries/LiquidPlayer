using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace LiquidPlayer.Sprockets
{
    public static class Color
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct RGBA
        {
            [FieldOffset(0)]
            public byte R;
            [FieldOffset(1)]
            public byte G;
            [FieldOffset(2)]
            public byte B;
            [FieldOffset(3)]
            public byte A;

            [FieldOffset(0)]
            public uint Uint;

            public RGBA(byte r, byte g, byte b, byte a = 255)
            {
                Uint = 0;

                R = r;
                G = g;
                B = b;
                A = a;
            }

            public RGBA(uint data)
            {
                R = 0;
                G = 0;
                B = 0;
                A = 0;

                Uint = data;
            }
        }

        public static readonly RGBA White = new RGBA(255, 255, 255);
        public static readonly RGBA Gray = new RGBA(128, 128, 128);
        public static readonly RGBA Silver = new RGBA(192, 192, 192);
        public static readonly RGBA Black = new RGBA(0, 0, 0);

        public static readonly RGBA Red = new RGBA(255, 0, 0);
        public static readonly RGBA Orange = new RGBA(255, 128, 0);
        public static readonly RGBA Yellow = new RGBA(255, 255, 0);
        public static readonly RGBA Green = new RGBA(0, 255, 0);
        public static readonly RGBA Blue = new RGBA(0, 0, 255);
        public static readonly RGBA Purple = new RGBA(0, 128, 255);

        public static readonly RGBA Border = new RGBA(16, 16, 16);

        private static int plasmaState;
        private static RGBA plasma;

        public static uint GetColor(byte r, byte g, byte b, byte a = 255)
        {
            //return r | (uint)g << 8 | (uint)b << 16 | (uint)a << 24;

            var c = new RGBA()
            {
                R = r,
                G = g,
                B = b,
                A = a
            };

            return c.Uint;
        }

        public static uint GetPlasmaColor()
        {
            return plasma.Uint;
        }

        public static uint GetRandomColor(byte a = 255)
        {
            //var r = (byte)Program.Random.Range(0, 255);
            //var g = (byte)Program.Random.Range(0, 255);
            //var b = (byte)Program.Random.Range(0, 255);

            //return GetColor(r, g, b, a);

            var c = new RGBA()
            {
                R = (byte)Program.Random.Range(0, 255),
                G = (byte)Program.Random.Range(0, 255),
                B = (byte)Program.Random.Range(0, 255),
                A = a
            };

            return c.Uint;
        }

        public static uint Blend(uint src, uint dst)
        {
            //var sr = src & 255;
            //var sg = src >> 8 & 255;
            //var sb = src >> 16 & 255;
            //var sa = src >> 24;

            //if (sa == 0)
            //{
            //    return 0;
            //}

            //var dr = dst & 255;
            //var dg = dst >> 8 & 255;
            //var db = dst >> 16 & 255;
            //var da = dst >> 24;

            //var r = (sr * dr) >> 8;
            //var g = (sg * dg) >> 8;
            //var b = (sb * db) >> 8;
            //var a = (sa * da) >> 8;

            //return r | g << 8 | b << 16 | a << 24;

            var s = new RGBA(src);
            var d = new RGBA(dst);

            if (s.A == 0)
            {
                return 0;
            }

            var c = new RGBA()
            {
                R = (byte)(s.R * d.R >> 8),
                G = (byte)(s.G * d.G >> 8),
                B = (byte)(s.B * d.B >> 8),
                A = (byte)(s.A * d.A >> 8)
            };

            return c.Uint;
        }

        public static uint Darken(uint color, float amt)
        {
            var i = new RGBA(color);

            var c = new RGBA()
            {
                R = (byte)Util.Clamp(i.R - (i.R * amt), 0, 255),
                G = (byte)Util.Clamp(i.G - (i.G * amt), 0, 255),
                B = (byte)Util.Clamp(i.B - (i.B * amt), 0, 255),
                A = i.A
            };

            return c.Uint;
        }

        public static uint Lighten(uint color, float amt)
        {
            var i = new RGBA(color);

            var c = new RGBA()
            {
                R = (byte)Util.Clamp(i.R + (i.R * amt), 0, 255),
                G = (byte)Util.Clamp(i.G + (i.G * amt), 0, 255),
                B = (byte)Util.Clamp(i.B + (i.B * amt), 0, 255),
                A = i.A
            };

            return c.Uint;
        }

        public static uint Gradient(uint color1, uint color2, float amt)
        {
            var i1 = new RGBA(color1);
            var i2 = new RGBA(color2);

            var iamt = 1f - amt;

            var c = new RGBA()
            {
                R = (byte)Util.Clamp(i2.R * amt + i1.R * iamt, 0, 255),
                G = (byte)Util.Clamp(i2.G * amt + i1.G * iamt, 0, 255),
                B = (byte)Util.Clamp(i2.B * amt + i1.B * iamt, 0, 255),
                A = (byte)Util.Clamp(i2.A * amt + i1.A * iamt, 0, 255)
            };

            return c.Uint;
        }

        public static uint GrayScale(uint color)
        {
            var i = new RGBA(color);

            var gray = (byte)(i.R * 0.299f + i.G * 0.587f + i.B * 0.114f);

            var c = new RGBA()
            {
                R = gray,
                G = gray,
                B = gray,
                A = i.A
            };

            return c.Uint;
        }

        public static uint Sepia(uint color)
        {
            var i = new RGBA(color);

            var c = new RGBA()
            {
                R = (byte)Util.Clamp(i.R * 0.393f + i.G * 0.769f + i.B * 0.189f, 0, 255),
                G = (byte)Util.Clamp(i.R * 0.349f + i.G * 0.686f + i.B * 0.168f, 0, 255),
                B = (byte)Util.Clamp(i.R * 0.272f + i.G * 0.534f + i.B * 0.131f, 0, 255),
                A = i.A
            };

            return c.Uint;
        }

        public static uint Negative(uint color)
        {
            var i = new RGBA(color);

            var c = new RGBA()
            {
                R = (byte)(255 - i.R),
                G = (byte)(255 - i.G),
                B = (byte)(255 - i.B),
                A = i.A
            };

            return c.Uint;
        }

        public static void UpdatePlasmaColor()
        {
            switch (plasmaState)
            {
                case 0:
                    plasma = new RGBA
                    {
                        R = 0,
                        G = 255,
                        B = 0,
                        A = 255
                    };
                    plasmaState = 1;
                    break;
                case 1:
                    plasma.R++;
                    if (plasma.R == 255)
                    {
                        plasmaState = 2;
                    }
                    break;
                case 2:
                    plasma.G--;
                    if (plasma.G == 0)
                    {
                        plasmaState = 3;
                    }
                    break;
                case 3:
                    plasma.B++;
                    if (plasma.B == 255)
                    {
                        plasmaState = 4;
                    }
                    break;
                case 4:
                    plasma.R--;
                    if (plasma.R == 0)
                    {
                        plasmaState = 5;
                    }
                    break;
                case 5:
                    plasma.G++;
                    if (plasma.G == 255)
                    {
                        plasmaState = 6;
                    }
                    break;
                case 6:
                    plasma.B--;
                    if (plasma.B == 0)
                    {
                        plasmaState = 1;
                    }
                    break;
            }
        }
    }
}
