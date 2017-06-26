using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiquidPlayer.Exec
{
    public struct Transaction : IComparable<Transaction>
    {
        public int Key
        {
            get;
            set;
        }

        public int MessageId
        {
            get;
            set;
        }

        public int DeliveryTime
        {
            get;
            set;
        }

        public int Delay
        {
            get;
            set;
        }

        public int Iterations
        {
            get;
            set;
        }

        public int CompareTo(Transaction other)
        {
            if (DeliveryTime < other.DeliveryTime)
            {
                return -1;
            }
            else if (DeliveryTime > other.DeliveryTime)
            {
                return 1;
            }

            return 0;
        }
    }

    public class Router
    {
        private int uniqueId = 1;
        private List<Transaction> transactionPrioritytQueue = new List<Transaction>();

        private int find(int key)
        {
            for (var index = 0; index < transactionPrioritytQueue.Count; index++)
            {
                if (transactionPrioritytQueue[index].Key == key)
                {
                    return index;
                }
            }

            return -1;
        }

        private int add(int messageId, int delay = 0, int iterations = 1)
        {
            var transaction = new Transaction
            {
                Key = uniqueId,
                MessageId = messageId,
                DeliveryTime = Program.SystemClock + delay,
                Delay = delay,
                Iterations = iterations
            };

            enqueue(transaction);

            return uniqueId++;
        }

        private void enqueue(Transaction transaction)
        {
            var index = transactionPrioritytQueue.BinarySearch(transaction);

            if (index >= 0)
            {
                transactionPrioritytQueue.Insert(index, transaction);
            }
            else
            {
                transactionPrioritytQueue.Insert(~index, transaction);
            }
        }

        private void remove(int key)
        {
            var index = find(key);

            Debug.Assert(index != -1);

            transactionPrioritytQueue.RemoveAt(index);
        }

        public void CleanRouterQueue(int id)
        {
            var queue = new List<Transaction>(transactionPrioritytQueue.Capacity);

            for (var index = 0; index < transactionPrioritytQueue.Count; index++)
            {
                var transaction = transactionPrioritytQueue[index];

                var message = Program.Exec.ObjectManager[transaction.MessageId].LiquidObject as Liquid.Message;

                if (!message.IsTo(id))
                {
                    queue.Add(transaction);
                }
                else
                {
                    Program.Exec.ObjectManager.Mark(transaction.MessageId);
                }
            }

            transactionPrioritytQueue = queue;
        }

        public void Run()
        {
            var pulseTransactions = new Queue<Transaction>();

            while (transactionPrioritytQueue.Count != 0)
            {
                var transaction = transactionPrioritytQueue[0];

                if (Program.SystemClock >= transaction.DeliveryTime)
                {
                    transactionPrioritytQueue.RemoveAt(0);

                    var messageId = Program.Exec.ObjectManager.Copy(transaction.MessageId);

                    SendToTask(messageId);

                    if (transaction.Iterations != 0)
                    {
                        transaction.DeliveryTime += transaction.Delay;

                        if (transaction.Iterations >= 1)
                        {
                            transaction.Iterations--;
                        }

                        if (transaction.Iterations != 0)
                        {
                            pulseTransactions.Enqueue(transaction);
                        }
                    }

                    if (transaction.Iterations == 0)
                    {
                        Program.Exec.ObjectManager.Mark(messageId);
                    }
                }
                else
                {
                    break;
                }
            }

            while (pulseTransactions.Count != 0)
            {
                var transaction = pulseTransactions.Dequeue();

                enqueue(transaction);
            }
        }

        public void Send(int from, int to, MessageBody body, string data, int parentId = 0)
        {
            Debug.Assert(to != 0);
            Debug.Assert(Program.Exec.ObjectManager[to].LiquidClass != LiquidClass.None);

            var messageId = Liquid.Message.NewMessage(from, to, body, data, parentId);

            SendToTask(messageId);
        }

        public void Delay(int from, int to, MessageBody body, string data, int delay, int parentId = 0)
        {
            Debug.Assert(to != 0);
            Debug.Assert(Program.Exec.ObjectManager[to].LiquidClass != LiquidClass.None);
            Debug.Assert(delay >= 100 && delay <= 86400000);

            var messageId = Liquid.Message.NewMessage(from, to, body, data, parentId);

            add(messageId, delay);
        }

        public void Pulse(int from, int to, MessageBody body, string data, int pulse, int iterations, int parentId = 0)
        {
            Debug.Assert(to != 0);
            Debug.Assert(Program.Exec.ObjectManager[to].LiquidClass != LiquidClass.None);
            Debug.Assert(pulse >= 100 && pulse <= 86400000);
            Debug.Assert(iterations == -1 || iterations >= 1);

            var messageId = Liquid.Message.NewMessage(from, to, body, data, parentId);

            add(messageId, pulse, iterations);
        }

        public void SendToTask(int messageId)
        {
            var message = Program.Exec.ObjectManager[messageId].LiquidObject as Liquid.Message;

            var to = message.GetTo();

            Debug.Assert(to != 0);
            Debug.Assert(Program.Exec.ObjectManager[to].LiquidClass != LiquidClass.None);

            var taskId = Program.Exec.ObjectManager[to].TaskId;
            var task = Program.Exec.ObjectManager[taskId].LiquidObject as Liquid.Task;

            task.EnqueueMessage(messageId);
        }
    }
}
