using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Message : Object
    {
        protected int from;
        protected int to;
        protected MessageBody body;
        protected string data;

        public static int NewMessage(int from, int to, MessageBody body, string data, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Message);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Message(id, from, to, body, data);

            return id;
        }

        public Message(int id, int from, int to, MessageBody body, string data)
            : base(id)
        {
            this.from = from;
            this.to = to;
            this.body = body;
            this.data = data;
        }

        public override string ToString()
        {
            return $"Message {body} From {from} To {to}: Data \"{data}\"";
        }

        public bool IsFrom(int from)
        {
            return (this.from == from);
        }

        public int GetFrom()
        {
            return from;
        }

        public bool IsTo(int to)
        {
            return (this.to == to);
        }

        public int GetTo()
        {
            return to;
        }

        public MessageBody GetBody()
        {
            return body;
        }

        public string GetData()
        {
            return data;
        }

        public void Dispatch()
        {
            var id = to;

            while (id != 0)
            {
                var obj = objectManager[id].LiquidObject as Object;

                if (obj.IsEnabled)
                {
                    var liquidClass = objectManager[id].LiquidClass;

                    var results = obj.VCallback(liquidClass, objectId);

                    if (results || obj.IsA(LiquidClass.Task))
                    {
                        break;
                    }
                }

                id = objectManager[id].ParentId;
            }
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}
