using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Sprite : GEL
    {
        protected int bitmapId;
        protected Bitmap bitmap;

        protected int width;
        protected int height;
        protected float direction;
        protected float speed;

        public int BitmapId
        {
            get
            {
                return bitmapId;
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

        public float Direction
        {
            get
            {
                return direction;
            }
        }

        public float Speed
        {
            get
            {
                return speed;
            }
        }

        public static int NewSprite(int bitmapId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Sprite);

            if (id == 0)
            {
                throw new Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Sprite(id, bitmapId);

            return id;
        }

        public Sprite(int id, int bitmapId)
            : base(id)
        {
            if (bitmapId == 0)
            {
                RaiseError(ErrorCode.NullObject);
                return;
            }

            this.bitmapId = objectManager.Copy(bitmapId);
            this.bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            this.width = bitmap.Width;
            this.height = bitmap.Height;
            this.direction = 0f;
            this.speed = 0f;
        }

        public override string ToString()
        {
            return $"Sprite (Resolution: {width}x{height})";
        }

        public void AutoMove(float direction, float speed)
        {
            if (direction < 0f || direction >= 360f)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            if (speed < 0f || speed >= 15f)
            {
                RaiseError(ErrorCode.IllegalQuantity);
                return;
            }

            this.direction = direction;
            this.speed = speed;
        }

        public int GetBitmap()
        {
            return objectManager.Copy(bitmapId);
        }

        protected override void update()
        {
            if (speed != 0f)
            {
                MoveDirection(direction, speed);

                var w = width * xScale / 2;
                var h = height * yScale / 2;

                if (xPosition < -w)
                {
                    xPosition = (int)(LiquidPlayer.Program.ScreenWidth + w);
                }
                else if (xPosition >= LiquidPlayer.Program.ScreenWidth + w)
                {
                    xPosition = (int)-w;
                }

                if (yPosition < -h)
                {
                    yPosition = (int)(LiquidPlayer.Program.ScreenHeight + h);
                }
                else if (yPosition >= LiquidPlayer.Program.ScreenHeight + h)
                {
                    yPosition = (int)-h;
                }
            }
        }

        protected override void render(int orthoId)
        {
            var bitmap = objectManager[bitmapId].LiquidObject as Bitmap;

            bitmap.Render();
        }

        public override void shutdown()
        {
            objectManager.Mark(bitmapId);
            bitmap = null;

            base.shutdown();
        }
    }
}