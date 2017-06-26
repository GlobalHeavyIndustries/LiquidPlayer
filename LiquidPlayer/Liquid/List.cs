using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class List : Collection
    {
        protected int capacity;
        protected int count;
        protected bool isSorted;
        protected int elementSize;
        protected int maxElementCount;

        protected int enumeratorPosition;

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

        public static int NewList(int classId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.List);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new List(id, classId);

            return id;
        }

        public List(int id, int classId)
            : base(id, classId)
        {
            this.capacity = 0;
            this.count = 0;
            this.isSorted = true;
            this.elementSize = sizeof(int);
            this.maxElementCount = int.MaxValue / this.elementSize;

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
        }

        public override string ToString()
        {
            return $"List (Count: {count})";
        }

        private void setCapacity(int newCapacity)
        {
            if (capacity != newCapacity)
            {
                if (newCapacity != 0)
                {
                    if (newCapacity < 0)
                    {
                        RaiseError(ErrorCode.IllegalQuantity);
                        return;
                    }
                    else if (newCapacity > maxElementCount)
                    {
                        RaiseError(ErrorCode.OutOfMemory);
                        return;
                    }

                    if (newCapacity < count)
                    {
                        if (free != null)
                        {
                            for (var index = newCapacity; index < count; index++)
                            {
                                free(dataSpace[index]);
                            }
                        }
                    }

                    if (capacity !=0)
                    {
                        System.Array.Resize(ref dataSpace, newCapacity);

                        objectManager[objectId].DataSpace = dataSpace;
                    }
                    else
                    {
                        dataSpace = new int[newCapacity];

                        objectManager[objectId].DataSpace = dataSpace;
                    }
                }
                else
                {
                    if (free != null)
                    {
                        for (var index = 0; index < count; index++)
                        {
                            free(dataSpace[index]);
                        }
                    }

                    dataSpace = null;

                    objectManager[objectId].DataSpace = dataSpace;
                }

                if (newCapacity < capacity)
                {
                    if (count > newCapacity)
                    {
                        setCount(newCapacity);
                    }
                }

                capacity = newCapacity;
            }
        }

        private void setCount(int newCount)
        {
            if (count != newCount)
            {
                if (newCount > capacity)
                {
                    setCapacity(newCount);
                }

                if (newCount > count)
                {
                    for (var index = count; index < newCount; index++)
                    {
                        dataSpace[index] = 0;
                    }

                    isSorted = false;
                }

                count = newCount;
            }
        }

        private void expand()
        {
            var newCapacity = 0;

            if (capacity < 4)
            {
                newCapacity = 4;
            }
            else if (capacity < 16)
            {
                newCapacity = capacity + 4;
            }
            else if (capacity < 64)
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
                    RaiseError(ErrorCode.OutOfMemory);
                    return;
                }
            }

            setCapacity(newCapacity);
        }

        private int locate(int item)
        {
            for (var index = 0; index < count; index++)
            {
                if (dataSpace[index] == item)
                {
                    return index;
                }
            }

            return -1;
        }

        private bool binarySearch(int item, ref int index)
        {
            var L = 0;
            var R = count - 1;
            while (L <= R)
            {
                var M = (L + R) / 2;
                var compareResult = compare(dataSpace[M], item);

                if (compareResult < 0)
                {
                    L = M + 1;
                }
                else if (compareResult > 0)
                {
                    R = M - 1;
                }
                else
                {
                    index = M;
                    return true;
                }
            }

            index = L;
            return false;
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
            if (index < 0 || index >= count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return -1;
            }

            return index;
        }

        public void Populate(int item)
        {
            Add(item);
        }

        public void EnumeratorStart()
        {
            enumeratorPosition = -1;
        }

        public bool EnumeratorNext()
        {
            if (enumeratorPosition < count - 1)
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

        public int Add(int item)
        {
            var index = count;

            Insert(count, item);

            return index;
        }

        public void Clear()
        {
            setCapacity(0);

            isSorted = true;
        }

        public bool Contains(int item)
        {
            return (locate(item) != -1);
        }

        public void Delete(int index)
        {
            if (index < 0 || index >= count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return;
            }

            var freeItem = dataSpace[index];

            if (free != null && freeItem != 0)
            {
                free(freeItem);
            }

            count--;

            if (index < count)
            {
                for (var i = index; i < count; i++)
                {
                    dataSpace[i] = dataSpace[i + 1];
                }
            }

            if (count < 1)
            {
                isSorted = true;
            }
        }

        public int Enqueue(int item)
        {
            if (isSorted == false)
            {
                RaiseError(ErrorCode.NotSorted);
                return -1;
            }

            var index = 0;

            binarySearch(item, ref index);
             
            Insert(index, item);

            isSorted = true;

            return index;
        }

        public int IndexOf(int item)
        {
            var result = 0;

            if (isSorted)
            {
                if (binarySearch(item, ref result))
                {
                    return result;
                }
            }
            else
            {
                for (var index = 0; index < count; index++)
                {
                    if (compare(dataSpace[index], item) == 0)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        public void Insert(int index, int item)
        {
            if (index < 0 || index > count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return;
            }

            //if (free != null & item == 0)
            //{
            //    Throw(ExceptionCode.BadItem);
            //    return;
            //}

            if (count == capacity)
            {
                expand();
            }

            if (index < count)
            {
                for (var i = count; i > index; i--)
                {
                    dataSpace[i] = dataSpace[i - 1];
                }
            }

            if (clone != null && item != 0)
            {
                item = clone(objectId, item);
            }

            dataSpace[index] = item;

            count++;

            isSorted = (count == 1);
        }

        public bool IsEmpty()
        {
            return (count == 0);
        }

        public int Remove(int item)
        {
            var index = locate(item);

            if (index != -1)
            {
                Delete(index);
            }

            return index;
        }

        public void Reverse()
        {
            Reverse(0, count - 1);
        }

        public void Reverse(int first, int last)
        {
            if (first < 0 || first >= count || last < 0 || last >= count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            var toIndex = last;

            for (var index = first; index < (last + first) / 2; index++)
            {
                Util.Swap(ref dataSpace[index], ref dataSpace[toIndex]);

                toIndex--;
            }

            isSorted = false;
        }

        public void Shuffle()
        {
            Shuffle(0, count - 1);
        }

        public void Shuffle(int first, int last)
        {
            if (first < 0 || first >= count || last < 0 || last >= count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                RaiseError(ErrorCode.Denied);
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

            isSorted = false;
        }

        public void Sort()
        {
            Sort(0, count - 1);
        }

        public void Sort(int first, int last)
        {
            if (first < 0 || first >= count || last < 0 || last >= count)
            {
                RaiseError(ErrorCode.BadSubscript);
                return;
            }

            if (first > last)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            if (count <= 1)
            {
                return;
            }

            quickSort(first, last);

            insertionSort(first, last);

            if (first == 0 && last == count - 1)
            {
                isSorted = true;
            }
        }

        public override void shutdown()
        {
            setCapacity(0);

            base.shutdown();
        }
    }
 }
