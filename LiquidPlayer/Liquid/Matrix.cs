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
                throw new Exception("Out of memory");
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
                    RaiseError(ErrorCode.IllegalQuantity);
                    return;
                }
                else if (newCapacity > maxElementCount)
                {
                    RaiseError(ErrorCode.OutOfMemory);
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

            isSquared = (width > 0 && height > 0 && width == height);
            capacity = newCapacity;
            width = newWidth;
            height = newHeight;
        }

        public int Index(int xIndex, int yIndex)
        {
            if (xIndex < 0 || xIndex >= width || yIndex < 0 || yIndex >= height)
            {
                RaiseError(ErrorCode.BadSubscript);
                return -1;
            }

            return yIndex * width + xIndex;
        }

        public static int Add(int matrix1Id, int matrix2Id)
        {
            if (matrix1Id == 0 || matrix2Id == 0)
            {
                RaiseError(matrix1Id, ErrorCode.NullObject);
                return 0;
            }

            var matrix1 = LiquidPlayer.Program.Exec.ObjectManager[matrix1Id].LiquidObject as Matrix;
            var matrix2 = LiquidPlayer.Program.Exec.ObjectManager[matrix2Id].LiquidObject as Matrix;

            if (matrix1.ClassId != matrix2.ClassId || matrix1.Width != matrix2.Width || matrix1.Height != matrix2.Height)
            {
                RaiseError(matrix1Id, ErrorCode.Denied);
                return 0;
            }

            var matrixId = NewMatrix(matrix1.ClassId, matrix1.width, matrix1.height);
            var matrix = LiquidPlayer.Program.Exec.ObjectManager[matrixId].LiquidObject as Matrix;

            switch ((LiquidClass)matrix.ClassId)
            {
                case LiquidClass.Byte:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (byte)(matrix1.dataSpace[i] + matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Short:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (short)(matrix1.dataSpace[i] + matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Int:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = matrix1.dataSpace[i] + matrix2.dataSpace[i];
                    }
                    break;
                case LiquidClass.Long:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var L1 = GetLong(matrix1.dataSpace[i]);
                        var L2 = GetLong(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewLong(matrixId, L1 + L2);
                    }
                    break;
                case LiquidClass.Float:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var f1 = Util.Int2Float(matrix1.dataSpace[i]);
                        var f2 = Util.Int2Float(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = Util.Float2Int(f1 + f2);
                    }
                    break;
                case LiquidClass.Double:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var d1 = GetDouble(matrix1.dataSpace[i]);
                        var d2 = GetDouble(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewDouble(matrixId, d1 + d2);
                    }
                    break;
                default:
                    RaiseError(matrix1Id, ErrorCode.Denied);
                    break;
            }

            return matrixId;
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

        public void Fill(int item)
        {
            if (clone != null)
            {
                if (free != null)
                {
                    for (var i = 0; i < capacity; i++)
                    {
                        free(dataSpace[i]);
                        dataSpace[i] = clone(objectId, item);
                    }
                }
                else
                {
                    for (var i = 0; i < capacity; i++)
                    {
                        dataSpace[i] = clone(objectId, item);
                    }
                }
            }
            else
            {
                if (free != null)
                {
                    for (var i = 0; i < capacity; i++)
                    {
                        free(dataSpace[i]);
                        dataSpace[i] = item;
                    }
                }
                else
                {
                    for (var i = 0; i < capacity; i++)
                    {
                        dataSpace[i] = item;
                    }
                }
            }
        }

        public void Identity()
        {
            if (!isSquared)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            switch ((LiquidClass)classId)
            {
                case LiquidClass.Byte:
                case LiquidClass.Short:
                case LiquidClass.Int:
                    for (var i = 0; i < capacity; i++)
                    {
                        dataSpace[i] = 0;
                    }

                    for (var i = 0; i < height; i++)
                    {
                        var index = i * width + i;
                        dataSpace[index] = 1;
                    }
                    break;
                case LiquidClass.Long:
                    for (var i = 0; i < capacity; i++)
                    {
                        free(dataSpace[i]);
                        dataSpace[i] = 0;
                    }

                    for (var i = 0; i < height; i++)
                    {
                        var index = i * width + i;
                        dataSpace[index] = NewLong(objectId, 1L);
                    }
                    break;
                case LiquidClass.Float:
                    for (var i = 0; i < capacity; i++)
                    {
                        dataSpace[i] = 0;
                    }

                    for (var i = 0; i < height; i++)
                    {
                        var index = i * width + i;
                        dataSpace[index] = Util.Float2Int(1f);
                    }
                    break;
                case LiquidClass.Double:
                    for (var i = 0; i < capacity; i++)
                    {
                        free(dataSpace[i]);
                        dataSpace[i] = 0;
                    }

                    for (var i = 0; i < height; i++)
                    {
                        var index = i * width + i;
                        dataSpace[index] = NewDouble(objectId, 1d);
                    }
                    break;
                default:
                    RaiseError(ErrorCode.Denied);
                    break;
            }
        }

        public static int Multiply(int matrix1Id, int matrix2Id)
        {
            if (matrix1Id == 0 || matrix2Id == 0)
            {
                RaiseError(matrix1Id, ErrorCode.NullObject);
                return 0;
            }

            var matrix1 = LiquidPlayer.Program.Exec.ObjectManager[matrix1Id].LiquidObject as Matrix;
            var matrix2 = LiquidPlayer.Program.Exec.ObjectManager[matrix2Id].LiquidObject as Matrix;

            if (matrix1.ClassId != matrix2.ClassId || matrix1.Width != matrix2.Width || matrix1.Height != matrix2.Height)
            {
                RaiseError(matrix1Id, ErrorCode.Denied);
                return 0;
            }

            var matrixId = NewMatrix(matrix1.ClassId, matrix1.width, matrix1.height);
            var matrix = LiquidPlayer.Program.Exec.ObjectManager[matrixId].LiquidObject as Matrix;

            switch ((LiquidClass)matrix.ClassId)
            {
                case LiquidClass.Byte:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (byte)(matrix1.dataSpace[i] * matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Short:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (short)(matrix1.dataSpace[i] * matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Int:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = matrix1.dataSpace[i] * matrix2.dataSpace[i];
                    }
                    break;
                case LiquidClass.Long:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var L1 = GetLong(matrix1.dataSpace[i]);
                        var L2 = GetLong(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewLong(matrixId, L1 * L2);
                    }
                    break;
                case LiquidClass.Float:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var f1 = Util.Int2Float(matrix1.dataSpace[i]);
                        var f2 = Util.Int2Float(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = Util.Float2Int(f1 * f2);
                    }
                    break;
                case LiquidClass.Double:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var d1 = GetDouble(matrix1.dataSpace[i]);
                        var d2 = GetDouble(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewDouble(matrixId, d1 * d2);
                    }
                    break;
                default:
                    RaiseError(matrix1Id, ErrorCode.Denied);
                    break;
            }

            return matrixId;
        }

        public static int Multiply(int matrixId, double scalarValue)
        {
            if (matrixId == 0)
            {
                RaiseError(matrixId, ErrorCode.NullObject);
                return 0;
            }

            var matrix = LiquidPlayer.Program.Exec.ObjectManager[matrixId].LiquidObject as Matrix;

            if (!matrix.IsSquared)
            {
                RaiseError(matrixId, ErrorCode.Denied);
                return 0;
            }

            switch ((LiquidClass)matrix.ClassId)
            {
                case LiquidClass.Byte:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        matrix.dataSpace[i] = (byte)(matrix.dataSpace[i] * scalarValue);
                    }
                    break;
                case LiquidClass.Short:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        matrix.dataSpace[i] = (short)(matrix.dataSpace[i] * scalarValue);
                    }
                    break;
                case LiquidClass.Int:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        matrix.dataSpace[i] = (int)(matrix.dataSpace[i] * scalarValue);
                    }
                    break;
                case LiquidClass.Long:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        var L = GetLong(matrix.dataSpace[i]);

                        matrix.dataSpace[i] = NewLong(matrixId, (long)(L * scalarValue));
                    }
                    break;
                case LiquidClass.Float:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        var f = Util.Int2Float(matrix.dataSpace[i]);

                        matrix.dataSpace[i] = Util.Float2Int((float)(f * scalarValue));
                    }
                    break;
                case LiquidClass.Double:
                    for (var i = 0; i < matrix.capacity; i++)
                    {
                        var d = GetDouble(matrix.dataSpace[i]);

                        matrix.dataSpace[i] = NewDouble(matrixId, d * scalarValue);
                    }
                    break;
                default:
                    RaiseError(matrixId, ErrorCode.Denied);
                    break;
            }

            return matrixId;
        }

        public static int Subtract(int matrix1Id, int matrix2Id)
        {
            if (matrix1Id == 0 || matrix2Id == 0)
            {
                RaiseError(matrix1Id, ErrorCode.NullObject);
                return 0;
            }

            var matrix1 = LiquidPlayer.Program.Exec.ObjectManager[matrix1Id].LiquidObject as Matrix;
            var matrix2 = LiquidPlayer.Program.Exec.ObjectManager[matrix2Id].LiquidObject as Matrix;

            if (matrix1.ClassId != matrix2.ClassId || matrix1.Width != matrix2.Width || matrix1.Height != matrix2.Height)
            {
                RaiseError(matrix1Id, ErrorCode.Denied);
                return 0;
            }

            var matrixId = NewMatrix(matrix1.ClassId, matrix1.width, matrix1.height);
            var matrix = LiquidPlayer.Program.Exec.ObjectManager[matrixId].LiquidObject as Matrix;

            switch ((LiquidClass)matrix.ClassId)
            {
                case LiquidClass.Byte:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (byte)(matrix1.dataSpace[i] - matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Short:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = (short)(matrix1.dataSpace[i] - matrix2.dataSpace[i]);
                    }
                    break;
                case LiquidClass.Int:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        matrix.dataSpace[i] = matrix1.dataSpace[i] - matrix2.dataSpace[i];
                    }
                    break;
                case LiquidClass.Long:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var L1 = GetLong(matrix1.dataSpace[i]);
                        var L2 = GetLong(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewLong(matrixId, L1 - L2);
                    }
                    break;
                case LiquidClass.Float:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var f1 = Util.Int2Float(matrix1.dataSpace[i]);
                        var f2 = Util.Int2Float(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = Util.Float2Int(f1 - f2);
                    }
                    break;
                case LiquidClass.Double:
                    for (var i = 0; i < matrix1.capacity; i++)
                    {
                        var d1 = GetDouble(matrix1.dataSpace[i]);
                        var d2 = GetDouble(matrix2.dataSpace[i]);

                        matrix.dataSpace[i] = NewDouble(matrixId, d1 - d2);
                    }
                    break;
                default:
                    RaiseError(matrix1Id, ErrorCode.Denied);
                    break;
            }

            return matrixId;
        }

        public override void shutdown()
        {
            setCapacity(0, 0);

            base.shutdown();
        }
    }
}
