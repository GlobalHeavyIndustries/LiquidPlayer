using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class DataStream : Stream
    {
        public const int BUFFER_SIZE = 4096;

        protected int capacity;
        protected byte[] buffer;

        public static int NewDataStream(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.DataStream);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new DataStream(id);

            return id;
        }

        public DataStream(int id)
            : base(id)
        {
            this.canRead = true;
            this.canSeek = true;
            this.canWrite = true;

            this.capacity = BUFFER_SIZE;
            this.buffer = new byte[capacity];

            this.length = capacity;
            this.position = 0;
        }

        public override string ToString()
        {
            return $"DataStream (Position: {position}, Length: {length})";
        }

        public override bool EndOfStream()
        {
            base.EndOfStream();

            if (IsErrorRaised())
            {
                return false;
            }

            return (position >= length);
        }

        public override int Peek()
        {
            base.Peek();

            if (IsErrorRaised())
            {
                return 0;
            }

            if (position >= length)
            {
                return END_OF_STREAM;
            }

            return buffer[position];
        }

        public override int Read()
        {
            base.Read();

            if (IsErrorRaised())
            {
                return 0;
            }

            if (position >= length)
            {
                return END_OF_STREAM;
            }

            return buffer[position++];
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

            while (b != END_OF_STREAM && count > 0)
            {
                data.Append((char)b);

                count--;

                b = Read();
            }

            return data.ToString();
        }

        public override void SetLength(int length)
        {
            base.SetLength(length);

            if (IsErrorRaised())
            {
                return;
            }

            var blocks = (length / BUFFER_SIZE) + 1;

            capacity = blocks * BUFFER_SIZE;

            System.Array.Resize(ref buffer, capacity);
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
                buffer[position++] = b;

                if (position >= capacity)
                {
                    capacity += BUFFER_SIZE;

                    System.Array.Resize(ref buffer, capacity);
                }
            }

            if (position > length)
            {
                length = position;
            }
        }

        public override void shutdown()
        {
            buffer = null;

            base.shutdown();
        }
    }
}
