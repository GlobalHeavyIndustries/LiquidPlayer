using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Pipe : Stream
    {
        protected Queue<int> buffer;

        public static int NewPipe(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Pipe);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Pipe(id);

            return id;
        }

        public Pipe(int id)
            : base(id)
        {
            this.canRead = true;
            this.canSeek = false; 
            this.canWrite = true;

            this.buffer = new Queue<int>();

            this.length = 0;
            this.position = 0;
        }

        public override string ToString()
        {
            return $"Pipe (Length: {length})";
        }

        public override void End()
        {
            buffer.Enqueue(END_OF_STREAM);
        }

        public override bool EndOfStream()
        {
            base.EndOfStream();

            if (IsErrorRaised())
            {
                return false;
            }

            if (buffer.Count == 0)
            {
                isBlocked = true;

                return false;
            }

            isBlocked = false;

            return (buffer.Peek() == END_OF_STREAM);
        }

        public override int Peek()
        {
            base.Peek();

            if (IsErrorRaised())
            {
                return 0;
            }

            if (buffer.Count == 0)
            {
                isBlocked = true;

                return 0;
            }

            isBlocked = false;

            return buffer.Peek();
        }

        public override int Read()
        {
            base.Read();

            if (IsErrorRaised())
            {
                return 0;
            }

            if (buffer.Count == 0)
            {
                isBlocked = true;

                return 0;
            }

            isBlocked = false;

            if (buffer.Peek() == END_OF_STREAM)
            {
                return END_OF_STREAM;
            }

            var data = buffer.Dequeue();

            length = buffer.Count;

            return data;
        }

        public override string Read(int count)
        {
            base.Read(count);

            if (IsErrorRaised())
            {
                return "";
            }

            var data = new StringBuilder();

            var b = Read();

            while (!isBlocked && b != END_OF_STREAM && count > 0)
            {
                data.Append((char)b);

                count--;

                b = Read();
            }

            return data.ToString();
        }

        public override void Write(string data)
        {
            base.Write(data);

            if (IsErrorRaised())
            {
                return;
            }

            var bytes = Encoding.ASCII.GetBytes(data);

            foreach (var b in bytes)
            {
                buffer.Enqueue(b);
            }

            length = buffer.Count;
        }

        public override void shutdown()
        {
            buffer = null;

            base.shutdown();
        }
    }
}
