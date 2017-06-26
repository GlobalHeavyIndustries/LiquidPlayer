using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class GEL3D : Entity
    {
        protected bool visible;
        //protected bool lighting;
        //protected bool fog;
        protected Sprockets.Color.RGBA tint;
        protected double xPosition;
        protected double yPosition;
        protected double zPosition;
        protected double xScale;
        protected double yScale;
        protected double zScale;
        protected double xRotation;
        protected double yRotation;
        protected double zRotation;

        protected List<int> render3DList;

        public bool IsVisible
        {
            get
            {
                return visible;
            }
        }

        public bool GotFocus
        {
            get
            {
                return (objectManager.Focus == objectId) ? true : false;
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

        public double ZPosition
        {
            get
            {
                return zPosition;
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

        public double ZScale
        {
            get
            {
                return zScale;
            }
        }

        public double XRotation
        {
            get
            {
                return xRotation;
            }
        }

        public double YRotation
        {
            get
            {
                return yRotation;
            }
        }

        public double ZRotation
        {
            get
            {
                return zRotation;
            }
        }

        public List<int> Render3DList
        {
            get
            {
                return render3DList;
            }
        }

        public GEL3D(int id)
            : base(id)
        {
            this.visible = false;
            this.tint = Sprockets.Color.White;
            this.xPosition = 0d;
            this.yPosition = 0d;
            this.zPosition = 0d;
            this.xScale = 1d;
            this.yScale = 1d;
            this.zScale = 1d;
            this.xRotation = 0d;
            this.yRotation = 0d;
            this.zRotation = 0d;

            this.render3DList = new List<int>();
        }

        public override string ToString()
        {
            return $"GEL3D";
        }

        protected virtual void render(int perspectiveId)
        {
            //Throw(ExceptionCode.NotImplemented);
        }

        public void VRender(LiquidClass liquidClass, int perspectiveId)
        {
            // preRender

            Sprockets.Graphics.PushMatrix();

            var inkColor = Sprockets.Graphics.Ink;

            Sprockets.Graphics.Ink = new Sprockets.Color.RGBA
            {
                R = (byte)(Sprockets.Graphics.Ink.R * tint.R >> 8),
                G = (byte)(Sprockets.Graphics.Ink.G * tint.G >> 8),
                B = (byte)(Sprockets.Graphics.Ink.B * tint.B >> 8),
                A = (byte)(Sprockets.Graphics.Ink.A * tint.A >> 8)
            };

            Sprockets.Graphics.Reset3D(Sprockets.Graphics.Ink.Uint);

            Sprockets.Graphics.Translate(xPosition, yPosition, zPosition);
            Sprockets.Graphics.Rotate(xRotation, yRotation, zRotation);
            Sprockets.Graphics.Scale(xScale, yScale, zScale);

            // Render

            var address = LiquidPlayer.Program.ClassManager[liquidClass].Render;

            if (address != -1)
            {
                var stack = new int[LiquidPlayer.Program.VM_STACK_SIZE];

                stack[1] = objectManager.Copy(perspectiveId);

                LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId, address, stack, 1);

                objectManager.Mark(stack[1]);

                stack = null;
            }
            else
            {
                render(perspectiveId);
            }

            // postRender

            Sprockets.Graphics.CleanMatrixStack(objectId);

            var renderList3DClone = render3DList.Clone();

            foreach (var gel3DId in renderList3DClone)
            {
                var gel3D = objectManager[gel3DId].LiquidObject as GEL3D;

                liquidClass = objectManager[gel3DId].LiquidClass;

                gel3D.VRender(liquidClass, perspectiveId);
            }

            Sprockets.Graphics.Ink = inkColor;
            Sprockets.Graphics.PopMatrix();
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

        public void Move(double xPos, double yPos, double zPos)
        {
            if (zPosition != zPos)
            {
                Sprockets.Graphics.Sorted3D = false;
            }

            xPosition = xPos;
            yPosition = yPos;
            zPosition = zPos;
        }

        public void Center()
        {
            if (zPosition != 0d)
            {
                Sprockets.Graphics.Sorted3D = false;
            }

            xPosition = 0d;
            yPosition = 0d;
            zPosition = 0d;
        }

        public void Rotate(double xScale, double yScale, double zScale)
        {
            this.xScale = xScale;
            this.yScale = yScale;
            this.zScale = zScale;
        }

        public void Scale(double xScale, double yScale, double zScale)
        {
            this.xScale = xScale;
            this.yScale = yScale;
            this.zScale = zScale;
        }

        public override void Destructor()
        {
            Hide();

            if (Sprockets.Input.MousePointingAt == objectId)
            {
                Sprockets.Input.MousePointingAt = 0;
                Sprockets.Input.MousePointingAtNode = 0;
            }

            base.Destructor();
        }

        public override void shutdown()
        {
            render3DList = null;

            base.shutdown();
        }
    }

    class GEL3DComparer : IComparer<int>
    {
        int IComparer<int>.Compare(int x, int y)
        {
            var gel3DX = LiquidPlayer.Program.Exec.ObjectManager[x].LiquidObject as GEL3D;
            var gel3DY = LiquidPlayer.Program.Exec.ObjectManager[y].LiquidObject as GEL3D;

            if (gel3DX == null && gel3DY == null)
            {
                return 0;
            }
            else if (gel3DX == null)
            {
                return -1;
            }
            else if (gel3DY == null)
            {
                return 1;
            }

            var zPositionX = gel3DX.ZPosition;
            var zPositionY = gel3DY.ZPosition;

            if (zPositionX < zPositionY)
            {
                return -1;
            }
            else if (zPositionX > zPositionY)
            {
                return 1;
            }

            return 0;
        }
    }
}