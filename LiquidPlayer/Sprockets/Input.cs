using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

using System.Windows.Forms;

namespace LiquidPlayer.Sprockets
{
    public static class Input
    {
        public static int MouseButton
        {
            get;
            set;
        }

        public static int MouseXPosition
        {
            get;
            set;
        }

        public static int MouseYPosition
        {
            get;
            set;
        }

        public static int MouseSnappedXPosition
        {
            get;
            set;
        }

        public static int MouseSnappedYPosition
        {
            get;
            set;
        }

        public static int MousePointingAt
        {
            get;
            set;
        }

        public static int MousePointingAtNode
        {
            get;
            set;
        }

        public static int MouseClickedOn
        {
            get;
            set;
        }

        public static int MouseClickedOnNode
        {
            get;
            set;
        }

        public static void MouseClicked()
        {
            MouseClickedOn = MousePointingAt;

            MouseClickedOnNode = MousePointingAtNode;
        }

        public static void MouseUnClicked()
        {
            MouseClickedOn = 0;

            MouseClickedOnNode = 0;
        }

        public static int TranslateKey(Key key)
        {
            var capsLock = Control.IsKeyLocked(Keys.CapsLock);

            var numLock = Control.IsKeyLocked(Keys.NumLock);

            var shiftKey = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            var controlKey = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            switch (key)
            {
                case Key.F1:
                    return (int)LiquidKey.F1;
                case Key.F2:
                    return (int)LiquidKey.F2;
                case Key.F3:
                    return (int)LiquidKey.F3;
                case Key.F4:
                    return (int)LiquidKey.F4;
                case Key.F5:
                    return (int)LiquidKey.F5;
                case Key.F6:
                    return (int)LiquidKey.F6;
                case Key.F7:
                    return (int)LiquidKey.F7;
                case Key.F8:
                    return (int)LiquidKey.F8;
                case Key.F9:
                    return (int)LiquidKey.F9;
                case Key.F10:
                    return (int)LiquidKey.F10;
                case Key.F11:
                    return (int)LiquidKey.F11;
                case Key.F12:
                    return (int)LiquidKey.F12;

                case Key.Number0:
                    return (!shiftKey) ? '0' : ')';
                case Key.Number1:
                    return (!shiftKey) ? '1' : '!';
                case Key.Number2:
                    return (!shiftKey) ? '2' : '@';
                case Key.Number3:
                    return (!shiftKey) ? '3' : '#';
                case Key.Number4:
                    return (!shiftKey) ? '4' : '$';
                case Key.Number5:
                    return (!shiftKey) ? '5' : '%';
                case Key.Number6:
                    return (!shiftKey) ? '6' : '^';
                case Key.Number7:
                    return (!shiftKey) ? '7' : '&';
                case Key.Number8:
                    return (!shiftKey) ? '8' : '*';
                case Key.Number9:
                    return (!shiftKey) ? '9' : '(';

                case Key.A:
                    return (!shiftKey ^ capsLock) ? 'a' : 'A';
                case Key.B:
                    return (!shiftKey ^ capsLock) ? 'b' : 'B';
                case Key.C:
                    if (controlKey)
                    {
                        return (int)LiquidKey.CTRL_C;
                    }

                    return (!shiftKey ^ capsLock) ? 'c' : 'C';
                case Key.D:
                    if (controlKey)
                    {
                        return (int)LiquidKey.CTRL_D;
                    }

                    return (!shiftKey ^ capsLock) ? 'd' : 'D';
                case Key.E:
                    if (controlKey)
                    {
                        return (int)LiquidKey.CTRL_E;
                    }

                    return (!shiftKey ^ capsLock) ? 'e' : 'E';
                case Key.F:
                    if (controlKey)
                    {
                        return (int)LiquidKey.CTRL_F;
                    }

                    return (!shiftKey ^ capsLock) ? 'f' : 'F';
                case Key.G:
                    return (!shiftKey ^ capsLock) ? 'g' : 'G';
                case Key.H:
                    return (!shiftKey ^ capsLock) ? 'h' : 'H';
                case Key.I:
                    return (!shiftKey ^ capsLock) ? 'i' : 'I';
                case Key.J:
                    return (!shiftKey ^ capsLock) ? 'j' : 'J';
                case Key.K:
                    return (!shiftKey ^ capsLock) ? 'k' : 'K';
                case Key.L:
                    return (!shiftKey ^ capsLock) ? 'l' : 'L';
                case Key.M:
                    return (!shiftKey ^ capsLock) ? 'm' : 'M';
                case Key.N:
                    return (!shiftKey ^ capsLock) ? 'n' : 'N';
                case Key.O:
                    return (!shiftKey ^ capsLock) ? 'o' : 'O';
                case Key.P:
                    return (!shiftKey ^ capsLock) ? 'p' : 'P';
                case Key.Q:
                    return (!shiftKey ^ capsLock) ? 'q' : 'Q';
                case Key.R:
                    return (!shiftKey ^ capsLock) ? 'r' : 'R';
                case Key.S:
                    return (!shiftKey ^ capsLock) ? 's' : 'S';
                case Key.T:
                    return (!shiftKey ^ capsLock) ? 't' : 'T';
                case Key.U:
                    return (!shiftKey ^ capsLock) ? 'u' : 'U';
                case Key.V:
                    return (!shiftKey ^ capsLock) ? 'v' : 'V';
                case Key.W:
                    return (!shiftKey ^ capsLock) ? 'w' : 'W';
                case Key.X:
                    return (!shiftKey ^ capsLock) ? 'x' : 'X';
                case Key.Y:
                    return (!shiftKey ^ capsLock) ? 'y' : 'Y';
                case Key.Z:
                    return (!shiftKey ^ capsLock) ? 'z' : 'Z';

                case Key.Enter:
                    return (int)LiquidKey.Enter;
                case Key.Escape:
                    return (int)LiquidKey.Escape;
                case Key.Space:
                    return ' ';
                case Key.Tab:
                    return (!shiftKey) ? (int)LiquidKey.ShiftTab : (int)LiquidKey.Tab;
                case Key.BackSpace:
                    return (int)LiquidKey.Backspace;

                case Key.Tilde:
                    return (!shiftKey) ? '`' : '~';
                case Key.Minus:
                    return (!shiftKey) ? '-' : '_';
                case Key.Plus:
                    return (!shiftKey) ? '=' : '+';
                case Key.BracketLeft:
                    return (!shiftKey) ? '[' : '{';
                case Key.BracketRight:
                    return (!shiftKey) ? ']' : '}';
                case Key.BackSlash:
                    return (!shiftKey) ? '\\' : '|';
                case Key.Semicolon:
                    return (!shiftKey) ? ';' : ':';
                case Key.Quote:
                    return (!shiftKey) ? '\'' : '"';
                case Key.Comma:
                    return (!shiftKey) ? ',' : '<';
                case Key.Period:
                    return (!shiftKey) ? '.' : '>';
                case Key.Slash:
                    return (!shiftKey) ? '/' : '?';

                case Key.Insert:
                    return (int)LiquidKey.Insert;
                case Key.Delete:
                    return (int)LiquidKey.Delete;
                case Key.Home:
                    return (int)LiquidKey.Home;
                case Key.End:
                    return (int)LiquidKey.End;
                case Key.PageUp:
                    return (int)LiquidKey.PageUp;
                case Key.PageDown:
                    return (int)LiquidKey.PageDown;

                case Key.Left:
                    return (int)LiquidKey.Left;
                case Key.Right:
                    return (int)LiquidKey.Right;
                case Key.Up:
                    return (int)LiquidKey.Up;
                case Key.Down:
                    return (int)LiquidKey.Down;

                case Key.Keypad0:
                    return (numLock) ? '0' : (int)LiquidKey.Insert;
                case Key.Keypad1:
                    return (numLock) ? '1' : (int)LiquidKey.End;
                case Key.Keypad2:
                    return (numLock) ? '2' : (int)LiquidKey.Down;
                case Key.Keypad3:
                    return (numLock) ? '3' : (int)LiquidKey.PageDown;
                case Key.Keypad4:
                    return (numLock) ? '4' : (int)LiquidKey.Left;
                case Key.Keypad5:
                    return (numLock) ? '5' : 0;
                case Key.Keypad6:
                    return (numLock) ? '6' : (int)LiquidKey.Right;
                case Key.Keypad7:
                    return (numLock) ? '7' : (int)LiquidKey.Home;
                case Key.Keypad8:
                    return (numLock) ? '8' : (int)LiquidKey.Up;
                case Key.Keypad9:
                    return (numLock) ? '9' : (int)LiquidKey.PageUp;
                case Key.KeypadDivide:
                    return '/';
                case Key.KeypadMultiply:
                    return '*';
                case Key.KeypadSubtract:
                    return '-';
                case Key.KeypadAdd:
                    return '+';
                case Key.KeypadDecimal:
                    return (numLock) ? '.' : (int)LiquidKey.Delete;
                case Key.KeypadEnter:
                    return (int)LiquidKey.Enter;
            }

            return 0;
        }
    }
}