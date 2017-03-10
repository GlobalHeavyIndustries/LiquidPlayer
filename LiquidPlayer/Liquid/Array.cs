using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Array : Collection
    {
        protected bool isDimensioned;
        protected int capacity;
        protected int count;
        protected int elementSize;
        protected int maxElementCount;

        protected int enumeratorPosition;

        protected CloneDelegate clone;
        protected CompareDelegate compare;
        protected FreeDelegate free;
        
        public int Capacity
        {
            get
            {
                return capacity;
            }
        }

        public static int NewArray(int classId, int? capacity = null, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Array);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Array(id, classId, capacity);

            return id;
        }

        public Array(int id, int classId, int? capacity = null)
            : base(id, classId)
        {
            this.isDimensioned = false;
            this.capacity = 0;
            this.elementSize = sizeof(int);
            this.maxElementCount = int.MaxValue / this.elementSize;

            this.enumeratorPosition = -1;

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

            if (capacity != null)
            {
                Dim((int)capacity);
            }
        }

        public override string ToString()
        {
            return $"Array (Capacity: {capacity})";
        }

        private void insertionSort(int first, int last)
        {
            var indexOfMin = first;
            var j = first + 15;
            if (j > last)
            {
                j = last;
            }

            var result = 0;

            for (var i = first + 1; i <= j; i++)
            {
                result = compare(dataSpace[i], dataSpace[indexOfMin]);

                if (result < 0)
                {
                    indexOfMin = i;
                }
            }

            if (first != indexOfMin)
            {
                Util.Swap(ref dataSpace[first], ref dataSpace[indexOfMin]);
            }

            for (var i = first + 2; i <= last; i++)
            {
                var temp = dataSpace[i];

                j = i;

                result = compare(temp, dataSpace[j - 1]);

                while (result < 0)
                {
                    dataSpace[j] = dataSpace[j - 1];
                    j--;

                    result = compare(temp, dataSpace[j - 1]);
                }

                dataSpace[j] = temp;
            }
        }

        private void quickSort(int first, int last)
        {
            var stack = new int[64];
            stack[0] = first;
            stack[1] = last;

            var sp = 2;

            while (sp != 0)
            {
                sp -= 2;
                first = stack[sp];
                last = stack[sp + 1];

                while ((last - first) > 15)
                {
                    var L = 0;
                    var R = (first + last) / 2;

                    var result = 0;

                    result = compare(dataSpace[first], dataSpace[R]);

                    if (result > 0)
                    {
                        Util.Swap(ref dataSpace[first], ref dataSpace[R]);
                    }

                    result = compare(dataSpace[first], dataSpace[last]);

                    if (result > 0)
                    {
                        Util.Swap(ref dataSpace[first], ref dataSpace[last]);
                    }

                    result = compare(dataSpace[R], dataSpace[last]);

                    if (result > 0)
                    {
                        Util.Swap(ref dataSpace[R], ref dataSpace[last]);
                    }

                    var pivot = dataSpace[R];

                    L = first;
                    R = last;

                    while (true)
                    {
                        do
                        {
                            R--;
                            result = compare(dataSpace[R], pivot);
                        } while (result > 0);

                        do
                        {
                            L++;
                            result = compare(dataSpace[L], pivot);
                        } while (result < 0);

                        if (L >= R)
                        {
                            break;
                        }

                        Util.Swap(ref dataSpace[L], ref dataSpace[R]);
                    }

                    if ((R - first) < (last - R))
                    {
                        stack[sp] = R + 1;
                        stack[sp + 1] = last;
                        sp += 2;
                        last = R;
                    }
                    else
                    {
                        stack[sp] = first;
                        stack[sp + 1] = R;
                        sp += 2;
                        first = R + 1;
                    }
                }
            }
        }

        public int Index(int index)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return -1;
            }

            if (index < 0 || index >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return -1;
            }

            return index;
        }

        public void Populate(int item)
        {
            if (capacity == 0)
            {
                Dim(16);
            }
            else if (count == capacity)
            {
                var newCapacity = 0;

                if (capacity < 64)
                {
                    newCapacity = capacity + 16;
                }
                else
                {
                    newCapacity = capacity + (capacity / 4);
                }

                if (newCapacity > maxElementCount)
                {
                    newCapacity = maxElementCount;

                    if (newCapacity == capacity)
                    {
                        Throw(ExceptionCode.OutOfMemory);
                        return;
                    }
                }

                ReDimPreserve(newCapacity);
            }

            if (clone != null && item != 0)
            {
                item = clone(item);
            }

            dataSpace[count] = item;

            count++;
        }

        public void PostPopulate(int count)
        {
            if (count != 0)
            {
                ReDimPreserve(count);

                count = 0;
            }
        }

        public void EnumeratorStart()
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            enumeratorPosition = -1;
        }

        public bool EnumeratorNext()
        {
            if (enumeratorPosition < capacity - 1)
            {
                enumeratorPosition++;

                return true;
            }

            return false;
        }

        public int EnumeratorGet()
        {
            return enumeratorPosition;
        }

        public void Clear()
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (free != null)
            {
                for (var index = 0; index < capacity; index++)
                {
                    free(dataSpace[index]);
                }
            }

            objectManager[objectId].DataSpace.Fill(0);
        }

        public void Delete(int index)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (index < 0 || index >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return;
            }

            var freeItem = dataSpace[index];

            if (free != null && freeItem != 0)
            {
                free(freeItem);
            }

            if (index < capacity - 1)
            {
                for (var i = index; i < capacity - 1; i++)
                {
                    dataSpace[i] = dataSpace[i + 1];
                }
            }

            dataSpace[capacity - 1] = 0;
        }

        public void Dim(int newCapacity)
        {
            if (isDimensioned)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            if (newCapacity < 0)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }
            else if (newCapacity > maxElementCount)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            dataSpace = new int[newCapacity];

            objectManager[objectId].DataSpace = dataSpace;

            capacity = newCapacity;

            isDimensioned = true;
        }

        public void Insert(int index, int item)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (index < 0 || index >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return;
            }

            var freeItem = dataSpace[capacity - 1];

            if (free != null && freeItem != 0)
            {
                free(freeItem);
            }

            if (index < capacity - 1)
            {
                for (var i = capacity - 1; i > index; i--)
                {
                    dataSpace[i] = dataSpace[i - 1];
                }
            }

            if (clone != null && item != 0)
            {
                item = clone(item);
            }

            dataSpace[index] = item;
        }

        public void ReDim(int newCapacity)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            UnDim();

            Dim(newCapacity);
        }

        public void ReDimPreserve(int newCapacity)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (newCapacity < 0)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }
            else if (newCapacity > maxElementCount)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            if (capacity != newCapacity)
            {
                if (newCapacity < capacity)
                {
                    if (free != null)
                    {
                        for (var index = newCapacity; index < capacity; index++)
                        {
                            free(dataSpace[index]);
                        }
                    }
                }

                System.Array.Resize(ref dataSpace, newCapacity);

                objectManager[objectId].DataSpace = dataSpace;

                capacity = newCapacity;
            }
        }

        public void Reverse()
        {
            Reverse(0, capacity - 1);
        }

        public void Reverse(int first, int last)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (first < 0 || first >= capacity || last < 0 || last >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            var toIndex = last;

            for (var index = first; index < (last + first) / 2; index++)
            {
                Util.Swap(ref dataSpace[index], ref dataSpace[toIndex]);

                toIndex--;
            }
        }

        public void Shuffle()
        {
            Shuffle(0, capacity - 1);
        }

        public void Shuffle(int first, int last)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (first < 0 || first >= capacity || last < 0 || last >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            for (var index = last; index >= first + 1; index--)
            {
                var randomIndex = first + LiquidPlayer.Program.Random.Range(0, index - first);

                if (randomIndex != index)
                {
                    Util.Swap(ref dataSpace[index], ref dataSpace[randomIndex]);
                }
            }
        }

        public void Sort()
        {
            Sort(0, capacity - 1);
        }

        public void Sort(int first, int last)
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (first < 0 || first >= capacity || last < 0 || last >= capacity)
            {
                Throw(ExceptionCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                Throw(ExceptionCode.Denied);
                return;
            }

            quickSort(first, last);

            insertionSort(first, last);
        }

        public void UnDim()
        {
            if (!isDimensioned)
            {
                Throw(ExceptionCode.NotDimensioned);
                return;
            }

            if (free != null)
            {
                for (var index = 0; index < capacity; index++)
                {
                    free(dataSpace[index]);
                }
            }

            dataSpace = null;

            objectManager[objectId].DataSpace = dataSpace;

            isDimensioned = false;
        }

        public override void shutdown()
        {
            if (isDimensioned)
            {
                UnDim();
            }

            base.shutdown();
        }
    }
}
