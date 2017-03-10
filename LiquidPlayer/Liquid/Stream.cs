using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Stream : Object
    {
        public const int END_OF_STREAM = -1;

        protected bool isOpen;
        protected bool isBlocked;

        protected bool canRead;
        protected bool canSeek;
        protected bool canWrite;

        protected int length;
        protected int position;

        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
        }

        public bool IsBlocked
        {
            get
            {
                return isBlocked;
            }
        }

        public bool CanRead
        {
            get
            {
                return canRead;
            }
        }

        public bool CanSeek
        {
            get
            {
                return canSeek;
            }
        }

        public bool CanWrite
        {
            get
            {
                return canWrite;
            }
        }

        public static int NewStream(int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Stream);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Stream(id);

            return id;
        }

        public Stream(int id)
            : base(id)
        {
            this.isOpen = false;
            this.isBlocked = false;

            this.canRead = false;
            this.canSeek = false;
            this.canWrite = false;

            this.length = 0;
            this.position = 0;
        }

        public override string ToString()
        {
            return $"Stream (Position: {position}, Length: {length})";
        }

        public virtual void Close()
        {
            Flush();

            isOpen = false;
        }

        public virtual void End()
        {

        }

        public virtual bool EndOfStream()
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return false;
            }

            return false;
        }

        public virtual void Flush()
        {
            isBlocked = false;
        }

        public int GetLength()
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return 0;
            }

            return length;
        }


        public int GetPosition()
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return 0;
            }

            if (!canSeek)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            return position;
        }

        public virtual void Open()
        {
            if (isOpen)
            {
                Throw(ExceptionCode.StreamOpen);
                return;
            }

            isOpen = true;
        }

        public virtual int Peek()
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return 0;
            }

            if (!canRead)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            if (!canSeek)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            return 0;
        }

        public virtual int Read()
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return 0;
            }

            if (!canRead)
            {
                Throw(ExceptionCode.Denied);
                return 0;
            }

            return 0;
        }

        public virtual string Read(int count)
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return "";
            }

            if (!canRead)
            {
                Throw(ExceptionCode.Denied);
                return "";
            }

            if (count <= 0)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return "";
            }

            return "";
        }

        public virtual void Seek(int position)
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return;
            }

            if (!canSeek)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            if (position < 0 || position > length)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            this.position = position;
        }

        public virtual void SetLength(int length)
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return;
            }

            if (length < 0)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            this.length = length;
        }

        public virtual void Write(string data)
        {
            if (!isOpen)
            {
                Throw(ExceptionCode.StreamNotOpen);
                return;
            }

            if (!canWrite)
            {
                Throw(ExceptionCode.Denied);
                return;
            }
        }

        public override void Destructor()
        {
            Close();
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
