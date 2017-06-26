using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{

    public class Raster : Object
    {
        protected delegate void PixelOperatorDelegate(int index);

        protected int bitmapId;
        protected Bitmap bitmap;

        protected uint inkColor;
        protected int clipX1;
        protected int clipY1;
        protected int clipX2;
        protected int clipY2;
        protected PixelOperator pixelOperator;
        protected PixelOperatorDelegate pixelOperatorDelegate;
        protected uint pixelMask;
        protected uint pixelXorMask;
        protected uint lineStipple;
        protected int[] scanLineX1;
        protected int[] scanLineX2;

        public uint InkColor
        {
            get
            {
                return inkColor;
            }
            set
            {
                inkColor = value;
            }
        }

        public PixelOperator PixelOperator
        {
            get
            {
                return pixelOperator;
            }
            set
            {
                pixelOperator = value;

                switch (pixelOperator)
                {
                    case PixelOperator.Write:
                        pixelOperatorDelegate = pixelOperatorWrite;
                        break;
                    case PixelOperator.Blend:
                        pixelOperatorDelegate = pixelOperatorBlend;
                        break;
                    case PixelOperator.Min:
                        pixelOperatorDelegate = pixelOperatorMin;
                        break;
                    case PixelOperator.Max:
                        pixelOperatorDelegate = pixelOperatorMax;
                        break;
                    case PixelOperator.And:
                        pixelOperatorDelegate = pixelOperatorAnd;
                        break;
                    case PixelOperator.Or:
                        pixelOperatorDelegate = pixelOperatorOr;
                        break;
                    case PixelOperator.Xor:
                        pixelOperatorDelegate = pixelOperatorXor;
                        break;
                    case PixelOperator.Add:
                        pixelOperatorDelegate = pixelOperatorAdd;
                        break;
                    case PixelOperator.Sub:
                        pixelOperatorDelegate = pixelOperatorSub;
                        break;
                    case PixelOperator.Avg:
                        pixelOperatorDelegate = pixelOperatorAvg;
                        break;
                    case PixelOperator.Mult:
                        pixelOperatorDelegate = pixelOperatorMult;
                        break;
                    case PixelOperator.Zero:
                        pixelOperatorDelegate = pixelOperatorZero;
                        break;
                    case PixelOperator.Replace:
                        pixelOperatorDelegate = pixelOperatorReplace;
                        break;
                    case PixelOperator.AlphaTest:
                        pixelOperatorDelegate = pixelOperatorAlphaTest;
                        break;
                    default:
                        RaiseError(ErrorCode.IllegalQuantity);
                        return;
                }
            }
        }

        public uint PixelMask
        {
            get
            {
                return pixelMask;
            }
            set
            {
                pixelMask = value;
                pixelXorMask = pixelMask ^ 0xFFFFFFFF;
            }
        }

        public uint LineStipple
        {
            get
            {
                return lineStipple;
            }
            set
            {
                lineStipple = value;
            }
        }

        public static int NewRaster(int bitmapId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Raster);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Raster(id, bitmapId);

            return id;
        }

        public Raster(int id, int bitmapId)
            : base(id)
        {
            if (bitmapId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            this.inkColor = 0xFFFFFFFF;
            this.clipX1 = 0;
            this.clipY1 = 0;
            this.clipX2 = bitmap.Width - 1;
            this.clipY2 = bitmap.Height - 1;
            this.pixelOperator = PixelOperator.Write;
            this.pixelOperatorDelegate = pixelOperatorWrite;
            this.pixelMask = 0xFFFFFFFF;
            this.pixelXorMask = 0x00000000;
            this.LineStipple = 0xFFFFFFFF;
            this.scanLineX1 = new int[Sprockets.Graphics.MaxTextureSize];
            this.scanLineX2 = new int[Sprockets.Graphics.MaxTextureSize];
        }

        public override string ToString()
        {
            return $"Raster";
        }

        #region Pixel Operators
        private void pixelOperatorWrite(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorBlend(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var alpha = da;
            var ialpha = 255 - da;

            var rr = (alpha * dr + ialpha * sr) >> 8;
            var gg = (alpha * dg + ialpha * sg) >> 8;
            var bb = (alpha * db + ialpha * sb) >> 8;
            var aa = (uint)255;

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorMin(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = Math.Min(sr, dr);
            var gg = Math.Min(sg, dg);
            var bb = Math.Min(sb, db);
            var aa = Math.Min(sa, da);

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorMax(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = Math.Max(sr, dr);
            var gg = Math.Max(sg, dg);
            var bb = Math.Max(sb, db);
            var aa = Math.Max(sa, da);

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorAnd(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = src & inkColor;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorOr(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = src | inkColor;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorXor(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = src ^ inkColor;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorAdd(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = Math.Min(sr + dr, 255);
            var gg = Math.Min(sg + dg, 255);
            var bb = Math.Min(sb + db, 255);
            var aa = Math.Min(sa + db, 255);

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorSub(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = (uint)Math.Max((int)sr - (int)dr, 0);
            var gg = (uint)Math.Max((int)sg - (int)dg, 0);
            var bb = (uint)Math.Max((int)sb - (int)db, 0);
            var aa = (uint)Math.Max((int)sa - (int)db, 0);

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorAvg(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = (sr + dr) >> 1;
            var gg = (sg + dg) >> 1;
            var bb = (sb + db) >> 1;
            var aa = (sa + da) >> 1;

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorMult(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            var sr = src & 255;
            var sg = src >> 8 & 255;
            var sb = src >> 16 & 255;
            var sa = src >> 24;

            var dr = dst & 255;
            var dg = dst >> 8 & 255;
            var db = dst >> 16 & 255;
            var da = dst >> 24;

            var rr = (sr * dr) >> 8;
            var gg = (sg * dg) >> 8;
            var bb = (sb * db) >> 8;
            var aa = (sa * da) >> 8;

            dst = rr | gg << 8 | bb << 16 | aa << 24;

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorZero(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            if ((src & 0x00FFFFFF) != 0)
            {
                return;
            }

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorReplace(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            if ((src & 0x00FFFFFF) == 0)
            {
                return;
            }

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }

        private void pixelOperatorAlphaTest(int index)
        {
            var src = bitmap.FastPeek(index);
            var dst = inkColor;

            if ((dst & 0xFF000000) == 0)
            {
                return;
            }

            bitmap.FastPoke(index, (dst & pixelMask) | (src & pixelXorMask));
        }
        #endregion

        public void Clip(int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x1 >= bitmap.Width || x2 < 0 || x2 >= bitmap.Width)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            if (y1 < 0 || y1 >= bitmap.Height || y2 < 0 || y2 >= bitmap.Height)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }

            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            clipX1 = x1;
            clipY1 = y1;
            clipX2 = x2;
            clipY2 = y2;
        }

        public void Fill(uint color)
        {
            inkColor = color;

            for (var index = 0; index < bitmap.Size; index++)
            {
                pixelOperatorDelegate(index);
            }
        }

        public void Plot(int x, int y)
        {
            if ((x >= clipX1 && x <= clipX2) && (y >= clipY1 && y <= clipY2))
            {
                var index = y * bitmap.Width + x;

                pixelOperatorDelegate(index);
            }
        }

        public void Plot(int x, int y, uint color)
        {
            if ((x >= clipX1 && x <= clipX2) && (y >= clipY1 && y <= clipY2))
            {
                var index = y * bitmap.Width + x;

                bitmap.FastPoke(index, color);
            }
        }

        public uint Point(int x, int y)
        {
            if ((x >= clipX1 && x <= clipX2) && (y >= clipY1 && y <= clipY2))
            {
                return bitmap.FastPeek(y * bitmap.Width + x);
            }
            else
            {
                return 0;
            }
        }

        public void hLine(int x1, int x2, int y)
        {
            if (y >= clipY1 && y <= clipY2)
            {
                if (x1 > x2)
                {
                    Util.Swap(ref x1, ref x2);
                }
                if (x2 >= clipX1 && x1 <= clipX2)
                {
                    var pattern = lineStipple;

                    x1 = Math.Max(x1, clipX1);
                    x2 = Math.Min(x2, clipX2);

                    var index = y * bitmap.Width + x1;

                    for (var x = x1; x <= x2; x++)
                    {
                        if (Util.IsOdd((int)pattern))
                        {
                            pixelOperatorDelegate(index);
                        }

                        pattern = Util.RotateRight(pattern);

                        index++;
                    }
                }
            }
        }

        public void vLine(int x, int y1, int y2)
        {
            if (x >= clipX1 && x <= clipX2)
            {
                if (y1 > y2)
                {
                    Util.Swap(ref y1, ref y2);
                }
                if (y2 >= clipY1 && y1 <= clipY2)
                {
                    var pattern = lineStipple;

                    y1 = Math.Max(y1, clipY1);
                    y2 = Math.Min(y2, clipY2);

                    var index = y1 * bitmap.Width + x;

                    for (var y = y1; y <= y2; y++)
                    {
                        if (Util.IsOdd((int)pattern))
                        {
                            pixelOperatorDelegate(index);
                        }

                        pattern = Util.RotateRight(pattern);

                        index += bitmap.Width;
                    }
                }
            }
        }

        public void Line(int x1, int y1, int x2, int y2)
        {
            var u = x2 - x1;
            if (u == 0)
            {
                vLine(x1, y1, y2);
                return;
            }

            var v = y2 - y1;
            if (v == 0)
            {
                hLine(x1, x2, y1);
                return;
            }

            var pattern = lineStipple;

            var d1x = Math.Sign(u);
            var d1y = Math.Sign(v);
            var d2x = Math.Sign(u);
            var d2y = 0;
            var m = Math.Abs(u);
            var n = Math.Abs(v);

            if (m <= n)
            {
                d2x = 0;
                d2y = Math.Sign(v);
                m = Math.Abs(v);
                n = Math.Abs(u);
            }

            var s = m / 2;

            for (var i = 0; i <= m; i++)
            {
                if (Util.IsOdd((int)pattern))
                {
                    if ((x1 >= clipX1 && x1 <= clipX2) && (y1 >= clipY1 && y1 <= clipY2))
                    {
                        var index = y1 * bitmap.Width + x1;

                        pixelOperatorDelegate(index);
                    }
                }

                pattern = Util.RotateRight(pattern);

                s += n;
                if (s >= m)
                {
                    s -= m;
                    x1 += d1x;
                    y1 += d1y;
                }
                else
                {
                    x1 += d2x;
                    y1 += d2y;
                }
            }
        }

        public void Radial(int x, int y, float direction, float step)
        {
            var x2 = x + Math.Sin(direction * Math.PI / 180d) * step;
            var y2 = y - Math.Cos(direction * Math.PI / 180d) * step;

            Line(x, y, (int)Math.Round(x2), (int)Math.Round(y2));
        }

        private void polyClear()
        {
            for (var i = clipY1; i <= clipY2; i++)
            {
                scanLineX1[i] = 0x7FFFFFFF;
                scanLineX2[i] = 0x7FFFFFFF;
            }
        }

        private void polyLine(int x1, int y1, int x2, int y2)
        {
            if (y2 == y1)
            {
                return;
            }

            if (y2 < y1)
            {
                Util.Swap(ref x1, ref x2);
                Util.Swap(ref y1, ref y2);
            }

            var x = x1 << 10;

            var m = ((x2 - x1) << 10) / (y2 - y1);

            x += m;
            y1++;

            for (var y = y1; y <= y2; y++)
            {
                if (y >= clipY1 && y <= clipY2)
                {
                    if (scanLineX1[y] == 0x7FFFFFFF)
                    {
                        scanLineX1[y] = x >> 10;
                    }
                    else
                    {
                        scanLineX2[y] = x >> 10;
                    }
                }

                x += m;
            }
        }

        private void polyFill()
        {
            for (var i = clipY1; i <= clipY2; i++)
            {
                if (scanLineX1[i] != 0x7FFFFFFF)
                {
                    if (scanLineX2[i] == 0x7FFFFFFF)
                    {
                        Plot(scanLineX1[i], i);
                    }
                    else
                    {
                        hLine(scanLineX1[i], scanLineX2[i], i);
                    }
                }
            }
        }

        public void Triangle(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            Line(x1, y1, x2, y2);
            Line(x2, y2, x3, y3);
            Line(x3, y3, x1, y1);
        }

        public void TriangleFill(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            polyClear();
            polyLine(x1, y1, x2, y2);
            polyLine(x2, y2, x3, y3);
            polyLine(x3, y3, x1, y1);
            polyFill();
        }

        public void Rectangle(int x1, int y1, int x2, int y2)
        {
            hLine(x1, x2, y1);
            vLine(x2, y1 + 1, y2);
            hLine(x1, x2 - 1, y2);
            vLine(x1, y1 + 1, y2 - 1);
        }

        public void RectangleFill(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }

            if (x2 >= clipX1 && x1 <= clipX2)
            {
                x1 = Math.Max(x1, clipX1);
                x2 = Math.Min(x2, clipX2);
            }
            else
            {
                return;
            }

            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            if (y2 >= clipY1 && y1 <= clipY2)
            {
                y1 = Math.Max(y1, clipY1);
                y2 = Math.Min(y2, clipY2);
            }
            else
            {
                return;
            }

            for (var y = y1; y <= y2; y++)
            {
                var index = y * bitmap.Width + x1;
                for (var x = x1; x <= x2; x++)
                {
                    pixelOperatorDelegate(index);
                    index++;
                }
            }
        }

        private void drawCorner(int mx, int my, int r, RectangleCorner corner)
        {
            if (r <= 0)
            {
                return;
            }

            r--;
            var x2 = r;
            for (var y = 0; y <= r; y++)
            {
                var y2 = y - 0.5d;
                var x = (int)Math.Round(Math.Sqrt(r * r - y2 * y2));
                for (var j = x; j <= x2; j++)
                {
                    switch (corner)
                    {
                        case RectangleCorner.UpperLeft:
                            Plot(mx - j, my - y);
                            break;
                        case RectangleCorner.UpperRight:
                            Plot(mx + j, my - y);
                            break;
                        case RectangleCorner.LowerLeft:
                            Plot(mx - j, my + y);
                            break;
                        case RectangleCorner.LowerRight:
                            Plot(mx + j, my + y);
                            break;
                    }
                }
                x2 = x;
            }

            r++;
            for (var j = 0; j <= x2; j++)
            {
                switch (corner)
                {
                    case RectangleCorner.UpperLeft:
                        Plot(mx - j, my - r);
                        break;
                    case RectangleCorner.UpperRight:
                        Plot(mx + j, my - r);
                        break;
                    case RectangleCorner.LowerLeft:
                        Plot(mx - j, my + r);
                        break;
                    case RectangleCorner.LowerRight:
                        Plot(mx + j, my + r);
                        break;
                }
            }
        }

        public void RoundedRectangle(int x1, int y1, int x2, int y2, int r)
        {
            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            if (r > (x2 - x1) / 3)
            {
                r = (x2 - x1) / 3;
            }
            if (r > (y2 - y1) / 3)
            {
                r = (y2 - y1) / 3;
            }

            drawCorner(x1 + r - 1, y1 + r, r, RectangleCorner.UpperLeft);

            drawCorner(x2 - r + 1, y1 + r, r, RectangleCorner.UpperRight);

            drawCorner(x1 + r - 1, y2 - r, r, RectangleCorner.LowerLeft);

            drawCorner(x2 - r + 1, y2 - r, r, RectangleCorner.LowerRight);

            hLine(x1 + r, x2 - r, y1);                              // top
            hLine(x1 + r, x2 - r, y2);                              // bottom
            vLine(x1, y1 + r + 1, y2 - r - 1);                      // left
            vLine(x2, y1 + r + 1, y2 - r - 1);                      // right
        }

        private void drawCornerFill(int mx, int my, int r, RectangleCorner corner)
        {
            if (r <= 0)
            {
                return;
            }

            for (var y = 0; y <= r; y++)
            {
                var y2 = y - 0.5d;
                var x = (int)Math.Round(Math.Sqrt(r * r - y2 * y2));
                switch (corner)
                {
                    case RectangleCorner.UpperLeft:
                        hLine(mx - x, mx, my - y);
                        break;
                    case RectangleCorner.UpperRight:
                        hLine(mx, mx + x, my - y);
                        break;
                    case RectangleCorner.LowerLeft:
                        hLine(mx - x, mx, my + y);
                        break;
                    case RectangleCorner.LowerRight:
                        hLine(mx, mx + x, my + y);
                        break;
                }
            }
        }

        public void RoundedRectangleFill(int x1, int y1, int x2, int y2, int r)
        {
            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            if (r > (x2 - x1) / 3)
            {
                r = (x2 - x1) / 3;
            }
            if (r > (y2 - y1) / 3)
            {
                r = (y2 - y1) / 3;
            }

            drawCornerFill(x1 + r, y1 + r, r, RectangleCorner.UpperLeft);

            drawCornerFill(x2 - r, y1 + r, r, RectangleCorner.UpperRight);

            drawCornerFill(x1 + r, y2 - r, r, RectangleCorner.LowerLeft);

            drawCornerFill(x2 - r, y2 - r, r, RectangleCorner.LowerRight);

            RectangleFill(x1 + r + 1, y1, x2 - r - 1, y1 + r);      // top
            RectangleFill(x1 + r + 1, y2 - r, x2 - r - 1, y2);      // bottom
            RectangleFill(x1, y1 + r + 1, x2, y2 - r - 1);          // rest
        }

        public void Quad(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            Line(x1, y1, x2, y2);
            Line(x2, y2, x3, y3);
            Line(x3, y3, x4, y4);
            Line(x4, y4, x1, y1);
        }

        public void QuadFill(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            polyClear();
            polyLine(x1, y1, x2, y2);
            polyLine(x2, y2, x3, y3);
            polyLine(x3, y3, x4, y4);
            polyLine(x4, y4, x1, y1);
            polyFill();
        }

        public void Circle(int mx, int my, int r)
        {
            Ellipse(mx, my, r, r);
        }

        public void CircleFill(int mx, int my, int r)
        {
            EllipseFill(mx, my, r, r);
        }

        public void Ellipse(int mx, int my, int xr, int yr)
        {
            if (yr < 0)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            if (yr == 0)
            {
                return;
            }

            var x = 0;
            var x2 = xr;

            yr--;
            for (var y = 0; y <= yr; y++)
            {
                var y2 = y - 0.5d;
                x = (yr == 0) ? 0 : (int)Math.Round(((double)xr / yr) * Math.Sqrt(yr * yr - y2 * y2));
                for (var c = x; c <= x2; c++)
                {
                    Plot(mx + c, my + y);
                    Plot(mx - c, my + y);
                    Plot(mx + c, my - y);
                    Plot(mx - c, my - y);
                }
                x2 = x;
            }

            yr++;
            for (var c = 0; c <= x; c++)
            {
                Plot(mx + c, my + yr);
                Plot(mx - c, my + yr);
                Plot(mx + c, my - yr);
                Plot(mx - c, my - yr);
            }
        }

        public void EllipseFill(int mx, int my, int xr, int yr)
        {
            if (yr < 0)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            if (yr == 0)
            {
                return;
            }

            hLine(mx - xr, mx + xr, my);

            for (var y = 1; y <= yr; y++)
            {
                var y2 = y - 0.5d;
                var x = (int)Math.Round(((double)xr / yr) * Math.Sqrt(yr * yr - y2 * y2));
                hLine(mx - x, mx + x, my + y);
                hLine(mx - x, mx + x, my - y);
            }
        }

        public void BezierCurve(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            // Polynomial coefficients
            var cx = 3d * (x1 - x0);
            var bx = 3d * (x2 - x1) - cx;
            var ax = x3 - x0 - cx - bx;

            var cy = 3d * (y1 - y0);
            var by = 3d * (y2 - y1) - cy;
            var ay = y3 - y0 - cy - by;

            var dt = 1d / 40d;

            var t = 0d;
            var tSquared = 0d;
            var tCubed = 0d;
            var dx = (int)Math.Round((ax * tCubed) + (bx * tSquared) + (cx * t) + x0);
            var dy = (int)Math.Round((ay * tCubed) + (by * tSquared) + (cy * t) + y0);
            for (var i = 1; i <= 40; i++)
            {
                t = i * dt;
                tSquared = t * t;
                tCubed = tSquared * t;
                var x = (int)Math.Round((ax * tCubed) + (bx * tSquared) + (cx * t) + x0);
                var y = (int)Math.Round((ay * tCubed) + (by * tSquared) + (cy * t) + y0);
                Line(dx, dy, x, y);
                dx = x;
                dy = y;
            }
        }

        public void FloodFill(int x, int y)
        {
            var stack = new int[Sprockets.Graphics.MaxTextureSize / 16];

            var stackPointer = 0;

            if ((x >= clipX1 && x <= clipX2) && (y >= clipY1 && y <= clipY2))
            {
                var oldColor = bitmap.FastPeek(y * bitmap.Width + x);
                var newColor = inkColor;

                if (oldColor == newColor)
                {
                    return;
                }

                stackPointer = 0;
                stack[stackPointer++] = x;
                stack[stackPointer++] = y;

                while (stackPointer > 0)
                {
                    y = stack[--stackPointer]; 
                    x = stack[--stackPointer]; 

                    var index = y * bitmap.Width + x;
                    var spanTop = false;
                    var spanBottom = false;

                    // Find the left side of the scanline
                    while (x >= clipX1 && bitmap.FastPeek(index) == oldColor)
                    {
                        x--;
                        index--;
                    }
                    x++;
                    index++;

                    // Loop until the right side of the scanline is hit
                    while (x <= clipX2 && bitmap.FastPeek(index) == oldColor)
                    {
                        bitmap.FastPoke(index, newColor);

                        if (!spanTop && y > clipY1 && bitmap.FastPeek(index - bitmap.Width) == oldColor)
                        {
                            if (stackPointer < stack.Length - 2)
                            {
                                stack[stackPointer++] = x;
                                stack[stackPointer++] = y - 1;
                            }
                            else
                            {
                                return;
                            }
                            spanTop = true;
                        }
                        else if (spanTop && y > clipY1 && bitmap.FastPeek(index - bitmap.Width) != oldColor)
                        {
                            spanTop = false;
                        }

                        if (!spanBottom && y < clipY2 && bitmap.FastPeek(index + bitmap.Width) == oldColor)
                        {
                            if (stackPointer < stack.Length - 2)
                            {
                                stack[stackPointer++] = x;
                                stack[stackPointer++] = y + 1;
                            }
                            else
                            {
                                return;
                            }
                            spanBottom = true;
                        }
                        else if (spanBottom && y < clipY2 && bitmap.FastPeek(index + bitmap.Width) != oldColor)
                        {
                            spanBottom = false;
                        }

                        x++;
                        index++;
                    }
                }
            }
        }

        public void Scroll(int x1, int y1, int x2, int y2, ScrollDirection direction, int count = 1, bool wrap = false)
        {
            switch (direction)
            {
                case ScrollDirection.Up:
                    ScrollUp(x1, y1, x2, y2, count, wrap);
                    break;
                case ScrollDirection.Down:
                    ScrollDown(x1, y1, x2, y2, count, wrap);
                    break;
                case ScrollDirection.Left:
                    ScrollLeft(x1, y1, x2, y2, count, wrap);
                    break;
                case ScrollDirection.Right:
                    ScrollRight(x1, y1, x2, y2, count, wrap);
                    break;
                default:
                    RaiseError(ErrorCode.IllegalQuantity);
                    return;
            }
        }

        public void ScrollUp(int x1, int y1, int x2, int y2, int count = 1, bool wrap = false)
        {
            var c = 0u;

            for (var i = 1; i <= count; i++)
            {
                for (var x = x1; x <= x2; x++)
                {
                    var s = (y1 + 1) * bitmap.Width + x;
                    var d = y1 * bitmap.Width + x;

                    if (wrap)
                    {
                        c = bitmap.FastPeek(d);   
                    }

                    for (var y = y1 + 1; y <= y2; y++)
                    {
                        bitmap.FastPoke(d, bitmap.FastPeek(s));
                        s += bitmap.Width;
                        d += bitmap.Width;
                    }

                    bitmap.FastPoke(d, c);
                }
            }
        }

        public void ScrollDown(int x1, int y1, int x2, int y2, int count = 1, bool wrap = false)
        {
            var c = 0u;

            for (var i = 1; i <= count; i++)
            {
                for (var x = x1; x <= x2; x++)
                {
                    var s = (y2 - 1) * bitmap.Width + x;
                    var d = y2 * bitmap.Width + x;

                    if (wrap)
                    {
                        c = bitmap.FastPeek(d);
                    }

                    for (var y = y2; y >= y1 + 1; y--)
                    {
                        bitmap.FastPoke(d, bitmap.FastPeek(s));
                        s -= bitmap.Width;
                        d -= bitmap.Width;
                    }

                    bitmap.FastPoke(d, c);
                }
            }
        }

        public void ScrollLeft(int x1, int y1, int x2, int y2, int count = 1, bool wrap = false)
        {
            var c = 0u;

            for (var i = 1; i <= count; i++)
            {
                for (var y = y1; y <= y2; y++)
                {
                    var s = y * bitmap.Width + x1 + 1;
                    var d = y * bitmap.Width + x1;

                    if (wrap)
                    {
                        c = bitmap.FastPeek(d);
                    }

                    for (var x = x1; x <= x2 - 1; x++)
                    {
                        bitmap.FastPoke(d, bitmap.FastPeek(s));
                        s++;
                        d++;
                    }

                    bitmap.FastPoke(d, c);
                }
            }
        }

        public void ScrollRight(int x1, int y1, int x2, int y2, int count = 1, bool wrap = false)
        {
            var c = 0u;

            for (var i = 1; i <= count; i++)
            {
                for (var y = y1; y <= y2; y++)
                {
                    var s = y * bitmap.Width + x2 - 1;
                    var d = y * bitmap.Width + x2;

                    if (wrap)
                    {
                        c = bitmap.FastPeek(d);
                    }

                    for (var x = x2; x >= x1 + 1; x--)
                    {
                        bitmap.FastPoke(d, bitmap.FastPeek(s));
                        s--;
                        d--;
                    }

                    bitmap.FastPoke(d, c);
                }
            }
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            scanLineX1 = null;
            scanLineX2 = null;

            base.shutdown();
        }
    }
}
