using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace MouseAsteroids.Models
{
    public class Projectile : Entity
    {
        private Ellipse Ellipse { get; set; }
        public override UIElement CanvasElement { get => Ellipse; set => Ellipse = (Ellipse)value; }

        public Projectile(Vector direction, System.Windows.Point position, double scale)
        {
            Ellipse = new()
            {
                Width = 5 * scale,
                Height = 5 * scale,
                Stroke = Brushes.Red,
                StrokeThickness = 8
            };

            Speed = 30;
            Direction = direction;
            Position = position;
        }

        public override void Update(Dictionary<string, object>? parameters = null)
        {
            if (parameters == null) return;
            
            double x = Position.X + (Math.Round(Direction.X, 2) * Speed);
            double y = Position.Y + (Math.Round(Direction.Y, 2) * Speed);

            if (x < 0) Destroyed = true;
            if (y < 0) Destroyed = true;

            if (x > (double)parameters["Width"] - Ellipse.Width) Destroyed = true;
            if (y > (double)parameters["Height"] - Ellipse.Height) Destroyed = true;

            Position = new((int)x, (int)y);
        }

        public override void Draw(Dictionary<string, object>? parameters = null)
        {
            Canvas.SetLeft(Ellipse, Position.X);
            Canvas.SetTop(Ellipse, Position.Y);
        }
    }
}
