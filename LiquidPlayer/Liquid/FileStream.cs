using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class FileStream : Stream
    {
        protected string fileName;
        protected System.IO.FileStream fileStream;

        public static int NewFileStream(string fileName, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.FileStream);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new FileStream(id, fileName);

            return id;
        }

        public FileStream(int id, string fileName)
            : base(id)
        {
            this.canRead = true;
            this.canSeek = true;
            this.canWrite = true;

            this.fileName = fileName;
            this.fileStream = null;

            this.length = 0;
            this.position = 0;
        }

        public override string ToString()
        {
            return $"FileStream (Filename: \"{fileName}\", Position: {position}, Length: {length})";
        }

        public override void Close()
        {
            base.Close();

            if (IsError())
            {
                return;
            }

            if (fileStream != null)
            {
                fileStream.Close();

                fileStream = null;
            }
        }

        public override bool EndOfStream()
        {
            base.EndOfStream();

            if (IsError())
            {
                return false;
            }

            return (position >= length) ? true : false;
        }

        public override void Flush()
        {
            base.Flush();

            if (IsError())
            {
                return;
            }

            if (fileStream != null)
            {
                fileStream.Flush();
            }
        }

        public override void Open()
        {
            base.Open();

            if (IsError())
            {
                return;
            }

            fileStream = System.IO.File.Open(fileName, System.IO.FileMode.OpenOrCreate);

            length = (int)fileStream.Length;

            position = (int)fileStream.Position;
        }

        public override int Read()
        {
            base.Read();

            if (IsError())
            {
                return 0;
            }

            if (position >= length)
            {
                return END_OF_STREAM;
            }

            var data = fileStream.ReadByte();

            position = (int)fileStream.Position;

            return data;
        }

        public override string Read(int count)
        {
            base.Read(count);

            if (IsError())
            {
                return "";
            }

            if (position >= length)
            {
                return "";
            }

            var bytes = new byte[count];

            var bytesRead = fileStream.Read(bytes, 0, count);

            return Encoding.UTF8.GetString(bytes);
        }

        public override void Seek(int position)
        {
            base.Seek(position);

            if (IsError())
            {
                return;
            }

            fileStream.Seek(position, System.IO.SeekOrigin.Begin);
        }

        public override void SetLength(int length)
        {
            base.SetLength(length);

            if (IsError())
            {
                return;
            }

            fileStream.SetLength(length);
        }

        public override void Write(string data)
        {
            base.Write(data);

            if (IsError())
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(data);

            fileStream.Write(bytes, 0, bytes.Length);

            length = (int)fileStream.Length;

            position = (int)fileStream.Position;
        }

        public override void shutdown()
        {
            fileStream = null;

            base.shutdown();
        }
    }
}
