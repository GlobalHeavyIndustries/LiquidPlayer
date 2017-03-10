using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Console : View
    {
        protected int characterSetId;
        protected CharacterSet characterSet;

        protected int characterSetBitmapId;
        protected Bitmap characterSetBitmap;

        protected int bitmapId;
        protected Bitmap bitmap;

        protected int brushId;
        protected Brush brush;

        protected int spriteId;
        protected Sprite sprite;

        protected bool parent;
        protected bool autoScroll;
        protected int characterWidth;
        protected int characterHeight;
        protected int width;
        protected int height;
        protected int size;
        protected uint inkColor;
        protected uint highlightColor;
        protected uint cursorColor;
        protected byte[] screenData;
        protected uint[] colorData;
        protected int attributes;
        protected int[] attributeData;
        protected int state;
        protected int cursorState;
        protected bool cursorInsertMode;
        protected int cursorX;
        protected int cursorY;
        protected int inputMode;
        protected int cursorStart;
        protected int inputBufferLength;
        protected string inputBuffer;
        protected int maximumInputBufferLength;

        public bool AutoScroll
        {
            get
            {
                return autoScroll;
            }
            set
            {
                autoScroll = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public uint InkColor
        {
            get
            {
                return inkColor;
            }
            set
            {
                inkColor = value;
            }
        }

        public uint HighlightColor
        {
            get
            {
                return highlightColor;
            }
            set
            {
                highlightColor = value;
            }
        }

        public uint CursorColor
        {
            get
            {
                return cursorColor;
            }
            set
            {
                cursorColor = value;
            }
        }

        public int CursorX
        {
            get
            {
                return cursorX;
            }
        }

        public int CursorY
        {
            get
            {
                return cursorY;
            }
        }

        public string InputBuffer
        {
            get
            {
                return inputBuffer;
            }
        }

        public static int NewConsole(int width, int height, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Console);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Console(id, width, height);

            return id;
        }

        public Console(int id, int width, int height)
            : base(id)
        {
            if (width < 4 || width > 256 || height < 4 || height > 256)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            var characterSetId = objectManager.ConsoleFontId;

            this.parent = false;
            this.autoScroll = true;

            this.characterSetId = objectManager.Copy(characterSetId);
            this.characterSet = objectManager[characterSetId].LiquidObject as CharacterSet;

            this.characterSetBitmapId = objectManager.Copy(characterSet.BitmapId);
            this.characterSetBitmap = objectManager[characterSetBitmapId].LiquidObject as Bitmap;

            this.characterWidth = characterSet.GlyphWidth;
            this.characterHeight = characterSet.GlyphHeight;
            this.width = width;
            this.height = height;
            this.size = width * height;
            this.inkColor = 0xFFFFFFFF;              
            this.highlightColor = 0;            
            this.cursorColor = 0xFF00FFFF;           
            this.screenData = new byte[size]; screenData.Fill((byte)32);
            this.colorData = new uint[size]; colorData.Fill(0xFFFFFFFF);
            this.attributes = 0;
            this.attributeData = new int[size];
            this.state = 0;
            this.cursorState = 1;
            this.cursorX = 0;
            this.cursorY = 0;
            this.inputMode = 0;
            this.cursorStart = 0;
            this.inputBufferLength = 0;
            this.inputBuffer = "";
            this.maximumInputBufferLength = 0;

            var bitmapId = Bitmap.NewBitmap(width, height, id);

            if (bitmapId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            this.bitmapId = bitmapId;
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            bitmap.Clear();

            var brushId = Brush.NewBrush(bitmapId, id);

            if (brushId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            this.brushId = brushId;
            this.brush = objectManager[brushId].LiquidObject as Brush;

            var spriteId = Sprite.NewSprite(bitmapId, id);

            if (spriteId == 0)
            {
                Throw(ExceptionCode.OutOfMemory);
                return;
            }

            this.spriteId = spriteId;
            this.sprite = objectManager[spriteId].LiquidObject as Sprite;

            var dx = (Util.IsOdd(width)) ? -(characterWidth / 2) : 0;
            var dy = (Util.IsOdd(height)) ? -(characterHeight / 2) : 0;

            sprite.Move(dx, dy);
            sprite.Scale(characterWidth, characterHeight);

            Show();
            Center();
        }

        public override string ToString()
        {
            return $"Console (Resolution: {width}x{height})";
        }

        public void Blink(bool blink)
        {
            if (blink)
            {
                attributes |= (int)ConsoleAttribute.Blink;
            }
            else
            {
                attributes &= ~(int)ConsoleAttribute.Blink;
            }
        }

        public void Bold(bool bold)
        {
            if (bold)
            {
                attributes |= (int)ConsoleAttribute.Bold;
            }
            else
            {
                attributes &= ~(int)ConsoleAttribute.Bold;
            }
        }

        protected override bool callback(int messageId)
        {
            var message = objectManager[messageId].LiquidObject as Message;

            if (message.IsTo(objectId))
            {
                if ((MessageBody)message.GetBody() == MessageBody.KeyDown)
                {
                    if (state != 1)
                    {
                        goto cleanExit;
                    }

                    var key = message.GetData();

                    var oldCursorX = cursorX;
                    var oldCursorY = cursorY;

                    var pos = cursorX - cursorStart;

                    if (key >= 32 && key <= 255)
                    {
                        if (inputMode != 0)
                        {
                            var legalChar = false;

                            if ((key >= 48 && key <= 57) || (key == 45 && inputBufferLength == 0) || (key == 46 && inputBuffer.IndexOf('.') == -1))
                            {
                                legalChar = true;
                            }

                            if (!legalChar)
                            {
                                goto cleanExit;
                            }
                        }

                        if (cursorInsertMode)
                        {
                            if (inputBufferLength < maximumInputBufferLength)
                            {
                                Print((char)key + inputBuffer.Substring(pos));
                                cursorX = oldCursorX + 1;
                                inputBuffer = inputBuffer.Substring(0, pos) + (char)key + inputBuffer.Substring(pos);
                            }
                        }
                        else
                        {
                            if (pos < inputBufferLength)
                            {
                                Print("" + (char)key);
                                inputBuffer = inputBuffer.Substring(0, pos) + (char)key + inputBuffer.Substring(pos + 1);
                            }
                            else if (inputBufferLength < maximumInputBufferLength - 1)
                            {
                                Print("" + (char)key);
                                inputBuffer += (char)key;
                            }
                        }
                    }
                    else
                    {
                        switch ((LiquidKey)key)
                        {
                            case LiquidKey.Insert:
                                cursorInsertMode = !cursorInsertMode;
                                break;
                            case LiquidKey.Delete:
                                if (inputBufferLength != 0)
                                {
                                    Print(inputBuffer.Substring(pos + 1) + " ");
                                    cursorX = oldCursorX;
                                    inputBuffer = inputBuffer.Substring(0, pos) + inputBuffer.Substring(pos + 1);
                                }
                                break;
                            case LiquidKey.Home:
                                cursorX = cursorStart;
                                break;
                            case LiquidKey.End:
                                if (cursorX < width)
                                {
                                    cursorX = cursorStart + inputBufferLength;
                                }
                                break;
                            case LiquidKey.Backspace:
                                if (inputBufferLength != 0 && pos != 0)
                                {
                                    cursorX--;
                                    Print(inputBuffer.Substring(pos) + " ");
                                    cursorX = oldCursorX - 1;
                                    inputBuffer = inputBuffer.Substring(0, pos - 1) + inputBuffer.Substring(pos);
                                }
                                break;
                            case LiquidKey.Enter:
                                PrintLine();
                                state = 2;
                                break;
                            case LiquidKey.Left:
                                if (cursorX > cursorStart)
                                {
                                    cursorX--;
                                }
                                break;
                            case LiquidKey.Right:
                                if (cursorX < cursorStart + inputBufferLength)
                                {
                                    cursorX++;
                                }
                                break;
                        }   
                    }

                    inputBufferLength = inputBuffer.Length;

                    // Relay the message
                    // TODO
                    // CALL objSend(ID, ID, %CHM_KEYDOWN, FORMAT$(nKey)) 

                    return true;
                }
            }

        cleanExit:

            return base.callback(messageId);
        }

        public void Clear()
        {
            cursorX = 0;
            cursorY = 0;

            screenData = new byte[size]; screenData.Fill((byte)32);
            colorData = new uint[size]; colorData.Fill(0xFFFFFFFF);
            attributes = 0;
            attributeData = new int[size];

            bitmap.Clear();
        }

        public bool InputLine()
        {
            switch (state)
            {
                case 0:
                    state = 1;

                    inputMode = 0;
                    cursorStart = cursorX;
                    inputBufferLength = 0;
                    inputBuffer = "";
                    maximumInputBufferLength = width - cursorX;

                    objectManager.Focus = objectId;

                    return true;
                case 1:
                    return true;
                case 2:
                    state = 0;

                    objectManager.Focus = objectManager[objectId].TaskId;

                    return false;
            }

            return false;
        }

        public bool InputNumber()
        {
            switch (state)
            {
                case 0:
                    state = 1;

                    inputMode = 1;
                    cursorStart = cursorX;
                    inputBufferLength = 0;
                    inputBuffer = "";
                    maximumInputBufferLength = width - cursorX;

                    objectManager.Focus = objectId;

                    return true;
                case 1:
                    return true;
                case 2:
                    state = 0;

                    objectManager.Focus = objectManager[objectId].TaskId;

                    return false;
            }

            return false;
        }

        public void Locate(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            cursorX = x;
            cursorY = y;
        }

        public void Print(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);

            foreach (var b in bytes)
            {
                if (b >= 32 && b <= 255)
                {
                    var position = cursorY * width + cursorX;
                    bitmap.FastPoke(position, highlightColor);
                    screenData[position] = b;
                    colorData[position] = inkColor;
                    attributeData[position] = attributes;
                    cursorX++;
                }
                else if (b == (byte)LiquidKey.Tab)
                {
                    var tabStop = 4 - (cursorX % 4);
                    cursorX += tabStop;
                }
                else if (b == (byte)LiquidKey.Enter)
                {
                    PrintLine();
                }
                
                if (cursorX >= width)
                {
                    PrintLine();
                }
            }
        }

        public void PrintLine()
        {
            cursorX = 0;
            if (cursorY < height - 1)
            {
                cursorY++;
            }
            else if (autoScroll)
            {
                ScrollUp();
            }
        }

        public void PrintLine(string text)
        {
            Print(text + (char)LiquidKey.Enter);
        }

        protected override void render(int orthoId)
        {
            var rasterX = -(width * characterWidth) / 2;
            var rasterY = -(height * characterHeight) / 2;

            var blink = (LiquidPlayer.Program.SystemClock % 1000 >= 500) ? true : false;

            var showCursor = (blink && objectManager.Focus == objectId) ? true : false;

            bitmap.SwapBuffers();

            Sprockets.Graphics.LinkNextObject(spriteId);
            sprite.Render(LiquidClass.Sprite, orthoId);
            Sprockets.Graphics.LinkNextObject(objectId);

//          characterSetBitmap.SwapBuffers();

            Sprockets.Graphics.RenderConsole(characterSet.DisplayList, width, height, characterWidth, characterHeight, screenData, colorData, attributeData, rasterX, rasterY, blink);

            if (showCursor)
            {
                var cx = rasterX + (cursorX * characterWidth);
                var cy = rasterY + (cursorY * characterHeight);

                Sprockets.Graphics.MixColor(cursorColor);
                
                if (cursorInsertMode)
                {
                    Sprockets.Graphics.RectangleFill(cx, cy + (characterHeight / 2), cx + characterWidth - 1, cy + characterHeight - 1);
                } 
                else
                {
                    Sprockets.Graphics.RectangleFill(cx, cy, cx + characterWidth - 1, cy + characterHeight - 1);
                }
            }
        }

        public void ScrollUp()
        {
            var index = 0;

            for (index = 0; index < size - width; index++)
            {
                screenData[index] = screenData[index + width];
                colorData[index] = colorData[index + width];
                attributeData[index] = attributeData[index + width];
            }

            for (index = size - width; index < size; index++)
            {
                screenData[index] = 32;
                colorData[index] = 0xFFFFFFFF;
                attributeData[index] = 0;
            }

            brush.ScrollUp(0, 0, bitmap.Width - 1, bitmap.Height - 1);
        }

        public void Tab(int x)
        {
            if (x < 0 || x >= width)
            {
                Throw(ExceptionCode.IllegalQuantity);
                return;
            }

            cursorX = x;
        }

        public void Underline(bool underline)
        {
            if (underline)
            {
                attributes |= (int)ConsoleAttribute.Underline;
            }
            else
            {
                attributes &= ~(int)ConsoleAttribute.Underline;
            }
        }

        public override void shutdown()
        {
            objectManager.Mark(characterSetId);
            characterSet = null;

            objectManager.Mark(characterSetBitmapId);
            characterSetBitmap = null;

            objectManager.Mark(bitmapId);
            bitmap = null;

            objectManager.Mark(brushId);
            brush = null;

            objectManager.Mark(spriteId);
            sprite = null;

            screenData = null;
            colorData = null;
            attributeData = null;

            base.shutdown();
        }
    }
}