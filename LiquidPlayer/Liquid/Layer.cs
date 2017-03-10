using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Layer : GEL
    {
        public Layer(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"Layer";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
