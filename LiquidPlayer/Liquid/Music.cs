using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WMPLib;

namespace LiquidPlayer.Liquid
{
    public class Music : Audio
    {
        protected string path;

        protected WindowsMediaPlayer windowsMediaPlayer;

        public static int NewMusic(string path, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Music);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Music(id, path);

            return id;
        }

        public Music(int id, string path)
            : base(id)
        {
            // check file exists, throw exception if not (see Image)

            this.path = path;

            this.windowsMediaPlayer = new WindowsMediaPlayer();
            this.windowsMediaPlayer.URL = path;
        }

        public override string ToString()
        {
            return $"Music (Path: \"{path}\")";
        }

        public void Play()
        {
            windowsMediaPlayer.controls.play();
        }

        public void Stop()
        {
            windowsMediaPlayer.controls.stop();
        }

        public override void shutdown()
        {
            windowsMediaPlayer = null;

            base.shutdown();
        }
    }
}
