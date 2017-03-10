using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public struct HashSlot
    {
        public bool InUse;
        public string Key;
    }

    public class Dictionary : Collection
    {
        protected int capacity;
        protected int count;
        protected int elementSize;
        protected int maxElementCount;
        protected int tableCount;

        protected HashSlot[] hashSlots;

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

        public static int NewDictionary(int classId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Dictionary);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Dictionary(id, classId);

            return id;
        }

        public Dictionary(int id, int classId)
            : base(id, classId)
        {
            this.capacity = 0;
            this.count = 0;
            this.elementSize = sizeof(int);
            this.maxElementCount = int.MaxValue / this.elementSize;
            this.tableCount = 0;

            this.hashSlots = null;

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

            setCount(Sprockets.Math.GetClosestPrime(16));
        }

        public override string ToString()
        {
            return $"Dictionary (Capacity: {capacity})";
        }

        private void setCapacity(int newCapacity)
        {
            if (capacity != newCapacity)
            {
                if (newCapacity != 0)
                {
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

                    if (newCapacity < count)
                    {
                        if (free != null)
                        {
                            for (var index = newCapacity; index < count; index++)
                            {
                                var item = dataSpace[index];

                                free(item);

                                hashSlots[index].InUse = false;
                            }
                        }
                    }

                    if (capacity !=0)
                    {
                        System.Array.Resize(ref dataSpace, newCapacity);

                        objectManager[objectId].DataSpace = dataSpace;

                        System.Array.Resize(ref hashSlots, newCapacity);
                    }
                    else
                    {
                        dataSpace = new int[newCapacity];

                        objectManager[objectId].DataSpace = dataSpace;

                        hashSlots = new HashSlot[newCapacity];
                    }
                }
                else
                {
                    if (free != null)
                    {
                        for (var index = 0; index < count; index++)
                        {
                            var item = dataSpace[index];

                            free(item);

                            hashSlots[item].InUse = false;
                        }
                    }

                    dataSpace = null;

                    objectManager[objectId].DataSpace = dataSpace;

                    hashSlots = null;
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

                        hashSlots[index] = default(HashSlot);
                    }
                }

                count = newCount;
            }
        }

        private int hash(string key, int tableSize)
        {
            var H = 0U;

            for (var i = 0; i < key.Length; i++)
            {
                H <<= 4;
                H += key[i];
                var G = H & 0xF0000000;
                if (G != 0)
                {
                    var F = G;
                    F >>= 24;
                    H = (H ^ F) ^ G;
                }
            }

            return (int)H % tableSize;
        }

        private void alterTableSize(int newTableSize)
        {
            var oldDataspace = dataSpace;
            var oldHashSlots = hashSlots;
            var oldCount = count;

            dataSpace = null;
            hashSlots = null;
            setCount(newTableSize);

            tableCount = 0;

            for (var index = 0; index < oldCount; index++)
            {
                var item = oldDataspace[index];

                var hashSlot = oldHashSlots[index];

                if (hashSlot.InUse)
                {
                    Insert(hashSlot.Key, item);
                }

                free?.Invoke(item);
            }

            oldDataspace = null;
        }

        private void growTable()
        {
            alterTableSize(Sprockets.Math.GetClosestPrime(count * 2 + 1));
        }

        private int search(string key, out int hashSlot)
        {
            var index = hash(key, count);
            var firstIndex = index;

            while (true)
            {
                var currentSlot = hashSlots[index];

                if (!currentSlot.InUse)
                {
                    hashSlot = index;

                    return -1;
                }
                else
                {
                    if (currentSlot.Key == key)
                    {
                        hashSlot = index;

                        return index;
                    }
                }

                index++;

                if (index == count)
                {
                    index = 0;
                }

                if (index == firstIndex)
                {
                    hashSlot = -1;

                    return -1;
                }
            }
        }

        public int Index(string key)
        {
            int hashSlot;

            var index = search(key, out hashSlot);

            if (index == -1)
            {
                Throw(ExceptionCode.BadSubscript);
                return -1;
            }

            return index;
        }

        public void Clear()
        {
            for (var index = 0; index < count; index++)
            {
                var item = dataSpace[index];

                var hashItem = hashSlots[index];

                if (hashItem.InUse)
                {
                    free?.Invoke(item);

                    hashSlots[index].InUse = false;
                }
            }

            tableCount = 0;
        }

        public void Delete(string key)
        {
            int index;

            if (search(key, out index) == -1)
            {
                Throw(ExceptionCode.KeyNotFound);
                return;
            }

            var item = dataSpace[index];

            free?.Invoke(item);

            hashSlots[index].InUse = false;

            tableCount--;

            while (true)
            {
                index++;

                if (index == count)
                {
                    index = 0;
                }

                item = dataSpace[index];

                if (item == 0)
                {
                    break;
                }

                var hashSlot = hashSlots[index];

                if (hashSlot.InUse == false)
                {
                    break;
                }

                var oldKey = hashSlot.Key;

                var oldValue = item;

                hashSlots[index].InUse = false;

                tableCount--;

                Insert(oldKey, oldValue);

                free?.Invoke(oldValue);
            }
        }

        public bool Exists(string key)
        {
            int index;

            return (search(key, out index) != -1) ? true : false;
        }

        public int GetCount()
        {
            return count;
        }

        public void Insert(string key, int value)
        {
            int index;

            if (search(key, out index) != -1)
            {
                Throw(ExceptionCode.DuplicateKey);
                return;
            }

            if (index == -1)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            if (clone != null && value != 0)
            {
                value = clone(value);
            }

            var hashSlot = new HashSlot
            {
                InUse = true,
                Key = key
            };

            dataSpace[index] = value;

            hashSlots[index] = hashSlot;

            tableCount++;

            if ((tableCount * 3) > (count * 2))
            {
                growTable();
            }
        }

        public override void shutdown()
        {
            setCapacity(0);

            base.shutdown();
        }
    }
 }
