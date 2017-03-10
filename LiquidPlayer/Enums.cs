using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer
{
    #region ExceptionCode enums
    public enum ExceptionCode
    {
        None,
        InternalError,
        NotImplemented,
        User,
        OutOfMemory,
        FileNotFound,
        Denied,
        IllegalQuantity,
        NotDimensioned,
        BadSubscript,
        KeyNotFound,
        DuplicateKey,
        NotSorted,
        BadItem,
        ItemNotFound,
        DuplicateItem,
        StreamOpen,
        StreamNotOpen,
        Timeout,
        StackOverflow,
        Overflow,
        DivisionByZero,
        NullObject
    }
    #endregion

    #region EventCode enums
    public enum EventCode
    {
        None,
        KeyDown,
        Timer
    }
    #endregion

    #region MessageBody enums
    public enum MessageBody
    {
        AFI,
        Exit,
        KeyDown,
        KeyUp,
        KeyPress,
        MouseMove,
        MouseOver,
        MouseDown,
        MouseUp,
        Clicked,
        DoubleClicked,
        MouseWheel,
        Timer
    }
    #endregion

    #region LiquidKey enums
    public enum LiquidKey
    {
        None,
        Insert = 2,
        Delete = 3,
        Home = 4,
        End = 5,
        PageUp = 6,
        PageDown = 7,
        Backspace = 8,
        Tab = 9,
        LineFeed = 10,
        ShiftTab = 11,
        Enter = 13,
        Left = 14,
        Right = 15,
        Up = 16,
        Down = 17,
        F1 = 18,
        F2 = 19,
        F3 = 20,
        F4 = 21,
        F5 = 22,
        F6 = 23,
        F7 = 24,
        F8 = 25,
        F9 = 26,
        Escape = 27,
        F10 = 28,
        F11 = 29,
        F12 = 30,
        Space = 32,
        CTRL_C = -1,
        CTRL_D = -2,
        CTRL_E = -3,
        CTRL_F = -4
    }
    #endregion

    #region PixelOperator enums
    public enum PixelOperator
    {
        None,
        Write,
        Blend,
        Min,
        Max,
        And,
        Or,
        Xor,
        Add,
        Sub,
        Avg,
        Mult,
        Zero,
        Replace,
        AlphaTest
    }
    #endregion

    #region RectangleCorner enums
    public enum RectangleCorner
    {
        None,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }
    #endregion

    #region ScrollDirection enums
    public enum ScrollDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    #endregion

    #region ConsoleAttribute enums
    public enum ConsoleAttribute
    {
        None,
        Blink = 1,
        Bold = 2,
        BlinkBold = 3,
        Underline = 4,
        BlinkUnderline = 5,
        BoldUnderline = 6,
        BlinkBoldUnderline = 7
    }
    #endregion

    #region ClipRectangleStyle enums
    public enum ClipRectangleStyle
    {
        None,
        Outline,
        Raised,
        Sunken
    }
    #endregion
}