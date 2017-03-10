using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Text : GEL
    {
        protected int fontId;
        protected Font font;

        protected string caption;
        protected int width;
        protected int height;
        protected int xAlignment;
        protected int yAlignment;
        protected int xOffset;
        protected int yOffset;

        public int FontId
        {
            get
            {
                return fontId;
            }
        }

        public string Caption
        {
            get
            {
                return caption;
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

        public int XAlignment
        {
            get
            {
                return xAlignment;
            }
        }

        public int YAlignment
        {
            get
            {
                return yAlignment;
            }
        }

        public static int NewText(int fontId, string caption, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Text);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Text(id, fontId, caption);

            return id;
        }

        public Text(int id, int fontId, string caption, int xAlignment = 0, int yAlignment = 0)
            : base(id)
        {
            if (fontId == 0)
            {
                fontId = objectManager.ConsoleFontId;
            }

            this.fontId = objectManager.Copy(fontId);
            this.font = objectManager[fontId].LiquidObject as Font;

            this.xAlignment = 0;
            this.yAlignment = 0;

            SetCaption(caption);
        }

        public override string ToString()
        {
            return $"Text (\"{caption}\")";
        }

        public void hAlign(int alignment)
        {
            xAlignment = Math.Sign(alignment);
        }

        public void vAlign(int alignment)
        {
            yAlignment = Math.Sign(alignment);
        }

        public void SetCaption(string caption)
        {
            this.caption = caption;

            width = font.GetWidth(caption);
            height = font.GetHeight();

            if (xAlignment < 0)
            {
                xOffset = 0;
            }
            else if (xAlignment > 0)
            {
                xOffset = -width;
            }
            else
            {
                xOffset = -(width / 2);
            }

            if (yAlignment < 0)
            {
                yOffset = 0;
            }
            else if (yAlignment > 0)
            {
                yOffset = -height;
            }
            else
            {
                yOffset = -(height / 2);
            }
        }

        protected override void render(int orthoId)
        {
            font.Print(xOffset, yOffset, caption);
        }

        public override void shutdown()
        {
            objectManager.Mark(fontId);
            font = null;

            base.shutdown();
        }
    }
}