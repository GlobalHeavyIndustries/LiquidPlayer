using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class CharacterSet : Font
    {
        #region Dispose pattern
        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources here
            }

            // free unmanaged resources here
            if (displayList != 0)
            {
                Sprockets.Graphics.FreeLists(displayList, 256);
                displayList = 0;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        protected int bitmapId;
        protected Bitmap bitmap;

        protected int brushId;
        protected Brush brush;

        protected int displayList;
        protected int tileWidth;
        protected int tileHeight;
        protected int glyphWidth;
        protected int glyphHeight;
        protected int tilesAcross;
        protected int tilesDown;
        protected int maxTileCount;
        protected int[] x1;
        protected int[] y1;
        protected uint[] palette;

        public int BitmapId
        {
            get
            {
                return bitmapId;
            }
        }

        public int BrushId
        {
            get
            {
                return brushId;
            }
        }

        public int DisplayList
        {
            get
            {
                return displayList;
            }
        }

        public int TileWidth
        {
            get
            {
                return tileWidth;
            }
        }

        public int TileHeight
        {
            get
            {
                return tileHeight;
            }
        }

        public int GlyphWidth
        {
            get
            {
                return glyphWidth;
            }
        }

        public int GlyphHeight
        {
            get
            {
                return glyphHeight;
            }
        }

        public int[] X1
        {
            get
            {
                return x1;
            }
        }

        public int[] Y1
        {
            get
            {
                return y1;
            }
        }

        public static int NewCharacterSet(int bitmapId, int tileWidth, int tileHeight, int glyphWidth, int glyphHeight, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.CharacterSet);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new CharacterSet(id, bitmapId, tileWidth, tileHeight, glyphWidth, glyphHeight);

            return id;
        }

        public CharacterSet(int id, int bitmapId, int tileWidth = 0, int tileHeight = 0, int glyphWidth = 0, int glyphHeight = 0)
            : base(id, "")
        {
            if (bitmapId == 0)
            {
                Throw(ExceptionCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            var brushId = Brush.NewBrush(bitmapId, id);

            if (brushId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            this.brushId = brushId;
            this.brush = objectManager[brushId].LiquidObject as Brush;

            this.displayList = Sprockets.Graphics.GenLists(256);
            this.tileWidth = (tileWidth == 0) ? bitmap.Width / 16 : tileWidth;
            this.tileHeight = (tileHeight == 0) ? bitmap.Height / 16 : tileHeight;
            this.glyphWidth = (glyphWidth == 0) ? tileWidth : glyphWidth;
            this.glyphHeight = (glyphHeight == 0) ? tileHeight : glyphHeight;
            this.tilesAcross = bitmap.Width / tileWidth;
            this.tilesDown = bitmap.Height / tileHeight;
            this.maxTileCount = tilesAcross * tilesDown - 1;
            this.x1 = new int[256];
            this.y1 = new int[256];
            this.palette = new uint[256];

            this.isLoaded = true;
        }

        public override string ToString()
        {
            return $"Character Set (Name: \"{name}\")";
        }

        public void Clear(int character, uint color = 0)
        {
            if (character < 0 || character > 255)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            if (!isAvailable[character])
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            var x1 = this.x1[character];
            var y1 = this.y1[character];

            for (var y = y1; y < y1 + tileHeight; y++)
            {
                for (var x = x1; x < x1 + tileWidth; x++)
                {
                    brush.Plot(x, y, color);
                }
            }
        }

        public void CustomCharacter(int character, int scanLine, string data)
        {
            if (character < 0 || character > 255 || scanLine < 0 || scanLine >= tileHeight || data.Length > tileWidth)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            if (!isAvailable[character])
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            var x = x1[character];
            var y = y1[character] + scanLine;

            foreach(var ch in data)
            {
                brush.Plot(x, y, palette[ch]);
                x++;
            }
        }

        private bool mapCharacter(int character, int tile)
        {
            if (character < 0 || character > 255 || tile < 0 || tile > maxTileCount)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return true;
            }

            if (isAvailable[character])
            {
                Throw(ExceptionCode.Denied);
                return true;
            }

            var x1 = (tile % tilesAcross) * tileWidth;
            var x2 = x1 + tileWidth;
            var y1 = (tile / tilesAcross) * tileHeight;
            var y2 = y1 + tileHeight;

            Sprockets.Graphics.CreateTile(displayList + character, bitmap.Handle, x1, y1, x2, y2, bitmap.Width, bitmap.Height, glyphWidth, glyphHeight);

            isAvailable[character] = true;
            width[character] = glyphWidth;
            height = glyphHeight;

            this.x1[character] = x1;
            this.y1[character] = y1;

            return false;
        }

        public void MapAll()
        {
            var tile = 0;

            for (var character = 0; character < 256; character++)
            {
                if (mapCharacter(character, tile))
                {
                    return;
                }

                tile++;
            }
        }

        public void MapCharacter(int character, int tile)
        {
            if (mapCharacter(character, tile))
            {
                return;
            }
        }

        public void MapCharacters(int character, int tile, int count)
        {
            for (var index = 1; index <= count; index++)
            {
                if (mapCharacter(character, tile))
                {
                    return;
                }

                character++;
                tile++;
            }
        }

        public void MapTheseCharacters(string text, int tile)
        {
            foreach (var character in text)
            {
                if (mapCharacter(character, tile))
                {
                    return;
                }

                tile++;
            }
        }

        public void Palette(int index, uint color)
        {
            if (index < 0 || index > 255)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            palette[index] = color;
        }

        public override void Print(int x, int y, string caption)
        {
            bitmap.SwapBuffers();

            Sprockets.Graphics.Print(displayList, x, y, caption);
        }

        public void Scroll(int character, ScrollDirection direction, int count = 1, bool wrap = false)
        {
            if (character < 0 || character > 255 || count < 1 || count >= tileWidth)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            if (!isAvailable[character])
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            var x = x1[character];
            var y = y1[character];

            brush.Scroll(x, y, x + tileWidth - 1, y + tileHeight - 1, direction, count, wrap);
        }

        public override void Destructor()
        {
            Sprockets.Graphics.FreeLists(displayList, 256);
            displayList = 0;

            base.Destructor();
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            objectManager.Mark(brushId);
            brush = null;

            x1 = null;
            y1 = null;
            palette = null;

            base.shutdown();
        }
    }
}
