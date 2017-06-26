using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Program : Task
    {
        protected int[] timers;
        protected Queue<int> inputBuffer;

        protected uint borderColor;
        protected uint paperColor;

        protected bool autoSnap;
        protected bool preserveAspectRatio;
        protected double xBorder;
        protected double yBorder;
        protected double xSnap;
        protected double ySnap;

        protected List<int> updateList;

        protected List<int> renderList;

        public Queue<int> InputBuffer
        {
            get
            {
                return inputBuffer;
            }
        }

        public uint BorderColor
        {
            get
            {
                return borderColor;
            }

            set
            {
                borderColor = value;
            }
        }

        public uint PaperColor
        {
            get
            {
                return paperColor;
            }
            set
            {
                paperColor = value;
            }
        }

        public static int NewProgram(string path, string arguments, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Program);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Program(id, path, arguments);

            return id;
        }

        protected Program(int id, string path, string arguments)
            : base(id, path, arguments)
        {
            this.timers = new int[256];
            this.inputBuffer = new Queue<int>();

            this.borderColor = Sprockets.Color.Border.Uint;
            this.paperColor = Sprockets.Color.Black.Uint;

            this.autoSnap = false;
            this.preserveAspectRatio = false;
            this.xBorder = 0;
            this.yBorder = 0;
            this.xSnap = 1d;
            this.ySnap = 1d;

            this.updateList = new List<int>();

            this.renderList = new List<int>();

            AutoSnap(true);

            objectManager.Focus = objectId;
        }

        public override string ToString()
        {
            return $"Program (Tag: \"{tag}\"), Path: \"{path}\")";
        }

        private void GameWindow_Resize(object sender, EventArgs e)
        {
            var screenWidth = LiquidPlayer.Program.ScreenWidth;
            var screenHeight = LiquidPlayer.Program.ScreenHeight;

            var windowWidth = LiquidPlayer.Program.WindowWidth;
            var windowHeight = LiquidPlayer.Program.WindowHeight;

            xSnap = (double)windowWidth / screenWidth;
            ySnap = (double)windowHeight / screenHeight;

            if (preserveAspectRatio)
            {
                if (xSnap < ySnap)
                {
                    ySnap = xSnap;
                }
                else if (xSnap > ySnap)
                {
                    xSnap = ySnap;
                }
            }

            xBorder = (int)(windowWidth - screenWidth * xSnap) / 2;
            yBorder = (int)(windowHeight - screenHeight * ySnap) / 2;
        }

        protected override bool callback(int messageId)
        {
            var message = objectManager[messageId].LiquidObject as Message;

            if (message.IsTo(objectId))
            {
                var data = Convert.ToInt32(message.GetData());

                switch (message.GetBody())
                {
                    case MessageBody.KeyDown:
                        inputBuffer.Enqueue((int)EventCode.KeyDown);
                        inputBuffer.Enqueue(data);
                        return true;
                    case MessageBody.Timer:
                        inputBuffer.Enqueue((int)EventCode.Timer);
                        inputBuffer.Enqueue(data);
                        return true;
                }
            }

            return base.callback(messageId);
        }

        public override void UpdateScene()
        {
            while (messageQueue.Count != 0)
            {
                var messageId = messageQueue.Dequeue();
                var message = objectManager[messageId].LiquidObject as Message;

                message.Dispatch();

                objectManager.Mark(messageId);
            }

            var updateListClone = updateList.Clone();

            foreach (var entityId in updateListClone)
            {
                var entity = objectManager[entityId].LiquidObject as Entity;

                var liquidClass = objectManager[entityId].LiquidClass;

                entity.VUpdate(liquidClass);
            }
        }

        public override void Start(int id)
        {
            if (id == objectId)
            {
                return;
            }

            var parentId = objectManager[id].ParentId;

            if (parentId == objectId)
            {
                updateList.Add(id);
            }
            else
            {
                var entity = objectManager[parentId].LiquidObject as Entity;

                entity.UpdateList.Add(id);
            }
        }

        public override void Stop(int id)
        {
            if (id == objectId)
            {
                return;
            }

            var parentId = objectManager[id].ParentId;

            if (parentId == objectId)
            {
                updateList.Remove(id);
            }
            else
            {
                var entity = objectManager[parentId].LiquidObject as Entity;

                entity.UpdateList.Remove(id);
            }
        }

        public override void RenderScene()
        {
            var c = new Sprockets.Color.RGBA();

            c.Uint = paperColor;

            Sprockets.Graphics.ClearColor(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

            Sprockets.Graphics.Clear();
            Sprockets.Graphics.ResetNodes();

            var windowWidth = LiquidPlayer.Program.WindowWidth;
            var windowHeight = LiquidPlayer.Program.WindowHeight;

            Sprockets.Graphics.ScissorTest();
            Sprockets.Graphics.Scissor((int)xBorder, (int)yBorder, windowWidth - (int)xBorder * 2, windowHeight - (int)yBorder * 2);

            renderGELs();

            Sprockets.Graphics.NoScissorTest();

            var mouseX = Sprockets.Input.MouseXPosition;
            var mouseY = Sprockets.Input.MouseYPosition;

            Sprockets.Input.MouseSnappedXPosition = (int)((mouseX - xBorder) / xSnap);
            Sprockets.Input.MouseSnappedYPosition = (int)((mouseY - yBorder) / ySnap);

            Sprockets.Graphics.ReadPixel(mouseX, mouseY);

            renderBorders();

            if (debugMode)
            {
                renderDebugInfo();
            }

            LiquidPlayer.Program.SwapBuffers();
        }

        private void renderGELs()
        {
            if (!Sprockets.Graphics.Sorted)
            {
                renderList = renderList.OrderBy(a => a, new GELComparer()).ToList();

                Sprockets.Graphics.Sorted = true;
            }

            Sprockets.Graphics.ViewOrtho();

            Sprockets.Graphics.Reset(Sprockets.Color.White.Uint);

            Sprockets.Graphics.LinkObject(objectId);

            var renderListClone = renderList.Clone();

            foreach (var gelId in renderListClone)
            {
                Sprockets.Graphics.LinkNextObject(gelId);

                var gel = objectManager[gelId].LiquidObject as GEL;

                var liquidClass = objectManager[gelId].LiquidClass;

                gel.VRender(liquidClass, 0, xBorder, yBorder, xSnap, ySnap);
            }
        }

        private void renderBorders()
        {
            Sprockets.Graphics.SetColor(borderColor);

            var windowWidth = LiquidPlayer.Program.WindowWidth;
            var windowHeight = LiquidPlayer.Program.WindowHeight;

            if (xBorder != 0)
            {
                Sprockets.Graphics.RectangleFill(0, 0, (int)xBorder - 1, windowHeight - 1);
                Sprockets.Graphics.RectangleFill(windowWidth - (int)xBorder, 0, windowWidth - 1, windowHeight - 1);
            }

            if (yBorder != 0)
            {
                Sprockets.Graphics.RectangleFill(0, 0, windowWidth - 1, (int)yBorder - 1);
                Sprockets.Graphics.RectangleFill(0, windowHeight - (int)yBorder, windowWidth - 1, windowHeight - 1);
            }
        }

        private void renderDebugInfo()
        {
            var width = LiquidPlayer.Program.GameWindow.Width;
            var height = LiquidPlayer.Program.GameWindow.Height;

            var consoleFont = objectManager[objectManager.SystemCharacterSetId].LiquidObject as CharacterSet;

            var displayList = consoleFont.DisplayList;


            Sprockets.Graphics.SetColor(0, 85, 216, 192);
            Sprockets.Graphics.RectangleFill(5, 5, 95, 115);

            Sprockets.Graphics.SetColor(0, 0, 0, 255);
            Sprockets.Graphics.Rectangle(5, 5, 95, 115);

            Sprockets.Graphics.PrintLists(displayList, 10, 10, "X=" + Sprockets.Input.MouseSnappedXPosition, 0xFFFFFFFF, 0xFF000000);
            Sprockets.Graphics.PrintLists(displayList, 10, 30, "Y=" + Sprockets.Input.MouseSnappedYPosition, 0xFFFFFFFF, 0xFF000000);
            Sprockets.Graphics.PrintLists(displayList, 10, 50, "O=" + Sprockets.Input.MousePointingAt, 0xFFFFFFFF, 0xFF000000);
            Sprockets.Graphics.PrintLists(displayList, 10, 70, "N=" + Sprockets.Input.MousePointingAtNode, 0xFFFFFFFF, 0xFF000000);
            Sprockets.Graphics.PrintLists(displayList, 10, 90, "#=" + LiquidPlayer.Program.Exec.ObjectManager.Count, 0xFFFFFFFF, 0xFF000000);


            Sprockets.Graphics.SetColor(0, 85, 216, 192);
            Sprockets.Graphics.RectangleFill(width - 165, 5, width - 5, 210);

            Sprockets.Graphics.SetColor(0, 0, 0, 255);
            Sprockets.Graphics.Rectangle(width - 165, 5, width - 5, 210);

            for (var index = 0; index < 10; index++)
            {
                Sprockets.Graphics.PrintLists(displayList, width - 155, 10 + index * 20, index.ToString() + "=" + LiquidPlayer.Program.Exec.Watch[index].ToString(), 0xFFFFFFFF, 0xFF000000);
            }
        }

        public override void Show(int id)
        {
            if (id == objectId)
            {
                return;
            }

            //if (objectManager.IsA(id, LiquidClass.View) ||
            //    objectManager.IsA(id, LiquidClass.Sprite) ||
            //    objectManager.IsA(id, LiquidClass.Text))
            //{
            //    renderList.Add(id);

            //    Sprockets.Graphics.Sorted = false;
            //}

            System.Diagnostics.Debug.Assert(objectManager.IsA(id, LiquidClass.GEL));

            var parentId = objectManager[id].ParentId;

            if (parentId == objectId)
            {
                renderList.Add(id);
            }
            else
            {
                var gel = objectManager[parentId].LiquidObject as GEL;

                gel.RenderList.Add(id);
            }

            Sprockets.Graphics.Sorted = false;
        }

        public override void Hide(int id)
        {
            if (id == objectId)
            {
                return;
            }

            //if (objectManager.IsA(id, LiquidClass.View) ||
            //    objectManager.IsA(id, LiquidClass.Sprite) ||
            //    objectManager.IsA(id, LiquidClass.Text))
            //{
            //    renderList.Remove(id);
            //}

            System.Diagnostics.Debug.Assert(objectManager.IsA(id, LiquidClass.GEL));

            var parentId = objectManager[id].ParentId;

            if (parentId == objectId)
            {
                renderList.Remove(id);
            }
            else
            {
                var gel = objectManager[parentId].LiquidObject as GEL;

                gel.RenderList.Remove(id);
            }
        }

        public void AutoSnap(bool autoSnap, bool preserveAspectRatio = true)
        {
            if (autoSnap)
            {
                LiquidPlayer.Program.GameWindow.Resize += GameWindow_Resize;
            }
            else
            {
                LiquidPlayer.Program.GameWindow.Resize -= GameWindow_Resize;
            }

            this.autoSnap = autoSnap;

            this.preserveAspectRatio = preserveAspectRatio;
        }

        public int GetKey()
        {
            var key = 0;

            while (inputBuffer.Count > 0)
            {
                var c = inputBuffer.Dequeue();
                var d = inputBuffer.Dequeue();

                if (c == (int)EventCode.KeyDown)
                {
                    key = d;
                    break;
                }
            }

            return key;
        }

        public void Screen(int width, int height)
        {
            LiquidPlayer.Program.ScreenWidth = width;
            LiquidPlayer.Program.ScreenHeight = height;

            LiquidPlayer.Program.WindowWidth = width;
            LiquidPlayer.Program.WindowHeight = height;

            LiquidPlayer.Program.ResizeGameWindow(width, height);

            LiquidPlayer.Program.CenterGameWindow();
        }

        public override void shutdown()
        {
            LiquidPlayer.Program.GameWindow.Resize -= GameWindow_Resize;

            timers = null;
            inputBuffer = null;
            updateList = null;
            renderList = null;

            base.shutdown();
        }
    }
}
