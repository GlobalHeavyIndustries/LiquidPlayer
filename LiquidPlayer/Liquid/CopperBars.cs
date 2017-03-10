using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class CopperBars : View
    {
        protected int width, height;
        protected uint[] colors;

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

        public static int NewCopperBars(int width, int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.CopperBars);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new CopperBars(id, width, height);

            return id;
        }

        public CopperBars(int id, int width, int height)
            : base(id)
        {
            this.width = width;
            this.height = height;
            this.colors = new uint[height];

            Show();
            Center();
        }

        public override string ToString()
        {
            return $"CopperBars (Resolution: {width}x{height})";
        }

        public override void shutdown()
        {
            colors = null;

            base.shutdown();
        }
    }
}