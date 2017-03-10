using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Ortho : Object
    {
        public static int NewOrtho(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Ortho);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Ortho(id);

            return id;
        }

        public Ortho(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"Ortho";
        }

        public void BezierCurve(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            Sprockets.Graphics.BezierCurve(x0, y0, x1, y1, x2, y2, x3, y3);
        }

        public void Blend(bool mode)
        {
            if (mode)
            {
                Sprockets.Graphics.Blend();
            }
            else
            {
                Sprockets.Graphics.NoBlend();
            }
        }

        public void BlendFunc(int sFactor, int dFactor)
        {
            Sprockets.Graphics.BlendFunc((OpenTK.Graphics.OpenGL.BlendingFactorSrc)sFactor, (OpenTK.Graphics.OpenGL.BlendingFactorDest)dFactor);
        }

        public void CircleFill(int cx, int cy, int radius)
        {
            Sprockets.Graphics.CircleFill(cx, cy, radius);
        }

        public void Ink(uint color)
        {
            Sprockets.Graphics.MixColor(color);
        }

        public void Rectangle(int x1, int y1, int x2, int y2)
        {
            Sprockets.Graphics.Rectangle(x1, y1, x2, y2);
        }

        public void RectangleFill(int x1, int y1, int x2, int y2)
        {
            Sprockets.Graphics.RectangleFill(x1, y1, x2, y2);
        }

        public void Scale(double x, double y)
        {
            Sprockets.Graphics.Scale(x, y);
        }

        public void Stamp(int bitmapId, int x, int y)
        {
            if (bitmapId == 0)
            {
                Throw(ExceptionCode.NullObject);
                return;
            }

            var bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            bitmap.Render(x, y);
        }

        public void Translate(int x, int y)
        {
            Sprockets.Graphics.Translate(x, y);
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}