using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public delegate int CloneDelegate(int ownerId, int id);
    public delegate int CompareDelegate(int lhs, int rhs);
    public delegate void FreeDelegate(int id);

    public class Collection : Object
    {
        protected int classId;

        public int ClassId
        {
            get
            {
                return classId;
            }
        }

        public Collection(int id, int classId)
            : base(id)
        {
            this.classId = classId;
        }

        public override string ToString()
        {
            return $"Collection";
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
