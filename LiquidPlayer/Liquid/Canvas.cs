using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Canvas : View
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        protected int width;
        protected int height;

        public int BitmapId
        {
            get
            {
                return bitmapId;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public static int NewCanvas(int width, int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Canvas);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Canvas(id, width, height);

            return id;
        }

        public Canvas(int id, int width, int height)
            : base(id)
        {
            this.bitmapId = Bitmap.NewBitmap(width, height, id);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            this.width = width;
            this.height = height;

            Show();
            Center();
        }

        public Canvas(int id, int bitmapId)
            : base(id)
        {
            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            this.width = bitmap.Width;
            this.height = bitmap.Height;

            Show();
            Center();
        }

        public override string ToString()
        {
            return $"Canvas (Resolution: {width}x{height})";
        }

        protected override void render(int orthoId)
        {
            var bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            bitmap.Render();
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            base.shutdown();
        }
    }
}
