using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Perspective : Object
    {
        public static int NewPerspective(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Perspective);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Perspective(id);

            return id;
        }

        public Perspective(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"Perspective";
        }

        public void AlphaTest(bool mode)
        {
            if (mode)
            {
                Sprockets.Graphics.AlphaTest();
            }
            else
            {
                Sprockets.Graphics.NoAlphaTest();
            }
        }

        public void Begin(int mode)
        {
            Sprockets.Graphics.Begin((OpenTK.Graphics.OpenGL.PrimitiveType)mode);
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

        public void DepthFunc(int func)
        {
            Sprockets.Graphics.DepthFunc((OpenTK.Graphics.OpenGL.DepthFunction)func);
        }

        public void DepthTest(bool mode)
        {
            if (mode)
            {
                Sprockets.Graphics.DepthTest();
            }
            else
            {
                Sprockets.Graphics.NoDepthTest();
            }
        }

        public void End()
        {
            Sprockets.Graphics.End();
        }

        public void Ink(uint color)
        {
            Sprockets.Graphics.MixColor(color);
        }

        public void LineWidth(float width)
        {
            Sprockets.Graphics.LineWidth(width);
        }

        public void LoadIdentity()
        {
            Sprockets.Graphics.LoadIdentity();
        }

        public void Normal(double x, double y, double z)
        {
            Sprockets.Graphics.Normal(x, y, z);
        }

        public void PointSize(float size)
        {
            Sprockets.Graphics.PointSize(size);
        }

        public void PopMatrix()
        {
            Sprockets.Graphics.PopMatrix();
        }

        public void PushMatrix()
        {
            Sprockets.Graphics.PushMatrix();
        }

        public void Rotate(double angle, double x, double y, double z)
        {
            Sprockets.Graphics.Rotate(angle, x, y, z);
        }

        public void Scale(double x, double y, double z)
        {
            Sprockets.Graphics.Scale(x, y, z);
        }

        public void TexCoord(double x, double y)
        {
            Sprockets.Graphics.TexCoord(x, y);
        }

        public void TextureMap(int bitmapId)
        {
            if (bitmapId == 0)
            {
                Sprockets.Graphics.NoTextureMap();
            }
            else
            {
                var bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

                bitmap.TextureMap();
            }
        }

        public void Translate(double x, double y, double z)
        {
            Sprockets.Graphics.Translate(x, y, z);
        }

        public void Vertex(double x, double y, double z)
        {
            Sprockets.Graphics.Vertex(x, y, z);
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}