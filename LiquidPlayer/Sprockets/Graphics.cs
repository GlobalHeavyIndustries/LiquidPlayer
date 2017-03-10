using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace LiquidPlayer.Sprockets
{
    public static class Graphics
    {
        private struct graphicsLinks
        {
            public int ObjectId
            {
                get;
                set;
            }

            public int Node
            {
                get;
                set;
            }
        }

        private struct clipNode
        {
            public int ObjectId
            {
                get;
                set;
            }

            public int X1
            {
                get;
                set;
            }

            public int Y1
            {
                get;
                set;
            }

            public int X2
            {
                get;
                set;
            }

            public int Y2
            {
                get;
                set;
            }

            public double[] Matrix
            {
                get;
                set;
            }
        }

        public struct GLState
        {
            public float[] Paper;
            public float[] Ink;
            public double[] ModelViewMatrix;
            public double[] ProjectionMatrix;
            public double[] TextureMatrix;
        }

        public class FrameBuffer
        {
            public int Width;
            public int Height;

            public int Fbo;
            public int FboDepth;
            public int FboTexture;

            public GLState GLState;

            public FrameBuffer(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        private static int gameWindowWidth;
        private static int gameWindowHeight;

        private static bool isResized;

        private static int objectId;
        private static int node;
        private static int depthBase;
        private static graphicsLinks[] links;

        private static bool sorted;
        private static bool sorted3D;

        private static Color.RGBA inkColor;

        private static double[] matrixStack;
        private static int matrixStackPointer;

        private static clipNode[] clipStack;
        private static int clipStackPointer;

        private static int maxTextureSize;
        private static int textureUnits;

        public static bool IsResized
        {
            get
            {
                var temp = isResized;

                isResized = false;

                return temp;
            }
        }

        public static int DepthBase
        {
            get
            {
                return depthBase;
            }
        }

        public static bool Sorted
        {
            get
            {
                return sorted;
            }
            set
            {
                sorted = value;
            }
        }

        public static bool Sorted3D
        {
            get
            {
                return sorted3D;
            }
            set
            {
                sorted3D = value;
            }
        }

        public static Color.RGBA Ink
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

        public static int MaxTextureSize
        {
            get
            {
                return maxTextureSize;
            }
        }

        public static void Init(int width, int height)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1f);
            GL.ClearStencil(0);

            Clear();

            GL.ShadeModel(ShadingModel.Smooth);
            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PolygonSmooth);
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.Lighting);

            GL.Disable(EnableCap.CullFace);

            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.FogHint, HintMode.Nicest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            gameWindowWidth = width;
            gameWindowHeight = height;

            ResetNodes();

            links = new graphicsLinks[65536];

            sorted = true;
            sorted3D = true;

            inkColor = Sprockets.Color.White;

            matrixStack = new double[16];
            matrixStackPointer = 0;

            clipStack = new clipNode[256];
            clipStackPointer = 0;

            maxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);

            textureUnits = GL.GetInteger(GetPName.MaxTextureUnits);
            if (textureUnits < 1)
            {
                textureUnits = 1;
            }
            else if (textureUnits > 32)
            {
                textureUnits = 32;
            }
        }

        public static ErrorCode GetError()
        {
            return GL.GetError();
        }

        // Rendering

        public static void EnableRendering()
        {
            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilMask(0xFF);
        }

        public static void DisableRendering()
        {
            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.StencilMask(0x00);
        }

        // Reset

        public static void Reset(uint color)
        {
            inkColor.Uint = color;

            SetColor(color);

            PointSize(1f);
            LineWidth(1f);
            NoLineStipple();
            AlphaTest();
            AlphaFunc(AlphaFunction.Notequal, 0f);
            Blend();
            BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            DepthTest();
            DepthFunc(DepthFunction.Lequal);

            ResetMatrixStack();
            ResetTextureStack();
        }

        public static void Reset3D(uint color)
        {
            inkColor.Uint = color;

            SetColor(color);

            PointSize(1f);
            LineWidth(1f);
            NoLineStipple();
            AlphaTest();
            AlphaFunc(AlphaFunction.Notequal, 0f);
            Blend();
            BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            DepthTest();
            DepthFunc(DepthFunction.Less);
            EdgeFlag(true);

            ResetMatrixStack();
            ResetTextureStack();
        }

        // Clearscreen

        public static void ClearColor(float r, float g, float b, float a = 1f)
        {
            GL.ClearColor(r, g, b, a);
        }

        public static void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.LoadIdentity();
        }

        public static void ClearColorBuffer()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public static void ClearDepthBuffer()
        {
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public static void ClearStencilBuffer()
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);
        }

        // Views

        public static void Resize(int width, int height)
        {
            gameWindowWidth = width;
            gameWindowHeight = height;

            isResized = true;
        }

        public static void ViewOrtho()
        {
            GL.Viewport(0, 0, gameWindowWidth, gameWindowHeight);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0, gameWindowWidth, gameWindowHeight, 0, -65535, 0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        public static void ViewPerspective(double fovy, double aspect, double zNear, double zFar)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            var yMax = zNear * System.Math.Tan((fovy * 3.14159265358979d) / 360d);
            var yMin = -yMax;
            var xMin = yMin * aspect;
            var xMax = yMax * aspect;

            GL.Frustum(xMin, xMax, yMin, yMax, zNear, zFar);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        // Links

        public static void ResetNodes()
        {
            objectId = 0;
            node = 0;
            depthBase = 1;
        }

        private static void pushLink()
        {
            links[depthBase] = new graphicsLinks
            {
                ObjectId = objectId,
                Node = node
            };
            depthBase++;
        }

        private static void popLink()
        {
            depthBase--;
        }

        public static void LinkObject(int objectId)
        {
            GL.LoadIdentity();
            GL.Translate(0f, 0f, depthBase);

            Graphics.objectId = objectId;
            Graphics.node = 0;

            pushLink();
        }

        public static void LinkNextObject(int objectId)
        {
            GL.Translate(0f, 0f, 1f);

            Graphics.objectId = objectId;
            Graphics.node = 0;

            pushLink();
        }

        public static void LinkNextNode(int node)
        {
            GL.Translate(0f, 0f, 1f);

            Graphics.node = node;

            pushLink();
        }

        // Points

        public static float GetPointSize()
        {
            return GL.GetFloat(GetPName.PointSize);
        }

        public static void PointSize(float size)
        {
            GL.PointSize(size);
        }

        // Lines

        public static float GetLineWidth()
        {
            return GL.GetFloat(GetPName.LineWidth);
        }

        public static void LineWidth(float width)
        {
            GL.LineWidth(width);
        }

        public static void LineStipple(int factor, short pattern)
        {
            GL.Enable(EnableCap.LineStipple);
            GL.LineStipple(factor, pattern);
        }

        public static void NoLineStipple()
        {
            GL.Disable(EnableCap.LineStipple);
        }

        // Alpha testing

        public static bool IsAlphaTest()
        {
            return GL.IsEnabled(EnableCap.AlphaTest);
        }

        public static void AlphaTest()
        {
            GL.Enable(EnableCap.AlphaTest);
        }

        public static void AlphaFunc(AlphaFunction func, float refValue)
        {
            GL.AlphaFunc(func, refValue);
        }

        public static void NoAlphaTest()
        {
            GL.Disable(EnableCap.AlphaTest);
        }

        // Blending

        public static bool IsBlend()
        {
            return GL.IsEnabled(EnableCap.Blend);
        }

        public static void Blend()
        {
            GL.Enable(EnableCap.Blend);
        }

        public static void BlendFunc(BlendingFactorSrc sFactor, BlendingFactorDest dFactor)
        {
            GL.BlendFunc(sFactor, dFactor);
        }

        public static void NoBlend()
        {
            GL.Disable(EnableCap.Blend);
        }

        // Depth testing

        public static bool IsDepthTest()
        {
            return GL.IsEnabled(EnableCap.DepthTest);
        }

        public static void DepthTest()
        {
            GL.Enable(EnableCap.DepthTest);
        }

        public static void DepthFunc(DepthFunction func)
        {
            GL.DepthFunc(func);
        }

        public static void NoDepthTest()
        {
            GL.Disable(EnableCap.DepthTest);
        }

        // Edge flag

        public static void EdgeFlag(bool edgeFlag)
        {
            GL.EdgeFlag(edgeFlag);
        }

        // Scissor testing

        public static bool IsScissorTest()
        {
            return GL.IsEnabled(EnableCap.ScissorTest);
        }

        public static void ScissorTest()
        {
            GL.Enable(EnableCap.ScissorTest);
        }

        public static void Scissor(int x, int y, int width, int height)
        {
            GL.Scissor(x, y, width, height);
        }

        public static void NoScissorTest()
        {
            GL.Disable(EnableCap.ScissorTest);
        }

        // Stencil testing

        public static bool IsStencilTest()
        {
            return GL.IsEnabled(EnableCap.StencilTest);
        }

        public static void StencilTest()
        {
            GL.Enable(EnableCap.StencilTest);
        }

        public static void StencilFunc(StencilFunction func, int refValue, int mask)
        {
            GL.StencilFunc(func, refValue, mask);
        }

        public static void StencilOp(StencilOp fail, StencilOp zFail, StencilOp zPass)
        {
            GL.StencilOp(fail, zFail, zPass);
        }

        public static void NoStencilTest()
        {
            GL.Disable(EnableCap.StencilTest);
        }

        // Clipping
        
        public static void ClipRectangle(int objectId, int x1, int y1, int x2, int y2, Color.RGBA bg, ClipRectangleStyle style = ClipRectangleStyle.None)
        {
            if (clipStackPointer == 255)
            {
                throw new Exception("Stack overflow");
            }

            var sp = clipStackPointer;

            if (sp == 0)
            {
                GL.Enable(EnableCap.StencilTest);
                GL.StencilFunc(StencilFunction.Equal, sp, 0xFF);
                GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep);
            }

            switch (style)
            {
                case ClipRectangleStyle.None:
                    break;
                case ClipRectangleStyle.Outline:
                    GL.Color4(0f, 0f, 0f, 1f);
                    Rectangle(x1 - 1, y1 - 1, x2 + 1, y2 + 1);
                    break;
                case ClipRectangleStyle.Raised:
                    ThickFrame(x1 - 2, y1 - 2, x2 + 2, y2 + 2, Sprockets.Color.White, Sprockets.Color.Gray);
                    GL.Color4(0f, 0f, 0f, 1f);
                    Rectangle(x1 - 3, y1 - 3, x2 + 3, y2 + 3);
                    break;
                case ClipRectangleStyle.Sunken:
                    ThickFrame(x1 - 2, y1 - 2, x2 + 2, y2 + 2, Sprockets.Color.Gray, Sprockets.Color.White);
                    GL.Color4(0f, 0f, 0f, 1f);
                    Rectangle(x1 - 3, y1 - 3, x2 + 3, y2 + 3);
                    break;
                default:
                    throw new Exception("Illegal quantity");
            }

            GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Incr);

            if (bg.A == 0f)
            {
                GL.ColorMask(false, false, false, false);
                GL.Color4(1f, 1f, 1f, 1f);
                RectangleFill(x1, y1, x2, y2);
                GL.ColorMask(true, true, true, true);
            }
            else
            {
                MixColor(bg.Uint);
                RectangleFill(x1, y1, x2, y2);
            }

            sp++;

            clipStack[sp] = new clipNode
            {
                ObjectId = objectId,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };
            GL.GetDouble(GetPName.ModelviewMatrix, clipStack[sp].Matrix);

            GL.StencilFunc(StencilFunction.Equal, sp, 0xFF);
            GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep);

            clipStackPointer = sp;
        }

        public static void NoClipRectangle()
        {
            System.Diagnostics.Debug.Assert(clipStackPointer >= 0);

            var sp = clipStackPointer;

            GL.StencilFunc(StencilFunction.Equal, sp, 0xFF);
            GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Decr, OpenTK.Graphics.OpenGL.StencilOp.Decr, OpenTK.Graphics.OpenGL.StencilOp.Decr);

            GL.PushMatrix();
            GL.LoadMatrix(clipStack[sp].Matrix);
            GL.ColorMask(false, false, false, false);
            GL.Color4(1f, 1f, 1f, 1f);
            RectangleFill(clipStack[sp].X1, clipStack[sp].Y1, clipStack[sp].X2, clipStack[sp].Y2);
            GL.ColorMask(true, true, true, true);
            GL.PopMatrix();

            sp--;

            if (sp == 0)
            {
                GL.Disable(EnableCap.StencilTest);
            }
            else
            {
                GL.StencilFunc(StencilFunction.Equal, sp, 0xFF);
                GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep);
            }

            clipStackPointer = sp;
        }

        public static void CleanClipStack(int objectId)
        {
            if (clipStackPointer != 0)
            {
                if (clipStack[clipStackPointer].ObjectId == objectId)
                {
                    NoClipRectangle();
                }
            }
        }

        // Camera

        public static void LoadIdentity()
        {
            GL.LoadIdentity();
        }

        public static void PushMatrix()
        {
            GL.PushMatrix();
        }

        public static void PopMatrix()
        {
            GL.PopMatrix();
        }

        public static void Translate(int x, int y)
        {
            GL.Translate(x, y, 0f);
        }

        public static void Translate(double x, double y, double z)
        {
            GL.Translate(x, y, z);
        }

        public static void Scale(double x, double y, double z = 1d)
        {
            GL.Scale(x, y, z);
        }

        public static void Rotate(double r)
        {
            GL.Rotate(r, 0d, 0d, 1d);
        }

        public static void Rotate(double x, double y, double z)
        {
            GL.Rotate(x, 1d, 0d, 0d);
            GL.Rotate(y, 0d, 1d, 0d);
            GL.Rotate(z, 0d, 0d, 1d);
        }

        public static void Rotate(double angle, double x, double y, double z)
        {
            GL.Rotate(angle, x, y, z);
        }

        public static void ResetMatrixStack()
        {
            matrixStackPointer = 0;
        }

        public static void CleanMatrixStack(int objectId)
        {
            if (matrixStackPointer != 0)
            {
                var matrix = new double[15];
                GL.GetDouble(GetPName.ModelviewMatrix, matrix);

                while (matrixStackPointer != 0)
                {
                    GL.PopMatrix();
                    matrixStackPointer--;
                }

                GL.LoadMatrix(matrix);
            }
        }

        // Colors

        public static void SetColor(uint color)
        {
            var i = new Color.RGBA(color);

            GL.Color4(i.R, i.G, i.B, i.A);
        }

        public static void SetColor(byte r, byte g, byte b, byte a = 255)
        {
            GL.Color4(r, g, b, a);
        }

        public static void MixColor(uint color)
        {
            var i = new Color.RGBA(color);

            GL.Color4((byte)(inkColor.R * i.R >> 8), (byte)(inkColor.G * i.G >> 8), (byte)(inkColor.B * i.B >> 8), (byte)(inkColor.A * i.A >> 8));
        }

        public static void MixColor(byte r, byte g, byte b, byte a = 255)
        {
            GL.Color4((byte)(inkColor.R * r >> 8), (byte)(inkColor.G * g >> 8), (byte)(inkColor.B * b >> 8), (byte)(inkColor.A * a >> 8));
        }

        // Primitives

        public static void Plot(int x, int y)
        {
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(x + 0.375f, y + 0.375f);
            GL.End();
        }

        public static void Line(int x1, int y1, int x2, int y2)
        {
            if (x1 < x2)
            {
                x2++;
            }
            else if (x1 > x2)
            {
                x2--;
            }

            if (y1 < y2)
            {
                y2++;
            }
            else if (y1 > y2)
            {
                y2--;
            }

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(x1 + 0.375f, y1 + 0.375f);
            GL.Vertex2(x2 + 0.375f, y2 + 0.375f);
            GL.End();
        }

        public static void Rectangle(int x1, int y1, int x2, int y2)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(x1 + 0.375f, y1 + 0.375f);
            GL.Vertex2(x1 + 0.375f, y2 + 0.375f);
            GL.Vertex2(x2 + 0.375f, y2 + 0.375f);
            GL.Vertex2(x2 + 0.375f, y1 + 0.375f);
            GL.End();
        }

        public static void RectangleFill(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }

            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            x2++;
            y2++;

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x1, y2);
            GL.Vertex2(x2, y2);
            GL.Vertex2(x2, y1);
            GL.End();
        }

        public static void ThickFrame(int x1, int y1, int x2, int y2, Color.RGBA c1, Color.RGBA c2)
        {
            MixColor(c1.Uint);

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(x1 + 0.375f, y2 + 0.375f);
            GL.Vertex2(x1 + 0.375f, y1 + 0.375f);
            GL.Vertex2(x2 + 1.375f, y1 + 0.375f);
            GL.End();

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(x1 + 1.375f, y2 - 0.625f);
            GL.Vertex2(x1 + 1.375f, y1 + 1.375f);
            GL.Vertex2(x2 + 0.375f, y1 + 1.375f);
            GL.End();

            MixColor(c2.Uint);

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(x2 + 0.375f, y1 + 1.375f);
            GL.Vertex2(x2 + 0.375f, y2 + 0.375f);
            GL.Vertex2(x1 + 0.375f, y2 + 0.375f);
            GL.End();

            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(x2 - 0.625f, y1 + 2.375f);
            GL.Vertex2(x2 - 0.625f, y2 - 0.625f);
            GL.Vertex2(x1 + 1.375f, y2 - 0.625f);
            GL.End();
        }

        public static void Circle(double cx, double cy, double radius, int numSegments = 72)
        {
            Ellipse(cx, cy, radius, radius, numSegments);
        }

        public static void CircleFill(double cx, double cy, double radius, int numSegments = 72)
        {
            EllipseFill(cx, cy, radius, radius, numSegments);
        }

        public static void Ellipse(double cx, double cy, double xRadius, double yRadius, int numSegments = 72)
        {
            var twicePi = 2 * 3.1415926;

            GL.Begin(PrimitiveType.LineLoop);

            for (var i = 0; i <= numSegments; i++)
            {
                var x = xRadius * System.Math.Cos(i * twicePi / numSegments);
                var y = yRadius * System.Math.Sin(i * twicePi / numSegments);

                GL.Vertex2(cx + x, cy + y);
            }

            GL.End();
        }

        public static void EllipseFill(double cx, double cy, double xRadius, double yRadius, int numSegments = 72)
        {
            var twicePi = 2 * 3.1415926;

            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex2(cx, cy);

            for (var i = 0; i <= numSegments; i++)
            {
                var x = xRadius * System.Math.Cos(i * twicePi / numSegments);
                var y = yRadius * System.Math.Sin(i * twicePi / numSegments);

                GL.Vertex2(cx + x, cy + y);
            }

            GL.End();
        }

        public static void BezierCurve(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            var cx = 3d * (x1 - x0);
            var bx = 3d * (x2 - x1) - cx;
            var ax = x3 - x0 - cx - bx;

            var cy = 3d * (y1 - y0);
            var by = 3d * (y2 - y1) - cy;
            var ay = y3 - y0 - cy - by;

            var dt = 1d / 31d;

            GL.Begin(PrimitiveType.LineStrip);

            var t = 0d;

            GL.Vertex2(x0 + 0.375, y0 + 0.375);

            for (var i = 1; i <= 31; i++)
            {
                t = i * dt;

                var tSquared = t * t;
                var tCubed = tSquared * t;

                var x = (ax * tCubed) + (bx * tSquared) + (cx * t) + x0;
                var y = (ay * tCubed) + (by * tSquared) + (cy * t) + y0;

                GL.Vertex2(x + 0.375, y + 0.375);
            }

            GL.End();
        }

        // 3D Primitives

        public static void Begin(PrimitiveType mode)
        {
            GL.Begin(mode);
        }

        public static void End()
        {
            GL.End();
        }

        public static void Vertex(double x, double y, double z)
        {
            GL.Vertex3(x, y, z);
        }

        public static void TexCoord(double x, double y)
        {
            GL.TexCoord2(x, y);
        }

        // Textures

        public static void ResetTextureStack()
        {
            //activeTexture = 1;
            //textureUnitsFlags = 0;
        }

        public static int GenTexture()
        {
            return GL.GenTexture();
        }

        public static void BindTexture(int handle, int width, int height, uint[] data, bool linearFilter = false)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            if (linearFilter)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Nearest);
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.Disable(EnableCap.Texture2D);
        }

        public static void UpdateTexture(int handle, int width, int height, uint[] data)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, data);
            GL.Disable(EnableCap.Texture2D);
        }

        public static void TextureMap(int handle)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

        public static void NoTextureMap()
        {
            GL.Disable(EnableCap.Texture2D);
        }

        public static void RenderTexture(int handle, int x1, int y1, int x2, int y2)
        {
            if (x1 < x2)
            {
                x2++;
            }
            else if (x1 > x2)
            {
                x2--;
            }

            if (y1 < y2)
            {
                y2++;
            }
            else if (y1 > y2)
            {
                y2--;
            }

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(x1, y1);
            GL.TexCoord2(0f, 1f); GL.Vertex2(x1, y2);
            GL.TexCoord2(1f, 1f); GL.Vertex2(x2, y2);
            GL.TexCoord2(1f, 0f); GL.Vertex2(x2, y1);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }

        public static void FreeTexture(int handle)
        {
            GL.DeleteTexture(handle);
        }

        // Images

        public static uint[] LoadImage(string fileName, out int width, out int height)
        {
            width = 0;
            height = 0;

            using (var bmp = new Bitmap(fileName))
            {
                width = bmp.Width;
                height = bmp.Height;

                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                var ptr = bmpData.Scan0;

                var tempSize = System.Math.Abs(bmpData.Stride) * bmp.Height;
                var temp = new byte[tempSize];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, temp, 0, tempSize);

                bmp.UnlockBits(bmpData);

                var size = width * height;
                var data = new uint[size];

                var p = 0;

                for (var index = 0; index < size; index++, p += 4)
                {
                    var b = temp[p];
                    var g = temp[p + 1];
                    var r = temp[p + 2];
                    var a = temp[p + 3];

                    data[index] = r | (uint)g << 8 | (uint)b << 16 | (uint)a << 24;
                }

                return data;
            }
        }

        // Display lists

        public static int GenLists(int range)
        {
            return GL.GenLists(range);
        }

        public static void CreateTile(int list, int handle, int x1, int y1, int x2, int y2, int bitmapWidth, int bitmapHeight, int glyphWidth, int glyphHeight)
        {
            if (x1 > x2)
            {
                Util.Swap(ref x1, ref x2);
            }

            if (y1 > y2)
            {
                Util.Swap(ref y1, ref y2);
            }

            // Necessary fix?! (re: rotate text 225 degrees or 315 degrees)
            //x1++;
            //y1++;

            var tx1 = (float)x1 / bitmapWidth;
            var ty1 = (float)y1 / bitmapHeight;
            var tx2 = (float)x2 / bitmapWidth;
            var ty2 = (float)y2 / bitmapHeight;

            GL.NewList(list, ListMode.Compile);

            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(tx1, ty1); GL.Vertex2(0, 0);
            GL.TexCoord2(tx2, ty1); GL.Vertex2(glyphWidth, 0);
            GL.TexCoord2(tx2, ty2); GL.Vertex2(glyphWidth, glyphHeight);
            GL.TexCoord2(tx1, ty2); GL.Vertex2(0, glyphHeight);
            GL.End();

            GL.Translate(glyphWidth, 0f, 0f);

            GL.EndList();
        }

        public static void Print(int list, int x, int y, int ch)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448); // GL_MODULATE = 8448
            GL.PushMatrix();
            GL.Translate(x, y, 0f);
            GL.CallList(list + ch);
            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);
        }

        public static void Print(int list, int x, int y, string text)
        {
            var length = text.Length;

            if (length == 0)
            {
                return;
            }

            var lists = Encoding.ASCII.GetBytes(text);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448); // GL_MODULATE = 8448
            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);
            GL.PushMatrix();
            GL.Translate(x, y, 0f);
            GL.CallLists(length, ListNameType.UnsignedByte, lists);
            GL.PopMatrix();
            GL.PopAttrib();
            GL.Disable(EnableCap.Texture2D);
        }

        public static void PrintShadow(int list, int x, int y, string text, uint foregroundColor = 0xFFFFFFFF, uint backgroundColor = 0xFF000000)
        {
            SetColor(backgroundColor);
            Print(list, x + 1, y + 1, text);

            SetColor(foregroundColor);
            Print(list, x, y, text);
        }

        public static void RenderConsole(int list, int width, int height, int characterWidth, int characterHeight, byte[] screenData, uint[] colorData, int[] attributeData, int rasterX, int rasterY, bool blink)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448); // GL_MODULATE = 8448
            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);

            for (var y = 0; y < height; y++)
            {
                var offset = y * width;
                var length = width;

                GL.PushMatrix();
                GL.Translate(rasterX, rasterY, 0f);

                do
                {
                    var anchor = offset;

                    var inkColor = colorData[offset];
                    var attributes = attributeData[offset];

                    var count = 0;

                    do
                    {
                        offset++;
                        length--;
                        count++;
                    } while (length > 0 && inkColor == colorData[offset] && attributes == attributeData[offset]);

                    if (inkColor >> 24 == 0)
                    {
                        GL.Translate(count * characterWidth, 0f, 0f);
                    }
                    else
                    {
                        var lists = new byte[count];

                        for (var index = 0; index < count; index++)
                        {
                            lists[index] = screenData[anchor + index];
                        }

                        switch ((ConsoleAttribute)attributes)
                        {
                            case ConsoleAttribute.None:
                                MixColor(inkColor);
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.Blink:
                                if (blink)
                                {
                                    GL.Translate(count * characterWidth, 0f, 0f);
                                }
                                else
                                {
                                    MixColor(inkColor);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                break;
                            case ConsoleAttribute.Bold:
                                MixColor(Sprockets.Color.Lighten(inkColor, 0.5f));
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.BlinkBold:
                                if (blink)
                                {
                                    MixColor(Sprockets.Color.Lighten(inkColor, 0.5f));
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                else
                                {
                                    MixColor(inkColor);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                break;
                            case ConsoleAttribute.Underline:
                                MixColor(inkColor);
                                GL.Disable(EnableCap.Texture2D);
                                Line(0, characterHeight - 1, count * characterWidth - 1, characterHeight - 1);
                                GL.Enable(EnableCap.Texture2D);
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.BlinkUnderline:
                                if (blink)
                                {
                                    MixColor(inkColor);
                                    GL.Disable(EnableCap.Texture2D);
                                    Line(0, characterHeight - 1, count * characterWidth - 1, characterHeight - 1);
                                    GL.Enable(EnableCap.Texture2D);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                else
                                {
                                    MixColor(inkColor);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                break;
                            case ConsoleAttribute.BoldUnderline:
                                MixColor(Sprockets.Color.Lighten(inkColor, 0.5f));
                                GL.Disable(EnableCap.Texture2D);
                                Line(0, characterHeight - 1, count * characterWidth - 1, characterHeight - 1);
                                GL.Enable(EnableCap.Texture2D);
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.BlinkBoldUnderline:
                                if (blink)
                                {
                                    MixColor(Sprockets.Color.Lighten(inkColor, 0.5f));
                                    GL.Disable(EnableCap.Texture2D);
                                    Line(0, characterHeight - 1, count * characterWidth - 1, characterHeight - 1);
                                    GL.Enable(EnableCap.Texture2D);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                else
                                {
                                    MixColor(inkColor);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                break;
                            default:
                                GL.Translate(count * characterWidth, 0f, 0f);
                                break;
                        }
                    }
                } while (length > 0);

                GL.PopMatrix();

                rasterY += characterHeight;
            }

            GL.PopAttrib();

            GL.Disable(EnableCap.Texture2D);
        }

        public static void RenderTileMap(int list, int width, int height, int characterWidth, int characterHeight, byte[] screenData, uint[] colorData, int rasterX, int rasterY)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448); // GL_MODULATE = 8448
            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);

            for (var y = 0; y < height; y++)
            {
                var offset = y * width;
                var length = width;

                GL.PushMatrix();
                GL.Translate(rasterX, rasterY, 0f);

                do
                {
                    var anchor = offset;

                    var inkColor = colorData[offset];

                    var count = 0;

                    do
                    {
                        offset++;
                        length--;
                        count++;
                    } while (length > 0 && inkColor == colorData[offset]);

                    if (inkColor >> 24 == 0)
                    {
                        GL.Translate(count * characterWidth, 0f, 0f);
                    }
                    else
                    {
                        var lists = new byte[count];

                        for (var index = 0; index < count; index++)
                        {
                            lists[index] = screenData[anchor + index];
                        }

                        MixColor(inkColor);
                        GL.CallLists(count, ListNameType.UnsignedByte, lists);
                    }
                } while (length > 0);

                GL.PopMatrix();

                rasterY += characterHeight;
            }

            GL.PopAttrib();

            GL.Disable(EnableCap.Texture2D);
        }

        public static void FreeLists(int list, int range)
        {
            GL.DeleteLists(list, range);
        }

        // Fonts

        public static uint[] BuildMonoSpacedFont(string fontName, float fontSize, FontStyle fontStyle, out int charWidth, out int charHeight, out int width, out int height)
        {
            charWidth = 0;
            charHeight = 0;

            width = 0;
            height = 0;

            var font = new Font(new FontFamily(fontName), fontSize, fontStyle);

            using (var graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
            {
                var c = (char)33;

                var charSize = graphics.MeasureString(c.ToString(), font);

                charWidth = (int)charSize.Width;
                charHeight = (int)charSize.Height;

                for (var ch = 33; ch < 128; ch++)
                {
                    c = (char)ch;

                    charSize = graphics.MeasureString(c.ToString(), font);

                    if ((int)charSize.Width != charWidth || (int)charSize.Height != charHeight)
                    {
                        return null;
                    }
                }
            }

            var atlasOffsetX = -3;
            var atlasOffsetY = -1;

            using (var bmp = new Bitmap(charWidth * 16, charHeight * 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                width = bmp.Width;
                height = bmp.Height;

                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.TextContrast = 4;
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    for (var y = 0; y < 16; y++)
                    {
                        for (var x = 0; x < 16; x++)
                        {
                            var c = (char)(x + y * 16);

                            graphics.DrawString(c.ToString(), font, Brushes.White, x * charWidth + atlasOffsetX, y * charHeight + atlasOffsetY);
                        }
                    }

                    var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    var ptr = bmpData.Scan0;

                    var tempSize = System.Math.Abs(bmpData.Stride) * bmp.Height;
                    var temp = new byte[tempSize];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, temp, 0, tempSize);

                    bmp.UnlockBits(bmpData);

                    var size = width * height;
                    var data = new uint[size];

                    var p = 0;

                    for (var index = 0; index < size; index++, p += 4)
                    {
                        var b = temp[p];
                        var g = temp[p + 1];
                        var r = temp[p + 2];
                        var a = temp[p + 3];

                        var grayScale = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);

                        data[index] = 255 | (uint)255 << 8 | (uint)255 << 16 | (uint)grayScale << 24;
                    }

                    return data;
                }
            }
        }

        public static void DrawMonoSpacedText(int handle, int x, int y, string text, int charWidth, int charHeight, int width, int height)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.Begin(PrimitiveType.Quads);

            var uStep = (float)charWidth / width;
            var vStep = (float)charHeight / height;

            for (var n = 0; n < text.Length; n++)
            {
                var idx = text[n];
                var u = idx % 16 * uStep;
                var v = idx / 16 * vStep;

                GL.TexCoord2(u, v);
                GL.Vertex2(x, y);
                GL.TexCoord2(u + uStep, v);
                GL.Vertex2(x + charWidth, y);
                GL.TexCoord2(u + uStep, v + vStep);
                GL.Vertex2(x + charWidth, y + charHeight);
                GL.TexCoord2(u, v + vStep);
                GL.Vertex2(x, y + charHeight);

                x += charWidth;
            }

            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }

        public static uint[] BuildBanner(string fontName, float fontSize, FontStyle fontStyle, string caption, out int width, out int height)
        {
            width = 0;
            height = 0;

            var font = new Font(new FontFamily(fontName), fontSize, fontStyle);

            using (var graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
            {
                var stringSize = graphics.MeasureString(caption, font);

                width = (int)stringSize.Width;
                height = (int)stringSize.Height;
            }

            using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.TextContrast = 4;
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    graphics.DrawString(caption, font, Brushes.White, 0, 0);

                    var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    var ptr = bmpData.Scan0;

                    var tempSize = System.Math.Abs(bmpData.Stride) * bmp.Height;
                    var temp = new byte[tempSize];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, temp, 0, tempSize);

                    bmp.UnlockBits(bmpData);

                    var size = width * height;
                    var data = new uint[size];

                    var p = 0;
                    for (var index = 0; index < size; index++, p += 4)
                    {
                        var b = temp[p];
                        var g = temp[p + 1];
                        var r = temp[p + 2];
                        var a = temp[p + 3];

                        var grayScale = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);

                        data[index] = 255 | (uint)255 << 8 | (uint)255 << 16 | (uint)grayScale << 24;
                    }

                    return data;
                }
            }
        }

        // Read

        public static void ReadPixel(int x, int y)
        {
            if ((x >= 0 && x <= gameWindowWidth - 1) && (y >= 0 && y <= gameWindowHeight - 1))
            {
                ushort depth = 0;

                GL.ReadPixels(x, gameWindowHeight - y - 1, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.UnsignedShort, ref depth);

                var link = 65535 - depth;

                Input.MousePointingAt = links[link].ObjectId;

                Input.MousePointingAtNode = links[link].Node;
            }
            else
            {
                Input.MousePointingAt = 0;

                Input.MousePointingAtNode = 0;
            }
        }

        // FBOs

        public static FrameBuffer CreateFrameBuffer(int width, int height, bool linearFilter)
        {
            var frameBuffer = new FrameBuffer(width, height);

            var fbo = 0;
            var fboDepth = 0;
            var fboTexture = 0;


            GL.GenRenderbuffers(1, out fboDepth);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, fboDepth);

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);


            GL.GenTextures(1, out fboTexture);

            GL.BindTexture(TextureTarget.Texture2D, fboTexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            if (linearFilter)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Nearest);
            }

            GL.GenFramebuffers(1, out fbo);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fboTexture, 0);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, fboDepth);

            GL.BindTexture(TextureTarget.Texture2D, 0);


            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Failed to create FBO");
            }


            var error = GetError();

            System.Diagnostics.Debug.Assert(error == ErrorCode.NoError);


            frameBuffer.Fbo = fbo;
            frameBuffer.FboDepth = fboDepth;
            frameBuffer.FboTexture = fboTexture;


            GL.ClearColor(0, 0, 0, 0);
            GL.Color4(1f, 1f, 1f, 1f);

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, height, 0, -65535, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            frameBuffer.GLState = SaveGLState();


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return frameBuffer;
        }

        public static void BindFBO(int fbo)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        }

        public static void UnbindFBO()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public static GLState SaveGLState()
        {
            var glState = default(GLState);

            glState.Paper = new float[4];
            GL.GetFloat(GetPName.ColorClearValue, glState.Paper);

            glState.Ink = new float[4];
            GL.GetFloat(GetPName.CurrentColor, glState.Ink);

            glState.ModelViewMatrix = new double[16];
            GL.GetDouble(GetPName.ModelviewMatrix, glState.ModelViewMatrix);

            glState.ProjectionMatrix = new double[16];
            GL.GetDouble(GetPName.ProjectionMatrix, glState.ProjectionMatrix);

            glState.TextureMatrix = new double[16];
            GL.GetDouble(GetPName.TextureMatrix, glState.TextureMatrix);

            return glState;
        }

        public static void RestoreGLState(int width, int height, GLState glState)
        {
            GL.Viewport(0, 0, width, height);

            GL.ClearColor(glState.Paper[0], glState.Paper[1], glState.Paper[2], glState.Paper[3]);

            GL.Color4(glState.Ink[0], glState.Ink[1], glState.Ink[2], glState.Ink[3]);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(glState.ModelViewMatrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(glState.ProjectionMatrix);

            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadMatrix(glState.TextureMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
        }

        public static void FreeFrameBuffer(FrameBuffer frameBuffer)
        {
            GL.DeleteFramebuffers(1, ref frameBuffer.Fbo);

            GL.DeleteRenderbuffers(1, ref frameBuffer.FboDepth);

            GL.DeleteTextures(1, ref frameBuffer.FboTexture);

            //GL.DeleteFramebuffer(frameBuffer.Fbo);

            //GL.DeleteRenderbuffer(frameBuffer.FboDepth);

            //GL.DeleteTexture(frameBuffer.FboTexture);
        }
    }
}
