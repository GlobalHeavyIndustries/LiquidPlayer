using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiquidPlayer.Exec
{
    public class Sweeper
    {
        private Queue<int> objectQueue = new Queue<int>();
        private Queue<int> taskQueue = new Queue<int>();

        public void Add(int id)
        {
            Debug.Assert(id != 0);
            Debug.Assert(Program.Exec.ObjectManager.IsA(id, LiquidClass.Object));

            if (Program.Exec.ObjectManager.IsA(id, LiquidClass.Task))
            {
                taskQueue.Enqueue(id);
            }
            else
            {
                objectQueue.Enqueue(id);
            }
        }

        private void freeObjects()
        {
            while (objectQueue.Count != 0)
            {
                var id = objectQueue.Dequeue();

                Program.Exec.ObjectManager.Free(id);
            }
        }

        public void Run()
        {
            freeObjects();

            while (taskQueue.Count != 0)
            {
                var taskId = taskQueue.Dequeue();

                UnloadTask(taskId);

                var list = Program.Exec.ObjectManager.GetLeftoverObjects(taskId);

                Debug.Assert(list.Count == 0);
            }
        }

        public void UnloadTask(int taskId)
        {
            Program.Exec.RemoveTask(taskId);

            var task = Program.Exec.ObjectManager[taskId].LiquidObject as Liquid.Task;

            if (task.PCB.State == Liquid.Task.ProcessState.Finished)
            {
                Program.Exec.ObjectManager.Free(taskId);
            }
            else
            {
                // Something bad happened. Free all objects connected to this task using brute force.

                var tree = task.GetTree();

                while (tree.Count != 0)
                {
                    var id = tree[0];

                    tree.RemoveAt(0);

                    Program.Exec.ObjectManager.Free(id);
                }
            }

            freeObjects();
        }
    }
}
