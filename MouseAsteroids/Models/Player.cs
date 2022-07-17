using System;
using System.Windows;
using System.Windows.Media;

namespace MouseAsteroids.Models
{
    public class Player
    {
        private int rotation;

        public Point Position { get; set; }
        public Vector Direction
        {
            get
            {
                Vector vector = new(0, -1);
                Matrix tranform = Matrix.Identity;
                tranform.Rotate(Rotation);

                return vector * tranform;
            }
        }
        public double Deceleration { get; set; }
        public double Acceleration { get; set; }
        public double Speed { get; set; }
        public double MaxSpeed { get; set; }
        public int Rotation
        {
            get { return rotation; }
            set
            {
                if (value < 0)
                    rotation = 360 - Math.Abs(value);
                else if (value > 360)
                    rotation = value - 360;
                else
                    rotation = value;

            }
        }
        public int RotationSpeed { get; set; }

    }
}
