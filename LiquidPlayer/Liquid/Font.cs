using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Font : Object
    {
        protected string name;
        protected bool isLoaded;
        protected bool[] isAvailable;
        protected bool isFixedWidth;
        protected int[] width;
        protected int height;

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

        public static int NewFont(string name, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Font);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Font(id, name);

            return id;
        }

        public Font(int id, string name)
            : base(id)
        {
            this.name = name;
            this.isLoaded = false;
            this.isAvailable = new bool[256];
            this.isFixedWidth = false;
            this.width = new int[256];
            this.height = 0;
        }

        public override string ToString()
        {
            return $"Font (Name: \"{name}\")";
        }

        public int GetWidth(byte character)
        {
            if (!isLoaded || !isAvailable[character])
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            return width[character];
        }

        public int GetWidth(string text)
        {
            if (!isLoaded)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            var textWidth = 0;

            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];

                if (!isAvailable[character])
                {
                    Throw(ExceptionCode.Denied);
                    return 0;
                }

                textWidth += width[character];
            }

            return textWidth;
        }

        public int GetHeight()
        {
            if (!isLoaded)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            return height;
        }

        //public virtual void Load(string filename)
        //{

        //}

        public virtual void Print(int x, int y, string caption)
        {
            Throw(ExceptionCode.NotImplemented);
        }

        //public virtual void Unload()
        //{

        //}

        public override void shutdown()
        {
            isAvailable = null;
            width = null;

            base.shutdown();
        }
    }
}
