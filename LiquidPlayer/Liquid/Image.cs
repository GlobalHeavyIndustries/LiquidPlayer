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
        protected string path;

        public static int NewImage(string path, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Image);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Image(id, path);

            return id;
        }

        public Image(int id, string path)
            : base(id)
        {
            // Load a BMP, GIF, EXIF, JPG, PNG, or TIFF image file

            var resolvedPath = Util.FindFile(path, LiquidPlayer.Program.SharedPath);

            if (resolvedPath == "")
            {
                RaiseError(ErrorCode.FileNotFound, Path.GetFileName(path));
                return;
            }

            var width = 0;
            var height = 0;

            var data = Sprockets.Graphics.LoadImage(resolvedPath, out width, out height);

            if (data == null)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            this.width = width;
            this.height = height;
            this.size = width * height;

            this.topX = width - 1;
            this.topY = height - 1;

            this.data = data;
            this.inSync = false;
            this.doubleBuffered = false;

            this.handle = Sprockets.Graphics.GenTexture();
            Sprockets.Graphics.BindTexture(handle, width, height, data);
            this.inSync = true;

            this.path = resolvedPath;
        }

        public override string ToString()
        {
            return $"Image (Path: \"{path}\", Resolution: {width}x{height})";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}