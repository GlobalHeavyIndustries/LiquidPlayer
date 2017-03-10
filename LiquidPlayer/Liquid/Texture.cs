using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Texture : Bitmap
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        public static int NewTexture(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Texture);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Texture(id);

            return id;
        }

        public Texture(int id)
            : base(id)
        {
            this.width = 256;
            this.height = 256;
            this.size = width * height;

            this.data = new uint[size];
            this.inSync = false;
            this.doubleBuffered = false;

            this.handle = Sprockets.Graphics.GenTexture();
            Sprockets.Graphics.BindTexture(handle, width, height, data);
            this.inSync = true;
        }

        public override string ToString()
        {
            return $"Texture";
        }

        public void Checkerboard(int dx, int dy, uint color1, uint color2)
        {
            var flipColor = ((256 / dx) & 1) == 0 && (256 % dx) != 0;

            var index = 0;

            for (var y = 0; y < 256; y++)
            {
                if (y % dy == 0)
                {
                    Util.Swap(ref color1, ref color2);
                }

                for (var x = 0; x < 256; x++)
                {
                    if (x % dx == 0)
                    {
                        Util.Swap(ref color1, ref color2);
                    }

                    data[index++] = color1;
                }

                if (flipColor)
                {
                    Util.Swap(ref color1, ref color2);
                }
            }

            inSync = false;
        }

        public void Particle(double size)
        {
            var oo1sxd2 = 1d / (width / 2);
            var oo1syd2 = 1d / (height / 2);

            var f = (100 / size) * 180;

            var index = 0;

            for (var y = 0; y < 256; y++)
            {
                var ny = (y - height / 2) * oo1syd2;

                for (var x = 0; x < 256; x++)
                {
                    var nx = (x - width / 2) * oo1sxd2;

                    var r = 255 - f * Math.Sqrt(nx * nx + ny * ny);

                    var s = (byte)Util.Clamp(r, 0, 255);

                    data[index++] = new Sprockets.Color.RGBA(s, s, s).Uint;
                }
            }

            inSync = false;
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}