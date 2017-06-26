using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Diagnostics;

namespace LiquidPlayer.Exec
{
    public class Object
    {
        public int TaskId
        {
            get;
            set;
        }

        public int ParentId
        {
            get;
            set;
        }

        public LiquidClass LiquidClass
        {
            get;
            set;
        }

        public int RefCount
        {
            get;
            set;
        }

        public int[] DataSpace
        {
            get;
            set;
        }

        public Liquid.Object LiquidObject
        {
            get;
            set;
        }
    }

    public class ObjectManager
    {
        private DSL.FreeList<Object> bag = new DSL.FreeList<Object>();

        private int systemCharacterSetBitmapId;
        private int systemCharacterSetId;

        public int Count
        {
            get
            {
                return bag.Count;
            }
        }

        public int Focus
        {
            get;
            set;
        }

        public int SystemCharacterSetId
        {
            get
            {
                return systemCharacterSetId;
            }
        }

        public void LoadDefaultResources()
        {
            systemCharacterSetBitmapId = Liquid.Image.NewImage(@"Data\Font.bmp");

            var systemCharacterSetBitmap = bag[systemCharacterSetBitmapId].LiquidObject as Liquid.Bitmap;

            var filterId = Liquid.Filter.NewFilter(systemCharacterSetBitmapId);

            var filter = bag[filterId].LiquidObject as Liquid.Filter;

            filter.ReplaceAlpha(0xFF000000, 0);

            Mark(filterId);

            systemCharacterSetBitmap.SwapBuffers();

            systemCharacterSetId = Liquid.CharacterSet.NewCharacterSet(systemCharacterSetBitmapId, 8, 16, 8, 16);

            var systemCharacterSet = bag[systemCharacterSetId].LiquidObject as Liquid.CharacterSet;

            systemCharacterSet.MapCharacters(32, 32, 16);
            systemCharacterSet.MapCharacters(48, 48, 16);
            systemCharacterSet.MapCharacters(64, 64, 16);
            systemCharacterSet.MapCharacters(80, 80, 16);
            systemCharacterSet.MapCharacters(96, 96, 16);
            systemCharacterSet.MapCharacters(112, 112, 16);
            systemCharacterSet.MapCharacters(128, 128, 16);
            systemCharacterSet.MapCharacters(144, 144, 16);
            systemCharacterSet.MapCharacters(160, 160, 16);
            systemCharacterSet.MapCharacters(176, 176, 16);
            systemCharacterSet.MapCharacters(192, 192, 16);
            systemCharacterSet.MapCharacters(208, 208, 16);
            systemCharacterSet.MapCharacters(224, 224, 16);
            systemCharacterSet.MapCharacters(240, 240, 16);
        }

        public void FreeResources()
        {
            Mark(systemCharacterSetBitmapId);
            systemCharacterSetBitmapId = 0;

            Mark(systemCharacterSetId);
            systemCharacterSetId = 0;
        }

        public int GetNextFree()
        {
            return bag.GetNextFree();
        }

        public int New(LiquidClass liquidClass)
        {
            var memoryRequired = Program.ClassManager[liquidClass].MemoryRequired;

            var item = new Object
            {
                LiquidClass = liquidClass,
                RefCount = 0,
                DataSpace = (memoryRequired > 0) ? new int[memoryRequired] : null,
                LiquidObject = null
            };

            var id = bag.New(0, item);

            if (Program.ClassManager.IsA(liquidClass, LiquidClass.Task))
            {
                item.TaskId = id;
            }

            return id;
        }

        public Object this[int id]
        {
            get
            {
                return bag[id];
            }
            set
            {
                bag[id] = value;
            }
        }

        public Liquid.Task GetTask(int id)
        {
            var taskId = bag[id].TaskId;

            var task = bag[taskId].LiquidObject as Liquid.Task;

            return task;
        }

        public bool IsA(int id, LiquidClass baseLiquidClass)
        {
            if (baseLiquidClass < 0)
            {
                return false;
            }

            var liquidClass = bag[id].LiquidClass;

            return Program.ClassManager.IsA(liquidClass, baseLiquidClass);
        }

        public void Hook(int id, int parentId)
        {
            Debug.Assert(bag[id].ParentId == 0);
            Debug.Assert(parentId != 0);

            bag[id].ParentId = parentId;

            var taskId = id;

            while (taskId != 0)
            {
                if (IsA(taskId, LiquidClass.Task))
                {
                    break;
                }
                taskId = bag[taskId].ParentId;
            };

            bag[id].TaskId = taskId;
        }

        public void UnHook(int id)
        {
            Debug.Assert(bag[id].ParentId != 0);

            bag[id].ParentId = 0;

            bag[id].TaskId = 0;
        }

        public List<int> GetChildren(int parentId)
        {
            var children = new List<int>();

            bag.Head();

            var cursor = bag.Cursor;

            while (cursor != 0)
            {
                if (cursor != parentId && bag[cursor].ParentId == parentId)
                {
                    children.Add(cursor);
                }

                cursor = bag.Next();
            }

            return children;
        }

        public List<int> GetChildrenTasks(int parentId)
        {
            var children = new List<int>();

            bag.Head();

            var cursor = bag.Cursor;

            while (cursor != 0)
            {
                if (cursor != parentId && bag[cursor].ParentId == parentId)
                {
                    var liquidClass = bag[cursor].LiquidClass;

                    if (Program.ClassManager.IsA(liquidClass, LiquidClass.Task))
                    {
                        children.Add(cursor);
                    }
                }

                cursor = bag.Next();
            }

            return children;
        }

        public List<int> GetLeftoverObjects(int taskId)
        {
            var leftoverObjects = new List<int>();

            bag.Head();

            var cursor = bag.Cursor;

            while (cursor != 0)
            {
                if (cursor != taskId && bag[cursor].TaskId == taskId)
                {
                    leftoverObjects.Add(cursor);
                }

                cursor = bag.Next();
            }

            return leftoverObjects;
        }

        public void Assign(ref int a0, int id)
        {
            if (a0 != id)
            {
                if (a0 != 0)
                {
                    Mark(a0);
                }

                a0 = id;
            }
        }

        public int Copy(int id)
        {
            Debug.Assert(id != 0);
            Debug.Assert(bag[id].LiquidClass != LiquidClass.None);

            bag[id].RefCount++;

            return id;
        }

        public int VClone(int ownerId, int id)
        {
            Debug.Assert(id != 0);
            Debug.Assert(bag[id].LiquidClass != LiquidClass.None);

            var liquidClass = bag[id].LiquidClass;

            var address = Program.ClassManager[liquidClass].Clone;

            if (address != -1)
            {
                var a0 = Program.Exec.VirtualMachine.Run(id, address);

                return a0;
            }
            else
            {
                var obj = bag[id].LiquidObject as Liquid.Object;

                return obj.Clone();
            }
        }

        public int VCompare(int lhs, int rhs)
        {
            if (lhs == 0)
            {
                if (rhs == 0)
                {
                    return 0;
                }

                return -1;
            }
            else
            {
                if (rhs == 0)
                {
                    return 1;
                }
            }

            Debug.Assert(IsA(lhs, bag[rhs].LiquidClass) || IsA(rhs, bag[lhs].LiquidClass));

            var liquidClass = bag[lhs].LiquidClass;

            var address = Program.ClassManager[liquidClass].Compare;

            if (address != -1)
            {
                var stack = new int[Program.VM_STACK_SIZE];

                stack[1] = Copy(rhs);
                stack[2] = Copy(lhs);

                var a0 = Program.Exec.VirtualMachine.Run(lhs, address, stack, 2);

                Mark(stack[1]);
                Mark(stack[2]);

                return a0;
            }
            else
            {
                var obj = bag[lhs].LiquidObject as Liquid.Object;

                return obj.Compare(rhs);
            }
        }

        public void Mark(int id)
        {
            if (id == 0)
            {
                return;
            }

            Debug.Assert(bag[id].LiquidClass != LiquidClass.None);

            if (bag[id].RefCount > 0)
            {
                bag[id].RefCount--;

                return;
            }

            var obj = bag[id].LiquidObject as Liquid.Object;

            obj.Destructor();

            var liquidClass = bag[id].LiquidClass;

            obj.VShutdown(liquidClass);

            Program.Exec.Sweeper.Add(id);
        }

        public void Free(int id)
        {
            if (id == 0)
            {
                return;
            }

            Debug.Assert(bag[id].LiquidClass != LiquidClass.None);

            if (bag[id].ParentId != 0)
            {
                UnHook(id);
            }

            bag[id].LiquidObject.Dispose();

            bag[id].LiquidClass = LiquidClass.None;

            bag[id].DataSpace = null;

            bag[id].LiquidObject = null;

            bag.Free(id);
        }
    }
}
