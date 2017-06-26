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

            //public bool IsTrap
            //{
            //    get;
            //    set;
            //}

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

            public int LastPC
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

        protected string path;
        protected string arguments;
        protected string currentDirectory;

        protected string[] fileTable;
        protected string[] stringTable;
        protected int[] aliasTable;

        protected List<FunctionDelegate> stubs;

        protected List<CodeTracker> codeTrackers;
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

        protected ErrorCode errorCode;
        protected string errorData;

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

        public List<FunctionDelegate> Stubs
        {
            get
            {
                return stubs;
            }
        }

        public int[] CodeSpace
        {
            get
            {
                return codeSpace;
            }
        }

        public int[] DataSpace
        {
            get
            {
                return dataSpace;
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

        public ErrorCode ErrorCode
        {
            get
            {
                return errorCode;
            }
        }

        public string ErrorData
        {
            get
            {
                return errorData;
            }
        }

        public int ExitCode
        {
            get
            {
                return exitCode;
            }
        }

        public static int NewTask(string path, string arguments, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Task);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Task(id, path, arguments);

            return id;
        }

        public Task(int id, string path, string arguments)
            : base(id)
        {
            if (!path.EndsWith(".ldx"))
            {
                path += ".ldx";
            }

            var resolvedPath = Util.FindFile(path, LiquidPlayer.Program.SharedPath);

            if (resolvedPath == "")
            {
                base.RaiseError(ErrorCode.FileNotFound, Path.GetFileName(path));
                return;
            }

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

            this.path = resolvedPath;
            this.arguments = arguments;
            this.currentDirectory = Path.GetDirectoryName(resolvedPath);

            this.fileTable = null;
            this.stringTable = null;
            this.aliasTable = null;

            this.stubs = new List<FunctionDelegate>();

            this.codeTrackers = null;
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

            loadTask(resolvedPath, arguments);

            this.exitCode = 0;

            this.PCB.State = ProcessState.Ready;
        }

        public override string ToString()
        {
            return $"Task (Tag: \"{tag}\"), Path: \"{path}\")";
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
                    throw new Exception("Class not found");
                }
            }

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                RaiseError(ErrorCode.OutOfMemory);
                return 0;
            }

            return liquidClass;
        }

        private LiquidClass bindCustomClass(BinaryReader reader)
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
                    throw new Exception("Class not found");
                }
            }

            var memoryRequired = readInt(reader);

            liquidClass = LiquidPlayer.Program.ClassManager.New(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                RaiseError(ErrorCode.OutOfMemory);
                return 0;
            }

            LiquidPlayer.Program.ClassManager.Bind(liquidClass, objectId, baseLiquidClass, memoryRequired);

            return liquidClass;
        }

        private int bindStub(BinaryReader reader)
        {
            var stub = readString(reader);

            var functionDelegate = LiquidPlayer.Program.StandardLibrary.GetFunctionDelegate(stub);

            if (functionDelegate == null)
            {
                throw new Exception("Stub not found");
            }

            stubs.Add(functionDelegate);

            return stubs.Count - 1;
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

            var stub = alias;

            var index = alias.IndexOf('|');

            liquidClassTag = alias.Substring(0, index);

            liquidClass = LiquidPlayer.Program.ClassManager.Find(liquidClassTag);

            if (liquidClass == LiquidClass.None)
            {
                throw new Exception("Class not found");
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
                    throw new Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            methodTag = alias.Substring(0, index);

            var type = readInt(reader);

            if (type == 1)
            {
                methodId = LiquidPlayer.Program.ClassManager.FindVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

                if (methodId == -1)
                {
                    var functionDelegate = LiquidPlayer.Program.StandardLibrary.GetFunctionDelegate(stub);

                    if (functionDelegate == null)
                    {
                        throw new Exception("Virtual method not found");
                    }

                    methodId = LiquidPlayer.Program.ClassManager.AddVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), stub, functionDelegate);
                }
                else
                {
                    stub = LiquidPlayer.Program.ClassManager[liquidClass].Methods[methodId].Stub;

                    var functionDelegate = LiquidPlayer.Program.StandardLibrary.GetFunctionDelegate(stub);

                    if (functionDelegate == null)
                    {
                        throw new Exception("Virtual method not found");
                    }

                    LiquidPlayer.Program.ClassManager.SetVirtualMethodTarget(liquidClass, methodId, stub, functionDelegate);

                    LiquidPlayer.Program.ClassManager.BindMethod(liquidClass, methodTag, -1);
                }
            }
            else
            {
                methodId = LiquidPlayer.Program.ClassManager.FindVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

                if (methodId == -1)
                {
                    var address = readInt(reader);

                    methodId = LiquidPlayer.Program.ClassManager.AddVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), address);
                }
                else
                {
                    var address = readInt(reader);

                    LiquidPlayer.Program.ClassManager.SetVirtualMethodTarget(liquidClass, methodId, address);

                    LiquidPlayer.Program.ClassManager.BindMethod(liquidClass, methodTag, address);
                }
            }

            if (methodId == -1)
            {
                RaiseError(ErrorCode.OutOfMemory);

                return -1;
            }

            return methodId;
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
                throw new Exception("Class not found");
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
                    throw new Exception("Class not found");
                }

                alias = alias.Substring(index + 1);
            }

            index = alias.IndexOf('(');

            parameters = alias.Substring(index);

            functionTag = alias.Substring(0, index);

            var address = readInt(reader);

            functionId = LiquidPlayer.Program.ClassManager.FindFunction(liquidClass, functionTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass));

            if (functionId == -1)
            {
                functionId = LiquidPlayer.Program.ClassManager.AddFunction(liquidClass, functionTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), address);
            }
            
            if (functionId == -1)
            {
                RaiseError(ErrorCode.OutOfMemory);

                return -1;
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

                var stubId = 0;
                var methodId = 0;
                var functionId = 0;

                switch (readInt(reader))
                {
                    case 1:
                        liquidClass = bindClass(reader);

                        aliasTable[i] = (int)liquidClass;

                        break;
                    case 2:
                        liquidClass = bindCustomClass(reader);

                        aliasTable[i] = (int)liquidClass;

                        break;
                    case 3:
                        stubId = bindStub(reader);

                        aliasTable[i] = stubId;

                        break;
                    case 4:
                        methodId = bindVirtualMethod(reader);

                        aliasTable[i] = methodId;

                        break;
                    case 5:
                        functionId = bindReservedFunction(reader);

                        aliasTable[i] = functionId;

                        break;
                    default:
                        throw new Exception("Corrupted executable");
                }
            }
        }

        private void loadCodeTrackers(BinaryReader reader)
        {
            var count = readInt(reader);

            codeTrackers = new List<CodeTracker>();

            for (var i = 1; i <= count; i++)
            {
                var fileNumber = readInt(reader);
                var lineNumber = readInt(reader);
                var linePosition = readInt(reader);

                var startPosition = readInt(reader);
                var endPosition = readInt(reader);

                var codeTracker = new CodeTracker(fileNumber, lineNumber, linePosition);

                codeTracker.StartPosition = startPosition;
                codeTracker.EndPosition = endPosition;

                codeTrackers.Add(codeTracker);
            }
        }

        private void loadTask(string path, string commandLine)
        {
            Compression.DecompressFile(path, LiquidPlayer.Program.TempPath + "temp.ldx", CompressionType.GZip);

            using (BinaryReader reader = new BinaryReader(System.IO.File.Open(LiquidPlayer.Program.TempPath + "temp.ldx", FileMode.Open)))
            {
                var header = Encoding.ASCII.GetString(reader.ReadBytes(8));

                if (header.Substring(0, 4) != "LDXv")
                {
                    base.RaiseError(ErrorCode.Denied, "File header is corrupted");
                    return;
                }

                if (header.Substring(4, 4) != "2.00")
                {
                    base.RaiseError(ErrorCode.Denied, "Unsupported file version");
                    return;
                }

                var taskType = readInt(reader);

                if (taskType < 1 || taskType > 4)
                {
                    base.RaiseError(ErrorCode.Denied, "Unsupported task type");
                }

                tag = readString(reader);

                debugMode = (readInt(reader) == 1);

                loadFileTable(reader);

                loadStringTable(reader);

                loadAliasTable(reader);

                var memoryRequired = readInt(reader);

                loadCodeTrackers(reader);

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
        //        obj.ChildrenList.Remove(id);
        //    }
        //}

        public List<int> GetTree()
        {
            var tree = new List<int>();

            addChildren(tree, objectId);

            tree.Reverse();

            return tree;
        }

        private void addChildren(List<int> tree, int id)
        {
            tree.Add(id);

            var list = LiquidPlayer.Program.Exec.ObjectManager.GetChildren(id);

            for (var index = 0; index < list.Count; index++)
            {
                addChildren(tree, list[index]);
            }
        }

        public CodeTracker GetCodeTracker(int position)
        {
            foreach (var codeTracker in codeTrackers)
            {
                if (position >= codeTracker.StartPosition && position <= codeTracker.EndPosition)
                {
                    return codeTracker;
                }
            }

            return null;
        }

        public void RestoreState()
        {
            Directory.SetCurrentDirectory(currentDirectory);
        }

        public virtual void VRun()
        {
            LiquidPlayer.Program.Exec.VirtualMachine.Run(objectId);

            if (IsDone())
            {
                standardInput.End();
                standardOutput.End();
                standardError.End();
            }
        }

        public void SaveState()
        {
            currentDirectory = Directory.GetCurrentDirectory();
        }

        protected override bool callback(int messageId)
        {
            var message = objectManager[messageId].LiquidObject as Message;

            if (message.IsTo(objectId))
            {
                if (message.GetBody() == MessageBody.KeyUp)
                {
                    var data = Convert.ToInt32(message.GetData());

                    switch (data)
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

        //public int CheckTrap()
        //{
        //    return objectManager.Copy(exceptionId);
        //}

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

        //public int EmptyTrap()
        //{
        //    var exceptionId = this.exceptionId;

        //    this.exceptionId = 0;

        //    return exceptionId;
        //}

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
            standardError.Write(data + Environment.NewLine);
        }

        public int GetCommandLine()
        {
            return objectManager.Copy(commandLineId);
        }

        public string ReadErrorStream()
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

                FreeString(id);
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

                FreeString(id);
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
            return (PCB.State == ProcessState.Crashed || PCB.State == ProcessState.Finished);
        }

        public bool IsDone(int taskId)
        {
            var task = objectManager.GetTask(taskId);

            return (task.PCB.State == ProcessState.Crashed || task.PCB.State == ProcessState.Finished);
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

        public void RaiseError(ErrorCode errorCode, string errorData, int parentId = 0)
        {
            var pc = PCB.LastPC;

            var codeTracker = GetCodeTracker(pc);

            var fileNumber = codeTracker.FileNumber;
            var lineNumber = codeTracker.LineNumber;

            this.errorCode = errorCode;
            this.errorData = errorData;
        }

        public void Resume()
        {
            if (PCB.State != ProcessState.Suspended)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            PCB.State = prevState;
        }

        public void Run()
        {
            if (PCB.State != ProcessState.Ready)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            PCB.State = ProcessState.Running;

            LiquidPlayer.Program.Exec.Schedule(objectId);
        }

        public void Suspend()
        {
            if (PCB.State != ProcessState.Running && PCB.State != ProcessState.Waiting)
            {
                RaiseError(ErrorCode.Denied);
                return;
            }

            prevState = PCB.State;

            PCB.State = ProcessState.Suspended;
        }

        //public void Trap(bool isTrap)
        //{
        //    PCB.IsTrap = isTrap;
        //}

        public override void Destructor()
        {
            standardInput.Flush();
            standardOutput.Flush();
            standardError.Flush();

            base.Destructor();
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

            System.Diagnostics.Debug.Assert(messageQueue.Count == 0);

            fileTable = null;
            stringTable = null;
            aliasTable = null;
            stubs = null;
            codeSpace = null;
            stack = null;
            messageQueue = null;

            if (PCB.State == ProcessState.Crashed)
            {
                LiquidPlayer.Program.Exec.LongManager.FreeLeftover(objectId);

                LiquidPlayer.Program.Exec.DoubleManager.FreeLeftover(objectId);

                LiquidPlayer.Program.Exec.StringManager.FreeLeftover(objectId);
            }

            base.shutdown();
        }
    }
}
