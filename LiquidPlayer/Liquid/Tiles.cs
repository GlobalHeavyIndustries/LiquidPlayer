using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Tiles : GEL
    {
        protected int characterSetId;
        protected CharacterSet characterSet;

        protected string caption;
        protected int width;
        protected int height;
        protected int xAlignment;
        protected int yAlignment;
        protected int xOffset;
        protected int yOffset;

        public int CharacterSetId
        {
            get
            {
                return characterSetId;
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

        public static int NewTiles(int characterSetId, string caption, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Tiles);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Tiles(id, characterSetId, caption);

            return id;
        }

        public Tiles(int id, int characterSetId, string caption, int xAlignment = 0, int yAlignment = 0)
            : base(id)
        {
            if (characterSetId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.characterSetId = objectManager.Copy(characterSetId);
            this.characterSet = objectManager[characterSetId].LiquidObject as CharacterSet;

            this.xAlignment = 0;
            this.yAlignment = 0;

            SetCaption(caption);
        }

        public override string ToString()
        {
            return $"Tiles (\"{caption}\")";
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

            width = characterSet.GetWidth(caption);
            height = characterSet.GetHeight();

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
            characterSet.Print(xOffset, yOffset, caption);
        }

        public override void shutdown()
        {
            objectManager.Mark(characterSetId);
            characterSet = null;

            base.shutdown();
        }
    }
}