using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class FileSystem : Object
    {
        public FileSystem(int id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return $"File System";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
