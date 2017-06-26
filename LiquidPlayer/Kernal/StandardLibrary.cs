using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Kernal
{
    public class StandardLibrary
    {
        private ClassManager classManager;

        private Dictionary<string, FunctionDelegate> stubs = new Dictionary<string, FunctionDelegate>();

        private void initClasses()
        {
            Program.ClassManager.New("Object");
            Program.ClassManager.New("System");
            Program.ClassManager.New("Debugger");
            Program.ClassManager.New("Message");
            Program.ClassManager.New("Collection");
            Program.ClassManager.New("Array");
            Program.ClassManager.New("Matrix");
            Program.ClassManager.New("Dictionary");
            Program.ClassManager.New("Stack");
            Program.ClassManager.New("Queue");
            Program.ClassManager.New("List");
            Program.ClassManager.New("CommandLine");
            Program.ClassManager.New("FileSystem");
            Program.ClassManager.New("File");
            Program.ClassManager.New("Internet");
            Program.ClassManager.New("Stream");
            Program.ClassManager.New("DataStream");
            Program.ClassManager.New("FileStream");
            Program.ClassManager.New("Pipe");
            Program.ClassManager.New("TextReader");
            Program.ClassManager.New("TextWriter");
            Program.ClassManager.New("Keyboard");
            Program.ClassManager.New("Mouse");
            Program.ClassManager.New("Ortho");
            Program.ClassManager.New("Perspective");
            Program.ClassManager.New("Entity");
            Program.ClassManager.New("GEL");
            Program.ClassManager.New("GEL3D");
            Program.ClassManager.New("Task");
            Program.ClassManager.New("Program");
            Program.ClassManager.New("App");
            Program.ClassManager.New("Applet");
            Program.ClassManager.New("Clock");
            Program.ClassManager.New("Math");
            Program.ClassManager.New("Random");
            Program.ClassManager.New("RegularExpression");
            Program.ClassManager.New("Color");
            Program.ClassManager.New("Bitmap");
            Program.ClassManager.New("Image");
            Program.ClassManager.New("Texture");
            Program.ClassManager.New("Raster");
            Program.ClassManager.New("Brush");
            Program.ClassManager.New("Pen");
            Program.ClassManager.New("Turtle");
            Program.ClassManager.New("Filter");
            Program.ClassManager.New("CharacterSet");
            Program.ClassManager.New("Font");
            Program.ClassManager.New("View");
            Program.ClassManager.New("Console");
            Program.ClassManager.New("TileMap");
            Program.ClassManager.New("CopperBars");
            Program.ClassManager.New("Canvas");
            Program.ClassManager.New("Sprite");
            Program.ClassManager.New("Tiles");
            Program.ClassManager.New("Text");
            Program.ClassManager.New("Audio");
            Program.ClassManager.New("Sound");
            Program.ClassManager.New("Music");
            Program.ClassManager.New("Voice");
        }

        public void Load(ClassManager classManager)
        {
            this.classManager = classManager;

            initClasses();

            bindObject();
            bindSystem();
            bindDebugger();
            bindMessage();
            bindCollection();
            bindArray();
            bindMatrix();
            bindDictionary();
            bindStack();
            bindQueue();
            bindList();
            bindCommandLine();
            bindFileSystem();
            bindFile();
            bindInternet();
            bindStream();
            bindDataStream();
            bindFileStream();
            bindPipe();
            bindTextReader();
            bindTextWriter();
            bindKeyboard();
            bindMouse();
            bindOrtho();
            bindPerspective();
            bindEntity();
            bindGEL();
            bindGEL3D();
            bindTask();
            bindProgram();
            bindApp();
            bindApplet();
            bindClock();
            bindMath();
            bindRandom();
            bindRegularExpression();
            bindColor();
            bindBitmap();
            bindImage();
            bindTexture();
            bindRaster();
            bindBrush();
            bindPen();
            bindTurtle();
            bindFilter();
            bindCharacterSet();
            bindFont();
            bindView();
            bindConsole();
            bindTileMap();
            bindCopperBars();
            bindCanvas();
            bindSprite();
            bindTiles();
            bindText();
            bindAudio();
            bindSound();
            bindMusic();
            bindVoice();

            bindAPI();
        }

        public FunctionDelegate GetFunctionDelegate(string tag)
        {
            if (stubs.ContainsKey(tag))
            {
                return stubs[tag];
            }

            return null;
        }

        private int addMethod(LiquidClass liquidClass, string methodTag, string parameters, LiquidClass returnLiquidClass, LiquidClass returnLiquidSubclass, FunctionDelegate functionDelegate)
        {
            var classTag = classManager.GetTag(liquidClass);

            var returnClassTag = classManager.GetTag(returnLiquidClass, returnLiquidSubclass);

            var stub = classTag + "|" + returnClassTag + ":" + methodTag + parameters;

            stubs[stub] = functionDelegate;

            return -1;
        }

        private int addVirtualMethod(LiquidClass liquidClass, string methodTag, string parameters, LiquidClass returnLiquidClass, LiquidClass returnLiquidSubclass, FunctionDelegate functionDelegate)
        {
            var classTag = classManager.GetTag(liquidClass);

            var returnClassTag = classManager.GetTag(returnLiquidClass, returnLiquidSubclass);

            var stub = classTag + "|" + returnClassTag + ":" + methodTag + parameters;

            stubs[stub] = functionDelegate;

            var methodId = classManager.AddVirtualMethod(liquidClass, methodTag, parameters, new LiquidType(returnLiquidClass, returnLiquidSubclass), stub, functionDelegate);

            return methodId;
        }

        private int addFunction(LiquidClass liquidClass, string functionTag, string parameters, LiquidClass returnLiquidClass, LiquidClass returnLiquidSubclass, FunctionDelegate functionDelegate)
        {
            var classTag = classManager.GetTag(liquidClass);

            var returnClassTag = classManager.GetTag(returnLiquidClass, returnLiquidSubclass);

            var stub = classTag + "|" + returnClassTag + ":" + functionTag + parameters;

            stubs[stub] = functionDelegate;

            return -1;
        }

        private void bindObject()
        {
            var liquidClass = Program.ClassManager.Find("Object");

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Object(a0);

                return false;
            });

            addVirtualMethod(liquidClass, "Callback", "(Message)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = (obj.VCallback(lc, stack[sp])) ? 1 : 0;

                return false;
            });

            addFunction(liquidClass, "Compare", "(Object,Object)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = Program.Exec.ObjectManager.VCompare(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "DelayMessage", "(Object,int,string,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var data = Liquid.Object.GetString(stack[sp - 2]);

                Program.Exec.Router.Delay(id, stack[sp], (MessageBody)stack[sp - 1], data, stack[sp - 3], id);

                return false;
            });

            addMethod(liquidClass, "Disable", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                obj.Disable();

                return false;
            });

            addMethod(liquidClass, "Enable", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                obj.Enable();

                return false;
            });

            addMethod(liquidClass, "GetParent", "()", LiquidClass.Object, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = obj.GetParent();

                return false;
            });

            addMethod(liquidClass, "GetParentTask", "()", LiquidClass.Task, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = obj.GetParentTask();

                return false;
            });

            addMethod(liquidClass, "GetTimeStamp", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = obj.TimeStamp;

                return false;
            });

            addMethod(liquidClass, "IsEnabled", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                a0 = (obj.IsEnabled) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "PulseMessage", "(Object,int,string,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var data = Liquid.Object.GetString(stack[sp - 2]);

                Program.Exec.Router.Pulse(id, stack[sp], (MessageBody)stack[sp - 1], data, stack[sp - 3], stack[sp - 4], id);

                return false;
            });

            addMethod(liquidClass, "SendMessage", "(Object,int,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var data = Liquid.Object.GetString(stack[sp - 2]);

                Program.Exec.Router.Send(id, stack[sp], (MessageBody)stack[sp - 1], data, id);

                return false;
            });

            addVirtualMethod(liquidClass, "Shutdown", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var obj = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Object;

                obj.VShutdown(lc);

                return false;
            });
        }

        private void bindSystem()
        {
            var liquidClass = Program.ClassManager.Find("System");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "GetCharacterSet", "()", LiquidClass.CharacterSet, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.Exec.ObjectManager.Copy(Program.Exec.ObjectManager.SystemCharacterSetId);

                return false;
            });

            addFunction(liquidClass, "Launch", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var fileName = Liquid.Object.GetString(stack[sp]);

                System.Diagnostics.Process.Start(fileName);

                return false;
            });

            addFunction(liquidClass, "MessageBox", "(string,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                var caption = Liquid.Object.GetString(stack[sp - 1]);

                System.Windows.Forms.MessageBox.Show(text, caption);

                return false;
            });

            addFunction(liquidClass, "FileOpen", "(string,string,string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var initialDirectory = Liquid.Object.GetString(stack[sp]);

                var defaultExt = Liquid.Object.GetString(stack[sp - 1]);

                var filter = Liquid.Object.GetString(stack[sp - 2]);

                var openFileDialog = new System.Windows.Forms.OpenFileDialog();

                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.DefaultExt = defaultExt;
                openFileDialog.Filter = filter;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    a0 = Liquid.Object.NewString(id, openFileDialog.FileName);
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, "");
                }

                return false;
            });

            addFunction(liquidClass, "FileSaveAs", "(string,string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var initialDirectory = Liquid.Object.GetString(stack[sp]);

                var defaultExt = Liquid.Object.GetString(stack[sp - 1]);

                var filter = Liquid.Object.GetString(stack[sp - 2]);

                var saveFileDialog = new System.Windows.Forms.SaveFileDialog();

                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.DefaultExt = defaultExt;
                saveFileDialog.Filter = filter;

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    a0 = Liquid.Object.NewString(id, saveFileDialog.FileName);
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, "");
                }

                return false;
            });
        }

        private void bindDebugger()
        {
            var liquidClass = Program.ClassManager.Find("Debugger");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "Alert", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                System.Windows.Forms.MessageBox.Show(text, "Debugger");

                return false;
            });

            addFunction(liquidClass, "Assert", "(boolean,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp - 1]);

                if (stack[sp] == 0)
                {
                    System.Windows.Forms.MessageBox.Show(text, "Debugger");
                }

                return false;
            });

            addFunction(liquidClass, "Breakpoint", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                return false;
            });

            addFunction(liquidClass, "Watch", "(int,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var slot = stack[sp];

                if (slot < 0 || slot > 9)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                var value = Liquid.Object.GetDouble(stack[sp - 1]);

                Program.Exec.Watch[slot] = value;

                return false;
            });

            addFunction(liquidClass, "Watch", "(int,Object)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var slot = stack[sp];

                if (slot < 0 || slot > 9)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                Program.Exec.Watch[slot] = stack[sp - 1];

                return false;
            });
        }

        private void bindMessage()
        {
            var liquidClass = Program.ClassManager.Find("Message");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            //addMethod(liquidClass, "Constructor", "(Object,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            //{
            //    Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Message(a0, a0, stack[sp], stack[sp - 1], stack[sp - 2]);

            //    return false;
            //});

            addMethod(liquidClass, "Dispatch", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var message = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Message;

                message.Dispatch();

                return false;
            });

            addMethod(liquidClass, "GetBody", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var message = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Message;

                a0 = (int)message.GetBody();

                return false;
            });

            addMethod(liquidClass, "GetData", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var message = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Message;

                var data = message.GetData();

                a0 = Liquid.Object.NewString(id, data);

                return false;
            });

            addMethod(liquidClass, "IsFrom", "(Object)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var message = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Message;

                a0 = (message.IsFrom(stack[sp])) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsTo", "(Object)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var message = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Message;

                a0 = (message.IsTo(stack[sp])) ? 1 : 0;

                return false;
            });
        }

        private void bindCollection()
        {
            var liquidClass = Program.ClassManager.Find("Collection");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindArray()
        {
            var liquidClass = Program.ClassManager.Find("Array");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Array(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Array(a0, stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Index", "(int)", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                bx.LoAddress = id;
                bx.HiAddress = array.Index(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Populate", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Populate(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "PostPopulate", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.PostPopulate(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "EnumeratorStart", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.EnumeratorStart();

                return false;
            });

            addMethod(liquidClass, "EnumeratorNext", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                a0 = (array.EnumeratorNext() == true) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "EnumeratorGet", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                bx.LoAddress = id;
                bx.HiAddress = array.EnumeratorGet();

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Clear();

                return false;
            });

            addMethod(liquidClass, "Delete", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Delete(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Dim", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Dim(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetCapacity", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                a0 = array.Capacity;

                return false;
            });

            addMethod(liquidClass, "Insert", "(int,subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Insert(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "ReDim", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.ReDim(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "ReDimPreserve", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.ReDimPreserve(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Reverse", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Reverse();

                return false;
            });

            addMethod(liquidClass, "Reverse", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Reverse(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Shuffle", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Shuffle();

                return false;
            });

            addMethod(liquidClass, "Shuffle", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Shuffle(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Sort", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Sort();

                return false;
            });

            addMethod(liquidClass, "Sort", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.Sort(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "UnDim", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var array = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Array;

                array.UnDim();

                return false;
            });
        }

        private void bindMatrix()
        {
            var liquidClass = Program.ClassManager.Find("Matrix");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Matrix(a0, stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "Index", "(int,int)", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                bx.LoAddress = id;
                bx.HiAddress = matrix.Index(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                matrix.Clear();

                return false;
            });

            addMethod(liquidClass, "Fill", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                matrix.Fill(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                a0 = matrix.Height;

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                a0 = matrix.Width;

                return false;
            });

            addMethod(liquidClass, "Identity", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var matrix = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Matrix;

                matrix.Identity();

                return false;
            });

            addFunction(LiquidClass.Matrix, "Operator+", "(Matrix,Matrix)", LiquidClass.Matrix, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Liquid.Matrix.Add(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(LiquidClass.Matrix, "Operator-", "(Matrix,Matrix)", LiquidClass.Matrix, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Liquid.Matrix.Subtract(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(LiquidClass.Matrix, "Operator*", "(Matrix,Matrix)", LiquidClass.Matrix, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Liquid.Matrix.Multiply(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(LiquidClass.Matrix, "Operator*", "(Matrix,double)", LiquidClass.Matrix, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var scalarValue = Liquid.Object.GetDouble(stack[sp - 1]);

                a0 = Liquid.Matrix.Multiply(stack[sp], scalarValue);

                return false;
            });
        }

        private void bindDictionary()
        {
            var liquidClass = Program.ClassManager.Find("Dictionary");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Dictionary(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Index", "(string)", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                var key = Liquid.Object.GetString(stack[sp]);

                var hiAddress = dictionary.Index(key);

                if (hiAddress == -1)
                {
                    return false;
                }

                bx.LoAddress = id;
                bx.HiAddress = hiAddress;

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                dictionary.Clear();

                return false;
            });

            addMethod(liquidClass, "Delete", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                var key = Liquid.Object.GetString(stack[sp]);

                dictionary.Delete(key);

                return false;
            });

            addMethod(liquidClass, "Exists", "(string)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                var key = Liquid.Object.GetString(stack[sp]);

                a0 = (dictionary.Exists(key)) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "GetCount", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                a0 = dictionary.Count;

                return false;
            });

            addMethod(liquidClass, "Insert", "(string,subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var dictionary = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Dictionary;

                var key = Liquid.Object.GetString(stack[sp]);

                dictionary.Insert(key, stack[sp - 1]);

                return false;
            });
        }

        private void bindStack()
        {
            var liquidClass = Program.ClassManager.Find("Stack");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Stack(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Populate", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                st.Populate(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                st.Clear();

                return false;
            });

            addMethod(liquidClass, "GetCount", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                a0 = st.Count;

                return false;
            });

            addMethod(liquidClass, "IsEmpty", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                a0 = (st.IsEmpty == true) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Peek", "()", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                a0 = st.Peek();

                return false;
            });

            addMethod(liquidClass, "Pop", "()", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                a0 = st.Pop();

                return false;
            });

            addMethod(liquidClass, "Push", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var st = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stack;

                st.Push(stack[sp]);

                return false;
            });
        }

        private void bindQueue()
        {
            var liquidClass = Program.ClassManager.Find("Queue");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Queue(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Populate", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                queue.Populate(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                queue.Clear();

                return false;
            });

            addMethod(liquidClass, "Dequeue", "()", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                a0 = queue.Dequeue();

                return false;
            });

            addMethod(liquidClass, "Enqueue", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                queue.Enqueue(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetCount", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                a0 = queue.Count;

                return false;
            });

            addMethod(liquidClass, "IsEmpty", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                a0 = (queue.IsEmpty == true) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Peek", "()", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var queue = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Queue;

                a0 = queue.Peek();

                return false;
            });
        }

        private void bindList()
        {
            var liquidClass = Program.ClassManager.Find("List");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Collection);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.List(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Index", "(int)", LiquidClass.Subclass, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                bx.LoAddress = id;
                bx.HiAddress = list.Index(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Populate", "(subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Populate(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "EnumeratorStart", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.EnumeratorStart();

                return false;
            });

            addMethod(liquidClass, "EnumeratorNext", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = (list.EnumeratorNext() == true) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "EnumeratorGet", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                bx.LoAddress = id;
                bx.HiAddress = list.EnumeratorGet();

                return false;
            });

            addMethod(liquidClass, "Add", "(subclass)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = list.Add(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Clear();

                return false;
            });

            addMethod(liquidClass, "Contains", "(subclass)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = (list.Contains(stack[sp])) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Delete", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Delete(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Enqueue", "(subclass)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = list.Enqueue(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetCount", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = list.Count;

                return false;
            });

            addMethod(liquidClass, "IndexOf", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = list.IndexOf(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Insert", "(int,subclass)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Insert(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "IsEmpty", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = (list.IsEmpty()) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Remove", "(subclass)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                a0 = list.Remove(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Reverse", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Reverse();

                return false;
            });

            addMethod(liquidClass, "Reverse", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Reverse(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Shuffle", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Shuffle();

                return false;
            });

            addMethod(liquidClass, "Shuffle", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Shuffle(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Sort", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Sort();

                return false;
            });

            addMethod(liquidClass, "Sort", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var list = Program.Exec.ObjectManager[id].LiquidObject as Liquid.List;

                list.Sort(stack[sp], stack[sp - 1]);

                return false;
            });
        }

        private void bindCommandLine()
        {
            var liquidClass = Program.ClassManager.Find("CommandLine");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "GetArgumentCount", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var commandLine = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CommandLine;

                a0 = commandLine.GetArgumentCount();

                return false;
            });

            addMethod(liquidClass, "GetArgument", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var commandLine = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CommandLine;

                var argument = commandLine.GetArgument(stack[sp]);

                a0 = Liquid.Object.NewString(id, argument);

                return false;
            });

            addMethod(liquidClass, "GetArguments", "()", LiquidClass.List, LiquidClass.String, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var commandLine = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CommandLine;

                a0 = commandLine.GetArguments();

                return false;
            });

            addMethod(liquidClass, "GetSwitch", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var commandLine = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CommandLine;

                a0 = commandLine.GetSwitch(stack[sp]);

                return false;
            });
        }

        private void bindFileSystem()
        {
            var liquidClass = Program.ClassManager.Find("FileSystem");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "Exists", "(string)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var resolvedPath = Util.FindFile(Liquid.Object.GetString(stack[sp]), Program.SharedPath);

                a0 = (resolvedPath != "") ? 1 : 0;

                return false;
            });

            addFunction(liquidClass, "GetFileList", "(string)", LiquidClass.List, LiquidClass.String, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                var searchPattern = Liquid.Object.GetString(stack[sp]);

                a0 = task.GetFileList(searchPattern);

                return false;
            });
        }

        private void bindFile()
        {
            var liquidClass = Program.ClassManager.Find("File");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindInternet()
        {
            var liquidClass = Program.ClassManager.Find("Internet");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindStream()
        {
            var liquidClass = Program.ClassManager.Find("Stream");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addVirtualMethod(liquidClass, "Close", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                stream.Close();

                return false;
            });

            addVirtualMethod(liquidClass, "EndOfStream", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                var results = stream.EndOfStream();

                if (stream.IsBlocked)
                {
                    return true;
                }

                a0 = (results) ? 1 : 0;

                return false;
            });

            addVirtualMethod(liquidClass, "Flush", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                stream.Flush();

                return false;
            });

            addMethod(liquidClass, "GetLength", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                a0 = stream.GetLength();

                return false;
            });

            addMethod(liquidClass, "GetPosition", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                a0 = stream.GetPosition();

                return false;
            });

            addMethod(liquidClass, "IsOpen", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                a0 = (stream.IsOpen) ? 1 : 0;

                return false;
            });

            addVirtualMethod(liquidClass, "Open", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                stream.Open();

                return false;
            });

            addVirtualMethod(liquidClass, "Peek", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                var data = stream.Peek();

                if (stream.IsBlocked)
                {
                    return true;
                }

                a0 = data;

                return false;
            });

            addVirtualMethod(liquidClass, "Read", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                var data = stream.Read();

                if (stream.IsBlocked)
                {
                    return true;
                }

                a0 = data;

                return false;
            });

            addVirtualMethod(liquidClass, "Read", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                var data = stream.Read(stack[sp]);

                if (stream.IsBlocked)
                {
                    return true;
                }

                a0 = Liquid.Object.NewString(id, data);

                return false;
            });

            addVirtualMethod(liquidClass, "Seek", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                stream.Seek(stack[sp]);

                return false;
            });

            addVirtualMethod(liquidClass, "SetLength", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                stream.SetLength(stack[sp]);

                return false;
            });

            addVirtualMethod(liquidClass, "Write", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var stream = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Stream;

                var data = Liquid.Object.GetString(stack[sp]);

                stream.Write(data);

                return false;
            });
        }

        private void bindDataStream()
        {
            var liquidClass = Program.ClassManager.Find("DataStream");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Stream);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.DataStream(a0);

                return false;
            });
        }

        private void bindFileStream()
        {
            var liquidClass = Program.ClassManager.Find("FileStream");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Stream);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.FileStream(a0, path);

                return false;
            });
        }

        private void bindPipe()
        {
            var liquidClass = Program.ClassManager.Find("Pipe");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Stream);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Pipe(a0);

                return false;
            });
        }

        private void bindTextReader()
        {
            var liquidClass = Program.ClassManager.Find("TextReader");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(Stream)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.TextReader(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "EndOfStream", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textReader = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextReader;

                var results = textReader.EndOfStream();

                if (textReader.IsBlocked)
                {
                    return true;
                }

                a0 = (results) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "GetLength", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textReader = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextReader;

                a0 = textReader.GetLength();

                return false;
            });

            addMethod(liquidClass, "Read", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textReader = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextReader;

                var data = textReader.Read();

                if (textReader.IsBlocked)
                {
                    return true;
                }

                a0 = Liquid.Object.NewString(id, data);

                return false;
            });

            addMethod(liquidClass, "ReadLine", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textReader = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextReader;

                var data = textReader.ReadLine();

                if (textReader.IsBlocked)
                {
                    return true;
                }

                a0 = Liquid.Object.NewString(id, data);

                return false;
            });
        }

        private void bindTextWriter()
        {
            var liquidClass = Program.ClassManager.Find("TextWriter");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(Stream)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.TextWriter(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "WriteLine", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textWriter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextWriter;

                textWriter.WriteLine();

                return false;
            });

            addMethod(liquidClass, "WriteLine", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textWriter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextWriter;

                var data = Convert.ToString(stack[sp]);

                textWriter.WriteLine(data);

                return false;
            });

            addMethod(liquidClass, "WriteLine", "(long)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textWriter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextWriter;

                var data = Convert.ToString(Liquid.Object.GetLong(stack[sp]));

                textWriter.WriteLine(data);

                return false;
            });

            addMethod(liquidClass, "WriteLine", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textWriter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextWriter;

                var data = Convert.ToString(Liquid.Object.GetDouble(stack[sp]));

                textWriter.WriteLine(data);

                return false;
            });

            addMethod(liquidClass, "WriteLine", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var textWriter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TextWriter;

                var data = Liquid.Object.GetString(stack[sp]);

                textWriter.WriteLine(data);

                return false;
            });
        }

        private void bindKeyboard()
        {
            var liquidClass = Program.ClassManager.Find("Keyboard");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindMouse()
        {
            var liquidClass = Program.ClassManager.Find("Mouse");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "GetButton", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Sprockets.Input.MouseButton;

                return false;
            });

            addFunction(liquidClass, "GetXPosition", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Sprockets.Input.MouseSnappedXPosition;

                return false;
            });

            addFunction(liquidClass, "GetYPosition", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Sprockets.Input.MouseSnappedYPosition;

                return false;
            });
        }

        private void bindOrtho()
        {
            var liquidClass = Program.ClassManager.Find("Ortho");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "AlphaTest", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.AlphaTest(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "BezierCurve", "(int,int,int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.BezierCurve(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5], stack[sp - 6], stack[sp - 7]);

                return false;
            });

            addMethod(liquidClass, "Blend", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Blend(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "BlendFunc", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.BlendFunc(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Circle", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Circle(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "CircleFill", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.CircleFill(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "ClipRectangle", "(int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.ClipRectangle(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], (uint)stack[sp - 4], (uint)stack[sp - 5]);

                return false;
            });

            addMethod(liquidClass, "Ellipse", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Ellipse(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "EllipseFill", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.EllipseFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Ink", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Ink((uint)stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Line", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Line(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "LineWidth", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.LineWidth(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "Node", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Node(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Plot", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Plot(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "PointSize", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.PointSize(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "Print", "(CharacterSet,int,int,byte)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.PrintCharacterSet(stack[sp], stack[sp - 1], stack[sp - 2], (byte)stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Print", "(CharacterSet,int,int,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                var caption = Liquid.Object.GetString(stack[sp - 3]);

                ortho.PrintCharacterSet(stack[sp], stack[sp - 1], stack[sp - 2], caption);

                return false;
            });

            addMethod(liquidClass, "Print", "(Font,int,int,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                var caption = Liquid.Object.GetString(stack[sp - 3]);

                ortho.Print(stack[sp], stack[sp - 1], stack[sp - 2], caption);

                return false;
            });

            addMethod(liquidClass, "Rectangle", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Rectangle(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "RectangleFill", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.RectangleFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Rotate", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                var r = Liquid.Object.GetDouble(stack[sp]);

                ortho.Rotate(r);

                return false;
            });

            addMethod(liquidClass, "Scale", "(double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                ortho.Scale(x, y);

                return false;
            });

            addMethod(liquidClass, "Stamp", "(Bitmap,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Stamp(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "Translate", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ortho = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Ortho;

                ortho.Translate(stack[sp], stack[sp - 1]);

                return false;
            });
        }

        private void bindPerspective()
        {
            var liquidClass = Program.ClassManager.Find("Perspective");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "AlphaTest", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.AlphaTest(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Begin", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.Begin(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Blend", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.Blend(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "BlendFunc", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.BlendFunc(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "DepthFunc", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.DepthFunc(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "DepthTest", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.DepthTest((stack[sp] != 0));

                return false;
            });

            addMethod(liquidClass, "End", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.End();

                return false;
            });

            addMethod(liquidClass, "Ink", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.Ink((uint)stack[sp]);

                return false;
            });

            addMethod(liquidClass, "LineWidth", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.LineWidth(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "LoadIdentity", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.LoadIdentity();

                return false;
            });

            addMethod(liquidClass, "Normal", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                perspective.Normal(x, y, z);

                return false;
            });

            addMethod(liquidClass, "PointSize", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.PointSize(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "PopMatrix", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.PopMatrix();

                return false;
            });

            addMethod(liquidClass, "PushMatrix", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.PushMatrix();

                return false;
            });

            addMethod(liquidClass, "Rotate", "(double,double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var angle = Liquid.Object.GetDouble(stack[sp]);

                var x = Liquid.Object.GetDouble(stack[sp - 1]);

                var y = Liquid.Object.GetDouble(stack[sp - 2]);

                var z = Liquid.Object.GetDouble(stack[sp - 3]);

                perspective.Rotate(angle, x, y, z);

                return false;
            });

            addMethod(liquidClass, "Scale", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                perspective.Scale(x, y, z);

                return false;
            });

            addMethod(liquidClass, "TexCoord", "(double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                perspective.TexCoord(x, y);

                return false;
            });

            addMethod(liquidClass, "TextureMap", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                perspective.TextureMap(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Translate", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                perspective.Translate(x, y, z);

                return false;
            });

            addMethod(liquidClass, "Vertex", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var perspective = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Perspective;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                perspective.Vertex(x, y, z);

                return false;
            });
        }

        private void bindEntity()
        {
            var liquidClass = Program.ClassManager.Find("Entity");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Entity(id);

                return false;
            });

            addMethod(liquidClass, "IsRunning", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var entity = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Entity;

                a0 = (entity.IsRunning) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Start", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var entity = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Entity;

                entity.Start();

                return false;
            });

            addMethod(liquidClass, "Stop", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var entity = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Entity;

                entity.Stop();

                return false;
            });

            addVirtualMethod(liquidClass, "Update", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var entity = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Entity;

                entity.VUpdate(lc);

                return false;
            });
        }

        private void bindGEL()
        {
            var liquidClass = Program.ClassManager.Find("GEL");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Entity);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.GEL(id);

                return false;
            });

            addMethod(liquidClass, "Alpha", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Alpha(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "Center", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Center();

                return false;
            });

            addMethod(liquidClass, "GetMouseOverNode", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MousePointingAt == id) ? Sprockets.Input.MousePointingAtNode : -1;

                return false;
            });

            addMethod(liquidClass, "GetNodeClicked", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MouseClickedOn == id) ? Sprockets.Input.MouseClickedOnNode : -1;

                return false;
            });

            addMethod(liquidClass, "GetPriority", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = gel.Priority;

                return false;
            });

            addMethod(liquidClass, "GetRotation", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                d0 = gel.Rotation;

                return false;
            });

            addMethod(liquidClass, "GetXPosition", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (int)gel.XPosition;

                return false;
            });

            addMethod(liquidClass, "GetXScale", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                d0 = gel.XScale;

                return false;
            });

            addMethod(liquidClass, "GetYPosition", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (int)gel.YPosition;

                return false;
            });

            addMethod(liquidClass, "GetYScale", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                d0 = gel.YScale;

                return false;
            });

            addMethod(liquidClass, "GotFocus", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (gel.GotFocus) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Hide", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Hide();

                return false;
            });

            addMethod(liquidClass, "IsClicked", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MouseClickedOn == id) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsMouseOver", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MousePointingAt == id) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsMouseOverNode", "(int)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MousePointingAt == id && Sprockets.Input.MousePointingAtNode == stack[sp]) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsNodeClicked", "(int)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (Sprockets.Input.MouseClickedOn == id && Sprockets.Input.MouseClickedOnNode == stack[sp]) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsVisible", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                a0 = (gel.IsVisible) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Move", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Move(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "MoveDirection", "(double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                var direction = Liquid.Object.GetDouble(stack[sp]);

                var speed = Liquid.Object.GetDouble(stack[sp - 1]);

                gel.MoveDirection(direction, speed);

                return false;
            });

            addMethod(liquidClass, "MoveRelative", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.MoveRelative(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Priority", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Priority = stack[sp];

                return false;
            });

            addVirtualMethod(liquidClass, "Render", "(Ortho)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.VRender(lc, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Rotate", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                var rotation = Liquid.Object.GetDouble(stack[sp]);

                gel.Rotate(rotation);

                return false;
            });

            addMethod(liquidClass, "Scale", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                var scale = Liquid.Object.GetDouble(stack[sp]);

                gel.Scale(scale);

                return false;
            });

            addMethod(liquidClass, "Scale", "(double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                var xScale = Liquid.Object.GetDouble(stack[sp]);

                var yScale = Liquid.Object.GetDouble(stack[sp - 1]);

                gel.Scale(xScale, yScale);

                return false;
            });

            addMethod(liquidClass, "SetFocus", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                Program.Exec.ObjectManager.Focus = id;

                return false;
            });

            addMethod(liquidClass, "Show", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Show();

                return false;
            });

            addMethod(liquidClass, "Tint", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL;

                gel.Tint((uint)stack[sp]);

                return false;
            });
        }

        private void bindGEL3D()
        {
            var liquidClass = Program.ClassManager.Find("GEL3D");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Entity);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.GEL3D(id);

                return false;
            });

            addMethod(liquidClass, "Alpha", "(float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                gel3D.Alpha(Util.Int2Float(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "GetXPosition", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.XPosition;

                return false;
            });

            addMethod(liquidClass, "GetXScale", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.XScale;

                return false;
            });

            addMethod(liquidClass, "GetXRotation", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.XRotation;

                return false;
            });

            addMethod(liquidClass, "GetYPosition", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.YPosition;

                return false;
            });

            addMethod(liquidClass, "GetYScale", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.YScale;

                return false;
            });

            addMethod(liquidClass, "GetYRotation", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.YRotation;

                return false;
            });

            addMethod(liquidClass, "GetZPosition", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.ZPosition;

                return false;
            });

            addMethod(liquidClass, "GetZScale", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.ZScale;

                return false;
            });

            addMethod(liquidClass, "GetZRotation", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                d0 = gel3D.ZRotation;

                return false;
            });

            addMethod(liquidClass, "GotFocus", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                a0 = (gel3D.GotFocus) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Hide", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                gel3D.Hide();

                return false;
            });

            addMethod(liquidClass, "IsVisible", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                a0 = (gel3D.IsVisible) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Move", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                gel3D.Move(x, y, z);

                return false;
            });

            addVirtualMethod(liquidClass, "Render", "(Perspective)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                gel3D.VRender(lc, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Rotate", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                gel3D.Rotate(x, y, z);

                return false;
            });

            addMethod(liquidClass, "Scale", "(double,double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                var z = Liquid.Object.GetDouble(stack[sp - 2]);

                gel3D.Scale(x, y, z);

                return false;
            });

            addMethod(liquidClass, "SetFocus", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                Program.Exec.ObjectManager.Focus = id;

                return false;
            });

            addMethod(liquidClass, "Show", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                gel3D.Show();

                return false;
            });

            addMethod(liquidClass, "Tint", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var gel3D = Program.Exec.ObjectManager[id].LiquidObject as Liquid.GEL3D;

                gel3D.Tint((uint)stack[sp]);

                return false;
            });
        }

        private void bindTask()
        {
            var liquidClass = Program.ClassManager.Find("Task");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Task(a0, path, "");

                var task = Program.Exec.ObjectManager[a0].LiquidObject as Liquid.Task;

                task.CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

                Program.Exec.AddTask(a0);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(string,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                var arguments = Liquid.Object.GetString(stack[sp - 1]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Task(a0, path, arguments);

                var task = Program.Exec.ObjectManager[a0].LiquidObject as Liquid.Task;

                task.CurrentDirectory = System.IO.Directory.GetCurrentDirectory();

                Program.Exec.AddTask(a0);

                return false;
            });

            addMethod(liquidClass, "Await", "(Task)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                return task.Await(stack[sp]);
            });

            //addMethod(liquidClass, "CheckTrap", "()", LiquidClass.Exception, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            //{
            //    var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

            //    a0 = task.CheckTrap();

            //    return false;
            //});

            addMethod(liquidClass, "ClearSignals", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                task.ClearSignals();

                return false;
            });

            //addMethod(liquidClass, "EmptyTrap", "()", LiquidClass.Exception, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            //{
            //    var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

            //    a0 = task.EmptyTrap();

            //    return false;
            //});

            addMethod(liquidClass, "End", "(Task)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                task.End(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "ErrorOut", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                var data = Liquid.Object.GetString(stack[sp]);

                task.ErrorOut(data);

                return false;
            });

            addMethod(liquidClass, "ExitCode", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.ExitCode;

                return false;
            });

            addMethod(liquidClass, "GetCommandLine", "()", LiquidClass.CommandLine, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.GetCommandLine();

                return false;
            });

            addFunction(liquidClass, "GetTaskList", "(Task)", LiquidClass.List, LiquidClass.String, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.GetTaskList(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetTaskName", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = Liquid.Object.NewString(id, task.Tag);

                return false;
            });

            addMethod(liquidClass, "IsDone", "(Task)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = (task.IsDone(stack[sp])) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "IsSignal", "(int)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = (task.IsSignal(stack[sp])) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "PipeLine", "(Task,Task)", LiquidClass.Pipe, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.PipeLine(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "RaiseError", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                var data = Liquid.Object.GetString(stack[sp]);

                task.RaiseError(ErrorCode.User, data);

                return false;
            });

            addMethod(liquidClass, "ReadErrorStream", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = Liquid.Object.NewString(id, task.ReadErrorStream());

                return false;
            });

            addMethod(liquidClass, "Resume", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                task.Resume();

                return false;
            });

            addMethod(liquidClass, "Run", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                task.Run();

                return false;
            });

            addMethod(liquidClass, "Sleep", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                if (task.PCB.State == Liquid.Task.ProcessState.Running)
                {
                    var time = stack[sp];

                    if (time < 1)
                    {
                        task.RaiseError(ErrorCode.IllegalQuantity);
                        return false;
                    }

                    task.AlarmClock = Program.SystemClock + time;

                    return true;
                }

                if (Program.SystemClock < task.AlarmClock)
                {
                    return true;
                }

                task.AlarmClock = 0;

                return false;
            });

            addMethod(liquidClass, "Suspend", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                task.Suspend();

                return false;
            });

            addMethod(liquidClass, "StandardInput", "()", LiquidClass.Pipe, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.GetStandardInput();

                return false;
            });

            addMethod(liquidClass, "StandardOutput", "()", LiquidClass.Pipe, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = task.GetStandardOutput();

                return false;
            });

            addMethod(liquidClass, "Tick", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                return (task.PCB.State == Liquid.Task.ProcessState.Running);
            });

            addMethod(liquidClass, "Tock", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

                a0 = (task.PCB.IsTock) ? 1 : 0;

                task.PCB.IsTock = false;

                return false;
            });

            //addMethod(liquidClass, "Trap", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            //{
            //    var task = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Task;

            //    task.Trap(stack[sp] != 0);

            //    return false;
            //});
        }

        private void bindProgram()
        {
            var liquidClass = Program.ClassManager.Find("Program");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Task);

            addMethod(liquidClass, "AutoSnap", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                program.AutoSnap(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Border", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                program.BorderColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "GetKey", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                var key = program.GetKey();

                a0 = key;

                return false;
            });

            addMethod(liquidClass, "GetScreenHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.ScreenHeight;

                return false;
            });

            addMethod(liquidClass, "GetScreenWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.ScreenWidth;

                return false;
            });

            addMethod(liquidClass, "IsResized", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (Sprockets.Graphics.IsResized) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Paper", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                program.PaperColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "Screen", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                program.Screen(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "WaitKey", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var program = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Program;

                if (program.PCB.State == Liquid.Task.ProcessState.Running)
                {
                    program.InputBuffer.Clear();

                    return true;
                }

                var key = program.GetKey();

                if (key == 0)
                {
                    return true;
                }

                a0 = key;

                return false;
            });
        }

        private void bindApp()
        {
            var liquidClass = Program.ClassManager.Find("App");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Task);

            addMethod(liquidClass, "Be", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var app = Program.Exec.ObjectManager[id].LiquidObject as Liquid.App;

                if (app.PCB.State == Liquid.Task.ProcessState.Waiting)
                {
                    return false;
                }

                app.Be();

                return true;
            });

            addMethod(liquidClass, "DequeueMessage", "()", LiquidClass.Message, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var app = Program.Exec.ObjectManager[id].LiquidObject as Liquid.App;

                a0 = app.DequeueMessage();

                return false;
            });

            addMethod(liquidClass, "GetScreenHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.ScreenHeight;

                return false;
            });

            addMethod(liquidClass, "GetScreenWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.ScreenWidth;

                return false;
            });

            addMethod(liquidClass, "IsMessageQueueEmpty", "()", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var app = Program.Exec.ObjectManager[id].LiquidObject as Liquid.App;

                a0 = (app.IsMessageQueueEmpty()) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "Paper", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var app = Program.Exec.ObjectManager[id].LiquidObject as Liquid.App;

                app.PaperColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "Screen", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var app = Program.Exec.ObjectManager[id].LiquidObject as Liquid.App;

                app.Screen(stack[sp], stack[sp - 1]);

                return false;
            });
        }

        private void bindApplet()
        {
            var liquidClass = Program.ClassManager.Find("Applet");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Task);
        }

        private void bindClock()
        {
            var liquidClass = Program.ClassManager.Find("Clock");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "GetAtomic", "()", LiquidClass.Long, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                c0 = Program.AtomicClock;

                return false;
            });

            addFunction(liquidClass, "GetDate", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var date = DateTime.Now.ToString("dd/MM/yyyy");

                a0 = Liquid.Object.NewString(id, date);

                return false;
            });

            addFunction(liquidClass, "GetDay", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Day;

                return false;
            });

            addFunction(liquidClass, "GetHour", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Hour;

                return false;
            });

            addFunction(liquidClass, "GetMinute", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Minute;

                return false;
            });

            addFunction(liquidClass, "GetMonth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Month;

                return false;
            });

            addFunction(liquidClass, "GetSecond", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Second;

                return false;
            });

            addFunction(liquidClass, "GetTime", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var time = DateTime.Now.ToString("HH:mm:ss.ff");

                a0 = Liquid.Object.NewString(id, time);

                return false;
            });

            addFunction(liquidClass, "GetSystem", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.SystemClock;

                return false;
            });

            addFunction(liquidClass, "GetYear", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = DateTime.Now.Year;

                return false;
            });
        }

        private void bindMath()
        {
            var liquidClass = Program.ClassManager.Find("Math");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "Clamp", "(int,int,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Util.Clamp(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addFunction(liquidClass, "Clamp", "(double,double,double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var val = Liquid.Object.GetDouble(stack[sp]);

                var min = Liquid.Object.GetDouble(stack[sp - 1]);

                var max = Liquid.Object.GetDouble(stack[sp - 2]);

                d0 = Util.Clamp(val, min, max);

                return false;
            });

            addFunction(liquidClass, "Degrees", "(double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var radian = Liquid.Object.GetDouble(stack[sp]);

                d0 = radian * 57.29577951308232d;

                return false;
            });

            addFunction(liquidClass, "Max", "(int,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Util.Max(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(liquidClass, "Max", "(double,double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var val = Liquid.Object.GetDouble(stack[sp]);

                var max = Liquid.Object.GetDouble(stack[sp - 1]);

                d0 = Util.Max(val, max);

                return false;
            });

            addFunction(liquidClass, "Min", "(int,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Util.Min(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(liquidClass, "Min", "(double,double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var val = Liquid.Object.GetDouble(stack[sp]);

                var min = Liquid.Object.GetDouble(stack[sp - 1]);

                d0 = Util.Min(val, min);

                return false;
            });

            addFunction(liquidClass, "Radians", "(double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var degree = Liquid.Object.GetDouble(stack[sp]);

                d0 = degree * 0.0174532925199433d;

                return false;
            });
        }

        private void bindRandom()
        {
            var liquidClass = Program.ClassManager.Find("Random");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "Range", "(int,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Program.Random.Range(stack[sp], stack[sp - 1]);

                return false;
            });

            addFunction(liquidClass, "Range", "(double,double)", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var min = Liquid.Object.GetDouble(stack[sp]);

                var max = Liquid.Object.GetDouble(stack[sp - 1]);

                d0 = Program.Random.Range(min, max);

                return false;
            });

            addFunction(liquidClass, "Sample", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = Program.Random.Sample();

                return false;
            });
        }

        private void bindRegularExpression()
        {
            var liquidClass = Program.ClassManager.Find("RegularExpression");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "IsMatch", "(string,string)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var input = Liquid.Object.GetString(stack[sp]);

                var pattern = Liquid.Object.GetString(stack[sp - 1]);

                a0 = (System.Text.RegularExpressions.Regex.IsMatch(input, pattern)) ? 1 : 0;

                return false;
            });

            addFunction(liquidClass, "Match", "(string,string)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var input = Liquid.Object.GetString(stack[sp]);

                var pattern = Liquid.Object.GetString(stack[sp - 1]);

                var results = (System.Text.RegularExpressions.Regex.Match(input, pattern));

                a0 = (results.Success) ? results.Index : -1;

                return false;
            });

            addFunction(liquidClass, "Replace", "(string,string,string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var input = Liquid.Object.GetString(stack[sp]);

                var pattern = Liquid.Object.GetString(stack[sp - 1]);

                var replacement = Liquid.Object.GetString(stack[sp - 2]);

                var results = System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);

                a0 = Liquid.Object.NewString(id, results);

                return false;
            });
        }

        private void bindColor()
        {
            var liquidClass = Program.ClassManager.Find("Color");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addFunction(liquidClass, "Darken", "(int,float)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var color = stack[sp];

                var amount = Util.Int2Float(stack[sp - 1]);

                if (amount < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                a0 = (int)Sprockets.Color.Darken((uint)color, amount);

                return false;
            });

            addFunction(liquidClass, "GetAlpha", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = stack[sp] >> 24;

                return false;
            });

            addFunction(liquidClass, "GetBlue", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = stack[sp] >> 16 & 255;

                return false;
            });

            addFunction(liquidClass, "GetGreen", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = stack[sp] >> 8 & 255;

                return false;
            });

            addFunction(liquidClass, "GetPlasma", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GetPlasmaColor();

                return false;
            });

            addFunction(liquidClass, "GetRandom", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GetRandomColor();

                return false;
            });

            addFunction(liquidClass, "GetRandom", "(byte)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GetRandomColor((byte)stack[sp]);

                return false;
            });

            addFunction(liquidClass, "GetRed", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = stack[sp] & 255;

                return false;
            });

            addFunction(liquidClass, "Gradient", "(int,int,float)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var color1 = stack[sp];
                var color2 = stack[sp - 1];

                var amount = Util.Int2Float(stack[sp - 2]);

                if (amount < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                a0 = (int)Sprockets.Color.Gradient((uint)color1, (uint)color2, amount);

                return false;
            });

            addFunction(liquidClass, "Gray", "(byte)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var i = (byte)stack[sp];

                a0 = (int)(i | (uint)i << 8 | (uint)i << 16 | 255 << 24);

                return false;
            });

            addFunction(liquidClass, "GrayScale", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GrayScale((uint)stack[sp]);

                return false;
            });

            addFunction(liquidClass, "Lighten", "(int,float)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var color = stack[sp];

                var amount = Util.Int2Float(stack[sp - 1]);

                if (amount < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                a0 = (int)Sprockets.Color.Lighten((uint)color, amount);

                return false;
            });

            addFunction(liquidClass, "RGB", "(byte,byte,byte)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GetColor((byte)stack[sp], (byte)stack[sp - 1], (byte)stack[sp - 2]);

                return false;
            });

            addFunction(liquidClass, "RGBA", "(byte,byte,byte,byte)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.GetColor((byte)stack[sp], (byte)stack[sp - 1], (byte)stack[sp - 2], (byte)stack[sp - 3]);

                return false;
            });

            addFunction(liquidClass, "Sepia", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = (int)Sprockets.Color.Sepia((uint)stack[sp]);

                return false;
            });
        }

        private void bindBitmap()
        {
            var liquidClass = Program.ClassManager.Find("Bitmap");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Bitmap(a0, stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.Clear();

                return false;
            });

            addMethod(liquidClass, "DoubleBuffer", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.DoubleBuffer();

                return false;
            });

            addMethod(liquidClass, "Peek", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                a0 = (int)bitmap.Peek(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                a0 = bitmap.Height;

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                a0 = bitmap.Width;

                return false;
            });

            addMethod(liquidClass, "Poke", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.Poke(stack[sp], (uint)stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "SingleBuffer", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.SingleBuffer();

                return false;
            });

            addMethod(liquidClass, "Smooth", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.Smooth(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "SwapBuffers", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var bitmap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Bitmap;

                bitmap.SwapBuffers();

                return false;
            });
        }

        private void bindImage()
        {
            var liquidClass = Program.ClassManager.Find("Image");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Bitmap);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Image(id, path);

                return false;
            });
        }

        private void bindTexture()
        {
            var liquidClass = Program.ClassManager.Find("Texture");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Bitmap);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Texture(id);

                return false;
            });

            addMethod(liquidClass, "CellMachine", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var texture = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Texture;

                texture.CellMachine(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Checkerboard", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var texture = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Texture;

                texture.Checkerboard(stack[sp], stack[sp - 1], (uint)stack[sp - 2], (uint)stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Particle", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var texture = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Texture;

                var size = Liquid.Object.GetDouble(stack[sp]);

                texture.Particle(size);

                return false;
            });

            addMethod(liquidClass, "PerlinNoise", "(int,int,int,int,int,boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var texture = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Texture;

                texture.PerlinNoise(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], (stack[sp - 5] != 0) ? true : false);

                return false;
            });
        }

        private void bindRaster()
        {
            var liquidClass = Program.ClassManager.Find("Raster");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Raster(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Canvas)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[stack[sp]].LiquidObject as Liquid.Canvas;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Raster(a0, canvas.BitmapId);

                return false;
            });

            addMethod(liquidClass, "BezierCurve", "(int,int,int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.BezierCurve(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5], stack[sp - 6], stack[sp - 7]);

                return false;
            });

            addMethod(liquidClass, "Circle", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Circle(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "CircleFill", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.CircleFill(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "Clip", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Clip(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Ellipse", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Ellipse(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "EllipseFill", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.EllipseFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Fill", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Fill((uint)stack[sp]);

                return false;
            });

            addMethod(liquidClass, "FloodFill", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.FloodFill(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "hLine", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.hLine(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "Ink", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.InkColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "Line", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Line(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "LineStipple", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.LineStipple = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "PixelOperator", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.PixelOperator = (PixelOperator)stack[sp];

                return false;
            });

            addMethod(liquidClass, "Plot", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Plot(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Point", "(int,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                a0 = (int)raster.Point(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Quad", "(int,int,int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Quad(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5], stack[sp - 6], stack[sp - 7]);

                return false;
            });

            addMethod(liquidClass, "QuadFill", "(int,int,int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.QuadFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5], stack[sp - 6], stack[sp - 7]);

                return false;
            });

            addMethod(liquidClass, "Radial", "(int,int,float,float)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Radial(stack[sp], stack[sp - 1], Util.Int2Float(stack[sp - 2]), Util.Int2Float(stack[sp - 3]));

                return false;
            });

            addMethod(liquidClass, "Rectangle", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Rectangle(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "RectangleFill", "(int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.RectangleFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "Roll", "(int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Scroll(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], (ScrollDirection)stack[sp - 4], wrap: true);

                return false;
            });

            addMethod(liquidClass, "RoundedRectangle", "(int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.RoundedRectangle(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4]);

                return false;
            });

            addMethod(liquidClass, "RoundedRectangleFill", "(int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.RoundedRectangleFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4]);

                return false;
            });

            addMethod(liquidClass, "Scroll", "(int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Scroll(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], (ScrollDirection)stack[sp - 4], wrap: false);

                return false;
            });

            addMethod(liquidClass, "Triangle", "(int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.Triangle(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5]);

                return false;
            });

            addMethod(liquidClass, "TriangleFill", "(int,int,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.TriangleFill(stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4], stack[sp - 5]);

                return false;
            });

            addMethod(liquidClass, "vLine", "(int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var raster = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Raster;

                raster.vLine(stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });
        }

        private void bindBrush()
        {
            var liquidClass = Program.ClassManager.Find("Brush");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindPen()
        {
            var liquidClass = Program.ClassManager.Find("Pen");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Raster);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Pen(a0, stack[sp], Program.Exec.ObjectManager.SystemCharacterSetId);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Canvas)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[stack[sp]].LiquidObject as Liquid.Canvas;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Pen(a0, canvas.BitmapId, Program.Exec.ObjectManager.SystemCharacterSetId);

                return false;
            });

            addMethod(liquidClass, "PrintAt", "(int,int,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var pen = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Pen;

                var text = Liquid.Object.GetString(stack[sp - 2]);

                pen.PrintAt(stack[sp], stack[sp - 1], text);

                return false;
            });
        }

        private void bindTurtle()
        {
            var liquidClass = Program.ClassManager.Find("Turtle");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Raster);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Turtle(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Canvas)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[stack[sp]].LiquidObject as Liquid.Canvas;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Turtle(a0, canvas.BitmapId);

                return false;
            });

            addMethod(liquidClass, "GetHeading", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                d0 = turtle.Heading;

                return false;
            });

            addMethod(liquidClass, "GetTurtleX", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                d0 = turtle.TurtleX;

                return false;
            });

            addMethod(liquidClass, "GetTurtleY", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                d0 = turtle.TurtleY;

                return false;
            });

            addMethod(liquidClass, "GoBackward", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var distance = Liquid.Object.GetDouble(stack[sp]);

                turtle.GoBackward(distance);

                return false;
            });

            addMethod(liquidClass, "GoForward", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var distance = Liquid.Object.GetDouble(stack[sp]);

                turtle.GoForward(distance);

                return false;
            });

            addMethod(liquidClass, "Home", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                turtle.Home();

                return false;
            });

            addMethod(liquidClass, "MoveTo", "(double,double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var x = Liquid.Object.GetDouble(stack[sp]);

                var y = Liquid.Object.GetDouble(stack[sp - 1]);

                turtle.MoveTo(x, y);

                return false;
            });

            addMethod(liquidClass, "PenDown", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                turtle.PenDown();

                return false;
            });

            addMethod(liquidClass, "PenUp", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                turtle.PenUp();

                return false;
            });

            addMethod(liquidClass, "SetHeading", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var heading = Liquid.Object.GetDouble(stack[sp]);

                turtle.SetHeading(heading);

                return false;
            });

            addMethod(liquidClass, "TurnLeft", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var degrees = Liquid.Object.GetDouble(stack[sp]);

                turtle.TurnLeft(degrees);

                return false;
            });

            addMethod(liquidClass, "TurnRight", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var turtle = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Turtle;

                var degrees = Liquid.Object.GetDouble(stack[sp]);

                turtle.TurnRight(degrees);

                return false;
            });
        }

        private void bindFilter()
        {
            var liquidClass = Program.ClassManager.Find("Filter");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Filter(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Canvas)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[stack[sp]].LiquidObject as Liquid.Canvas;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Filter(a0, canvas.BitmapId);

                return false;
            });

            addMethod(liquidClass, "Dilate", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Dilate();

                return false;
            });

            addMethod(liquidClass, "GrayScale", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.GrayScale();

                return false;
            });

            addMethod(liquidClass, "Kaleidoscope", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Kaleidoscope(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "MakeTileable", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.MakeTileable(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Median", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Median();

                return false;
            });

            addMethod(liquidClass, "Noise", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Noise(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "ReplaceAlpha", "(int,byte)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.ReplaceAlpha((uint)stack[sp], (byte)stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "ReplaceColor", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.ReplaceColor((uint)stack[sp], (uint)stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Sepia", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Sepia();

                return false;
            });

            addMethod(liquidClass, "Sharpen", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var filter = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Filter;

                filter.Sharpen();

                return false;
            });
        }

        private void bindCharacterSet()
        {
            var liquidClass = Program.ClassManager.Find("CharacterSet");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Font);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.CharacterSet(id, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Bitmap,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.CharacterSet(id, stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Bitmap,int,int,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.CharacterSet(id, stack[sp], stack[sp - 1], stack[sp - 2], stack[sp - 3], stack[sp - 4]);

                return false;
            });

            addMethod(liquidClass, "Clear", "(byte)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.Clear((byte)stack[sp]);

                return false;
            });

            addMethod(liquidClass, "CustomCharacter", "(byte,int,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                var data = Liquid.Object.GetString(stack[sp - 2]);

                characterSet.CustomCharacter((byte)stack[sp], stack[sp - 1], data);

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                a0 = characterSet.GetHeight();

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                a0 = characterSet.GetWidth();

                return false;
            });

            addMethod(liquidClass, "GetWidth", "(string)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                var text = Liquid.Object.GetString(stack[sp]);

                a0 = characterSet.GetWidth(text);

                return false;
            });

            addMethod(liquidClass, "IsAvailable", "(byte)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                a0 = (characterSet.IsAvailable[stack[sp]]) ? 1 : 0;

                return false;
            });

            addMethod(liquidClass, "MapAll", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.MapAll();

                return false;
            });

            addMethod(liquidClass, "MapCharacter", "(byte,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.MapCharacter((byte)stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "MapCharacters", "(byte,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.MapCharacters((byte)stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "MapTheseCharacters", "(string,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                var text = Liquid.Object.GetString(stack[sp]);

                characterSet.MapTheseCharacters(text, stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Palette", "(byte,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.Palette((byte)stack[sp], (uint)stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Roll", "(byte,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.Scroll((byte)stack[sp], (ScrollDirection)stack[sp - 1], wrap: true);

                return false;
            });

            addMethod(liquidClass, "Scroll", "(byte,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSet = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CharacterSet;

                characterSet.Scroll((byte)stack[sp], (ScrollDirection)stack[sp - 1], wrap: false);

                return false;
            });
        }

        private void bindFont()
        {
            var liquidClass = Program.ClassManager.Find("Font");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);

            addMethod(liquidClass, "Constructor", "(string,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Font(id, path, stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var font = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Font;

                a0 = font.GetHeight();

                return false;
            });

            addMethod(liquidClass, "GetWidth", "(byte)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var font = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Font;

                a0 = font.GetWidth((byte)stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetWidth", "(string)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var font = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Font;

                var text = Liquid.Object.GetString(stack[sp]);

                a0 = font.GetWidth(text);

                return false;
            });
        }

        private void bindView()
        {
            var liquidClass = Program.ClassManager.Find("View");

            Program.ClassManager.Extends(liquidClass, LiquidClass.GEL);
        }

        private void bindConsole()
        {
            var liquidClass = Program.ClassManager.Find("Console");

            Program.ClassManager.Extends(liquidClass, LiquidClass.View);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSetId = Program.Exec.ObjectManager.SystemCharacterSetId;

                var characterSet = Program.Exec.ObjectManager[characterSetId].LiquidObject as Liquid.CharacterSet;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Console(a0, Program.ScreenWidth / characterSet.TileWidth, Program.ScreenHeight / characterSet.TileHeight);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Console(a0, stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "AutoScroll", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.AutoScroll = (stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Blink", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Blink(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Bold", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Bold(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Clear();

                return false;
            });

            addMethod(liquidClass, "GetCursorX", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                a0 = console.CursorX;

                return false;
            });

            addMethod(liquidClass, "GetCursorY", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                a0 = console.CursorY;

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                a0 = console.Height;

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                a0 = console.Width;

                return false;
            });

            addMethod(liquidClass, "Highlight", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.HighlightColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "Ink", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.InkColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "InputLine", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var wait = console.InputLine();

                if (!wait)
                {
                    a0 = Liquid.Object.NewString(id, console.InputBuffer);
                }

                return wait;
            });

            addMethod(liquidClass, "InputNumber", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var wait = console.InputNumber();

                if (!wait)
                {
                    if (console.InputBuffer != "")
                    {
                        if (!double.TryParse(console.InputBuffer, out d0))
                        {
                            task.RaiseError(ErrorCode.Denied);
                            return false;
                        }
                    }
                    else
                    {
                        d0 = 0;
                    }
                }

                return wait;
            });

            addMethod(liquidClass, "Locate", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Locate(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Print", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(stack[sp]);

                console.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(long)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(Liquid.Object.GetLong(stack[sp]));

                console.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(Liquid.Object.GetDouble(stack[sp]));

                console.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Liquid.Object.GetString(stack[sp]);

                console.Print(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.PrintLine();

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(stack[sp]);

                console.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(long)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(Liquid.Object.GetLong(stack[sp]));

                console.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Convert.ToString(Liquid.Object.GetDouble(stack[sp]));

                console.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                var text = Liquid.Object.GetString(stack[sp]);

                console.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "Tab", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Tab(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Underline", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var console = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Console;

                console.Underline(stack[sp] != 0);

                return false;
            });
        }

        private void bindTileMap()
        {
            var liquidClass = Program.ClassManager.Find("TileMap");

            Program.ClassManager.Extends(liquidClass, LiquidClass.View);

            addMethod(liquidClass, "Constructor", "(CharacterSet)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSetId = stack[sp];

                var characterSet = Program.Exec.ObjectManager[characterSetId].LiquidObject as Liquid.CharacterSet;

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.TileMap(a0, Program.ScreenWidth / characterSet.TileWidth, Program.ScreenHeight / characterSet.TileHeight, characterSetId);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(CharacterSet,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.TileMap(a0, stack[sp], stack[sp - 1], stack[sp - 2]);

                return false;
            });

            addMethod(liquidClass, "AutoScroll", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.AutoScroll = (stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.Clear();

                return false;
            });

            addMethod(liquidClass, "Ink", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.InkColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "GetCursorX", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                a0 = tileMap.CursorX;

                return false;
            });

            addMethod(liquidClass, "GetCursorY", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                a0 = tileMap.CursorY;

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                a0 = tileMap.Height;

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                a0 = tileMap.Width;

                return false;
            });

            addMethod(liquidClass, "Highlight", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.HighlightColor = (uint)stack[sp];

                return false;
            });

            addMethod(liquidClass, "InputLine", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var wait = tileMap.InputLine();

                if (!wait)
                {
                    a0 = Liquid.Object.NewString(id, tileMap.InputBuffer);
                }

                return wait;
            });

            addMethod(liquidClass, "InputNumber", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var wait = tileMap.InputNumber();

                if (!wait)
                {
                    if (tileMap.InputBuffer != "")
                    {
                        d0 = Convert.ToDouble(tileMap.InputBuffer);
                    }
                    else
                    {
                        d0 = 0;
                    }
                }

                return wait;
            });

            addMethod(liquidClass, "Locate", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.Locate(stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Print", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(stack[sp]);

                tileMap.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(long)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(Liquid.Object.GetLong(stack[sp]));

                tileMap.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(Liquid.Object.GetDouble(stack[sp]));

                tileMap.Print(text);

                return false;
            });

            addMethod(liquidClass, "Print", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Liquid.Object.GetString(stack[sp]);

                tileMap.Print(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.PrintLine();

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(stack[sp]);

                tileMap.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(long)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(Liquid.Object.GetLong(stack[sp]));

                tileMap.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(double)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Convert.ToString(Liquid.Object.GetDouble(stack[sp]));

                tileMap.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "PrintLine", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                var text = Liquid.Object.GetString(stack[sp]);

                tileMap.PrintLine(text);

                return false;
            });

            addMethod(liquidClass, "Tab", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tileMap = Program.Exec.ObjectManager[id].LiquidObject as Liquid.TileMap;

                tileMap.Tab(stack[sp]);

                return false;
            });
        }

        private void bindCopperBars()
        {
            var liquidClass = Program.ClassManager.Find("CopperBars");

            Program.ClassManager.Extends(liquidClass, LiquidClass.View);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.CopperBars(a0, Program.ScreenHeight);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.CopperBars(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "RollDown", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var copperBars = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CopperBars;

                copperBars.RollDown();

                return false;
            });

            addMethod(liquidClass, "RollUp", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var copperBars = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CopperBars;

                copperBars.RollUp();

                return false;
            });

            addMethod(liquidClass, "Set", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var copperBars = Program.Exec.ObjectManager[id].LiquidObject as Liquid.CopperBars;

                copperBars.Set(stack[sp], (uint)stack[sp - 1]);

                return false;
            });
        }

        private void bindCanvas()
        {
            var liquidClass = Program.ClassManager.Find("Canvas");

            Program.ClassManager.Extends(liquidClass, LiquidClass.View);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Canvas(a0, Program.ScreenWidth, Program.ScreenHeight);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Canvas(a0, stack[sp], stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Canvas(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Clear", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.Clear();

                return false;
            });

            addMethod(liquidClass, "DoubleBuffer", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.DoubleBuffer();

                return false;
            });

            addMethod(liquidClass, "GetHeight", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                a0 = canvas.Height;

                return false;
            });

            addMethod(liquidClass, "GetWidth", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                a0 = canvas.Width;

                return false;
            });

            addMethod(liquidClass, "Peek", "(int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                a0 = (int)canvas.Peek(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "Poke", "(int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.Poke(stack[sp], (uint)stack[sp - 1]);

                return false;
            });

            addMethod(liquidClass, "SingleBuffer", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.SingleBuffer();

                return false;
            });

            addMethod(liquidClass, "Smooth", "(boolean)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.Smooth(stack[sp] != 0);

                return false;
            });

            addMethod(liquidClass, "SwapBuffers", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var canvas = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Canvas;

                canvas.SwapBuffers();

                return false;
            });
        }

        private void bindSprite()
        {
            var liquidClass = Program.ClassManager.Find("Sprite");

            Program.ClassManager.Extends(liquidClass, LiquidClass.GEL);

            addMethod(liquidClass, "Constructor", "(Bitmap)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Sprite(a0, stack[sp]);

                return false;
            });

            addMethod(liquidClass, "GetBitmap", "()", LiquidClass.Bitmap, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var sprite = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Sprite;

                a0 = sprite.GetBitmap();

                return false;
            });
        }

        private void bindTiles()
        {
            var liquidClass = Program.ClassManager.Find("Tiles");

            Program.ClassManager.Extends(liquidClass, LiquidClass.GEL);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var characterSetId = Program.Exec.ObjectManager.Copy(Program.Exec.ObjectManager.SystemCharacterSetId);

                var caption = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Tiles(a0, characterSetId, caption);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(CharacterSet,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var caption = Liquid.Object.GetString(stack[sp - 1]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Tiles(a0, stack[sp], caption);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(CharacterSet,string,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var caption = Liquid.Object.GetString(stack[sp - 1]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Text(a0, stack[sp], caption, stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "GetText", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tiles = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Tiles;

                a0 = Liquid.Object.NewString(id, tiles.Caption);

                return false;
            });

            addMethod(liquidClass, "hAlign", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tiles = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Tiles;

                tiles.hAlign(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "SetText", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tiles = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Tiles;

                tiles.SetCaption(Liquid.Object.GetString(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "vAlign", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var tiles = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Tiles;

                tiles.vAlign(stack[sp]);

                return false;
            });
        }

        private void bindText()
        {
            var liquidClass = Program.ClassManager.Find("Text");

            Program.ClassManager.Extends(liquidClass, LiquidClass.GEL);

            addMethod(liquidClass, "Constructor", "(Font,string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var caption = Liquid.Object.GetString(stack[sp - 1]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Text(a0, stack[sp], caption);

                return false;
            });

            addMethod(liquidClass, "Constructor", "(Font,string,int,int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var caption = Liquid.Object.GetString(stack[sp - 1]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Text(a0, stack[sp], caption, stack[sp - 2], stack[sp - 3]);

                return false;
            });

            addMethod(liquidClass, "GetText", "()", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Text;

                a0 = Liquid.Object.NewString(id, text.Caption);

                return false;
            });

            addMethod(liquidClass, "hAlign", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Text;

                text.hAlign(stack[sp]);

                return false;
            });

            addMethod(liquidClass, "SetText", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Text;

                text.SetCaption(Liquid.Object.GetString(stack[sp]));

                return false;
            });

            addMethod(liquidClass, "vAlign", "(int)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Text;

                text.vAlign(stack[sp]);

                return false;
            });
        }

        private void bindAudio()
        {
            var liquidClass = Program.ClassManager.Find("Audio");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Object);
        }

        private void bindSound()
        {
            var liquidClass = Program.ClassManager.Find("Sound");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Audio);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Sound(a0, path);

                return false;
            });

            addMethod(liquidClass, "Loop", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var sound = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Sound;

                sound.Loop();

                return false;
            });

            addMethod(liquidClass, "Play", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var sound = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Sound;

                sound.Play();

                return false;
            });

            addMethod(liquidClass, "Stop", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var sound = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Sound;

                sound.Stop();

                return false;
            });
        }

        private void bindMusic()
        {
            var liquidClass = Program.ClassManager.Find("Music");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Audio);

            addMethod(liquidClass, "Constructor", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var path = Liquid.Object.GetString(stack[sp]);

                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Music(a0, path);

                return false;
            });

            addMethod(liquidClass, "Play", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var music = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Music;

                music.Play();

                return false;
            });

            addMethod(liquidClass, "Stop", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var music = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Music;

                music.Stop();

                return false;
            });
        }

        private void bindVoice()
        {
            var liquidClass = Program.ClassManager.Find("Voice");

            Program.ClassManager.Extends(liquidClass, LiquidClass.Audio);

            addMethod(liquidClass, "Constructor", "()", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                Program.Exec.ObjectManager[a0].LiquidObject = new Liquid.Voice(a0);

                return false;
            });

            addMethod(liquidClass, "Speak", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var voice = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Voice;

                var textToSpeak = Liquid.Object.GetString(stack[sp]);

                voice.Speak(textToSpeak);

                return false;
            });

            addMethod(liquidClass, "SpeakAsync", "(string)", LiquidClass.None, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var voice = Program.Exec.ObjectManager[id].LiquidObject as Liquid.Voice;

                var textToSpeak = Liquid.Object.GetString(stack[sp]);

                voice.SpeakAsync(textToSpeak);

                return false;
            });
        }

        private void bindAPI()
        {
            addFunction(LiquidClass.Int, "MaxValue", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = int.MaxValue;

                return false;
            });

            addFunction(LiquidClass.Int, "MinValue", "()", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = int.MinValue;

                return false;
            });

            addFunction(LiquidClass.Long, "MaxValue", "()", LiquidClass.Long, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                c0 = long.MaxValue;

                return false;
            });

            addFunction(LiquidClass.Long, "MinValue", "()", LiquidClass.Long, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                c0 = long.MinValue;

                return false;
            });

            addFunction(LiquidClass.Double, "Epsilon", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.Epsilon;

                return false;
            });

            addFunction(LiquidClass.Double, "MaxValue", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.MaxValue;

                return false;
            });

            addFunction(LiquidClass.Double, "MinValue", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.MinValue;

                return false;
            });

            addFunction(LiquidClass.Double, "NaN", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.NaN;

                return false;
            });

            addFunction(LiquidClass.Double, "NegativeInfinity", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.NegativeInfinity;

                return false;
            });

            addFunction(LiquidClass.Double, "PositiveInfinity", "()", LiquidClass.Double, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                d0 = double.PositiveInfinity;

                return false;
            });

            addFunction(LiquidClass.String, "Character", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var ch = (char)stack[sp];

                a0 = Liquid.Object.NewString(id, "" + ch);

                return false;
            });

            addFunction(LiquidClass.String, "Count", "(string,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                var ch = stack[sp - 1];

                var count = text.Count(f => f == ch);

                a0 = count;

                return false;
            });

            addFunction(LiquidClass.String, "EndsWith", "(string,string)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                var value = Liquid.Object.GetString(stack[sp - 1]);

                a0 = (text.EndsWith(value)) ? 1 : 0;

                return false;
            });

            addFunction(LiquidClass.String, "GetCharacter", "(string,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var index = stack[sp - 1];

                if (index < 0 || index >= text.Length)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                a0 = text[index];

                return false;
            });

            addFunction(LiquidClass.String, "GetLength", "(string)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = text.Length;

                return false;
            });

            addFunction(LiquidClass.String, "IndexOf", "(string,string)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = text.IndexOf(Liquid.Object.GetString(stack[sp - 1]));

                return false;
            });

            addFunction(LiquidClass.String, "IndexOf", "(string,string,int)", LiquidClass.Int, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = text.IndexOf(Liquid.Object.GetString(stack[sp - 1]), stack[sp - 2]);

                return false;
            });

            addFunction(LiquidClass.String, "Left", "(string,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var index = stack[sp - 1];

                if (index >= text.Length)
                {
                    a0 = Liquid.Object.NewString(id, text);
                }
                else if (index == 0)
                {
                    a0 = Liquid.Object.NewString(id);
                }
                else if (index < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, text.Substring(0, index));
                }

                return false;
            });

            addFunction(LiquidClass.String, "Pad", "(string,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var length = stack[sp - 1];

                if (length == text.Length)
                {
                    a0 = Liquid.Object.NewString(id, text);
                }
                else if (length > text.Length)
                {
                    a0 = Liquid.Object.NewString(id, text + new string(' ', length - text.Length));
                }
                else if (length == 0)
                {
                    a0 = Liquid.Object.NewString(id);
                }
                else if (length < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, text.Substring(0, length));
                }

                return false;
            });

            addFunction(LiquidClass.String, "Repeat", "(string,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, string.Concat(Enumerable.Repeat(text, stack[sp - 1])));

                return false;
            });

            addFunction(LiquidClass.String, "Right", "(string,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var index = stack[sp - 1];

                if (index >= text.Length)
                {
                    a0 = Liquid.Object.NewString(id, text);
                }
                else if (index == 0)
                {
                    a0 = Liquid.Object.NewString(id);
                }
                else if (index < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, text.Substring(text.Length - index, index));
                }

                return false;
            });

            addFunction(LiquidClass.String, "Slice", "(string,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var startIndex = stack[sp - 1];

                if (startIndex < 0)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }
                else if (startIndex >= text.Length)
                {
                    a0 = Liquid.Object.NewString(id);
                }
                else
                {
                    a0 = Liquid.Object.NewString(id, text.Substring(startIndex));
                }

                return false;
            });

            addFunction(LiquidClass.String, "Slice", "(string,int,int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var task = Program.Exec.ObjectManager.GetTask(id);

                var text = Liquid.Object.GetString(stack[sp]);

                var startIndex = stack[sp - 1];

                var length = stack[sp - 2];

                if (startIndex < 0 || length < 1 || startIndex + length > text.Length)
                {
                    task.RaiseError(ErrorCode.IllegalQuantity);
                    return false;
                }

                a0 = Liquid.Object.NewString(id, text.Substring(startIndex, length));

                return false;
            });

            addFunction(LiquidClass.String, "Space", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                a0 = Liquid.Object.NewString(id, new string(' ', stack[sp]));

                return false;
            });

            addFunction(LiquidClass.String, "StartsWith", "(string,string)", LiquidClass.Boolean, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                var value = Liquid.Object.GetString(stack[sp - 1]);

                a0 = (text.StartsWith(value)) ? 1 : 0;

                return false;
            });

            addFunction(LiquidClass.String, "ToBin", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Convert.ToString(stack[sp], 2);

                a0 = Liquid.Object.NewString(id, text);

                return false;
            });

            addFunction(LiquidClass.String, "ToHex", "(int)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Convert.ToString(stack[sp], 16);

                a0 = Liquid.Object.NewString(id, text);

                return false;
            });

            addFunction(LiquidClass.String, "ToLower", "(string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, text.ToLower());

                return false;
            });

            addFunction(LiquidClass.String, "ToUpper", "(string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, text.ToUpper());

                return false;
            });

            addFunction(LiquidClass.String, "Trim", "(string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, text.Trim());

                return false;
            });

            addFunction(LiquidClass.String, "TrimEnd", "(string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, text.TrimEnd());

                return false;
            });

            addFunction(LiquidClass.String, "TrimStart", "(string)", LiquidClass.String, LiquidClass.None, delegate (LiquidClass lc, int id, int[] stack, int sp, ref int a0, ref SmartPointer bx, ref long c0, ref double d0)
            {
                var text = Liquid.Object.GetString(stack[sp]);

                a0 = Liquid.Object.NewString(id, text.TrimStart());

                return false;
            });
        }
    }
}

