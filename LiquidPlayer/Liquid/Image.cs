using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LiquidPlayer.Liquid
{
    public class Image : Bitmap
    {
        protected string fileName;

        public static int NewImage(string filename, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Image);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Image(id, filename);

            return id;
        }

        public Image(int id, string fileName)
            : base(id)
        {
            // Load a BMP, GIF, EXIF, JPG, PNG, or TIFF image file

            if (string.IsNullOrEmpty(fileName))
            {
                Throw(ExceptionCode.FileNotFound);
                return;
            }

            var path = Directory.GetCurrentDirectory() + @"\" + fileName;

            if (!System.IO.File.Exists(path))
            {
                Throw(ExceptionCode.FileNotFound, path);
                return;
            }

            var width = 0;
            var height = 0;

            var data = Sprockets.Graphics.LoadImage(fileName, out width, out height);

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

            this.fileName = path;
        }

        public override string ToString()
        {
            return $"Image (Filename: \"{fileName}\")";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}