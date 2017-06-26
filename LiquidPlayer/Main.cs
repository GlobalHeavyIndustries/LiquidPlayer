using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

// design/choice > default/chance

namespace LiquidPlayer
{
    public class Program
    {
        public const int WIDTH = 1280, HEIGHT = 720;    // HD
        public const int VM_STACK_SIZE = 16384;

        private static string tempPath = "";
        private static string sharedPath = "";

        private static GameWindow gameWindow;

        private static int screenWidth, screenHeight;
        private static int windowWidth, windowHeight;

        private static HiResTimer hiResTimer = new HiResTimer();
        private static long startTime = hiResTimer.Value;
        private static Log log = new Log();
        private static Random random = new Random();

        private static Kernal.ClassManager classManager = new Kernal.ClassManager();
        private static Kernal.API api = new Kernal.API();
        private static Kernal.StandardLibrary standardLibrary = new Kernal.StandardLibrary();

        private static Exec.Exec exec = new Exec.Exec();

        private static int mouseUpEventTime;
        private static int mouseWheelEventTime;

        public static string TempPath
        {
            get
            {
                return tempPath;
            }
        }

        public static string SharedPath
        {
            get
            {
                return sharedPath;
            }
        }

        public static GameWindow GameWindow
        {
            get
            {
                return gameWindow;
            }
        }

        public static int ScreenWidth
        {
            get
            {
                return screenWidth;
            }
            set
            {
                screenWidth = value;
            }
        }

        public static int ScreenHeight
        {
            get
            {
                return screenHeight;
            }
            set
            {
                screenHeight = value;
            }
        }

        public static int WindowWidth
        {
            get
            {
                return windowWidth;
            }
            set
            {
                windowWidth = value;
            }
        }

        public static int WindowHeight
        {
            get
            {
                return windowHeight;
            }
            set
            {
                windowHeight = value;
            }
        }

        public static int SystemClock
        {
            get
            {
                // Accurate to 1,000 ms (1 second)

                var value = hiResTimer.Value - startTime;
                var resolution = hiResTimer.Frequency / 1000d;

                return (int)(value / resolution);
            }
        }

        public static long AtomicClock
        {
            get
            {
                // Accurate to 1,000,000 hz (1 second)

                var value = hiResTimer.Value - startTime;
                var resolution = hiResTimer.Frequency / 1000000d;

                return (long)(value / resolution);
            }
        }

        public static Log Log
        {
            get
            {
                return log;
            }
        }

        public static Random Random
        {
            get
            {
                return random;
            }
        }

        public static Kernal.ClassManager ClassManager
        {
            get
            {
                return classManager;
            }
        }

        public static Kernal.API API
        {
            get
            {
                return api;
            }
        }

        public static Kernal.StandardLibrary StandardLibrary
        {
            get
            {
                return standardLibrary;
            }
        }

        public static Exec.Exec Exec
        {
            get
            {
                return exec;
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(OnThreadException);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);


            tempPath = Path.GetTempPath() + "LiquidStudio-";

            sharedPath = Directory.GetCurrentDirectory() + @"\Shared\";


            var path = "";

            var commandLine = "";

            var argumentList = new List<string>(args);

            if (argumentList.Count == 0)
            {
                var openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "ldx files (*.ldx)|*.ldx|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            else
            {
                path = argumentList[0];

                if (argumentList.Count == 2)
                {
                    commandLine = argumentList[1];
                }
            }

            var options = new ToolkitOptions
            {
                EnableHighResolution = false
            };

            using (Toolkit.Init(options))
            {
                gameWindow = NewGameWindow(WIDTH, HEIGHT);

                screenWidth = WIDTH;
                screenHeight = HEIGHT;

                windowWidth = WIDTH;
                windowHeight = HEIGHT;

                Sprockets.Graphics.Init(WIDTH, HEIGHT);

                StandardLibrary.Load(ClassManager);

                exec.ObjectManager.LoadDefaultResources();

                var taskId = MultiLoad(path, commandLine);

                if (taskId == 0)
                {
                    return;
                }

                var task = Exec.ObjectManager[taskId].LiquidObject as Liquid.Task;

                exec.AddTask(taskId);

                task.Run();

                using (gameWindow)
                {
                    gameWindow.Run(60d);
                }

                exec.ObjectManager.FreeResources();

                exec.Sweeper.Run();
            }
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("CurrentDomain_UnhandledException: " + e.ExceptionObject.ToString(), "Guru Meditation Error");
        }

        public static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs t)
        {
            MessageBox.Show("OnThreadException: " + t.Exception.ToString(), "Guru Meditation Error");
        }

        public static int MultiLoad(string path, string commandLine)
        {
            // TODO!
            //
            // - Task and Applet should load in a "safe space"
            //

            var taskId = 0;
            var taskType = 0;

            if (!path.EndsWith(".ldx"))
            {
                path += ".ldx";
            }

            var resolvedPath = Util.FindFile(path, SharedPath);

            if (resolvedPath == "")
            {
                MessageBox.Show("File not found", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }

            Compression.DecompressFile(resolvedPath, TempPath + "temp.ldx", CompressionType.GZip);

            using (BinaryReader reader = new BinaryReader(File.Open(Program.TempPath + "temp.ldx", FileMode.Open)))
            {
                var header = Encoding.ASCII.GetString(reader.ReadBytes(8));

                if (header.Substring(0, 4) != "LDXv")
                {
                    MessageBox.Show("File header is corrupted", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                if (header.Substring(4, 4) != "2.00")
                {
                    MessageBox.Show("Unsupported file version", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }

                taskType = reader.ReadInt32();

                if (taskType < 2 || taskType > 3)
                {
                    MessageBox.Show("Unsupported task type", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }
            }

            switch (taskType)
            {
                case 1:
                    throw new NotImplementedException();
                case 2:
                    taskId = Liquid.Program.NewProgram(resolvedPath, commandLine);
                    break;
                case 3:
                    taskId = Liquid.App.NewApp(resolvedPath, commandLine);
                    break;
                case 4:
                    throw new NotImplementedException();
            }

            return taskId;
        }

        public static GameWindow NewGameWindow(int width, int height)
        {
            gameWindow = new GameWindow(width, height, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8, 4), "Liquid Player");

            gameWindow.Icon = new System.Drawing.Icon("player.ico");

            gameWindow.VSync = VSyncMode.On;

            CenterGameWindow();

            gameWindow.KeyDown += GameWindow_KeyDown;

            gameWindow.KeyUp += GameWindow_KeyUp;

            GameWindow.KeyPress += GameWindow_KeyPress;

            gameWindow.MouseMove += GameWindow_MouseMove;

            gameWindow.MouseDown += GameWindow_MouseDown;

            gameWindow.MouseUp += GameWindow_MouseUp;

            gameWindow.MouseWheel += GameWindow_MouseWheel;

            gameWindow.Resize += GameWindow_Resize;

            gameWindow.WindowStateChanged += GameWindow_WindowStateChanged;

            gameWindow.UpdateFrame += GameWindow_UpdateFrame;
                
            gameWindow.RenderFrame += GameWindow_RenderFrame;

            return gameWindow;
        }

        public static void CenterGameWindow()
        {
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;

            var screenHeight = Screen.PrimaryScreen.Bounds.Height;

            gameWindow.Location = new System.Drawing.Point((screenWidth - GameWindow.Width) / 2, (screenHeight - GameWindow.Height) / 2);
        }

        public static void ResizeGameWindow(int width, int height)
        {
            gameWindow.Width = width;

            gameWindow.Height = height;
        }

        public static void SwapBuffers()
        {
            gameWindow.SwapBuffers();
        }

        private static void GameWindow_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            var focus = Exec.ObjectManager.Focus;
            var key = Sprockets.Input.TranslateKey(e.Key);

            if (focus != 0)
            {
                Exec.Router.Send(0, focus, MessageBody.KeyDown, key.ToString(), Exec.ActiveTask);
            }
        }

        private static void GameWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            var focus = Exec.ObjectManager.Focus;
            var key = Sprockets.Input.TranslateKey(e.Key);

            if (focus != 0)
            {
                Exec.Router.Send(0, focus, MessageBody.KeyUp, key.ToString(), Exec.ActiveTask);

                Exec.Router.Send(0, focus, MessageBody.KeyPress, key.ToString(), Exec.ActiveTask);
            }
        }

        private static void GameWindow_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            //if (Exec.ObjectManager.Focus != 0)
            //{
            //    Exec.Router.Send(0, Exec.ObjectManager.Focus, MessageBody.KeyPress, e.KeyChar.ToString(), Exec.ActiveTask);
            //}
        }

        private static void GameWindow_MouseMove(object sender, MouseMoveEventArgs e)
        {
            Sprockets.Input.MouseXPosition = e.X;
            Sprockets.Input.MouseYPosition = e.Y;

            //Exec.Router.Send(0, id, (int)MessageBody.MouseMove);
        }

        private static void GameWindow_MouseDown(object sender, MouseButtonEventArgs e)
        { 
            Sprockets.Input.MouseButton = (e.IsPressed) ? 1 : 0;

            Sprockets.Input.MouseClicked();

            var clickedOn = Sprockets.Input.MouseClickedOn;
            var clickedOnNode = Sprockets.Input.MouseClickedOnNode;

            if (clickedOn != 0)
            {
                Exec.Router.Send(0, clickedOn, MessageBody.MouseDown, clickedOnNode.ToString(), Exec.ActiveTask);

                Exec.ObjectManager.Focus = clickedOn;
            }
            else
            {
                Exec.ObjectManager.Focus = Exec.ActiveTask;
            }
        }

        private static void GameWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Sprockets.Input.MouseButton = (e.IsPressed) ? 1 : 0;

            var clickedOn = Sprockets.Input.MouseClickedOn;
            var clickedOnNode = Sprockets.Input.MouseClickedOnNode;

            if (clickedOn != 0)
            {
                Exec.Router.Send(0, clickedOn, MessageBody.MouseUp, clickedOnNode.ToString(), Exec.ActiveTask);

                if (mouseUpEventTime != 0 && SystemClock <= mouseUpEventTime + 500)
                {
                    Exec.Router.Send(0, clickedOn, MessageBody.DoubleClicked, clickedOnNode.ToString(), Exec.ActiveTask);

                    mouseUpEventTime = 0;
                }
                else 
                {
                    Exec.Router.Send(0, clickedOn, MessageBody.Clicked, clickedOnNode.ToString(), Exec.ActiveTask);

                    mouseUpEventTime = SystemClock;
                }
            }

            Sprockets.Input.MouseUnClicked();
        }

        private static void GameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // WINDOWS 10 / TKS FIX!! (otherwise MouseWheel event fires twice)

            if (mouseWheelEventTime == SystemClock)
            {
                return;
            }

            var delta = e.Delta; // e.DeltaPrecise;

            Exec.Router.Send(0, Exec.ObjectManager.Focus, MessageBody.MouseWheel, delta.ToString(), Exec.ActiveTask);

            mouseWheelEventTime = SystemClock;
        }

        private static void GameWindow_Resize(object sender, EventArgs e)
        {
            windowWidth = gameWindow.Width;

            windowHeight = gameWindow.Height;

            Sprockets.Graphics.Resize(gameWindow.Width, gameWindow.Height);

            Sprockets.Graphics.ViewOrtho();
        }

        private static void GameWindow_WindowStateChanged(object sender, EventArgs e)
        {
            // TODO
            //
            // when a window's state changes an InvalidValue OpenGL error is generated
            //
            // for now just consume the error
            //

            var error = Sprockets.Graphics.GetError();

            //System.Diagnostics.Debug.Assert(error == OpenTK.Graphics.OpenGL.ErrorCode.NoError);
        }

        private static void GameWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            // TODO?
        }

        private static void GameWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            if (exec.UpdateScene())
            {
                gameWindow.Close();

                return;
            }

            if (gameWindow.WindowState != WindowState.Minimized)
            {
                exec.RenderScene();
            }
        }
    }
}