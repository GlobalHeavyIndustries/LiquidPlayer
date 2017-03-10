using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class GEL : Entity
    {
        protected bool visible;
        protected int priority;
        protected Sprockets.Color.RGBA tint;
        protected double xPosition;
        protected double yPosition;
        protected double xScale;
        protected double yScale;
        protected double rotation;

        protected List<int> renderList;

        public bool IsVisible
        {
            get
            {
                return visible;
            }
        }

        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                if (priority != value)
                {
                    priority = value;
                    Sprockets.Graphics.Sorted = false;
                }
            }
        }

        public double XPosition
        {
            get
            {
                return xPosition;
            }
        }

        public double YPosition
        {
            get
            {
                return yPosition;
            }
        }

        public double XScale
        {
            get
            {
                return xScale;
            }
        }

        public double YScale
        {
            get
            {
                return yScale;
            }
        }

        public double Rotation
        {
            get
            {
                return rotation;
            }
        }

        public List<int> RenderList
        {
            get
            {
                return renderList;
            }
        }

        public GEL(int id)
            : base(id)
        {
            this.visible = false;
            this.priority = 0;
            this.tint = Sprockets.Color.White;
            this.xPosition = 0d;
            this.yPosition = 0d;
            this.xScale = 1d;
            this.yScale = 1d;
            this.rotation = 0d;

            this.renderList = new List<int>();
        }

        public override string ToString()
        {
            return $"GEL";
        }

        protected virtual void render(int orthoId)
        {
            //Throw(ExceptionCode.NotImplemented);
        }

        public void Render(LiquidClass liquidClass, int orthoId, double xBorder = 0d, double yBorder = 0d, double xSnap = 1d, double ySnap = 1d)
        {
            // preRender

            /*

            ' Is this the modal layer?
            IF gModal = ID THEN
                CALL gxColor(0!, 0!, 0!, 0.5!)
                CALL gxRectf(0, 0, gxWidth - 1, gxHeight - 1)
            END IF 

            */

            Sprockets.Graphics.PushMatrix();

            var inkColor = Sprockets.Graphics.Ink;
            var depthBase = Sprockets.Graphics.DepthBase;

            Sprockets.Graphics.Ink = new Sprockets.Color.RGBA
            {
                R = (byte)(Sprockets.Graphics.Ink.R * tint.R >> 8),
                G = (byte)(Sprockets.Graphics.Ink.G * tint.G >> 8),
                B = (byte)(Sprockets.Graphics.Ink.B * tint.B >> 8),
                A = (byte)(Sprockets.Graphics.Ink.A * tint.A >> 8)
            };

            Sprockets.Graphics.Reset(Sprockets.Graphics.Ink.Uint);

            Sprockets.Graphics.Translate((int)(xPosition * xSnap + xBorder), (int)(yPosition * ySnap + yBorder));
            Sprockets.Graphics.Rotate(rotation);
            Sprockets.Graphics.Scale(xScale * xSnap, yScale * ySnap);

            Sprockets.Graphics.PushMatrix();

            var nextDepthBase = Sprockets.Graphics.DepthBase;

            // Render

            var address = LiquidPlayer.Program.ClassManager[liquidClass].Render;

            if (address != -1)
            {
                var stack = new int[LiquidPlayer.Program.VM_STACK_SIZE];

                stack[1] = objectManager.Copy(orthoId);

                LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId, address, stack, 1);

                objectManager.Mark(stack[1]);

                stack = null;
            }
            else
            {
                render(orthoId);
            }

            // postRender

            Sprockets.Graphics.CleanMatrixStack(objectId);

            if (!objectManager.IsA(objectId, LiquidClass.Layer))
            {
                Sprockets.Graphics.PopMatrix();
                Sprockets.Graphics.Translate(0f, 0f, Sprockets.Graphics.DepthBase - nextDepthBase);
                Sprockets.Graphics.PushMatrix();
            }

            var renderListClone = renderList.Clone();

            foreach (var gelId in renderListClone)
            {
                Sprockets.Graphics.LinkNextObject(gelId);

                var gel = objectManager[gelId].LiquidObject as GEL;

                liquidClass = objectManager[gelId].LiquidClass;

                gel.Render(liquidClass, orthoId);
            }
                
            Sprockets.Graphics.CleanClipStack(objectId);

            Sprockets.Graphics.PopMatrix();

            Sprockets.Graphics.Ink = inkColor;
            Sprockets.Graphics.PopMatrix();
            Sprockets.Graphics.Translate(0f, 0f, Sprockets.Graphics.DepthBase - depthBase);
        }

        public void Show()
        {
            if (!visible)
            {
                visible = true;

                var task = objectManager.GetTask(objectId);

                task.Show(objectId);
            }
        }

        public void Hide()
        {
            if (visible)
            {
                visible = false;

                var task = objectManager.GetTask(objectId);

                task.Hide(objectId);
            }
        }

        public void Tint(uint color)
        {
            this.tint.Uint = color;
        }

        public void Alpha(float a)
        {
            this.tint.A = (byte)(a * 255);
        }

        public void Center()
        {
            xPosition = LiquidPlayer.Program.ScreenWidth / 2;
            yPosition = LiquidPlayer.Program.ScreenHeight / 2;
        }

        public void Move(int xPosition, int yPosition)
        {
            this.xPosition = xPosition;
            this.yPosition = yPosition;
        }

        public void MoveDirection(double direction, double speed)
        {
            xPosition += Math.Sin(direction * Math.PI / 180d) * speed;
            yPosition += Math.Cos(direction * Math.PI / 180d) * speed;
        }

        public void MoveRelative(int xStep, int yStep)
        {
            xPosition += xStep;
            yPosition += yStep;
        }

        public void Scale(double scale)
        {
            xScale = scale;
            yScale = scale;
        }

        public void Scale(double xScale, double yScale)
        {
            this.xScale = xScale;
            this.yScale = yScale;
        }

        public void Rotate(double rotation)
        {
            this.rotation = rotation;
        }

        public override void shutdown()
        {
            renderList = null;

            base.shutdown();
        }
    }

    class GELComparer : IComparer<int>
    {
        int IComparer<int>.Compare(int x, int y)
        {
            var gelX = LiquidPlayer.Program.Exec.ObjectManager[x].LiquidObject as GEL;
            var gelY = LiquidPlayer.Program.Exec.ObjectManager[y].LiquidObject as GEL;

            if (gelX == null && gelY == null)
            {
                return 0;
            }
            else if (gelX == null)
            {
                return -1;
            }
            else if (gelY == null)
            {
                return 1;
            }

            var priorityX = gelX.Priority;
            var priorityY = gelY.Priority;

            if (priorityX < priorityY)
            {
                return -1;
            }
            else if (priorityX > priorityY)
            {
                return 1;
            }

            return 0;
        }
    }
}