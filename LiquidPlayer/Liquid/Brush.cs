using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Brush : Object
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        public static int NewBrush(int bitmapId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Brush);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Brush(id, bitmapId);

            return id;
        }

        protected Brush(int id, int bitmapId)
            : base(id)
        {
            if (bitmapId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;
        }

        public override string ToString()
        {
            return $"Brush";
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            base.shutdown();
        }
    }
}