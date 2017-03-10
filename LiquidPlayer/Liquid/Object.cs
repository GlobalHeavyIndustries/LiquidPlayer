using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Object : IDisposable
    {
        //private static List<Object> inUse = new List<Object>();

        #region Dispose pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources here
                //inUse.Remove(this);
            }

            // free unmanaged resources here

            disposed = true;
        }
        #endregion

        protected int objectId;
        protected Exec.ObjectManager objectManager;
        protected int[] dataSpace;

        protected int timeStamp;
        protected bool enabled;

        public int TimeStamp
        {
            get
            {
                return timeStamp;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        public Object(int id)
        {
            this.objectId = id;
            this.objectManager = LiquidPlayer.Program.Exec.ObjectManager;
            this.dataSpace = objectManager[id].DataSpace;

            this.timeStamp = LiquidPlayer.Program.SystemClock;
            this.enabled = true;

            //inUse.Add(this);
        }

        public override string ToString()
        {
            return $"Object {objectId}";
        }

        public virtual void Constructor()
        {

        }

        public virtual int Clone()
        {
            return objectManager.Copy(objectId);
        }

        public virtual int Compare(int rhs)
        {
            var lhs = objectId;

            if (lhs < rhs)
            {
                return -1;
            }
            else if (lhs > rhs)
            {
                return 1;
            }

            return 0;
        }

        protected virtual bool callback(int messageId)
        {
            return false;
        }

        public bool Callback(LiquidClass liquidClass, int messageId)
        {
            var address = LiquidPlayer.Program.ClassManager[liquidClass].Callback;

            if (address != -1)
            {
                var stack = new int[LiquidPlayer.Program.VM_STACK_SIZE];

                stack[1] = objectManager.Copy(messageId);

                var a0 = LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId, address, stack, 1);

                objectManager.Mark(stack[1]);

                stack = null;

                return (a0 == 1) ? true : false;
            }
            else
            {
                return callback(messageId);
            }
        }

        public int GetParent()
        {
            var parentId = objectManager[objectId].ParentId;

            return objectManager.Copy(parentId);
        }

        public int GetParentTask()
        {
            var parentId = objectManager[objectId].ParentId;

            var taskId = (parentId == 0) ? objectManager[objectId].TaskId : objectManager[parentId].TaskId; 

            return objectManager.Copy(taskId);
        }

        public bool IsA(LiquidClass baseLiquidClass)
        {
            var liquidClass = objectManager[objectId].LiquidClass;

            return LiquidPlayer.Program.ClassManager.IsA(liquidClass, baseLiquidClass);
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public static int NewLong(int id, long data = 0L)
        {
            return LiquidPlayer.Program.Exec.LongManager.New(id, data);
        }

        public static long GetLong(int id, int index)
        {
            return LiquidPlayer.Program.Exec.LongManager[index];
        }

        public static void FreeLong(int id, int index)
        {
            LiquidPlayer.Program.Exec.LongManager.Free(index);
        }

        public static int NewDouble(int id, double data = 0d)
        {
            return LiquidPlayer.Program.Exec.DoubleManager.New(id, data);
        }

        public static double GetDouble(int id, int index)
        {
            return LiquidPlayer.Program.Exec.DoubleManager[index];
        }

        public static void FreeDouble(int id, int index)
        {
            LiquidPlayer.Program.Exec.DoubleManager.Free(index);
        }

        public static int NewString(int id, string data = "")
        {
            return LiquidPlayer.Program.Exec.StringManager.New(id, data);
        }

        public static string GetString(int id, int index)
        {
            return LiquidPlayer.Program.Exec.StringManager[index];
        }

        public static void FreeString(int id, int index)
        {
            LiquidPlayer.Program.Exec.StringManager.Free(index);
        }

        public bool IsError()
        {
            var task = objectManager.GetTask(objectId);

            return (task.ThrowCode != ExceptionCode.None) ? true : false;
        }

        public void Throw(ExceptionCode code, string data = "")
        {
            var task = objectManager.GetTask(objectId);

            task.Raise(code, data);
        }

        public virtual void Destructor()
        {

        }

        public virtual void shutdown()
        {
            dataSpace = null;
        }

        public void Shutdown(LiquidClass liquidClass)
        {
            var address = LiquidPlayer.Program.ClassManager[liquidClass].Shutdown;

            if (address != -1)
            {
                LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId, address);
            }
            else
            {
                shutdown();
            }

            List<int> list;

            list = LiquidPlayer.Program.Exec.GetLeftoverLongs(objectId);

            System.Diagnostics.Debug.Assert(list.Count == 0);

            list = LiquidPlayer.Program.Exec.GetLeftoverDoubles(objectId);

            System.Diagnostics.Debug.Assert(list.Count == 0);

            list = LiquidPlayer.Program.Exec.GetLeftoverStrings(objectId);

            System.Diagnostics.Debug.Assert(list.Count == 0);
        }
    }
}
