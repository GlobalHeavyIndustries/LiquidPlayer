using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LiquidPlayer.Liquid
{
    public class Task : Object
    {
        #region Signals
        [Flags]
        public enum Signals
        {
            None        = 0x00,
            CTRL_C      = 0x01,
            CTRL_D      = 0x02,
            CTRL_E      = 0x04,
            CTRL_F      = 0x08
        }
        #endregion

        #region ProcessControlBlock
        public enum ProcessState
        {
            None,
            New,
            Ready,
            Running,
            Waiting,
            Suspended,
            Crashed,
            Finished
        }

        public class ProcessControlBlock
        {
            public ProcessState State
            {
                get;
                set;
            }

            public bool IsTock
            {
                get;
                set;
            }

            public bool IsTrap
            {
                get;
                set;
            }

            public int Quantum
            {
                get;
                set;
            }

            public int Elapsed
            {
                get;
                set;
            }

            public int PC
            {
                get;
                set;
            }

            public int DS
            {
                get;
                set;
            }

            public int A0
            {
                get;
                set;
            }

            public int A1
            {
                get;
                set;
            }

            public long C0
            {
                get;
                set;
            }

            public long C1
            {
                get;
                set;
            }

            public double D0
            {
                get;
                set;
            }

            public double D1
            {
                get;
                set;
            }

            public SmartPointer BX
            {
                get;
                set;
            }

            public int SP
            {
                get;
                set;
            }

            public int BP
            {
                get;
                set;
            }

            public int SZ
            {
                get;
                set;
            }

            public int ID
            {
                get;
                set;
            }

            public int LP
            {
                get;
                set;
            }

            public int LN
            {
                get;
                set;
            }

            public int FN
            {
                get;
                set;
            }
        }
        #endregion

        public ProcessControlBlock PCB
        {
            get;
            set;
        }

        protected string tag;
        protected bool debugMode;

        protected string filePath;
        protected string fileName;
        protected string arguments;
        protected string currentDirectory;

        protected string[] fileTable;
        protected string[] stringTable;
        protected int[] aliasTable;

        protected int[] codeSpace;
        protected int[] stack;
        protected Queue<int> messageQueue;

        protected Signals signals;

        protected int alarmClock;
        protected ProcessState prevState;

        protected int commandLineId;
        protected CommandLine commandLine;

        protected int standardInputId;
        protected Stream standardInput;

        protected int standardOutputId;
        protected Stream standardOutput;

        protected int standardErrorId;
        protected Stream standardError;

        protected int exceptionId;
        protected Exception exception;

        protected ExceptionCode throwCode;
        protected string throwData;
        protected int exitCode;

        public string Tag
        {
            get
            {
                return tag;
            }
        }

        public string CurrentDirectory
        {
            get
            {
                return currentDirectory;
            }
            set
            {
                currentDirectory = value;
            }
        }

        public string[] FileTable
        {
            get
            {
                return fileTable;
            }
        }

        public string[] StringTable
        {
            get
            {
                return stringTable;
            }
        }

        public int[] AliasTable
        {
            get
            {
                return aliasTable;
            }
        }

        public int[] DataSpace
        {
            get
            {
                return dataSpace;
            }
        }

        public int[] CodeSpace
        {
            get
            {
                return codeSpace;
            }
        }

        public int[] Stack
        {
            get
            {
                return stack;
            }
        }

        public int AlarmClock
        {
            get
            {
                return alarmClock;
            }
            set
            {
                alarmClock = value;
            }
        }

        public ExceptionCode ThrowCode
        {
            get
            {
                return throwCode;
            }
        }

        public string ThrowData
        {
            get
            {
                return throwData;
            }
        }

        public int ExitCode
        {
            get
            {
                return exitCode;
            }
        }

        public static int NewTask(string fileName, string arguments, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Task);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Task(id, fileName, arguments);

            return id;
        }

        public Task(int id, string fileName, string arguments)
            : base(id)
        {
            this.PCB = new ProcessControlBlock()
            {
                State = ProcessState.New,
                Quantum = 3333,
                DS = id,
                SZ = LiquidPlayer.Program.VM_STACK_SIZE,
                ID = id
            };

            this.tag = "";
            this.debugMode = false;

            var filePath = Path.GetDirectoryName(fileName);

            if (filePath == "")
            {
                filePath = Directory.GetCurrentDirectory();
            }

            this.filePath = filePath;
            this.fileName = Path.GetFileName(fileName);
            this.arguments = arguments;
            this.currentDirectory = filePath;

            this.fileTable = null;
            this.stringTable = null;
            this.aliasTable = null;

            this.codeSpace = null;
            this.stack = new int[PCB.SZ];
            this.messageQueue = new Queue<int>();

            this.alarmClock = 0;

            this.commandLineId = CommandLine.NewCommandLine(arguments, id);
            this.commandLine = objectManager[commandLineId].LiquidObject as CommandLine;

            this.standardInputId = Pipe.NewPipe(id);
            this.standardInput = objectManager[standardInputId].LiquidObject as Stream;

            standardInput.Open();

            this.standardOutputId = Pipe.NewPipe(id);
            this.standardOutput = objectManager[standardOutputId].LiquidObject as Stream;

            standardOutput.Open();

            this.standardErrorId = Pipe.NewPipe(id);
            this.standardError = objectManager[standardErrorId].LiquidObject as Stream;

            standardError.Open();

            loadTask(fileName, arguments);

            this.exceptionId = 0;
            this.exception = null;

            this.throwCode = ExceptionCode.None;
            this.throwData = "";
            this.exitCode = 0;

            this.PCB.State = ProcessState.Ready;
        }

        public override string ToString()
        {
            return $"Task (Tag: \"{tag}\")";
        }

        private int readInt(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        private long readLong(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        private double readDouble(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        private string readString(BinaryReader reader)
        {
            var length = reader.ReadInt32();

            return Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        private int[] readIntArray(BinaryReader reader)
        {
            var length = reader.ReadInt32() / 4;

            var data = new int[length];

            if (length > 0)
            {
                for (var index = 0; index < length; index++)
                {
                    data[index] = reader.ReadInt32();
                }
            }

            return data;
        }

        private void loadFileTable(BinaryReader reader)
        {
            var count = readInt(reader);

            fileTable = new string[count + 1];

            for (var i = 1; i <= count; i++)
            {
                var data = readString(reader);
                fileTable[i] = data;
            }
        }

        private void loadStringTable(BinaryReader reader)
        {
            var count = readInt(reader);

            stringTable = new string[count + 1];

            for (var i = 1; i <= count; i++)
            {
                var data = readString(reader);
                stringTable[i] = data;
            }
        }

        private LiquidClass bindNativeClass(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var baseLiquidClassTag = "";
            var baseLiquidClass = LiquidClass.None;

            var alias = readString(reader);

            liquidClassTag = alias;

            baseLiquidClassTag = readString(reader);

            if (baseLiquidClassTag != "")
            {
                baseLiquidClass = LiquidPlayer.Program.ClassManager.Find(baseLiquidClassTag);

                if (baseLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }
            }

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                Throw(ExceptionCode.OutOfMemory);
                return 0;
            }

            return liquidClass;
        }

        private LiquidClass bindClass(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var baseLiquidClassTag = "";
            var baseLiquidClass = LiquidClass.None;

            var alias = readString(reader);

            liquidClassTag = alias;

            baseLiquidClassTag = readString(reader);

            if (baseLiquidClassTag != "")
            {
                baseLiquidClass = LiquidPlayer.Program.ClassManager.Find(baseLiquidClassTag);

                if (baseLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }
            }

            var memoryRequired = readInt(reader);

            liquidClass = LiquidPlayer.Program.ClassManager.New(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                Throw(ExceptionCode.OutOfMemory);
                return 0;
            }

            LiquidPlayer.Program.ClassManager.Bind(liquidClass, objectId, baseLiquidClass, memoryRequired);

            return liquidClass;
        }

        private int bindNativeMethod(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var methodTag = "";
            var methodId = 0;
            var parameters = "";
            var returnLiquidClassTag = "";
            var returnLiquidClass = LiquidClass.None;
            var returnLiquidSubclass = LiquidClass.None;

            var alias = readString(reader);

            var index = alias.IndexOf('|');

            liquidClassTag = alias.Substring(0, index);

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                throw new System.Exception("Class not found");
            }
            alias = alias.Substring(index + 1);

            index = alias.IndexOf(':');

            if (index == 0)
            {
                alias = alias.Substring(1);

                returnLiquidClass = LiquidClass.None;
            }
            else
            {
                returnLiquidClassTag = alias.Substring(0, index);

                LiquidPlayer.Program.ClassManager.Find(returnLiquidClassTag, out returnLiquidClass, out returnLiquidSubclass);

                if (returnLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            methodTag = alias.Substring(0, index);

            var stub = LiquidPlayer.Program.StandardLibrary.BindMethod(liquidClass, methodTag, parameters, returnLiquidClass, returnLiquidSubclass);

            if (stub == null)
            {
                throw new System.Exception("Native method not found");
            }

            methodId = LiquidPlayer.Program.ClassManager.FindMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

            if (methodId == 0)
            {
                methodId = LiquidPlayer.Program.ClassManager.AddMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), stub);
            }

            if (methodId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);

                return 0;
            }

            return methodId;
        }

        private int bindVirtualMethod(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var methodTag = "";
            var methodId = 0;
            var parameters = "";
            var returnLiquidClassTag = "";
            var returnLiquidClass = LiquidClass.None;
            var returnLiquidSubclass = LiquidClass.None;

            var alias = readString(reader);

            var index = alias.IndexOf('|');

            liquidClassTag = alias.Substring(0, index);

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                throw new System.Exception("Class not found");
            }
            alias = alias.Substring(index + 1);

            index = alias.IndexOf(':');

            if (index == 0)
            {
                alias = alias.Substring(1);

                returnLiquidClass = LiquidClass.None;
            }
            else
            {
                returnLiquidClassTag = alias.Substring(0, index);

                LiquidPlayer.Program.ClassManager.Find(returnLiquidClassTag, out returnLiquidClass, out returnLiquidSubclass);

                if (returnLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            methodTag = alias.Substring(0, index);

            var type = readInt(reader);

            if (type == 1)
            {
                var stub = LiquidPlayer.Program.StandardLibrary.BindMethod(liquidClass, methodTag, parameters, returnLiquidClass, returnLiquidSubclass);

                if (stub == null)
                {
                    throw new System.Exception("Virtual method not found");
                }

                methodId = LiquidPlayer.Program.ClassManager.FindMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

                if (methodId == 0)
                {
                    methodId = LiquidPlayer.Program.ClassManager.AddVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), stub);
                }
            }
            else
            {
                var address = readInt(reader);

                methodId = LiquidPlayer.Program.ClassManager.FindMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

                if (methodId == 0)
                {
                    methodId = LiquidPlayer.Program.ClassManager.AddVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), address);
                }
            }

            if (methodId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);

                return 0;
            }

            return methodId;
        }

        private int bindFunction(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var functionTag = "";
            var functionId = 0;
            var parameters = "";
            var returnLiquidClassTag = "";
            var returnLiquidClass = LiquidClass.None;
            var returnLiquidSubclass = LiquidClass.None;

            var alias = readString(reader);

            var index = alias.IndexOf('|');

            liquidClassTag = alias.Substring(0, index);

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                throw new System.Exception("Class not found");
            }
            alias = alias.Substring(index + 1);

            index = alias.IndexOf(':');

            if (index == 0)
            {
                alias = alias.Substring(1);

                returnLiquidClass = LiquidClass.None;
            }
            else
            {
                returnLiquidClassTag = alias.Substring(0, index);

                LiquidPlayer.Program.ClassManager.Find(returnLiquidClassTag, out returnLiquidClass, out returnLiquidSubclass);

                if (returnLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            functionTag = alias.Substring(0, index);

            var stub = LiquidPlayer.Program.StandardLibrary.BindFunction(liquidClass, functionTag, parameters, returnLiquidClass, returnLiquidSubclass);

            if (stub == null)
            {
                throw new System.Exception("Function not found");
            }

            functionId = LiquidPlayer.Program.API.AddFunction(liquidClass, functionTag, parameters, returnLiquidClass, returnLiquidSubclass, stub);

            if (functionId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);

                return 0;
            }

            return functionId;
        }

        private int bindReservedFunction(BinaryReader reader)
        {
            var liquidClassTag = "";
            var liquidClass = LiquidClass.None;

            var functionTag = "";
            var functionId = 0;
            var parameters = "";
            var returnLiquidClassTag = "";
            var returnLiquidClass = LiquidClass.None;
            var returnLiquidSubclass = LiquidClass.None;

            var alias = readString(reader);

            var index = alias.IndexOf('|');

            liquidClassTag = alias.Substring(0, index);

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                throw new System.Exception("Class not found");
            }
            alias = alias.Substring(index + 1);

            index = alias.IndexOf(':');

            if (index == 0)
            {
                alias = alias.Substring(1);

                returnLiquidClass = LiquidClass.None;
            }
            else
            {
                returnLiquidClassTag = alias.Substring(0, index);

                LiquidPlayer.Program.ClassManager.Find(returnLiquidClassTag, out returnLiquidClass, out returnLiquidSubclass);

                if (returnLiquidClass == LiquidClass.None)
                {
                    throw new System.Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            functionTag = alias.Substring(0, index);

            var address = readInt(reader);

            functionId = LiquidPlayer.Program.ClassManager.FindFunction(liquidClass, functionTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

            if (functionId == 0)
            {
                functionId = LiquidPlayer.Program.ClassManager.AddFunction(liquidClass, functionTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), address);
            }
            
            if (functionId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);

                return 0;
            }

            return functionId;
        }

        private void loadAliasTable(BinaryReader reader)
        {
            var count = readInt(reader);

            aliasTable = new int[count + 1];

            for (var i = 1; i <= count; i++)
            {
                var liquidClass = LiquidClass.None;

                var methodId = 0;
                var functionId = 0;

                switch (readInt(reader))
                {
                    case 1:
                        liquidClass = bindNativeClass(reader);

                        aliasTable[i] = (int)liquidClass;

                        break;
                    case 2:
                        liquidClass = bindClass(reader);

                        aliasTable[i] = (int)liquidClass;

                        break;
                    case 3:
                        methodId = bindNativeMethod(reader);

                        aliasTable[i] = methodId;

                        break;
                    case 4:
                        methodId = bindVirtualMethod(reader);

                        aliasTable[i] = methodId;

                        break;
                    case 5:
                        functionId = bindFunction(reader);

                        aliasTable[i] = functionId;

                        break;
                    case 6:
                        functionId = bindReservedFunction(reader);

                        aliasTable[i] = functionId;

                        break;
                    default:
                        throw new System.Exception("Corrupted executable");
                }
            }
        }

        private void loadTask(string fileName, string commandLine)
        {
            var sourceFilePath = Path.GetDirectoryName(fileName);

            if (sourceFilePath == "")
            {
                sourceFilePath = Directory.GetCurrentDirectory();
            }

            var sourceFileName = Path.GetFileName(fileName);

            if (!sourceFileName.EndsWith(".ldx"))
            {
                sourceFileName += ".ldx";
            }

            if (!System.IO.File.Exists(sourceFilePath + "\\" + sourceFileName))
            {
                Throw(ExceptionCode.FileNotFound);
            }

            Compression.DecompressFile(sourceFilePath + "\\" + sourceFileName, LiquidPlayer.Program.TempPath + "temp.ldx", CompressionType.GZip);

            using (BinaryReader reader = new BinaryReader(System.IO.File.Open(LiquidPlayer.Program.TempPath + "temp.ldx", FileMode.Open)))
            {
                var header = Encoding.ASCII.GetString(reader.ReadBytes(8));

                if (header.Substring(0, 4) != "LDXv")
                {
                    Throw(ExceptionCode.Denied, "File header is corrupted");
                    return;
                }

                if (header.Substring(4, 4) != "2.00")
                {
                    Throw(ExceptionCode.Denied, "Unsupported file version");
                    return;
                }

                var taskType = readInt(reader);

                if (taskType < 1 || taskType > 4)
                {
                    Throw(ExceptionCode.Denied, "Unsupported task type");
                }

                tag = readString(reader);

                debugMode = (readInt(reader) == 1) ? true : false;

                loadFileTable(reader);

                loadStringTable(reader);

                loadAliasTable(reader);

                var memoryRequired = readInt(reader);

                dataSpace = new int[memoryRequired];

                codeSpace = readIntArray(reader);
            }
        }

        //public virtual void Hook(int id, int parentId)
        //{
        //    if (id == objectId)
        //    {
        //        return;
        //    }

        //    var parentId = objectManager[id].ParentId;

        //    if (parentId == objectId)
        //    {
        //        childrenList.Add(id);
        //    }
        //    else
        //    {
        //        var obj = objectManager[parentId].LiquidObject as Object;
        //        obj.ChildrenList.Add(id);
        //    }
        //}

        //public virtual void UnHook(int id)
        //{
        //    if (id == objectId)
        //    {
        //        return;
        //    }

        //    var parentId = objectManager[id].ParentId;

        //    if (parentId == objectId)
        //    {
        //        childrenList.Remove(id);
        //    }
        //    else
        //    {
        //        var obj = objectManager[parentId].LiquidObject as Object;
        //        obj.RenderList.Remove(id);
        //    }
        //}

        public virtual void NextStep()
        {
            LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId);

            if (IsDone())
            {
                standardInput.End();
                standardOutput.End();
                standardError.End();
            }
        }

        protected override bool callback(int messageId)
        {
            var message = objectManager[messageId].LiquidObject as Message;

            if (message.IsTo(objectId))
            {
                if (message.GetBody() == (int)MessageBody.KeyUp)
                {
                    switch (message.GetData())
                    {
                        case (int)LiquidKey.CTRL_C:
                            signals |= Signals.CTRL_C;
                            break;
                        case (int)LiquidKey.CTRL_D:
                            signals |= Signals.CTRL_D;
                            break;
                        case (int)LiquidKey.CTRL_E:
                            signals |= Signals.CTRL_E;
                            break;
                        case (int)LiquidKey.CTRL_F:
                            signals |= Signals.CTRL_F;
                            break;
                    }

                    return true;
                }
            }

            return base.callback(messageId);
        }

        public virtual void UpdateScene()
        {
            while (messageQueue.Count != 0)
            {
                var messageId = messageQueue.Dequeue();
                var message = objectManager[messageId].LiquidObject as Message;

                message.Dispatch();

                objectManager.Mark(messageId);
            }
        }

        public virtual void Start(int id)
        {

        }

        public virtual void Stop(int id)
        {

        }

        public virtual void RenderScene()
        {

        }

        public virtual void Show(int id)
        {

        }

        public virtual void Hide(int id)
        {

        }

        public bool Await(int taskId)
        {
            var task = objectManager.GetTask(taskId);

            return !IsDone(taskId);
        }

        public void CleanMessageQueue(int id)
        {
            var queue = new Queue<int>(messageQueue.Count);

            while (messageQueue.Count != 0)
            {
                var messageId = messageQueue.Dequeue();
                var message = objectManager[messageId].LiquidObject as Message;

                System.Diagnostics.Debug.Assert(messageId != id, "Does this ever happen?!");

                if (!message.IsTo(id))
                {
                    queue.Enqueue(messageId);
                }
            }

            messageQueue = queue;
        }

        public void ClearSignals()
        {
            signals = 0;
        }

        public void End(int taskId)
        {
            var task = objectManager.GetTask(taskId);

            task.PCB.State = ProcessState.Crashed;
        }

        public void EnqueueMessage(int messageId)
        {
            messageQueue.Enqueue(messageId);
        }

        public void ErrorOut(string data)
        {
            standardError.Write(data + (char)13 + (char)10);
        }

        public int GetCommandLine()
        {
            return objectManager.Copy(commandLineId);
        }

        public string GetError()
        {
            var data = new StringBuilder();

            var b = standardError.Read();

            while (b != Stream.END_OF_STREAM && b != 0 && b != 10)
            {
                if (b != 13)
                {
                    data.Append((char)b);
                }

                b = standardError.Read();
            }

            return data.ToString();
        }

        public int GetFileList(string searchPattern)
        {
            var listId = List.NewList((int)LiquidClass.String, objectId);
            var list = objectManager[listId].LiquidObject as List;

            var fileList = Directory.GetFiles(currentDirectory, searchPattern);

            for (var index = 0; index < fileList.Length; index++)
            {
                var id = NewString(objectId, fileList[index]);

                list.Add(id);

                FreeString(objectId, id);
            }

            return listId;
        }

        public int GetTaskList()
        {
            var listId = List.NewList((int)LiquidClass.String, objectId);
            var list = objectManager[listId].LiquidObject as List;

            var taskList = objectManager.GetChildrenTasks(objectId);

            for (var index = 0; index < taskList.Count; index++)
            {
                var taskId = taskList[index];
                var task = objectManager[taskId].LiquidObject as Task;

                var id = NewString(objectId, taskId + " : " + task.Tag);

                list.Add(id);

                FreeString(objectId, id);
            }

            return listId;
        }

        public int GetTaskList(int taskId)
        {
            var task = objectManager.GetTask(taskId);

            return task.GetTaskList();
        }

        public int GetStandardInput()
        {
            return objectManager.Copy(standardInputId);
        }

        public int GetStandardOutput()
        {
            return objectManager.Copy(standardOutputId);
        }

        public bool IsDone()
        {
            return (PCB.State == ProcessState.Crashed || PCB.State == ProcessState.Finished) ? true : false;
        }

        public bool IsDone(int taskId)
        {
            var task = objectManager.GetTask(taskId);

            return (task.PCB.State == ProcessState.Crashed || task.PCB.State == ProcessState.Finished) ? true : false;
        }

        public bool IsSignal(int signal)
        {
            if (((int)signals & signal) == signal)
            {
                signals &= (Signals)~signal;

                return true;
            }
            else
            {
                return false;
            }
        }

        public int PipeLine(int leftTaskId, int rightTaskId)
        {
            var leftTask = objectManager[leftTaskId].LiquidObject as Task;
            var rightTask = objectManager[rightTaskId].LiquidObject as Task;

            objectManager.Mark(leftTask.standardOutputId);
            objectManager.Mark(rightTask.standardInputId);

            var pipeId = Pipe.NewPipe(objectId);
            var pipe = objectManager[pipeId].LiquidObject as Pipe;

            leftTask.standardOutputId = objectManager.Copy(pipeId);
            leftTask.standardOutput = objectManager[pipeId].LiquidObject as Pipe;

            rightTask.standardInputId = objectManager.Copy(pipeId);
            rightTask.standardInput = objectManager[pipeId].LiquidObject as Pipe;

            pipe.Open();

            return pipeId;
        }

        public void Raise(ExceptionCode code, string data = "")
        {
            throwCode = code;
            throwData = data;
        }

        public void RaiseException(int fileNumber, int lineNumber, int parentId = 0)
        {
            var exceptionId = Exception.NewException(fileTable[fileNumber], lineNumber, throwCode, throwData, parentId);
            objectManager.Assign(ref this.exceptionId, exceptionId);

            throwCode = ExceptionCode.None;
            throwData = "";
        }

        public void Resume()
        {
            if (PCB.State != ProcessState.Suspended)
            {
                Raise(ExceptionCode.Denied);
                return;
            }

            PCB.State = prevState;
        }

        public void Run()
        {
            if (PCB.State != ProcessState.Ready)
            {
                Raise(ExceptionCode.Denied);
                return;
            }

            PCB.State = ProcessState.Running;

            LiquidPlayer.Program.Exec.Schedule(objectId);
        }

        public void Suspend()
        {
            if (PCB.State != ProcessState.Running && PCB.State != ProcessState.Waiting)
            {
                Raise(ExceptionCode.Denied);
                return;
            }

            prevState = PCB.State;

            PCB.State = ProcessState.Suspended;
        }

        public override void Destructor()
        {
            standardInput.Flush();
            standardOutput.Flush();
            standardError.Flush();
        }

        public override void shutdown()
        {
            objectManager.Mark(commandLineId);
            commandLine = null;

            objectManager.Mark(standardInputId);
            standardInput = null;

            objectManager.Mark(standardOutputId);
            standardOutput = null;

            objectManager.Mark(standardErrorId);
            standardError = null;

            objectManager.Mark(exceptionId);
            exception = null;

            System.Diagnostics.Debug.Assert(messageQueue.Count == 0);

            fileTable = null;
            stringTable = null;
            aliasTable = null;
            codeSpace = null;
            stack = null;
            messageQueue = null;

            base.shutdown();
        }
    }
}
