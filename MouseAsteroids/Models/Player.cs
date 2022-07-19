using MouseAsteroids.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace MouseAsteroids.Models
{
    public class Player : Entity
    {
        private int rotation;

        public override Vector Direction
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
        public double Scale { get; set; } = 1;
        private System.Windows.Controls.Image CursorImage { get; set; } = new();
        public override UIElement CanvasElement { get => CursorImage; set => CursorImage = (System.Windows.Controls.Image)value; }
        public bool IsNormalCursor { get; set; }
        public Vector LastPlayerDirection { get; set; }
        public Bitmap? CursorBitmap { get; set; }

        public override void Update(Dictionary<string, object>? parameters = null)
        {
            if (parameters == null) return;

            if ((bool)parameters["ForwardDown"])
            {
                Speed = Math.Min(Speed + Acceleration, MaxSpeed);
            }

            if ((bool)parameters["RotateLeftDown"])
            {
                Rotation -= RotationSpeed;
            }

            if ((bool)parameters["RotateRightDown"])
            {
                Rotation += RotationSpeed;
            }

            if (Speed <= 0) return;

            double x = Position.X + (Math.Round((bool)parameters["ForwardDown"] ? Direction.X : LastPlayerDirection.X, 2) * Speed);
            double y = Position.Y + (Math.Round((bool)parameters["ForwardDown"] ? Direction.Y : LastPlayerDirection.Y, 2) * Speed);

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (CursorBitmap != null && x > (double)parameters["Width"] - CursorBitmap.Width) x = (double)parameters["Width"] - CursorBitmap.Width;
            if (CursorBitmap != null && y > (double)parameters["Height"] - CursorBitmap.Height) y = (double)parameters["Height"] - CursorBitmap.Height;

            Position = new((int)x, (int)y);

            if ((bool)parameters["ForwardDown"])
                LastPlayerDirection = Direction;
            else
                Speed = Math.Max(Speed - Deceleration, 0);
        }

        public override void Draw(Dictionary<string, object>? parameters = null)
        {
            if (parameters == null) return;
            if (CursorBitmap is null) return;

            using Bitmap copy = new(CursorBitmap.Width, CursorBitmap.Height);
            using Graphics graphics = Graphics.FromImage(copy);
            graphics.DrawImage(CursorBitmap, 10, 10);

            if ((bool)parameters["ForwardDown"])
            {
                Font font = new("Tahoma", (int)(8 * Scale));
                string thruster = $"O";

                graphics.DrawString(thruster,
                                    font,
                                    System.Drawing.Brushes.OrangeRed,
                                    new PointF(copy.Width / 2 - (graphics.MeasureString(thruster, font).Width / 2), CursorBitmap.Height - (graphics.MeasureString(thruster, font).Height / 2)));

                Font font2 = new("Tahoma", (int)(5 * Scale));

                graphics.DrawString(thruster,
                                    font2,
                                    System.Drawing.Brushes.Yellow,
                                    new PointF(
                                        copy.Width / 2 - (graphics.MeasureString(thruster, font2).Width / 2),
                                        CursorBitmap.Height - (graphics.MeasureString(thruster, font2).Height / 2))
                                    );
            }

            // I cheated here with the +20, the cursor icon is not facing upwards by default
            int originalRotation = Rotation;
            if (IsNormalCursor) Rotation += 20;

            CursorImage.Source = ImageUtils.BitmapToSource(ImageUtils.RotateBitmap(copy, Rotation));

            Rotation = originalRotation;

            Canvas.SetLeft(CursorImage, Position.X);
            Canvas.SetTop(CursorImage, Position.Y);
        }
    }
}
