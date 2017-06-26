using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace LiquidPlayer
{
    // Core.cs
    // Version 0.01
    // 2017-06-01

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    [StructLayout(LayoutKind.Explicit)]
    public struct SmartPointer
    {
        [FieldOffset(0)]
        public long Address;

        [FieldOffset(0)]
        public int LoAddress;
        [FieldOffset(4)]
        public int HiAddress;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LiquidType
    {
        [FieldOffset(0)]
        public long Combo;

        [FieldOffset(0)]
        public LiquidClass Class;
        [FieldOffset(4)]
        public LiquidClass Subclass;

        public LiquidType(LiquidClass liquidClass, LiquidClass liquidSubclass = LiquidClass.None)
        {
            this.Combo = 0L;

            this.Class = liquidClass;
            this.Subclass = liquidSubclass;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class CodeTracker
    {
        public int FileNumber
        {
            get;
        }

        public int LineNumber
        {
            get;
        }

        public int LinePosition
        {
            get;
        }

        public int StartPosition
        {
            get;
            set;
        }

        public int EndPosition
        {
            get;
            set;
        }

        public CodeTracker(int fileNumber, int lineNumber, int linePosition)
        {
            FileNumber = fileNumber;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct Target
    {
        public FunctionDelegate FunctionDelegate
        {
            get;
            set;
        }

        public int Address
        {
            get;
            set;
        }

        public Target(FunctionDelegate stub)
        {
            FunctionDelegate = stub;
            Address = -1;
        }

        public Target(int address)
        {
            FunctionDelegate = null;
            Address = address;
        }

        public Target(Target other)
        {
            FunctionDelegate = other.FunctionDelegate;
            Address = other.Address;
        }

        public bool IsStub()
        {
            return (FunctionDelegate != null) ? true : false;
        }

        public bool IsAddress()
        {
            return (FunctionDelegate == null) ? true : false;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Class
    {
        public int TaskId
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public LiquidClass BaseLiquidClass
        {
            get;
            set;
        }

        public bool IsAbstract
        {
            get;
            set;
        }

        public bool IsFinal
        {
            get;
            set;
        }

        public int MemoryRequired
        {
            get;
            set;
        }

        public bool Used
        {
            get;
            set;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Constant
    {
        public string ClassTag
        {
            get;
            set;
        }

        public string FunctionTag
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public LiquidClass LiquidClass
        {
            get;
            set;
        }

        public int Int
        {
            get;
            set;
        }

        public double Double
        {
            get;
            set;
        }

        public Constant Clone()
        {
            var constant = new Constant()
            {
                ClassTag = this.ClassTag,
                FunctionTag = this.FunctionTag,
                Tag = this.Tag,
                LiquidClass = this.LiquidClass,
                Int = this.Int,
                Double = this.Double
            };

            return constant;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Field
    {
        public AccessModifier AccessModifier
        {
            get;
            set;
        }

        public bool IsRef
        {
            get;
            set;
        }

        public string ClassTag
        {
            get;
            set;
        }

        public string FunctionTag
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public LiquidType LiquidType
        {
            get;
            set;
        }

        public int Address
        {
            get;
            set;
        }

        public Field Clone()
        {
            var field = new Field()
            {
                AccessModifier = this.AccessModifier,
                IsRef = this.IsRef,
                ClassTag = this.ClassTag,
                FunctionTag = this.FunctionTag,
                Tag = this.Tag,
                LiquidType = this.LiquidType,
                Address = this.Address
            };

            return field;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Method
    {
        public AccessModifier AccessModifier
        {
            get;
            set;
        }

        public bool IsAbstract
        {
            get;
            set;
        }

        public bool IsVirtual
        {
            get;
            set;
        }

        public string ClassTag
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }

        public LiquidType ReturnLiquidType
        {
            get;
            set;
        }

        public string Stub
        {
            get;
            set;
        }

        public Target Target
        {
            get;
            set;
        }

        public int MemoryRequired
        {
            get;
            set;
        }

        public Method Clone()
        {
            var method = new Method()
            {
                AccessModifier = this.AccessModifier,
                IsAbstract = this.IsAbstract,
                IsVirtual = this.IsVirtual,
                ClassTag = this.ClassTag,
                Tag = this.Tag,
                Parameters = this.Parameters,
                ReturnLiquidType = this.ReturnLiquidType,
                Stub = this.Stub,
                Target = new Target(this.Target),
                MemoryRequired = this.MemoryRequired
            };

            return method;
        }
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Function
    {
        public AccessModifier AccessModifier
        {
            get;
            set;
        }

        public string ClassTag
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }

        public LiquidType ReturnLiquidType
        {
            get;
            set;
        }

        public pCode Inline
        {
            get;
            set;
        }

        public string Stub
        {
            get;
            set;
        }

        public Target Target
        {
            get;
            set;
        }

        public int MemoryRequired
        {
            get;
            set;
        }

        public Function Clone()
        {
            var function = new Function()
            {
                AccessModifier = this.AccessModifier,
                ClassTag = this.ClassTag,
                Tag = this.Tag,
                Parameters = this.Parameters,
                ReturnLiquidType = this.ReturnLiquidType,
                Inline = this.Inline,
                Stub = this.Stub,
                Target = new Target(this.Target),
                MemoryRequired = this.MemoryRequired
            };

            return function;
        }
    }
}
