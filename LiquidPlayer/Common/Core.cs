using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer
{
    // Core.cs
    // Version 0.01
    // 2016-04-01

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

        public int Value
        {
            get;
            set;
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
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Property
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

        public FunctionDelegate Stub
        {
            get;
            set;
        }

        public int Address
        {
            get;
            set;
        }

        public int MemoryRequired
        {
            get;
            set;
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

        public FunctionDelegate Stub
        {
            get;
            set;
        }

        public int Address
        {
            get;
            set;
        }

        public int MemoryRequired
        {
            get;
            set;
        }
    }
}
