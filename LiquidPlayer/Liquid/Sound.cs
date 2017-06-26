using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Media;

namespace LiquidPlayer.Liquid
{
    public class Sound : Audio
    {
        protected string path;

        protected SoundPlayer soundPlayer;

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
            if (soundPlayer != null)
            {
                soundPlayer.Dispose();
                soundPlayer = null;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        public static int NewSound(string path, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Sound);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Sound(id, path);

            return id;
        }

        public Sound(int id, string path)
            : base(id)
        {
            // check file exists, throw exception if not (see Image)

            this.path = path;

            this.soundPlayer = new SoundPlayer(path);
        }

        public override string ToString()
        {
            return $"Sound (Path: \"{path}\")";
        }

        public void Play()
        {
            soundPlayer.Play();
        }

        public void Loop()
        {
            soundPlayer.PlayLooping();
        }

        public void Stop()
        {
            soundPlayer.Stop();
        }

        public override void shutdown()
        {
            soundPlayer = null;

            base.shutdown();
        }
    }
}
