using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Pen : Brush
    {
        protected int characterSetId;
        protected CharacterSet characterSet;

        protected int characterSetBitmapId;
        protected Bitmap characterSetBitmap;

        public int CharacterSetId
        {
            get
            {
                return characterSetId;
            }
        }

        public static int NewPen(int bitmapId, int characterSetId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Pen);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Pen(id, bitmapId, characterSetId);

            return id;
        }

        public Pen(int id, int bitmapId, int characterSetId)
            : base(id, bitmapId)
        {
            if (bitmapId == 0 || characterSetId == 0)
            {
                Throw(ExceptionCode.NullObject);
                return;
            }

            this.characterSetId = objectManager.Copy(characterSetId);
            this.characterSet = objectManager[characterSetId].LiquidObject as CharacterSet;

            this.characterSetBitmapId = objectManager.Copy(characterSet.BitmapId);
            this.characterSetBitmap = objectManager[characterSetBitmapId].LiquidObject as Bitmap;
        }

        public override string ToString()
        {
            return $"Pen";
        }

        public void PrintAt(int x, int y, string text)
        {
            if (text.Length == 0)
            {
                return;
            }

            var bytes = Encoding.ASCII.GetBytes(text);

            foreach (var b in bytes)
            {
                if (characterSet.IsAvailable[b])
                {
                    var x1 = characterSet.X1[b];
                    var y1 = characterSet.Y1[b];

                    var startX = x;
                    var startY = y;
                    for (var i = 0; i < characterSet.TileHeight; i++)
                    {
                        var index = y1 * characterSetBitmap.Width + x1;
                        for (var j = 0; j < characterSet.TileWidth; j++)
                        {
                            var color = characterSetBitmap.FastPeek(index);

                            if (color >> 24 != 0)
                            {
                                Plot(x, y, Sprockets.Color.Blend(color, inkColor));
                            }

                            x++;
                            index++;
                        }
                        x = startX;
                        y++;
                        y1++;
                    }
                    x += characterSet.TileWidth;
                    y = startY;
                }
            }
        }

        public override void shutdown()
        {
            objectManager.Mark(characterSetId);
            characterSet = null;

            objectManager.Mark(characterSetBitmapId);
            characterSetBitmap = null;

            base.shutdown();
        }
    }
}