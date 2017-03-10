using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class FBO : View
    {
        #region Dispose pattern
        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources here
            }

            // free unmanaged resources here
            if (frameBuffer != null)
            {
                Sprockets.Graphics.FreeFrameBuffer(frameBuffer);
                frameBuffer = null;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        protected Sprockets.Graphics.FrameBuffer frameBuffer;

        protected float[] stack;
        protected int stackPointer;

        public FBO(int id, int width, int height)
            : base(id)
        {
            if (width < 1 || width > Sprockets.Graphics.MaxTextureSize || height < 1 || height > Sprockets.Graphics.MaxTextureSize)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            this.frameBuffer = Sprockets.Graphics.CreateFrameBuffer(width, height, false);

            Unbind();

            this.stackPointer = 0;

            Show();
            Center();
        }

        public override string ToString()
        {
            return $"FBO";
        }

        public void Bind()
        {
            var activeFBOId = objectManager.ActiveFBOId;

            if (activeFBOId != objectId)
            {
                if (activeFBOId != 0)
                {
                    var fbo = objectManager[activeFBOId].LiquidObject as FBO;

                    fbo.frameBuffer.GLState = Sprockets.Graphics.SaveGLState();
                }

                Sprockets.Graphics.BindFBO(frameBuffer.Fbo);

                objectManager.ActiveFBOId = objectId;

                Sprockets.Graphics.RestoreGLState(frameBuffer.Width, frameBuffer.Height, frameBuffer.GLState);
            }
        }

        public void Unbind()
        {
            var activeFBOId = objectManager.ActiveFBOId;

            if (activeFBOId != 0)
            {
                var fbo = objectManager[activeFBOId].LiquidObject as FBO;

                fbo.frameBuffer.GLState = Sprockets.Graphics.SaveGLState();

                Sprockets.Graphics.UnbindFBO();

                objectManager.ActiveFBOId = 0;
            }
        }

        //public void Circle(int cx, int cy, int radius)
        //{
        //    Bind();

        //    Sprockets.Graphics.Circle(cx, cy, radius);
        //}

        //public void CircleFill(int cx, int cy, int radius)
        //{
        //    Bind();

        //    Sprockets.Graphics.CircleFill(cx, cy, radius);
        //}

        public void Clear()
        {
            Bind();

            Sprockets.Graphics.Clear();
        }

        //public void Ellipse(int cx, int cy, int xRadius, int yRadius)
        //{
        //    Bind();

        //    Sprockets.Graphics.Ellipse(cx, cy, xRadius, yRadius);
        //}

        //public void EllipseFill(int cx, int cy, int xRadius, int yRadius)
        //{
        //    Bind();

        //    Sprockets.Graphics.EllipseFill(cx, cy, xRadius, yRadius);
        //}

        public void Ink(uint color)
        {
            Bind();

            Sprockets.Graphics.MixColor(color);
        }

        public void Line(int x1, int y1, int x2, int y2)
        {
            Bind();

            Sprockets.Graphics.Line(x1, y1, x2, y2);
        }
        public void Plot(int x, int y)
        {
            Bind();

            Sprockets.Graphics.Plot(x, y);
        }

        //public void Rectangle(int x1, int y1, int x2, int y2)
        //{
        //    Bind();

        //    Sprockets.Graphics.Rectangle(x1, y1, x2, y2);
        //}

        public void RectangleFill(int x1, int y1, int x2, int y2)
        {
            Bind();

            Sprockets.Graphics.RectangleFill(x1, y1, x2, y2);
        }

        protected override void render(int orthoId)
        {
            var x = frameBuffer.Width / 2;
            var y = frameBuffer.Height / 2;

            Sprockets.Graphics.RenderTexture(frameBuffer.FboTexture, -x, -y + frameBuffer.Height - 1, -x + frameBuffer.Width - 1, -y);
        }

        public override void Destructor()
        {
            if (frameBuffer != null)
            {
                Sprockets.Graphics.FreeFrameBuffer(frameBuffer);
                frameBuffer = null;
            }

            base.Destructor();
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
