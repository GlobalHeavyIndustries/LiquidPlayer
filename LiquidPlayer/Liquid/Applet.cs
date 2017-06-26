using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Applet : Task
    {
        public static int NewApplet(string path, string arguments, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Applet);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Applet(id, path, arguments);

            return id;
        }

        protected Applet(int id, string path, string arguments)
            : base(id, path, arguments)
        {

        }

        public override string ToString()
        {
            return $"Applet (Tag: \"{tag}\"), Path: \"{path}\")";
        }

        protected override bool callback(int messageId)
        {
            return base.callback(messageId);
        }

        public override void UpdateScene()
        {
            base.UpdateScene();
        }

        public override void RenderScene()
        {
            base.RenderScene();
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}