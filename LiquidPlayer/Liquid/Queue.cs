using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Queue : Collection
    {
        protected int capacity;
        protected int count;
        protected int head;
        protected int tail;

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
                return (count == 0);
            }
        }

        public static int NewQueue(int classId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Queue);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Queue(id, classId);

            return id;
        }

        public Queue(int id, int classId)
            : base(id, classId)
        {
            this.capacity = 16;
            this.count = 0;
            this.head = 0;
            this.tail = 0;

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
                        clone = objectManager.VClone;
                        compare = objectManager.VCompare;
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
            return $"Queue (Count: {count}, Capacity: {capacity})";
        }

        private void grow()
        {
            capacity = (capacity * 3) / 2;

            System.Array.Resize(ref dataSpace, capacity + 1);

            if (head == 0)
            {
                tail = count;
            }
            else
            {
                var toIndex = capacity;
                for (var index = count - 1; count >= head; count--)
                {
                    dataSpace[--toIndex] = dataSpace[index];
                }
            }
        }

        public void Populate(int item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            if (count != 0)
            {
                if (free != null)
                {
                    if (head < tail)
                    {
                        for (var index = 0; index < count; index++)
                        {
                            free(dataSpace[head + index]);
                        }
                    }
                    else
                    {
                        for (var index = 0; index <= tail; index++)
                        {
                            free(dataSpace[index]);
                        }

                        for (var index = head; index < capacity; index++)
                        {
                            free(dataSpace[index]);
                        }
                    }
                }

                count = 0;
                head = 0;
                tail = 0;
            }
        }

        public int Dequeue()
        {
            if (count == 0)
            {
                return 0;
            }

            var returnValue = dataSpace[head];

            head = (head + 1) % capacity;

            count--;

            return returnValue;
        }

        public void Enqueue(int item)
        {
            if (clone != null && item != 0)
            {
                item = clone(objectId, item);
            }

            dataSpace[tail] = item;

            tail = (tail + 1) % capacity;

            count++;

            if (tail == head)
            {
                grow();
            }
        }

        public int Peek()
        {
            if (count == 0)
            {
                return 0;
            }

            var item = dataSpace[head];

            if (clone != null && item != 0)
            {
                item = clone(objectId, item);
            }

            return item;
        }

        public override void shutdown()
        {
            Clear();

            base.shutdown();
        }
    }
}
