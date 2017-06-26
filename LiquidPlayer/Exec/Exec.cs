using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using System.Diagnostics;

namespace LiquidPlayer.Exec
{
    //
    // The Multitasking Executive
    //

    public class Exec
    {
        private DSL.LongManager longManager = new DSL.LongManager();
        private DSL.DoubleManager doubleManager = new DSL.DoubleManager();
        private DSL.StringManager stringManager = new DSL.StringManager();

        private ObjectManager objectManager = new ObjectManager();
        private Router router = new Router();
        private Sweeper sweeper = new Sweeper();
        private VirtualMachine virtualMachine = new VirtualMachine();

        private List<int> tasks = new List<int>();

        private DSL.LinkedQueue<int> thisQueue = new DSL.LinkedQueue<int>();
        private DSL.LinkedQueue<int> thatQueue = new DSL.LinkedQueue<int>();
        private DSL.LinkedQueue<int> waitQueue = new DSL.LinkedQueue<int>();

        private int activeTask;

        private double[] watch = new double[10];

        public DSL.LongManager LongManager
        {
            get
            {
                return longManager;
            }
        }

        public DSL.DoubleManager DoubleManager
        {
            get
            {
                return doubleManager;
            }
        }

        public DSL.StringManager StringManager
        {
            get
            {
                return stringManager;
            }
        }

        public ObjectManager ObjectManager
        {
            get
            {
                return objectManager;
            }
        }

        public Router Router
        {
            get
            {
                return router;
            }
        }

        public Sweeper Sweeper
        {
            get
            {
                return sweeper;
            }
        }

        public VirtualMachine VirtualMachine
        {
            get
            {
                return virtualMachine;
            }
        }

        public int ActiveTask
        {
            get
            {
                return activeTask;
            }
        }

        public double[] Watch
        {
            get
            {
                return watch;
            }
            set
            {
                watch = value;
            }
        }

        public void AddTask(int taskId)
        {
            Debug.Assert(taskId != 0);
            Debug.Assert(ObjectManager.IsA(taskId, LiquidClass.Task));
            Debug.Assert(!tasks.Contains(taskId));

            tasks.Add(taskId);

            if (activeTask == 0)
            {
                activeTask = taskId;

                var task = ObjectManager[taskId].LiquidObject as Liquid.Task;

                Program.GameWindow.Title = task.Tag;
            }

            //Console.WriteLine("addTask --> " + taskId);
        }

        public void Schedule(int taskId)
        {
            Debug.Assert(taskId != 0);
            Debug.Assert(ObjectManager.IsA(taskId, LiquidClass.Task));
            Debug.Assert(!thisQueue.Contains(taskId) && !thatQueue.Contains(taskId) && !waitQueue.Contains(taskId));

            thisQueue.Enqueue(taskId);

            //Console.WriteLine("schedule --> " + taskId);
        }

        public void RemoveTask(int taskId)
        {
            Debug.Assert(taskId != 0);
            Debug.Assert(ObjectManager.IsA(taskId, LiquidClass.Task));
            Debug.Assert(tasks.Contains(taskId));
            Debug.Assert(thisQueue.Contains(taskId) || thatQueue.Contains(taskId) || waitQueue.Contains(taskId));

            if (taskId == activeTask)
            {
                activeTask = 0;
            }

            thisQueue.Remove(taskId);
            thatQueue.Remove(taskId);
            waitQueue.Remove(taskId);

            tasks.Remove(taskId);

            //Console.WriteLine("removeTask --> " + taskId);
        }

        public bool UpdateScene()
        {
            var postQuitMessage = false;

            var beginTime = Program.AtomicClock;

            Sprockets.Color.UpdatePlasmaColor();

            while (waitQueue.Count != 0)
            {
                var taskId = waitQueue.Dequeue();

                thisQueue.Enqueue(taskId);
            }

            while (Program.AtomicClock < beginTime + 13333)
            {
                if (thisQueue.Count == 0)
                {
                    if (thatQueue.Count == 0)
                    {
                        break;
                    }

                    Util.Swap(ref thisQueue, ref thatQueue);
                }

                var taskId = thisQueue.Dequeue();

                var task = objectManager[taskId].LiquidObject as Liquid.Task;

                if (task.PCB.State == Liquid.Task.ProcessState.Running || task.PCB.State == Liquid.Task.ProcessState.Waiting)
                {
                    task.RestoreState();

                    task.VRun();

                    task.SaveState();
                }

                if (task.PCB.State == Liquid.Task.ProcessState.Waiting)
                {
                    waitQueue.Enqueue(taskId);
                }
                else
                {
                    thatQueue.Enqueue(taskId);
                }

                if (task.IsDone())
                {
                    if (taskId == activeTask)
                    {
                        objectManager.Mark(taskId);

                        postQuitMessage = true;

                        break;
                    }
                }
                else
                {
                    task.RestoreState();

                    task.UpdateScene();

                    task.SaveState();
                }
            }

            router.Run();

            sweeper.Run();

            var endTime = Program.AtomicClock;

            var elapsedTime = endTime - beginTime;

            System.Threading.Thread.Sleep(0);

            return postQuitMessage;
        }

        public void RenderScene()
        {
            if (activeTask != 0)
            {
                var task = objectManager[activeTask].LiquidObject as Liquid.Task;

                task.RestoreState();

                task.RenderScene();

                task.SaveState();
            }


            var error = Sprockets.Graphics.GetError();

            Debug.Assert(error == OpenTK.Graphics.OpenGL.ErrorCode.NoError);
        }
    }
}
