/*
 
    Liquid Player
    © 2017 by Bryan Flick

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

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
        public const int WIDTH = 960, HEIGHT = 540;
        public const int VM_STACK_SIZE = 16384;

        private static string tempPath = "";

        private static GameWindow gameWindow;

        private static int screenWidth, screenHeight;
        private static int windowWidth, windowHeight;

        private static HiResTimer hiResTimer = new HiResTimer();
        private static Log log = new Log();
        private static Random random = new Random();

        private static Kernal.ClassManager classManager = new Kernal.ClassManager();
        private static Kernal.API api = new Kernal.API();
        private static Kernal.StandardLibrary standardLibrary = new Kernal.StandardLibrary();

        private static Exec.Exec exec = new Exec.Exec();

        public static string TempPath
        {
            get
            {
                return tempPath;
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

                var value = hiResTimer.Value;
                var resolution = hiResTimer.Frequency / 1000d;

                return (int)(value / resolution);
            }
        }

        public static long AtomicClock
        {
            get
            {
                // Accurate to 1,000,000 hz (1 second)

                var value = hiResTimer.Value;
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
            tempPath = Path.GetTempPath() + "LiquidStudio-";

            var fileName = "";

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
                    fileName = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            else
            {
                fileName = argumentList[0];

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

                var taskId = MultiLoad(fileName, commandLine);

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

        public static int MultiLoad(string fileName, string commandLine)
        {
            // TODO!
            //
            // - Task and Applet should load in a "safe space"
            //

            var taskId = 0;
            var taskType = 0;

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

            if (!File.Exists(sourceFilePath + "\\" + sourceFileName))
            {
                MessageBox.Show("File not found", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }

            Compression.DecompressFile(sourceFilePath + "\\" + sourceFileName, Program.TempPath + "temp.ldx", CompressionType.GZip);

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
                    taskId = Liquid.Program.NewProgram(fileName, commandLine);
                    break;
                case 3:
                    taskId = Liquid.App.NewApp(fileName, commandLine);
                    break;
                case 4:
                    throw new NotImplementedException();
            }

            return taskId;
        }

        public static GameWindow NewGameWindow(int width, int height)
        {
            gameWindow = new GameWindow(width, height, new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8), "Liquid Player");

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
            if (Exec.ObjectManager.Focus != 0)
            {
                Exec.Router.Send(0, Exec.ObjectManager.Focus, (int)MessageBody.KeyDown, Sprockets.Input.TranslateKey(e.Key));
            }
        }

        private static void GameWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (Exec.ObjectManager.Focus != 0)
            {
                Exec.Router.Send(0, Exec.ObjectManager.Focus, (int)MessageBody.KeyUp, Sprockets.Input.TranslateKey(e.Key));
            }
        }

        private static void GameWindow_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            if (Exec.ObjectManager.Focus != 0)
            {
                Exec.Router.Send(0, Exec.ObjectManager.Focus, (int)MessageBody.KeyPress, e.KeyChar);
            }
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
                Exec.Router.Send(0, clickedOn, (int)MessageBody.MouseDown, clickedOnNode);
            }
        }

        private static void GameWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Sprockets.Input.MouseButton = (e.IsPressed) ? 1 : 0;

            var clickedOn = Sprockets.Input.MouseClickedOn;
            var clickedOnNode = Sprockets.Input.MouseClickedOnNode;

            if (clickedOn != 0)
            {
                Exec.Router.Send(0, clickedOn, (int)MessageBody.MouseUp, clickedOnNode);
            }

            Sprockets.Input.MouseUnClicked();
        }

        private static void GameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.DeltaPrecise * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / 120;

            //Exec.Router.Send(0, Exec.ObjectManager.Focus, (int)MessageBody.MouseWheel, (int)delta);
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