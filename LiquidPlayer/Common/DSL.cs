using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiquidPlayer.DSL
{
    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class FreeList<T>
    {
        public struct Node
        {
            public bool Used
            {
                get;
                set;
            }

            public int OwnerId
            {
                get;
                set;
            }

            public T Data
            {
                get;
                set;
            }

            public int Prev
            {
                get;
                set;
            }

            public int Next
            {
                get;
                set;
            }

            public void Clear()
            {
                Used = false;
                OwnerId = 0;
                Data = default(T);
                Prev = 0;
                Next = 0;
            }
        }

        private int headNode;
        private int tailNode;
        private int freeNode;
        private int count;
        private int capacity;
        private Node[] list;
        private int cursor;

        public int Count
        {
            get
            {
                return count;
            }
        }

        public int Cursor
        {
            get
            {
                return cursor;
            }
        }

        public FreeList()
        {
            headNode = 0;
            tailNode = 0;
            freeNode = 1;
            count = 0;
            capacity = 256;
            list = new Node[capacity];
            buildFreeList();
            cursor = 0;
        }

        public T this[int id]
        {
            get
            {
                return list[id].Data;
            }
            set
            {
                Debug.Assert(id >= 1 && id <= capacity - 1);
                Debug.Assert(list[id].Used);

                list[id].Data = value;
            }
        }

        public override string ToString()
        {
            return $"FreeList (Count: {count}, Cursor: {cursor})";
        }

        public int GetNextFree()
        {
            if (freeNode == 0)
            {
                freeNode = capacity;
                capacity = (capacity * 3) / 2;
                Array.Resize(ref list, capacity);
                buildFreeList();
            }

            return freeNode;
        }

        public int New(int ownerId, T item = default(T))
        {
            var id = GetNextFree();

            var prevNode = list[id].Prev;
            var nextNode = list[id].Next;

            freeNode = list[id].Next;
            if (freeNode != 0)
            {
                list[freeNode].Prev = 0;
            }

            if (headNode == 0)
            {
                headNode = id;
            }
            else
            {
                list[tailNode].Next = id;
                prevNode = tailNode;
            }
            nextNode = 0;
            tailNode = id;

            list[id] = new Node
            {
                Used = true,
                OwnerId = ownerId,
                Data = item,
                Prev = prevNode,
                Next = nextNode
            };

            count++;

            return id;
        }

        public int Once(int ownerId, T item)
        {
            var id = headNode;

            while (id != 0)
            {
                if (EqualityComparer<T>.Default.Equals(list[id].Data, item))
                {
                    return id;
                }
                id = list[id].Next;
            }

            return New(ownerId, item);
        }

        public int Head()
        {
            cursor = headNode;

            return cursor;
        }

        public int Next()
        {
            if (cursor != 0)
            {
                cursor = list[cursor].Next;
            }

            return cursor;
        }

        public int Prev()
        {
            if (cursor != 0)
            {
                cursor = list[cursor].Prev;
            }

            return cursor;
        }

        public int Tail()
        {
            cursor = tailNode;

            return cursor;
        }

        public int GetOwner(int id)
        {
            return list[id].OwnerId;
        }

        public void SetOwner(int id, int ownerId)
        {
            list[id].OwnerId = ownerId;
        }

        public T Read()
        {
            return list[cursor].Data;
        }

        public void Free(int id)
        {
            if (id == 0)
            {
                return;
            }

            Debug.Assert(id >= 1 && id <= capacity - 1);
            Debug.Assert(list[id].Used);

            var prevNode = list[id].Prev;
            var nextNode = list[id].Next;

            list[id].Clear();

            if (headNode == id)
            {
                if (tailNode == id)
                {
                    headNode = 0;
                    tailNode = 0;
                }
                else
                {
                    headNode = nextNode;
                    list[headNode].Prev = 0;
                }
            }
            else if (tailNode == id)
            {
                tailNode = prevNode;
                list[tailNode].Next = 0;
            }
            else
            {
                list[prevNode].Next = nextNode;
                list[nextNode].Prev = prevNode;
            }

            list[id].Prev = 0;
            list[id].Next = freeNode;
            if (freeNode != 0)
            {
                list[freeNode].Prev = id;
            }
            freeNode = id;

            count--;

            if (cursor == id)
            {
                cursor = nextNode;
            }
        }

        public List<int> GetLeftover(int ownerId)
        {
            var leftoverItems = new List<int>();

            var cursor = headNode;

            while (cursor != 0)
            {
                if (list[cursor].OwnerId == ownerId)
                {
                    leftoverItems.Add(cursor);
                }

                cursor = list[cursor].Next;
            }

            return leftoverItems;
        }

        public void FreeLeftover(int ownerId)
        {
            var leftoverItems = new List<int>();

            var cursor = headNode;

            while (cursor != 0)
            {
                if (list[cursor].OwnerId == ownerId)
                {
                    leftoverItems.Add(cursor);
                }

                cursor = list[cursor].Next;
            }

            foreach (var id in leftoverItems)
            {
                Free(id);
            }
        }

        private void buildFreeList()
        {
            list[freeNode] = new Node
            {
                Prev = 0,
                Next = freeNode + 1
            };

            for (var index = freeNode + 1; index < capacity - 1; index++)
            {
                list[index] = new Node
                {
                    Prev = index - 1,
                    Next = index + 1
                };
            }

            list[capacity - 1] = new Node
            {
                Prev = capacity - 2,
                Next = 0
            };
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LongManager
    {
        private FreeList<long> bag = new FreeList<long>();

        public int Count
        {
            get
            {
                return bag.Count;
            }
        }

        public int Cursor
        {
            get
            {
                return bag.Cursor;
            }
        }

        public long this[int id]
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

        public override string ToString()
        {
            return $"LongManager (Count: {bag.Count}, Cursor: {bag.Cursor})";
        }

        public int New(int ownerId, long data = 0L)
        {
            return bag.New(ownerId, data);
        }

        public int Once(int ownerId, long data)
        {
            return bag.Once(ownerId, data);
        }

        public int Clone(int ownerId, int id)
        {
            return bag.New(ownerId, bag[id]);
        }

        public int Compare(int lhs, int rhs)
        {
            var a0 = bag[lhs];
            var a1 = bag[rhs];

            if (a0 < a1)
            {
                return -1;
            }
            else if (a0 > a1)
            {
                return 1;
            }
            return 0;
        }

        public int Head()
        {
            return bag.Head();
        }

        public int Next()
        {
            return bag.Next();
        }

        public int Prev()
        {
            return bag.Prev();
        }

        public int Tail()
        {
            return bag.Tail();
        }

        public int GetOwner(int id)
        {
            return bag.GetOwner(id);
        }

        public void SetOwner(int id, int ownerId)
        {
            bag.SetOwner(id, ownerId);
        }

        public long Read()
        {
            return bag.Read();
        }

        public void Free(int id)
        {
            bag.Free(id);
        }

        public List<int> GetLeftover(int ownerId)
        {
            return bag.GetLeftover(ownerId);
        }

        public void FreeLeftover(int ownerId)
        {
            bag.FreeLeftover(ownerId);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DoubleManager
    {
        private FreeList<double> bag = new FreeList<double>();

        public int Count
        {
            get
            {
                return bag.Count;
            }
        }

        public int Cursor
        {
            get
            {
                return bag.Cursor;
            }
        }

        public double this[int id]
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

        public override string ToString()
        {
            return $"DoubleManager (Count: {bag.Count}, Cursor: {bag.Cursor})";
        }

        public int New(int ownerId, double data = 0d)
        {
            return bag.New(ownerId, data);
        }

        public int Once(int ownerId, double data)
        {
            return bag.Once(ownerId, data);
        }

        public int Clone(int ownerId, int id)
        {
            return bag.New(ownerId, bag[id]);
        }

        public int Compare(int lhs, int rhs)
        {
            var a0 = bag[lhs];
            var a1 = bag[rhs];

            if (a0 < a1)
            {
                return -1;
            }
            else if (a0 > a1)
            {
                return 1;
            }
            return 0;
        }

        public int Head()
        {
            return bag.Head();
        }

        public int Next()
        {
            return bag.Next();
        }

        public int Prev()
        {
            return bag.Prev();
        }

        public int Tail()
        {
            return bag.Tail();
        }

        public int GetOwner(int id)
        {
            return bag.GetOwner(id);
        }

        public void SetOwner(int id, int ownerId)
        {
            bag.SetOwner(id, ownerId);
        }

        public double Read()
        {
            return bag.Read();
        }

        public void Free(int id)
        {
            bag.Free(id);
        }

        public List<int> GetLeftover(int ownerId)
        {
            return bag.GetLeftover(ownerId);
        }

        public void FreeLeftover(int ownerId)
        {
            bag.FreeLeftover(ownerId);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class StringManager
    {
        private FreeList<string> bag = new FreeList<string>();

        public int Count
        {
            get
            {
                return bag.Count;
            }
        }

        public int Cursor
        {
            get
            {
                return bag.Cursor;
            }
        }

        public string this[int id]
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

        public override string ToString()
        {
            return $"StringManager (Count: {bag.Count}, Cursor: {bag.Cursor})";
        }

        public int New(int ownerId, string data = null)
        {
            return bag.New(ownerId, data);
        }

        public int Once(int ownerId, string data)
        {
            return bag.Once(ownerId, data);
        }

        public int Clone(int ownerId, int id)
        {
            return bag.New(ownerId, bag[id]);
        }

        public int Compare(int lhs, int rhs)
        {
            var results = string.Compare(bag[lhs], bag[rhs]);

            if (results < 0)
            {
                return -1;
            }
            else if (results > 0)
            {
                return 1;
            }
            return 0;
        }

        public int Head()
        {
            return bag.Head();
        }

        public int Next()
        {
            return bag.Next();
        }

        public int Prev()
        {
            return bag.Prev();
        }

        public int Tail()
        {
            return bag.Tail();
        }

        public int GetOwner(int id)
        {
            return bag.GetOwner(id);
        }

        public void SetOwner(int id, int ownerId)
        {
            bag.SetOwner(id, ownerId);
        }

        public string Read()
        {
            return bag.Read();
        }

        public int GetLength(int id)
        {
            return bag[id].Length;
        }

        public int GetCharacter(int id, int pos)
        {
            return bag[id][pos - 1];
        }

        public void Add(int id, string data)
        {
            bag[id] += data;
        }

        public void Insert(int id, int pos, string data)
        {
            bag[id] = bag[id].Insert(pos, data);
        }

        public void Delete(int id, int pos, int count)
        {
            bag[id] = bag[id].Remove(pos, count);
        }

        public void Replace(int id, string oldString, string newString)
        {
            bag[id] = bag[id].Replace(oldString, newString);
        }

        public void Reverse(int id)
        {
            var charArray = bag[id].ToCharArray();
            Array.Reverse(charArray);
            bag[id] = new string(charArray);
        }

        public void Free(int id)
        {
            bag.Free(id);
        }

        public List<int> GetLeftover(int ownerId)
        {
            return bag.GetLeftover(ownerId);
        }

        public void FreeLeftover(int ownerId)
        {
            bag.FreeLeftover(ownerId);
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    class LinkedQueue<T>
    {
        private LinkedList<T> data = new LinkedList<T>();

        public int Count
        {
            get
            {
                return data.Count;
            }
        }

        public override string ToString()
        {
            return $"LinkedQueue (Count: {data.Count})";
        }

        public void Enqueue(T item)
        {
            data.AddLast(item);
        }

        public T Peek()
        {
            return data.First.Value;
        }

        public T Dequeue()
        {
            if (data.First == null)
            {
                throw new InvalidOperationException("...");
            }
                
            var item = data.First.Value;

            data.RemoveFirst();

            return item;
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void Remove(T item)
        {
            data.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Remove(data.Skip(index).First());
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data = new List<T>();

        public int Count
        {
            get
            {
                return data.Count;
            }
        }

        public override string ToString()
        {
            return $"PriorityQueue (Count: {data.Count})";
        }

        public void Enqueue(T item)
        {
            data.Add(item);

            var childIndex = data.Count - 1;

            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2;

                if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
                {
                    break;
                }

                T tmp = data[childIndex];
                data[childIndex] = data[parentIndex];
                data[parentIndex] = tmp;

                childIndex = parentIndex;
            }
        }

        public T Peek()
        {
            return data[0];
        }

        public T Dequeue()
        {
            Debug.Assert(Count != 0);

            var lastIndex = data.Count - 1;

            T frontItem = data[0];

            data[0] = data[lastIndex];

            data.RemoveAt(lastIndex);

            --lastIndex;

            var parentIndex = 0;

            while (true)
            {
                var leftChildIndex = parentIndex * 2 + 1;

                if (leftChildIndex > lastIndex)
                {
                    break;
                }

                var rightChildIndex = leftChildIndex + 1;

                if (rightChildIndex <= lastIndex && data[rightChildIndex].CompareTo(data[leftChildIndex]) < 0)
                {
                    leftChildIndex = rightChildIndex;
                }

                if (data[parentIndex].CompareTo(data[leftChildIndex]) <= 0)
                {
                    break;
                }

                T tmp = data[parentIndex];
                data[parentIndex] = data[leftChildIndex];
                data[leftChildIndex] = tmp;

                parentIndex = leftChildIndex;
            }

            return frontItem;
        }

        public void Remove(T item)
        {
            for (var index = 0; index < data.Count; index++)
            {
                if (data[index].CompareTo(item) == 0)
                {
                    data.RemoveAt(index);

                    break;
                }
            }
        }

        public bool IsConsistent()
        {
            if (data.Count == 0)
            {
                return true;
            }

            var lastIndex = data.Count - 1;

            for (var parentIndex = 0; parentIndex < data.Count; ++parentIndex)
            {
                var leftChildIndex = 2 * parentIndex + 1;
                var rightChildIndex = 2 * parentIndex + 2;

                if (leftChildIndex <= lastIndex && data[parentIndex].CompareTo(data[leftChildIndex]) > 0)
                {
                    return false;
                }

                if (rightChildIndex <= lastIndex && data[parentIndex].CompareTo(data[rightChildIndex]) > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------
}
