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

using System.Drawing;
using System.Drawing.Imaging;

namespace LiquidPlayer.Sprockets
{
    public static class Graphics
    {
        public struct IVector2
        {
            public int X;
            public int Y;

            public IVector2(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public struct BoundingBox
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;
        }

        private struct GraphicsLinks
        {
            public int ObjectId;
            public int Node;
        }

        public struct ClipNode
        {
            public int ObjectId;
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;
            public double[] Matrix;
        }

        public struct Character
        {
            public int TextureId;
            public IVector2 Size;
            public IVector2 Bearing;
            public int Advance;
            public uint[] Data;
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
                this.Width = width;
                this.Height = height;

                this.Fbo = 0;
                this.FboDepth = 0;
                this.FboTexture = 0;

                this.GLState = default(GLState);
            }
        }

        private static int gameWindowWidth;
        private static int gameWindowHeight;

        private static bool isResized;

        private static int objectId;
        private static int node;
        private static int depthBase;
        private static GraphicsLinks[] links;

        private static bool sorted;
        private static bool sorted3D;

        private static Color.RGBA inkColor;

        private static double[] matrixStack;
        private static int matrixStackPointer;

        private static ClipNode[] clipStack;
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
            GL.ClearDepth(1d);
            GL.ClearStencil(0);

            Clear();

            GL.Enable(EnableCap.Multisample);

            GL.ShadeModel(ShadingModel.Smooth);

            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PolygonSmooth);

            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);

            GL.Disable(EnableCap.CullFace);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.Lighting);

            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.FogHint, HintMode.Nicest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            gameWindowWidth = width;
            gameWindowHeight = height;

            ResetNodes();

            links = new GraphicsLinks[65536];

            sorted = true;
            sorted3D = true;

            inkColor = Color.White;

            matrixStack = new double[16];
            matrixStackPointer = 0;

            clipStack = new ClipNode[256];
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public static OpenTK.Graphics.OpenGL.ErrorCode GetError()
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

            var yMax = zNear * System.Math.Tan((fovy * System.Math.PI) / 360d);
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
            links[depthBase] = new GraphicsLinks
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
            GL.Translate(0d, 0d, depthBase);

            Graphics.objectId = objectId;
            Graphics.node = 0;

            pushLink();
        }

        public static void LinkNextObject(int objectId)
        {
            GL.Translate(0d, 0d, 1d);

            Graphics.objectId = objectId;
            Graphics.node = 0;

            pushLink();
        }

        public static void LinkNextNode(int node)
        {
            GL.Translate(0d, 0d, 1d);

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
        
        public static void ClipRectangle(int x1, int y1, int x2, int y2, Color.RGBA bg, Color.RGBA border)
        {
            CleanClipStack();

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

            if (border.A != 0)
            {
                MixColor(border.Uint);
                Rectangle(x1 - 1, y1 - 1, x2 + 1, y2 + 1);
            }

            GL.StencilOp(OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Keep, OpenTK.Graphics.OpenGL.StencilOp.Incr);

            if (bg.A == 0)
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

            clipStack[sp] = new ClipNode
            {
                ObjectId = objectId,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Matrix = new double[16]
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

        public static void CleanClipStack()
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
            GL.Translate(x, y, 0d);
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
            GL.Vertex2(x + 0.375d, y + 0.375d);
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
            GL.Vertex2(x1 + 0.375d, y1 + 0.375d);
            GL.Vertex2(x2 + 0.375d, y2 + 0.375d);
            GL.End();
        }

        public static void Rectangle(int x1, int y1, int x2, int y2)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(x1 + 0.375d, y1 + 0.375d);
            GL.Vertex2(x1 + 0.375d, y2 + 0.375d);
            GL.Vertex2(x2 + 0.375d, y2 + 0.375d);
            GL.Vertex2(x2 + 0.375d, y1 + 0.375d);
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

        //public static void ThickFrame(int x1, int y1, int x2, int y2, Color.RGBA c1, Color.RGBA c2)
        //{
        //    MixColor(c1.Uint);

        //    GL.Begin(PrimitiveType.LineStrip);
        //    GL.Vertex2(x1 + 0.375d, y2 + 0.375d);
        //    GL.Vertex2(x1 + 0.375d, y1 + 0.375d);
        //    GL.Vertex2(x2 + 1.375d, y1 + 0.375d);
        //    GL.End();

        //    GL.Begin(PrimitiveType.LineStrip);
        //    GL.Vertex2(x1 + 1.375d, y2 - 0.625d);
        //    GL.Vertex2(x1 + 1.375d, y1 + 1.375d);
        //    GL.Vertex2(x2 + 0.375d, y1 + 1.375d);
        //    GL.End();

        //    MixColor(c2.Uint);

        //    GL.Begin(PrimitiveType.LineStrip);
        //    GL.Vertex2(x2 + 0.375d, y1 + 1.375d);
        //    GL.Vertex2(x2 + 0.375d, y2 + 0.375d);
        //    GL.Vertex2(x1 + 0.375d, y2 + 0.375d);
        //    GL.End();

        //    GL.Begin(PrimitiveType.LineStrip);
        //    GL.Vertex2(x2 - 0.625d, y1 + 2.375d);
        //    GL.Vertex2(x2 - 0.625d, y2 - 0.625d);
        //    GL.Vertex2(x1 + 1.375d, y2 - 0.625d);
        //    GL.End();
        //}

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
            var twicePi = 2 * System.Math.PI;

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
            var twicePi = 2 * System.Math.PI;

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

        public static void Vertex(double x, double y, double z)
        {
            GL.Vertex3(x, y, z);
        }

        public static void Normal(double x, double y, double z)
        {
            GL.Normal3(x, y, z);
        }

        public static void TexCoord(double x, double y)
        {
            GL.TexCoord2(x, y);
        }

        public static void End()
        {
            GL.End();
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
            GL.BindTexture(TextureTarget.Texture2D, handle);

            if (linearFilter)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public static void UpdateTexture(int handle, int width, int height, uint[] data)
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public static void TextureMap(int handle)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
            GL.BindTexture(TextureTarget.Texture2D, handle);
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
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0d, 1d); GL.Vertex2(x1, y1);
            GL.TexCoord2(0d, 0d); GL.Vertex2(x1, y2);
            GL.TexCoord2(1d, 0d); GL.Vertex2(x2, y2);
            GL.TexCoord2(1d, 1d); GL.Vertex2(x2, y1);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }

        public static void NoTextureMap()
        {
            GL.Disable(EnableCap.Texture2D);
        }

        public static void FreeTexture(int handle)
        {
            GL.DeleteTexture(handle);
        }

        // Images

        public static uint[] LoadImage(string path, out int width, out int height)
        {
            width = 0;
            height = 0;

            using (var bmp = new Bitmap(path))
            {
                width = bmp.Width;
                height = bmp.Height;

                bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                var ptr = bmpData.Scan0;

                var tempSize = System.Math.Abs(bmpData.Stride) * bmp.Height;
                var temp = new byte[tempSize];
                Marshal.Copy(bmpData.Scan0, temp, 0, tempSize);

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

        public static void PrintLists(int list, int x, int y, int ch)
        {
            GL.Disable(EnableCap.Multisample);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate); 

            GL.PushMatrix();
            GL.Translate(x, y, 0d);
            GL.CallList(list + ch);
            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Multisample);
        }

        public static void PrintLists(int list, int x, int y, string caption)
        {
            var length = caption.Length;

            if (length == 0)
            {
                return;
            }

            var lists = Encoding.ASCII.GetBytes(caption);

            GL.Disable(EnableCap.Multisample);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);
            GL.PushMatrix();
            GL.Translate(x, y, 0d);
            GL.CallLists(length, ListNameType.UnsignedByte, lists);
            GL.PopMatrix();
            GL.PopAttrib();

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Multisample);
        }

        public static void PrintLists(int list, int x, int y, string text, uint foregroundColor = 0xFFFFFFFF, uint backgroundColor = 0xFF000000)
        {
            SetColor(backgroundColor);
            PrintLists(list, x + 1, y + 1, text);

            SetColor(foregroundColor);
            PrintLists(list, x, y, text);
        }

        public static void FreeLists(int list, int range)
        {
            GL.DeleteLists(list, range);
        }

        // Tiles

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

            var tx1 = (double)x1 / bitmapWidth;
            var ty1 = (double)y1 / bitmapHeight;
            var tx2 = (double)x2 / bitmapWidth;
            var ty2 = (double)y2 / bitmapHeight;

            GL.NewList(list, ListMode.Compile);

            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(tx1, 1d - ty1); GL.Vertex2(0, 0);
            GL.TexCoord2(tx2, 1d - ty1); GL.Vertex2(glyphWidth, 0);
            GL.TexCoord2(tx2, 1d - ty2); GL.Vertex2(glyphWidth, glyphHeight);
            GL.TexCoord2(tx1, 1d - ty2); GL.Vertex2(0, glyphHeight);
            GL.End();

            GL.Translate(glyphWidth, 0d, 0d);

            GL.EndList();
        }

        public static void RenderConsole(int list, int width, int height, int characterWidth, int characterHeight, byte[] screenData, uint[] colorData, int[] attributeData, int rasterX, int rasterY, bool blink)
        {
            GL.Disable(EnableCap.Multisample);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);

            for (var y = 0; y < height; y++)
            {
                var offset = y * width;
                var length = width;

                GL.PushMatrix();
                GL.Translate(rasterX, rasterY, 0d);

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
                        GL.Translate(count * characterWidth, 0d, 0d);
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
                                    GL.Translate(count * characterWidth, 0d, 0d);
                                }
                                else
                                {
                                    MixColor(inkColor);
                                    GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                }
                                break;
                            case ConsoleAttribute.Bold:
                                MixColor(Color.Lighten(inkColor, 0.5f));
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.BlinkBold:
                                if (blink)
                                {
                                    MixColor(Color.Lighten(inkColor, 0.5f));
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
                                MixColor(Color.Lighten(inkColor, 0.5f));
                                GL.Disable(EnableCap.Texture2D);
                                Line(0, characterHeight - 1, count * characterWidth - 1, characterHeight - 1);
                                GL.Enable(EnableCap.Texture2D);
                                GL.CallLists(count, ListNameType.UnsignedByte, lists);
                                break;
                            case ConsoleAttribute.BlinkBoldUnderline:
                                if (blink)
                                {
                                    MixColor(Color.Lighten(inkColor, 0.5f));
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
                                GL.Translate(count * characterWidth, 0d, 0d);
                                break;
                        }
                    }
                } while (length > 0);

                GL.PopMatrix();

                rasterY += characterHeight;
            }

            GL.PopAttrib();

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Multisample);
        }

        public static void RenderTileMap(int list, int width, int height, int characterWidth, int characterHeight, byte[] screenData, uint[] colorData, int rasterX, int rasterY)
        {
            GL.Disable(EnableCap.Multisample);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            GL.PushAttrib(AttribMask.ListBit);
            GL.ListBase(list);

            for (var y = 0; y < height; y++)
            {
                var offset = y * width;
                var length = width;

                GL.PushMatrix();
                GL.Translate(rasterX, rasterY, 0d);

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
                        GL.Translate(count * characterWidth, 0d, 0d);
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

            GL.Enable(EnableCap.Multisample);
        }

        // Fonts

        public static void BuildFont(string path, int fontSize, bool linearFilter, out Dictionary<int, Character> characters)
        {
            characters = new Dictionary<int, Character>();

            using (var ft = new SharpFont.Library())
            {
                using (var face = ft.NewFace(path, 0))
                {
                    face.SetPixelSizes(0, (uint)fontSize);

                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    for (var c = 0; c < 128; c++)
                    {
                        face.LoadChar((uint)c, SharpFont.LoadFlags.Render, SharpFont.LoadTarget.Normal);

                        var textureId = GL.GenTexture();

                        GL.BindTexture(TextureTarget.Texture2D, textureId);

                        var width = face.Glyph.Bitmap.Width;
                        var height = face.Glyph.Bitmap.Rows;

                        var size = width * height;

                        uint[] data = null;

                        if (size != 0)
                        {
                            var managedArray = new byte[size];

                            Marshal.Copy(face.Glyph.Bitmap.Buffer, managedArray, 0, size);


                            data = new uint[size];

                            for (var index = 0; index < size; index++)
                            {
                                data[index] = 255 | (uint)255 << 8 | (uint)255 << 16 | (uint)managedArray[index] << 24;
                            }


                            if (linearFilter)
                            {
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
                            }
                            else
                            {
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
                            }

                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
                        }

                        var advance = face.Glyph.Advance.X.Round();

                        var character = new Character
                        {
                            TextureId = textureId,
                            Size = new IVector2(width, face.Glyph.Bitmap.Rows),
                            Bearing = new IVector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                            Advance = advance,
                            Data = data
                        };

                        characters[c] = character;
                    }
                }
            }
        }

        public static void RenderText(Dictionary<int, Character> characters, int x, int y, string text, int letterSpacing = 0)
        {
            GL.Disable(EnableCap.Multisample);

            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            foreach (var c in text)
            {
                var ch = characters[c];

                var xPos = x + ch.Bearing.X;
                var yPos = y + (characters['H'].Bearing.Y - ch.Bearing.Y);

                var w = ch.Size.X;
                var h = ch.Size.Y;

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureId);

                GL.Begin(PrimitiveType.Quads);

                GL.TexCoord2(0d, 0d); GL.Vertex2(xPos + 0, yPos + 0);
                GL.TexCoord2(1d, 0d); GL.Vertex2(xPos + w, yPos + 0);
                GL.TexCoord2(1d, 1d); GL.Vertex2(xPos + w, yPos + h);
                GL.TexCoord2(0d, 1d); GL.Vertex2(xPos + 0, yPos + h);

                GL.End();

                x += ch.Advance + letterSpacing;
            }

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Multisample);
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


            fboDepth = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, fboDepth);

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);


            fboTexture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, fboTexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            if (linearFilter)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            }


            fbo = GL.GenFramebuffer();

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

            System.Diagnostics.Debug.Assert(error == OpenTK.Graphics.OpenGL.ErrorCode.NoError);


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
            GL.DeleteFramebuffer(frameBuffer.Fbo);

            GL.DeleteRenderbuffer(frameBuffer.FboDepth);

            GL.DeleteTexture(frameBuffer.FboTexture);
        }
    }
}
