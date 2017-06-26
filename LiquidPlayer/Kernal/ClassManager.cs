using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Kernal
{
    public class SuperClass : Class
    {
        public int Clone
        {
            get;
            set;
        }

        public int Compare 
        {
            get;
            set;
        }

        public int Callback 
        {
            get;
            set;
        }

        public int Update 
        {
            get;
            set;
        }

        public int Render
        {
            get;
            set;
        }

        public int Shutdown 
        {
            get;
            set;
        }

        public List<Field> Fields
        {
            get;
            set;
        }

        public List<Method> Methods
        {
            get;
            set;
        }

        public List<Function> Functions
        {
            get;
            set;
        }
    }

    public class ClassManager
    {
        private DSL.FreeList<SuperClass> bag = new DSL.FreeList<SuperClass>();

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

        public LiquidClass New(string tag)
        {
            var item = new SuperClass
            {
                TaskId = 0,
                Tag = tag,
                BaseLiquidClass = LiquidClass.None,
                IsAbstract = false,
                IsFinal = false,
                MemoryRequired = 0,
                Clone = -1,
                Compare = -1,
                Callback = -1,
                Update = -1,
                Render = -1,
                Shutdown = -1,
                Fields = new List<Field>(),
                Methods = new List<Method>(),
                Functions = new List<Function>()
            };

            var liquidClass = (LiquidClass)bag.New(0, item);

            return liquidClass;
        }

        public SuperClass this[LiquidClass id]
        {
            get
            {
                return bag[(int)id];
            }
            set
            {
                bag[(int)id] = value;
            }
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

        public Class Read()
        {
            return bag.Read();
        }

        private string getTag(LiquidClass liquidClass)
        {
            switch (liquidClass)
            {
                case LiquidClass.None:
                    return "";
                case LiquidClass.Subclass:
                    return "subclass";
                case LiquidClass.Boolean:
                    return "boolean";
                case LiquidClass.Byte:
                    return "byte";
                case LiquidClass.Short:
                    return "short";
                case LiquidClass.Int:
                    return "int";
                case LiquidClass.Long:
                    return "long";
                case LiquidClass.Float:
                    return "float";
                case LiquidClass.Double:
                    return "double";
                case LiquidClass.String:
                    return "string";
            }

            return bag[(int)liquidClass].Tag;
        }

        public string GetTag(LiquidClass liquidClass, LiquidClass liquidSubclass = LiquidClass.None)
        {
            var tag = getTag(liquidClass);

            if (liquidSubclass != LiquidClass.None)
            {
                tag += "<" + getTag(liquidSubclass) + ">";
            }

            return tag;
        }

        public LiquidClass Find(string tag)
        {
            switch (tag)
            {
                case "subclass":
                    return LiquidClass.Subclass;
                case "boolean":
                    return LiquidClass.Boolean;
                case "byte":
                    return LiquidClass.Byte;
                case "short":
                    return LiquidClass.Short;
                case "int":
                    return LiquidClass.Int;
                case "long":
                    return LiquidClass.Long;
                case "float":
                    return LiquidClass.Float;
                case "double":
                    return LiquidClass.Double;
                case "string":
                    return LiquidClass.String;
            }

            Head();

            while (Cursor != 0)
            {
                if (Read().Tag == tag)
                {
                    return (LiquidClass)Cursor;
                }

                Next();
            }

            return LiquidClass.None;
        }

        public void Find(string tag, out LiquidClass liquidClass, out LiquidClass liquidSubclass)
        {
            liquidClass = LiquidClass.None;
            liquidSubclass = LiquidClass.None;

            var pos = tag.IndexOf("<");

            if (pos == -1)
            {
                liquidClass = Find(tag);
            }
            else
            {
                var liquidClassTag = tag.Substring(0, pos);
                var liquidSubclassTag = tag.Substring(pos + 1).Trim(' ', '<', '>');

                liquidClass = Find(liquidClassTag);
                liquidSubclass = Find(liquidSubclassTag);

                if (!IsA(liquidClass, LiquidClass.Collection))
                {
                    throw new Exception("Can't use subclass without collection");
                }
            }
        }

        public void Bind(LiquidClass liquidClass, int taskId, LiquidClass baseLiquidClass, int memoryRequired)
        {
            var id = (int)liquidClass;

            var baseId = (int)baseLiquidClass;

            bag[id].TaskId = taskId;
            bag[id].BaseLiquidClass = baseLiquidClass;
            bag[id].MemoryRequired = memoryRequired;

            bag[id].Clone = bag[baseId].Clone;
            bag[id].Callback = bag[baseId].Callback;
            bag[id].Update = bag[baseId].Update;
            bag[id].Render = bag[baseId].Render;
            bag[id].Shutdown = bag[baseId].Shutdown;

            Extends(liquidClass, baseLiquidClass);
        }

        public bool IsBuiltInType(LiquidClass liquidClass)
        {
            return (liquidClass < 0);
        }

        public bool IsPredefinedType(LiquidClass liquidClass)
        {
            var max = Enum.GetValues(typeof(LiquidClass)).Cast<int>().Max();

            return (liquidClass > 0 && (int)liquidClass <= max);
        }

        public bool IsCustomType(LiquidClass liquidClass)
        {
            var max = Enum.GetValues(typeof(LiquidClass)).Cast<int>().Max();

            return ((int)liquidClass > max);
        }

        public void Extends(LiquidClass liquidClass, LiquidClass baseLiquidClass = LiquidClass.Object)
        {
            var item = bag[(int)liquidClass];

            var baseItem = bag[(int)baseLiquidClass];

            item.BaseLiquidClass = baseLiquidClass;

            if (baseLiquidClass != LiquidClass.None)
            {
                foreach (var field in baseItem.Fields)
                {
                    item.Fields.Add(field);
                }

                foreach (var method in baseItem.Methods)
                {
                    if (method.Tag == "Constructor")
                    {
                        var emptyMethod = new Method();

                        item.Methods.Add(emptyMethod);
                    }
                    else
                    {
                        item.Methods.Add(method.Clone());
                    }
                }

                foreach (var function in baseItem.Functions)
                {
                    item.Functions.Add(function.Clone());
                }
            }
        }

        public bool IsA(LiquidClass liquidClass, LiquidClass baseLiquidClass)
        {
            if (liquidClass == baseLiquidClass)
            {
                return true;
            }
            else if (liquidClass < 0 || baseLiquidClass < 0)
            {
                return false;
            }

            var id = liquidClass;

            while (id != 0)
            {
                if (id == baseLiquidClass)
                {
                    return true;
                }

                id = bag[(int)id].BaseLiquidClass;
            }

            return false;
        }

        public void BindMethod(LiquidClass liquidClass, string tag, int address)
        {
            var item = bag[(int)liquidClass];

            switch (tag)
            {
                case "Clone":
                    item.Clone = address;
                    break;
                case "Callback":
                    item.Callback = address;
                    break;
                case "Update":
                    item.Update = address;
                    break;
                case "Render":
                    item.Render = address;
                    break;
                case "Shutdown":
                    item.Shutdown = address;
                    break;
            }
        }

        public int AddVirtualMethod(LiquidClass liquidClass, string tag, string parameters, LiquidType returnLiquidType, string stub, FunctionDelegate functionDelegate)
        {
            BindMethod(liquidClass, tag, -1);

            var item = bag[(int)liquidClass];

            item.Methods.Add(new Method
            {
                AccessModifier = AccessModifier.Public,
                IsVirtual = true,
                ClassTag = item.Tag,
                Tag = tag,
                Parameters = parameters,
                ReturnLiquidType = returnLiquidType,
                Stub = stub,
                Target = new Target(functionDelegate),
                MemoryRequired = 0
            });

            return item.Methods.Count - 1;
        }

        public int AddVirtualMethod(LiquidClass liquidClass, string tag, string parameters, LiquidType returnLiquidType, int address)
        {
            BindMethod(liquidClass, tag, address);

            var item = bag[(int)liquidClass];

            item.Methods.Add(new Method
            {
                AccessModifier = AccessModifier.Public,
                IsVirtual = true,
                ClassTag = item.Tag,
                Tag = tag,
                Parameters = parameters,
                ReturnLiquidType = returnLiquidType,
                Stub = "",
                Target = new Target(address),
                MemoryRequired = 0
            });

            return item.Methods.Count - 1;
        }

        public int FindVirtualMethod(LiquidClass liquidClass, string methodTag, string parameters = "()", LiquidType returnLiquidType = default(LiquidType))
        {
            var item = bag[(int)liquidClass];

            var classTag = item.Tag;

            for (var index = 0; index < item.Methods.Count; index++)
            {
                var method = item.Methods[index];

                if (method.Tag == methodTag && method.Parameters == parameters && method.ReturnLiquidType.Combo == returnLiquidType.Combo)
                {
                    return index;
                }
            }

            return -1;
        }

        public void SetVirtualMethodTarget(LiquidClass liquidClass, int index, string stub, FunctionDelegate functionDelegate)
        {
            var item = bag[(int)liquidClass];

            item.Methods[index].Stub = stub;
            item.Methods[index].Target = new Target(functionDelegate);
        }

        public void SetVirtualMethodTarget(LiquidClass liquidClass, int index, int address)
        {
            var item = bag[(int)liquidClass];

            item.Methods[index].Stub = "";
            item.Methods[index].Target = new Target(address);
        }

        public void BindFunction(LiquidClass liquidClass, string tag, int address)
        {
            var item = bag[(int)liquidClass];

            if (tag == "Compare")
            {
                item.Compare = address;
            }
        }

        public int AddFunction(LiquidClass liquidClass, string tag, string parameters, LiquidType returnLiquidType, int address)
        {
            BindFunction(liquidClass, tag, address);

            var item = bag[(int)liquidClass];

            item.Functions.Add(new Function
            {
                AccessModifier = AccessModifier.Public,
                ClassTag = item.Tag,
                Tag = tag,
                Parameters = parameters,
                ReturnLiquidType = returnLiquidType,
                Stub = "",
                Target = new Target(address)
            });

            return item.Functions.Count - 1;
        }

        public int FindFunction(LiquidClass liquidClass, string functionTag, string parameters = "()", LiquidType returnLiquidType = default(LiquidType))
        {
            var item = bag[(int)liquidClass];

            var classTag = item.Tag;

            for (var index = 0; index < item.Functions.Count; index++)
            {
                var function = item.Functions[index];

                if (function.ClassTag == classTag && function.Tag == functionTag && function.Parameters == parameters && function.ReturnLiquidType.Combo == returnLiquidType.Combo)
                {
                    return index;
                }
            }

            return -1;
        }

        public void Free(LiquidClass classId)
        {
            bag.Free((int)classId);
        }
    }
}
