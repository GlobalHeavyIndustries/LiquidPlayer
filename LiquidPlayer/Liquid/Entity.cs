using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Entity : Object
    {
        protected bool running;

        protected List<int> updateList;

        public bool IsRunning
        {
            get
            {
                return running;
            }
        }

        public List<int> UpdateList
        {
            get
            {
                return updateList;
            }
        }

        public Entity(int id)
            : base(id)
        {
            this.running = false;
            this.updateList = new List<int>();
        }

        public override string ToString()
        {
            return $"Entity";
        }

        protected virtual void update()
        {
            //Throw(ExceptionCode.NotImplemented);
        }

        public virtual void Update(LiquidClass liquidClass)
        {
            var address = LiquidPlayer.Program.ClassManager[liquidClass].Update;

            if (address != -1)
            {
                LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId, address);
            }
            else
            {
                update();
            }

            var updateListClone = updateList.Clone();

            foreach (var entityId in updateListClone)
            {
                var entity = objectManager[entityId].LiquidObject as Entity;

                liquidClass = objectManager[entityId].LiquidClass;

                entity.Update(liquidClass);
            }
        }

        public void Start()
        {
            if (!running)
            {
                running = true;

                var task = objectManager.GetTask(objectId);

                task.Start(objectId);
            }
        }

        public void Stop()
        {
            if (running)
            {
                running = false;

                var task = objectManager.GetTask(objectId);

                task.Stop(objectId);
            }
        }

        public override void shutdown()
        {
            updateList = null;

            base.shutdown();
        }
    }
}