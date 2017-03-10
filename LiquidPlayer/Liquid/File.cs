using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class File : Object
    {
        public File(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"File";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
