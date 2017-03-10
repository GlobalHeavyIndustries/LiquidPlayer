using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Exception : Object
    {
        protected string filename;
        protected int lineNumber;
        protected ExceptionCode code;
        protected string data;

        public string Filename
        {
            get
            {
                return filename;
            }
        }

        public int LineNumber
        {
            get
            {
                return lineNumber;
            }
        }

        public ExceptionCode Code
        {
            get
            {
                return code;
            }
        }

        public string Data
        {
            get
            {
                return data;
            }
        }

        public static int NewException(string filename, int lineNumber, ExceptionCode code, string data, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Exception);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Exception(id, filename, lineNumber, code, data);

            return id;
        }

        public Exception(int id, string filename, int lineNumber, ExceptionCode code, string data)
            : base(id)
        {
            this.filename = filename;
            this.lineNumber = lineNumber;
            this.code = code;
            this.data = data;
        }

        public override string ToString()
        {
            return $"Exception (Code: {code})";
        }

        public static bool IsFatal(ExceptionCode code)
        {
            if (code == ExceptionCode.InternalError ||
                code == ExceptionCode.OutOfMemory ||
                code == ExceptionCode.Timeout ||
                code == ExceptionCode.StackOverflow)
            {
                return true;
            }

            return false;
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
