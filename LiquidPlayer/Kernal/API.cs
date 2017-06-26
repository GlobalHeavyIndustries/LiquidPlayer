using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Kernal
{
    public class API
    {
        private DSL.FreeList<Function> bag = new DSL.FreeList<Function>();

        public int New(Function item)
        {
            return bag.New(0, item);
        }

        public Function this[int id]
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

        public int AddFunction(LiquidClass liquidClass, string tag, string parameters, LiquidClass returnLiquidClass, LiquidClass returnLiquidSubclass, string stub, FunctionDelegate functionDelegate)
        {
            var classTag = Program.ClassManager.GetTag(liquidClass);

            bag.New(0, new Function
            {
                AccessModifier = AccessModifier.Public,
                ClassTag = classTag,
                Tag = tag,
                Parameters = parameters,
                ReturnLiquidType = new LiquidType(returnLiquidClass, returnLiquidSubclass),
                Inline = pCode.None,
                Stub = stub,
                Target = new Target(functionDelegate),
                MemoryRequired = 0
            });

            return bag.Count;
        }

        public int AddFunction(LiquidClass liquidClass, string tag, string parameters, LiquidClass returnLiquidClass, LiquidClass returnLiquidSubclass, int address)
        {
            var classTag = Program.ClassManager.GetTag(liquidClass);

            bag.New(0, new Function
            {
                AccessModifier = AccessModifier.Public,
                ClassTag = classTag,
                Tag = tag,
                Parameters = parameters,
                ReturnLiquidType = new LiquidType(returnLiquidClass, returnLiquidSubclass),
                Inline = pCode.None,
                Target = new Target(address),
                MemoryRequired = 0
            });

            return bag.Count;
        }

        public void Free(int id)
        {
            bag.Free(id);
        }
    }
}
