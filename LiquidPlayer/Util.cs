using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using System.IO;
using System.IO.Compression;

namespace LiquidPlayer
{
    [StructLayout(LayoutKind.Explicit)]
    public struct BitUnion
    {
        [FieldOffset(0)]
        public byte Byte1;
        [FieldOffset(1)]
        public byte Byte2;
        [FieldOffset(2)]
        public byte Byte3;
        [FieldOffset(3)]
        public byte Byte4;

        [FieldOffset(0)]
        public int Int;

        [FieldOffset(0)]
        public float Float;

//      --------------------

        [FieldOffset(0)]
        public int Int1;
        [FieldOffset(4)]
        public int Int2;

        [FieldOffset(0)]
        public long Long;

        [FieldOffset(0)]
        public double Double;
    }

    internal static class UnsafeNativeMethods
    {
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("Kernel32.dll")]
        public static extern int AllocConsole();

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);
    }

    public static class Util
    {
        public static BitUnion BitUnion = new BitUnion();

        public static string FindFile(string path, string sharedFolder = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            var filePath = Path.GetDirectoryName(path);

            var fileName = Path.GetFileName(path);

            if (filePath == "")
            {
                filePath = Directory.GetCurrentDirectory() + @"\";
            }
            else if (filePath.Length >= 3 && filePath[1] == ':' && filePath[2] == '\\')
            {
                filePath += @"\";
            }
            else if (filePath[0] != '\\')
            {
                filePath = Directory.GetCurrentDirectory() + @"\" + filePath + @"\";
            }

            path = filePath + fileName;

            if (File.Exists(path))
            {
                return path;
            }

            if (sharedFolder != null)
            {
                path = sharedFolder + fileName;

                if (File.Exists(path))
                {
                    return path;
                }
            }

            return "";
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static uint RotateLeft(this uint value, int count = 1)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static uint RotateRight(this uint value, int count = 1)
        {
            return (value >> count) | (value << (32 - count));
        }

        public static float Int2Float(int i)
        {
            BitUnion.Int = i;

            return BitUnion.Float;
        }

        public static long Int2Long(int i1, int i2)
        {
            BitUnion.Int1 = i1;
            BitUnion.Int2 = i2;

            return BitUnion.Long;
        }

        public static double Int2Double(int i1, int i2)
        {
            BitUnion.Int1 = i1;
            BitUnion.Int2 = i2;

            return BitUnion.Double;
        }

        public static int Float2Int(float f)
        {
            BitUnion.Float = f;

            return BitUnion.Int;
        }

        public static Tuple<int, int> Long2Int(long l)
        {
            BitUnion.Long = l;

            return Tuple.Create(BitUnion.Int1, BitUnion.Int2);
        }

        public static Tuple<int, int> Double2Int(double d)
        {
            BitUnion.Double = d;

            return Tuple.Create(BitUnion.Int1, BitUnion.Int2);
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }
            else if (val.CompareTo(max) > 0)
            {
                return max;
            }

            return val;
        }

        public static T Min<T>(params T[] values)
        {
            return values.Min();
        }

        public static T Max<T>(params T[] values)
        {
            return values.Max();
        }
            
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a; a = b; b = temp;
        }
    }

    public static class Extensions
    {
        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            return new List<T>(list);
        }

        public static Stack<T> Clone<T>(this Stack<T> stack)
        {
            return new Stack<T>(stack.Reverse());
        }

        public static void Remove<T>(this Queue<T> queue, T value) where T : IComparable<T>
        {
            queue = new Queue<T>(queue.Where(s => s.CompareTo(value) != 0));
        }
    }

    public class HiResTimer
    {
        private bool isPerfCounterSupported = false;
        private long frequency = 0;

        public HiResTimer()
        {
            if (UnsafeNativeMethods.QueryPerformanceFrequency(out frequency))
            {
                isPerfCounterSupported = true;
            }
            else
            {
                frequency = 1000;
            }
        }

        public long Frequency
        {
            get
            {
                return frequency;
            }
        }

        public long Value
        {
            get
            {
                long tickCount = 0;

                if (isPerfCounterSupported)
                {
                    UnsafeNativeMethods.QueryPerformanceCounter(out tickCount);
                    return tickCount;
                }
                else
                {
                    return Environment.TickCount;
                }
            }
        }
    }

    public class Log
    {

    }

    public class Random
    {
        private System.Random rnd = new System.Random();

        public double Sample()
        {
            return rnd.NextDouble();
        }

        public int Range(int min, int max)
        {
            return rnd.Next(min, max + 1);
        }

        public double Range(double min, double max)
        {
            if (min > max)
            {
                Util.Swap(ref min, ref max);
            }

            return (rnd.NextDouble() * (max - min)) + min;
        }
    }

    public enum CompressionType
    {
        Deflate,
        GZip
    }

    public static class Compression
    {
        public static void CompressFile(string sourceFile, string destinationFile, CompressionType compressType)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (string.IsNullOrWhiteSpace(destinationFile))
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            Stream streamCompressed;

            using (var streamSource = new FileStream(sourceFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 4096))
            {
                using (var streamDestination = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: 4096))
                {
                    var fileLength = streamSource.Length;

                    var size = BitConverter.GetBytes(fileLength);
                    streamDestination.Write(size, 0, size.Length);

                    var data = new byte[fileLength];
                    streamSource.Read(data, 0, data.Length);

                    var compressedDataStream = new MemoryStream();

                    if (compressType == CompressionType.Deflate)
                    {
                        streamCompressed = new DeflateStream(compressedDataStream, CompressionMode.Compress);
                    }
                    else
                    {
                        streamCompressed = new GZipStream(compressedDataStream, CompressionMode.Compress);
                    }

                    using (streamCompressed)
                    {
                        streamCompressed.Write(data, 0, data.Length);
                    }

                    var compressedData = compressedDataStream.GetBuffer();

                    size = BitConverter.GetBytes(compressedData.Length);
                    streamDestination.Write(size, 0, size.Length);

                    streamDestination.Write(compressedData, 0, compressedData.Length);
                }
            }
        }

        public static void DecompressFile(string sourceFile, string destinationFile, CompressionType compressionType)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (string.IsNullOrWhiteSpace(destinationFile))
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            Stream streamUncompressed = null;

            using (var streamSource = new FileStream(sourceFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, bufferSize: 4096))
            {
                using (var streamDestination = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: 4096))
                {
                    var size = new byte[sizeof(long)];
                    streamSource.Read(size, 0, size.Length);

                    var fileLength = BitConverter.ToInt64(size, 0);

                    size = new byte[sizeof(int)];
                    streamSource.Read(size, 0, size.Length);

                    var storedSize = BitConverter.ToInt32(size, 0);

                    var uncompressedData = new byte[fileLength];

                    var compressedData = new byte[storedSize];

                    streamSource.Read(compressedData, 0, compressedData.Length);

                    var uncompressedDataStream = new MemoryStream(compressedData);

                    if (compressionType == CompressionType.Deflate)
                    {
                        streamUncompressed = new DeflateStream(uncompressedDataStream, CompressionMode.Decompress);
                    }
                    else
                    {
                        streamUncompressed = new GZipStream(uncompressedDataStream, CompressionMode.Decompress);
                    }

                    using (streamUncompressed)
                    {
                        streamUncompressed.Read(uncompressedData, 0, uncompressedData.Length);
                    }

                    streamDestination.Write(uncompressedData, 0, uncompressedData.Length);
                }
            }
        }
    }
}
