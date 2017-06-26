using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class App : Task
    {
        protected int orthoId;
        protected Ortho ortho;

        protected int perspectiveId;
        protected Perspective perspective;

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
        protected List<int> render3DList;

        //protected int taskBar;
        //protected int menuBar;
        //protected int menu;
        //protected int menuItemAbout;
        //protected int menuItemSettings;
        //protected int menuItemSaveSettings;
        //protected int menuItemExit;

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

        public static int NewApp(string path, string arguments, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.App);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new App(id, path, arguments);

            return id;
        }

        protected App(int id, string path, string arguments)
            : base(id, path, arguments)
        {
            var orthoId = Ortho.NewOrtho(id);

            if (orthoId == 0)
            {
                RaiseError(ErrorCode.OutOfMemory);
                return;
            }

            this.orthoId = orthoId;
            this.ortho = objectManager[orthoId].LiquidObject as Ortho;

            var perspectiveId = Perspective.NewPerspective(id);

            if (perspectiveId == 0)
            {
                RaiseError(ErrorCode.OutOfMemory);
                return;
            }

            this.perspectiveId = perspectiveId;
            this.perspective = objectManager[perspectiveId].LiquidObject as Perspective;

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
            this.render3DList = new List<int>();

            //this.taskBar = 0;
            //this.menuBar = 0;
            //this.menu = 0;
            //this.menuItemAbout = 0;
            //this.menuItemSettings = 0;
            //this.menuItemSaveSettings = 0;
            //this.menuItemExit = 0;

            AutoSnap(true);

            objectManager.Focus = objectId;
        }

        public override string ToString()
        {
            return $"App (Tag: \"{tag}\"), Path: \"{path}\")";
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
            return base.callback(messageId);
        }

        public override void UpdateScene()
        {

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

                gel.VRender(liquidClass, orthoId, xBorder, yBorder, xSnap, ySnap);
            }
        }

        private void renderGEL3Ds()
        {
            if (!Sprockets.Graphics.Sorted3D)
            {
                render3DList = render3DList.OrderBy(a => a, new GEL3DComparer()).ToList();

                Sprockets.Graphics.Sorted3D = true;
            }

            Sprockets.Graphics.ViewPerspective(45d, (double)LiquidPlayer.Program.WindowWidth / LiquidPlayer.Program.WindowHeight, 1d, 1000d);

            Sprockets.Graphics.Reset3D(Sprockets.Color.White.Uint);

            // lights
            // fog

            var renderList3DClone = render3DList.Clone();

            foreach (var gel3DId in renderList3DClone)
            {
                var gel3D = objectManager[gel3DId].LiquidObject as GEL3D;

                var liquidClass = objectManager[gel3DId].LiquidClass;

                gel3D.VRender(liquidClass, perspectiveId);
            }

            // no fog
            // no lights

            Sprockets.Graphics.ClearDepthBuffer();
            Sprockets.Graphics.ClearStencilBuffer();
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

            if (objectManager.IsA(id, LiquidClass.GEL))
            {
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
            else if (objectManager.IsA(id, LiquidClass.GEL3D))
            {
                var parentId = objectManager[id].ParentId;

                if (parentId == objectId)
                {
                    render3DList.Add(id);
                }
                else
                {
                    var gel3D = objectManager[parentId].LiquidObject as GEL3D;

                    gel3D.Render3DList.Add(id);
                }

                Sprockets.Graphics.Sorted3D = false;
            }
        }

        public override void Hide(int id)
        {
            if (id == objectId)
            {
                return;
            }

            if (objectManager.IsA(id, LiquidClass.GEL))
            {
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
            else if (objectManager.IsA(id, LiquidClass.GEL3D))
            {
                var parentId = objectManager[id].ParentId;

                if (parentId == objectId)
                {
                    render3DList.Remove(id);
                }
                else
                {
                    var gel3D = objectManager[parentId].LiquidObject as GEL3D;

                    gel3D.Render3DList.Remove(id);
                }
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

        public void Be()
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


            Sprockets.Graphics.EnableRendering();

            var c = new Sprockets.Color.RGBA();

            c.Uint = paperColor;

            Sprockets.Graphics.ClearColor(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

            Sprockets.Graphics.Clear();
            Sprockets.Graphics.ResetNodes();

            var windowWidth = LiquidPlayer.Program.WindowWidth;
            var windowHeight = LiquidPlayer.Program.WindowHeight;

            Sprockets.Graphics.ScissorTest();
            Sprockets.Graphics.Scissor((int)xBorder, (int)yBorder, windowWidth - (int)xBorder * 2, windowHeight - (int)yBorder * 2);

            if (render3DList.Count != 0)
            {
                renderGEL3Ds();
            }

            renderGELs();

            // render the layers

            // render the taskbar

            // render the menubar

            // render any exceptions

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

            Sprockets.Graphics.DisableRendering();

            LiquidPlayer.Program.SwapBuffers();
        }

        public int DequeueMessage()
        {
            return (messageQueue.Count != 0) ? messageQueue.Dequeue() : 0;
        }

        public bool IsMessageQueueEmpty()
        {
            return messageQueue.Count == 0;
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

            objectManager.Mark(orthoId);
            ortho = null;

            objectManager.Mark(perspectiveId);
            perspective = null;

            updateList = null;
            renderList = null;
            render3DList = null;

            base.shutdown();
        }
    }
}