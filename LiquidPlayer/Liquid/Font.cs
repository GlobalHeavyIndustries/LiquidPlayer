using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LiquidPlayer.Liquid
{
    public class Font : Object
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
            if (characters != null)
            {
                foreach (var ch in characters)
                {
                    var handle = characters[ch.Key].TextureId;

                    if (handle != 0)
                    {
                        Sprockets.Graphics.FreeTexture(handle);
                    }
                }
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        protected string path;

        protected bool isLoaded;
        protected bool[] isAvailable;
        protected bool isFixedWidth;
        protected int[] width;
        protected int height;

        protected Dictionary<int, Sprockets.Graphics.Character> characters;

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

        public bool IsFixedWidth
        {
            get
            {
                return isFixedWidth;
            }
        }

        public int[] Width
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

        public static int NewFont(string path, int fontSize, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Font);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Font(id, path, fontSize);

            return id;
        }

        public Font(int id, string path, int fontSize)
            : base(id)
        {
            var resolvedPath = Util.FindFile(path, LiquidPlayer.Program.SharedPath);

            if (resolvedPath == "")
            {
                RaiseError(ErrorCode.FileNotFound, Path.GetFileName(path));
                return;
            }

            this.path = resolvedPath;

            this.isLoaded = false;
            this.isAvailable = new bool[256];
            this.isFixedWidth = false;
            this.width = new int[256];
            this.height = 0;

            Sprockets.Graphics.BuildFont(resolvedPath, fontSize, false, out characters);

            this.isLoaded = true;

            for (var ch = 32; ch < 128; ch++)
            {
                this.isAvailable[ch] = true;
            }

            for (var ch = 32; ch < 128; ch++)
            {
                this.width[ch] = characters[ch].Advance;
            }

            var slice = width.Skip(33).Take(95);

            var min = slice.Min();
            var max = slice.Max();

            this.isFixedWidth = min == max;

            this.height = characters['H'].Size.Y;

            //var height = 0;

            //for (var ch = 32; ch < 128; ch++)
            //{
            //    if (characters[ch].Size.Y > height)
            //    {
            //        height = characters[ch].Size.Y;
            //    }
            //}

            //this.height = height;
        }

        public override string ToString()
        {
            return $"Font (Path: \"{path}\")";
        }

        public int GetHeight()
        {
            if (!isLoaded)
            {
                RaiseError(ErrorCode.Denied);
                return 0;
            }

            return height;
        }

        public int GetWidth(byte character)
        {
            if (!isLoaded || !isAvailable[character])
            {
                RaiseError(ErrorCode.Denied);
                return 0;
            }

            return width[character];
        }

        public int GetWidth(string text)
        {
            if (!isLoaded)
            {
                RaiseError(ErrorCode.Denied);
                return 0;
            }

            var textWidth = 0;

            foreach (var c in text)
            {
                if (!isAvailable[c])
                {
                    RaiseError(ErrorCode.Denied);
                    return 0;
                }

                textWidth += width[c];
            }

            return textWidth;
        }

        //public virtual void Load(string path)
        //{

        //}

        public virtual void Print(int x, int y, string text)
        {
            Sprockets.Graphics.RenderText(characters, x, y, text);
        }

        //public virtual void Save(string path)
        //{

        //}

        public override void Destructor()
        {
            if (characters != null)
            {
                foreach (var ch in characters)
                {
                    var handle = characters[ch.Key].TextureId;

                    if (handle != 0)
                    {
                        Sprockets.Graphics.FreeTexture(handle);
                    }
                }
            }

            base.Destructor();
        }

        public override void shutdown()
        {
            isAvailable = null;
            width = null;
            characters = null;

            base.shutdown();
        }
    }
}
