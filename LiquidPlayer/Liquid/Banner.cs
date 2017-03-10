using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Banner : Bitmap
    {
        protected string fontName;
        protected float fontSize;
        protected string caption;
        protected int fontStyle;

        protected int bitmapId;
        protected Bitmap bitmap;

        public static int NewBanner(string fontName, float fontSize, int fontStyle, string caption, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Banner);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Banner(id, fontName, fontSize, fontStyle, caption);

            return id;
        }

        public Banner(int id, string fontName, float fontSize, int fontStyle, string caption)
            : base(id)
        {
            var width = 0;
            var height = 0;

            var enumFontStyle = System.Drawing.FontStyle.Regular;

            switch (fontStyle)
            {
                case 0:
                    break;
                case 1:
                    enumFontStyle = System.Drawing.FontStyle.Bold;
                    break;
                case 2:
                    enumFontStyle = System.Drawing.FontStyle.Italic;
                    break;
                case 3:
                    enumFontStyle = System.Drawing.FontStyle.Underline;
                    break;
                case 4:
                    enumFontStyle = System.Drawing.FontStyle.Strikeout;
                    break;
                default:
                    Throw(ExceptionCode.IllegalQuantity);
                    return;
            }

            var data = Sprockets.Graphics.BuildBanner(fontName, fontSize, enumFontStyle, caption, out width, out height);

            if (data == null)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            this.width = width;
            this.height = height;
            this.size = width * height;

            this.data = new uint[size];
            this.inSync = false;
            this.doubleBuffered = false;

            Load(data);

            this.handle = Sprockets.Graphics.GenTexture();
            Sprockets.Graphics.BindTexture(handle, width, height, data);
            this.inSync = true;

            this.fontName = fontName;
            this.fontSize = fontSize;
            this.caption = caption;
            this.fontStyle = fontStyle;
        }

        public override string ToString()
        {
            return $"Banner (\"{caption}\")";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}