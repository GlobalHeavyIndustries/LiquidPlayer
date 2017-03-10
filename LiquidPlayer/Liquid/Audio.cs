using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Audio : Object
    {
        public Audio(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"Audio";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
