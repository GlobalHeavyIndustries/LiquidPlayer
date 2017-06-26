using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class TextWriter : Object
    {
        protected int streamId;
        protected Stream stream;

        public static int NewTextWriter(int streamId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.TextWriter);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new TextWriter(id, streamId);

            return id;
        }

        public TextWriter(int id, int streamId)
            : base(id)
        {
            if (streamId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.streamId = objectManager.Copy(streamId);
            this.stream = objectManager[streamId].LiquidObject as Stream;

            if (!stream.IsOpen)
            {
                RaiseError(ErrorCode.StreamNotOpen);
                return;
            }

            if (!stream.CanWrite)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }
        }

        public override string ToString()
        {
            return $"TextWriter (Stream: {streamId})";
        }

        public void WriteLine()
        {
            stream.Write(Environment.NewLine);
        }

        public void WriteLine(string data)
        {
            stream.Write(data + Environment.NewLine);
        }

        public override void shutdown()
        {
            objectManager.Mark(streamId);
            stream = null;

            base.shutdown();
        }
    }
}
