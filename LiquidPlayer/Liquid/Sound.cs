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
        protected string filename;

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

        public static int NewSound(string filename, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Sound);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Sound(id, filename);

            return id;
        }

        public Sound(int id, string filename)
            : base(id)
        {
            // check file exists, throw exception if not (see Image)

            this.filename = filename;

            this.soundPlayer = new SoundPlayer(filename);
        }

        public override string ToString()
        {
            return $"Sound (Filename: \"{filename}\")";
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
