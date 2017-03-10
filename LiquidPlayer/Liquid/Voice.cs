using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Speech.Synthesis;

namespace LiquidPlayer.Liquid
{
    public class Voice : Audio
    {
        protected static SpeechSynthesizer speechSynthesizer;

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
            if (speechSynthesizer != null)
            {
                speechSynthesizer.Dispose();
                speechSynthesizer = null;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion

        public static int NewVoice(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Voice);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Voice(id);

            return id;
        }

        public Voice(int id)
            : base(id)
        {
            if (speechSynthesizer == null)
            {
                speechSynthesizer = new SpeechSynthesizer();

                speechSynthesizer.SetOutputToDefaultAudioDevice();
            }
        }

        public override string ToString()
        {
            return $"Voice";
        }

        public void Speak(string textToSpeak)
        {
            speechSynthesizer.Speak(textToSpeak);
        }

        public void SpeakAsync(string textToSpeak)
        {
            speechSynthesizer.SpeakAsync(textToSpeak);
        }

        public override void shutdown()
        {
            speechSynthesizer = null;

            base.shutdown();
        }
    }
}
