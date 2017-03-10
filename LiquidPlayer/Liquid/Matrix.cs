using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Matrix : Collection
    {
        protected bool isSquared;
        protected int capacity;
        protected int width;
        protected int height;
        protected int elementSize;
        protected int maxElementCount;

        protected CloneDelegate clone;
        protected CompareDelegate compare;
        protected FreeDelegate free;
        
        public bool IsSquared
        {
            get
            {
                return isSquared;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public static int NewMatrix(int classId, int width, int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Matrix);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Matrix(id, classId, width, height);

            return id;
        }

        public Matrix(int id, int classId, int width, int height)
            : base(id, classId)
        {
            this.capacity = 0;
            this.width = 0;
            this.height = 0;
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

            setCapacity(width, height);
        }

        public override string ToString()
        {
            return $"Matrix (Capacity: {width} x {height})";
        }

        private void setCapacity(int newWidth, int newHeight)
        {
            if (newWidth == width && newHeight == height)
            {
                return;
            }

            var newCapacity = newWidth * newHeight;

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

                if (dataSpace != null)
                {
                    var newMatrix = new int[newCapacity];

                    if (free != null)
                    {
                        for (var yIndex = 0; yIndex < height - 1; yIndex++)
                        {
                            var t1 = yIndex * newWidth;
                            var t2 = yIndex * width;

                            for (var xIndex = 0; xIndex < width - 1; xIndex++)
                            {
                                var item = dataSpace[t2 + xIndex];

                                if (xIndex < width && yIndex < height)
                                {
                                    newMatrix[t1 + xIndex] = item;
                                }
                                else
                                {
                                    free.Invoke(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var yIndex = 0; yIndex < height - 1; yIndex++)
                        {
                            var t1 = yIndex * newWidth;
                            var t2 = yIndex * width;

                            for (var xIndex = 0; xIndex < width - 1; xIndex++)
                            {
                                var item = dataSpace[t2 + xIndex];

                                if (xIndex < width && yIndex < height)
                                {
                                    newMatrix[t1 + xIndex] = item;
                                }
                            }
                        }
                    }
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
                    for (var index = 0; index < capacity; index++)
                    {
                        free(dataSpace[index]);
                    }
                }

                dataSpace = null;

                objectManager[objectId].DataSpace = dataSpace;
            }

            isSquared = (width > 0 && height > 0 && width == height) ? true : false;
            capacity = newCapacity;
            width = newWidth;
            height = newHeight;
        }

        public int Index(int xIndex, int yIndex)
        {
            if (xIndex < 0 || xIndex >= width || yIndex < 0 || yIndex >= height)
            {
                Throw(ExceptionCode.BadSubscript);
                return -1;
            }

            return yIndex * width + xIndex;
        }

        public void Clear()
        {
            if (free != null)
            {
                for (var index = 0; index < capacity; index++)
                {
                    free(dataSpace[index]);
                }
            }

            objectManager[objectId].DataSpace.Fill(0);
        }

        public override void shutdown()
        {
            setCapacity(0, 0);

            base.shutdown();
        }
    }
}
