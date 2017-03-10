using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class View : GEL
    {
        public View(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"View";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
