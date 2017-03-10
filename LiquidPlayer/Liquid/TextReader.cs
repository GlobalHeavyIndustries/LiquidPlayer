using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class TextReader : Object
    {
        protected int streamId;
        protected Stream stream;

        public bool IsBlocked
        {
            get
            {
                return stream.IsBlocked;
            }
        }

        public static int NewTextReader(int streamId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.TextReader);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new TextReader(id, streamId);

            return id;
        }

        public TextReader(int id, int streamId)
            : base(id)
        {
            if (streamId == 0)
            {
                Throw(ExceptionCode.NullObject);
                return;
            }

            this.streamId = objectManager.Copy(streamId);
            this.stream = objectManager[streamId].LiquidObject as Stream;

            if (!stream.IsOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return;
            }

            if (!stream.CanRead)
            {
                Throw(ExceptionCode.Denied);
                return;
            }
        }

        public override string ToString()
        {
            return $"TextReader (Stream: {streamId})";
        }

        public bool EndOfStream()
        {
            return stream.EndOfStream();
        }

        public int GetLength()
        {
            return stream.GetLength();
        }

        public string Read()
        {
            var data = new StringBuilder();

            var b = stream.Read();

            data.Append((char)b);

            return data.ToString();
        }

        public string ReadLine()
        {
            var data = new StringBuilder();

            var b = stream.Read();

            while (!stream.IsBlocked && b != Stream.END_OF_STREAM && b != 10)
            {
                if (b != 13)
                {
                    data.Append((char)b);
                }
                
                b = stream.Read();
            }

            return data.ToString();
        }

        public override void shutdown()
        {
            objectManager.Mark(streamId);
            stream = null;

            base.shutdown();
        }
    }
}
