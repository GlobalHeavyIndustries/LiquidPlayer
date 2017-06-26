using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class CopperBars : View
    {
        protected int height;
        protected uint[] colors;

        public int Height
        {
            get
            {
                return height;
            }
        }

        public static int NewCopperBars(int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.CopperBars);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new CopperBars(id, height);

            return id;
        }

        public CopperBars(int id, int height)
            : base(id)
        {
            this.height = height;
            this.colors = new uint[height];

            Show();
            Center();
        }

        public override string ToString()
        {
            return $"CopperBars (Scan lines: {height})";
        }

        protected override void render(int orthoId)
        {
            var x = 32767;
            var y = -(LiquidPlayer.Program.ScreenHeight / 2);

            foreach (var color in colors)
            {
                Sprockets.Graphics.SetColor(color);

                Sprockets.Graphics.RectangleFill(-x, y, x, y);

                y++;
            }
        }

        public void RollDown()
        {
            var color = colors[height - 1];

            for (var index = height - 1; index > 0; index--)
            {
                colors[index] = colors[index - 1];
            }

            colors[0] = color;
        }

        public void RollUp()
        {
            var color = colors[0];

            for (var index = 0; index < height - 1; index++)
            {
                colors[index] = colors[index + 1];
            }

            colors[height - 1] = color;
        }

        public void Set(int index, uint color)
        {
            if (index < 0 || index >= height)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            colors[index] = color;
        }

        public override void shutdown()
        {
            colors = null;

            base.shutdown();
        }
    }
}