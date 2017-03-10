using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class MonoSpacedFont : Font
    {
        protected string fontName;
        protected float fontSize;

        protected int bitmapId;
        protected Bitmap bitmap;

        public static int NewMonoSpacedFont(string fontName, float fontSize, int fontStyle, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.MonoSpacedFont);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new MonoSpacedFont(id, fontName, fontSize, fontStyle);

            return id;
        }

        public MonoSpacedFont(int id, string fontName, float fontSize, int fontStyle)
            : base(id, "")
        {
            var charWidth = 0;
            var charHeight = 0;

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

            var data = Sprockets.Graphics.BuildMonoSpacedFont(fontName, fontSize, enumFontStyle, out charWidth, out charHeight, out width, out height);

            if (data == null)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            this.bitmapId = Bitmap.NewBitmap(width, height, id);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            bitmap.Load(data);

            bitmap.SwapBuffers();

            isLoaded = true;

            for (var ch = 32; ch < 128; ch++)
            {
                isAvailable[ch] = true;
            }

            isFixedWidth = true;

            for (var ch = 32; ch < 128; ch++)
            {
                this.width[ch] = charWidth;
            }

            this.height = charHeight;

            this.fontName = fontName;
            this.fontSize = fontSize;

        }

        public override string ToString()
        {
            return $"Mono-Spaced Font (Name: \"{name}\")";
        }

        public int GetBitmap()
        {
            return objectManager.Copy(bitmapId);
        }

        public override void Print(int x, int y, string caption)
        {
            var handle = bitmap.Handle;

            Sprockets.Graphics.DrawMonoSpacedText(handle, x, y, caption, width[33], height, bitmap.Width, bitmap.Height);
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            base.shutdown();
        }
    }
}
