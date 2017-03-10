using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidPlayer.Liquid
{
    public class Turtle : Brush
    {
        protected bool penDown;
        protected double turtleX;
        protected double turtleY;
        protected double heading;

        public double TurtleX
        {
            get
            {
                return turtleX;
            }
        }

        public double TurtleY
        {
            get
            {
                return turtleY;
            }
        }

        public double Heading
        {
            get
            {
                return heading;
            }
        }

        public static int NewTurtle(int bitmapId, int parentId = 0)
        {
            var id = LiquidPlayer.Program.Exec.ObjectManager.New(LiquidClass.Turtle);

            if (id == 0)
            {
                throw new System.Exception("Out of memory");
            }

            if (parentId != 0)
            {
                LiquidPlayer.Program.Exec.ObjectManager.Hook(id, parentId);
            }

            LiquidPlayer.Program.Exec.ObjectManager[id].LiquidObject = new Turtle(id, bitmapId);

            return id;
        }

        public Turtle(int id, int bitmapId)
            : base(id, bitmapId)
        {
            if (bitmapId == 0)
            {
                Throw(ExceptionCode.NullObject);
                return;
            }

            this.penDown = true;

            Home();
        }

        public override string ToString()
        {
            return $"Turtle";
        }

        public void GoBackward(double distance)
        {
            var theta = -(Math.PI / 180d) * heading;

            var x1 = turtleX;
            var y1 = turtleY;
            var x2 = turtleX + distance * Math.Sin(theta);
            var y2 = turtleY + distance * Math.Cos(theta);

            if (penDown)
            {
                Line((int)x1, (int)y1, (int)x2, (int)y2);
            }

            turtleX = x2;
            turtleY = y2;
        }

        public void GoForward(double distance)
        {
            var theta = -(Math.PI / 180d) * heading;

            var x1 = turtleX;
            var y1 = turtleY;
            var x2 = turtleX - distance * Math.Sin(theta);
            var y2 = turtleY - distance * Math.Cos(theta);

            if (penDown)
            {
                Line((int)x1, (int)y1, (int)x2, (int)y2);
            }

            turtleX = x2;
            turtleY = y2;
        }

        public void Home()
        {
            turtleX = bitmap.Width / 2d;
            turtleY = bitmap.Height / 2d;
            heading = 0d;
        }

        public void MoveTo(double x, double y)
        {
            turtleX = x;
            turtleY = y;
        }

        public void PenDown()
        {
            penDown = true;
        }

        public void PenUp()
        {
            penDown = false;
        }

        public void SetHeading(double heading)
        {
            this.heading = heading;
        }

        public void TurnLeft(double degrees)
        {
            heading -= degrees;

            while (heading < 0d)
            {
                heading += 360d;
            }
        }

        public void TurnRight(double degrees)
        {
            heading += degrees;

            while (heading >= 360d)
            {
                heading -= 360d;
            }
        }

        public override void shutdown()
        {
            base.shutdown();
        }
    }
}