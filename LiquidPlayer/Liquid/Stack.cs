using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Stack : Collection
    {
        protected int capacity;
        protected int count;

        protected CloneDelegate clone;
        protected CompareDelegate compare;
        protected FreeDelegate free;
        
        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (count == 0) ? true : false;
            }
        }

        public static int NewStack(int classId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Stack);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Stack(id, classId);

            return id;
        }

        public Stack(int id, int classId)
            : base(id, classId)
        {
            this.capacity = 16;
            this.count = 0;

            switch ((LiquidClass)classId)
            {
                case LiquidClass.Long:
                    clone = LiquidPlayer.Program.Exec.LongManager.Clone;
                    compare = LiquidPlayer.Program.Exec.LongManager.Compare;
                    free = LiquidPlayer.Program.Exec.LongManager.Free;
                    break;
                case LiquidClass.Double:
                    clone = LiquidPlayer.Program.Exec.DoubleManager.Clone;
                    compare = LiquidPlayer.Program.Exec.DoubleManager.Compare;
                    free = LiquidPlayer.Program.Exec.DoubleManager.Free;
                    break;
                case LiquidClass.String:
                    clone = LiquidPlayer.Program.Exec.StringManager.Clone;
                    compare = LiquidPlayer.Program.Exec.StringManager.Compare;
                    free = LiquidPlayer.Program.Exec.StringManager.Free;
                    break;
                default:
                    if (classId > 0)
                    {
                        clone = objectManager.Clone;
                        compare = objectManager.Compare;
                        free = objectManager.Mark;
                    }
                    else
                    {
                        compare = delegate (int lhs, int rhs)
                        {
                            if (lhs < rhs)
                            {
                                return -1;
                            }
                            else if (lhs > rhs)
                            {
                                return 1;
                            }

                            return 0;
                        };
                    }
                    break;
            }

            objectManager[objectId].DataSpace = new int[capacity + 1];

            dataSpace = objectManager[objectId].DataSpace;
        }

        public override string ToString()
        {
            return $"Stack (Count: {count}, Capacity: {capacity})";
        }

        private void grow()
        {
            capacity = (capacity * 3) / 2;

            System.Array.Resize(ref dataSpace, capacity + 1);
        }

        public void Populate(int item)
        {
            Push(item);
        }

        public void Clear()
        {
            if (count != 0)
            {
                if (free != null)
                {
                    for (var index = 0; index < count; index++)
                    {
                        free(dataSpace[index]);
                    }
                }

                count = 0;
            }
        }

        public int Peek()
        {
            if (count == 0)
            {
                return 0;
            }

            var item = dataSpace[count - 1];

            if (clone != null && item != 0)
            {
                item = clone(item);
            }

            return item;
        }

        public int Pop()
        {
            if (count == 0)
            {
                return 0;
            }

            return dataSpace[--count];
        }

        public void Push(int item)
        {
            if (count == capacity)
            {
                grow();
            }

            if (clone != null && item != 0)
            {
                item = clone(item);
            }

            dataSpace[count++] = item;
        }

        public override void shutdown()
        {
            Clear();

            base.shutdown();
        }
    }
}
