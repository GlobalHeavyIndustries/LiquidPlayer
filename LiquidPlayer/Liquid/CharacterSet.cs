using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class CharacterSet : Object
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

        protected string name;
        protected bool isLoaded;
        protected bool[] isAvailable;

        protected int bitmapId;
        protected Bitmap bitmap;

        protected int rasterId;
        protected Raster raster;

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

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
        }

        public bool[] IsAvailable
        {
            get
            {
                return isAvailable;
            }
        }

        public int BitmapId
        {
            get
            {
                return bitmapId;
            }
        }

        public int RasterId
        {
            get
            {
                return rasterId;
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
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new CharacterSet(id, bitmapId, tileWidth, tileHeight, glyphWidth, glyphHeight);

            return id;
        }

        public CharacterSet(int id, int bitmapId, int tileWidth = 0, int tileHeight = 0, int glyphWidth = 0, int glyphHeight = 0)
            : base(id)
        {
            this.isLoaded = false;
            this.isAvailable = new bool[256];

            if (bitmapId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            tileWidth = (tileWidth == 0) ? bitmap.Width / 16 : tileWidth;
            tileHeight = (tileHeight == 0) ? bitmap.Height / 16 : tileHeight;
            glyphWidth = (glyphWidth == 0) ? tileWidth : glyphWidth;
            glyphHeight = (glyphHeight == 0) ? tileHeight : glyphHeight;

            var rasterId = Raster.NewRaster(bitmapId, id);

            if (rasterId == 0)
            {
                RaiseError(ErrorCode.OutOfMemory);
                return;
            }

            this.rasterId = rasterId;
            this.raster = objectManager[rasterId].LiquidObject as Raster;

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

        public void Clear(byte character, uint color = 0)
        {
            if (!isLoaded || !isAvailable[character])
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            var x1 = this.x1[character];
            var y1 = this.y1[character];

            for (var y = y1; y < y1 + tileHeight; y++)
            {
                for (var x = x1; x < x1 + tileWidth; x++)
                {
                    raster.Plot(x, y, color);
                }
            }
        }

        public void CustomCharacter(byte character, int scanLine, string data)
        {
            if (!isLoaded || !isAvailable[character])
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            if (scanLine < 0 || scanLine >= tileHeight || data.Length > tileWidth)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            var x = x1[character];
            var y = y1[character] + scanLine;

            foreach(var ch in data)
            {
                raster.Plot(x, y, palette[ch]);
                x++;
            }
        }

        public int GetHeight()
        {
            if (!isLoaded)
            {
                RaiseError(ErrorCode.Denied);
                return 0;
            }

            return glyphHeight;
        }

        public int GetWidth()
        {
            return glyphWidth;
        }

        public int GetWidth(string text)
        {
            if (!isLoaded)
            {
                RaiseError(ErrorCode.Denied);
                return 0;
            }

            var textWidth = 0;

            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];

                if (!isAvailable[character])
                {
                    RaiseError(ErrorCode.Denied);
                    return 0;
                }

                textWidth += glyphWidth;
            }

            return textWidth;
        }

        private bool mapCharacter(byte character, int tile)
        {
            if (!isLoaded)
            {
                RaiseError(ErrorCode.Denied);
                return false;
            }

            if (tile < 0 || tile > maxTileCount)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return false;
            }

            var x1 = (tile % tilesAcross) * tileWidth;
            var x2 = x1 + tileWidth;
            var y1 = (tile / tilesAcross) * tileHeight;
            var y2 = y1 + tileHeight;

            Sprockets.Graphics.CreateTile(displayList + character, bitmap.Handle, x1, y1, x2, y2, bitmap.Width, bitmap.Height, glyphWidth, glyphHeight);

            isAvailable[character] = true;

            this.x1[character] = x1;
            this.y1[character] = y1;

            return true;
        }

        public void MapAll()
        {
            var tile = 0;

            for (var character = 0; character < 256; character++)
            {
                if (!mapCharacter((byte)character, tile))
                {
                    return;
                }

                tile++;
            }
        }

        public void MapCharacter(byte character, int tile)
        {
            if (!mapCharacter(character, tile))
            {
                return;
            }
        }

        public void MapCharacters(byte character, int tile, int count)
        {
            for (var index = 1; index <= count; index++)
            {
                if (!mapCharacter(character, tile))
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
                if (mapCharacter((byte)character, tile))
                {
                    return;
                }

                tile++;
            }
        }

        public void Palette(byte index, uint color)
        {
            palette[index] = color;
        }

        public virtual void Print(int x, int y, byte character)
        {
            bitmap.SwapBuffers();

            Sprockets.Graphics.PrintLists(displayList, x, y, character);
        }

        public virtual void Print(int x, int y, string caption)
        {
            bitmap.SwapBuffers();

            Sprockets.Graphics.PrintLists(displayList, x, y, caption);
        }

        public void Scroll(byte character, ScrollDirection direction, int count = 1, bool wrap = false)
        {
            if (!isLoaded || !isAvailable[character])
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            if (count < 1 || count >= tileWidth)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            var x = x1[character];
            var y = y1[character];

            raster.Scroll(x, y, x + tileWidth - 1, y + tileHeight - 1, direction, count, wrap);
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

            objectManager.Mark(rasterId);
            raster = null;

            isAvailable = null;
            x1 = null;
            y1 = null;
            palette = null;

            base.shutdown();
        }
    }
}
