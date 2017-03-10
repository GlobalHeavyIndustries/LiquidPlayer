using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Bitmap : Object
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
            if (handle != 0)
            {
                Sprockets.Graphics.FreeTexture(handle);
                handle = 0;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        protected int handle;

        protected int width;
        protected int height;
        protected int size;

        protected uint[] data;
        protected bool inSync;
        protected bool doubleBuffered;

        public int Handle
        {
            get
            {
                return handle;
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

        public int Size
        {
            get
            {
                return size;
            }
        }

        public static int NewBitmap(int width, int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Bitmap);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Bitmap(id, width, height);

            return id;
        }

        public Bitmap(int id)
            : base(id)
        {
            this.handle = 0;

            this.width = 0;
            this.height = 0;
            this.size = 0;

            this.data = null;
            this.inSync = false;
            this.doubleBuffered = false;
        }

        public Bitmap(int id, int width, int height)
            : base(id)
        {
            if (width < 1 || width > Sprockets.Graphics.MaxTextureSize || height < 1 || height > Sprockets.Graphics.MaxTextureSize)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            this.width = width;
            this.height = height;
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
            return $"Bitmap (Resolution: {width}x{height})";
        }

        public void SingleBuffer()
        {
            doubleBuffered = false;

            SwapBuffers();
        }

        public void DoubleBuffer()
        {
            doubleBuffered = true;

            SwapBuffers();
        }

        public void Smooth(bool linearFilter)
        {
            Sprockets.Graphics.BindTexture(handle, width, height, data, linearFilter);
        }

        public void Clear()
        {
            data.Fill(0u);

            inSync = false;
        }

        public void FastPoke(int index, uint color)
        {
            data[index] = color;

            inSync = false;
        }

        public uint FastPeek(int index)
        {
            return data[index];
        }

        public void Poke(int index, uint color)
        {
            if (index < 0 || index >= size)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            data[index] = color;

            inSync = false;
        }

        public uint Peek(int index)
        {
            if (index < 0 || index >= size)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return 0;
            }

            return data[index];
        }

        public void Load(uint[] data)
        {
            if (data.Length != this.data.Length)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            this.data = data;

            inSync = false;
        }

        public uint[] Save()
        {
            return (uint[])data.Clone();
        }

        public Sprockets.Color.RGBA[] CastToRGBA()
        {
            var array = new Sprockets.Color.RGBA[size];

            for (var i = 0; i < size; i++)
            {
                array[i].Uint = data[i];
            }

            return array;
        }

        public void SwapBuffers()
        {
            if (!inSync)
            {
                Sprockets.Graphics.UpdateTexture(handle, width, height, data);

                inSync = true;
            }

            if (doubleBuffered)
            {
                Clear();
            }
        }

        public void Render()
        {
            Render(0, 0);
        }

        public void Render(int x, int y)
        {
            var x1 = x - (width / 2);
            var y1 = y - (height / 2);

            Render(x1, y1, x1 + width - 1, y1 + height - 1);
        }

        public void Render(int x1, int y1, int x2, int y2)
        {
            if (!doubleBuffered && !inSync)
            {
                Sprockets.Graphics.UpdateTexture(handle, width, height, data);

                inSync = true;
            }

            Sprockets.Graphics.RenderTexture(handle, x1, y1, x2, y2);
        }

        public override void Destructor()
        {
            Sprockets.Graphics.FreeTexture(handle);
            handle = 0;

            base.Destructor();
        }

        public override void shutdown()
        {
            data = null;

            base.shutdown();
        }
    }
}
