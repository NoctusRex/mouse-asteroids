using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MouseAsteroids.Models
{
    public class Asteroid : Entity
    {
        private Ellipse Ellipse { get; set; }
        public override UIElement CanvasElement { get => Ellipse; set => Ellipse = (Ellipse)value; }
        public int Size { get; set; }
        public int MaxSize => 5;
        private int BaseSize => 30;

        public Asteroid(Vector direction, Point position, int size)
        {
            Size = Math.Min(size, MaxSize);

            Ellipse = new()
            {
                Width = BaseSize * Size,
                Height = BaseSize * Size,
                Stroke = Brushes.Blue,
                StrokeThickness = 8,
                Fill = Brushes.LightBlue
            };

            Speed = MaxSize - Size + 1;
            Direction = direction;
            Position = position;
        }

        public override void Draw(Dictionary<string, object>? parameters = null)
        {
            if (parameters == null) return;

            double x = Position.X + (Math.Round(Direction.X, 2) * Speed);
            double y = Position.Y + (Math.Round(Direction.Y, 2) * Speed);

            if (x < -Ellipse.Width)
                x = (double)parameters["Width"] - Ellipse.Width;

            if (y < -Ellipse.Height)
                y = (double)parameters["Height"] - Ellipse.Height;

            if (x > (double)parameters["Width"] + Ellipse.Width)
                x = Ellipse.Width;

            if (y > (double)parameters["Height"] + Ellipse.Height)
                y = Ellipse.Height;

            Position = new((int)x, (int)y);
        }

        public override void Update(Dictionary<string, object>? parameters = null)
        {
            Canvas.SetLeft(Ellipse, Position.X);
            Canvas.SetTop(Ellipse, Position.Y);
        }

        public override void OnCollision(Entity collider, Dictionary<string, object>? parameters = null)
        {
            if (collider is Projectile)
            {
                Destroyed = true;
                collider.Destroyed = true;

                if (parameters is null) return;
                if (Size <= 1) return;

                List<Entity> entities = (List<Entity>)parameters["Entities"];
                Canvas canvas = (Canvas)parameters["Canvas"];
                Random random = new();

                for (int i = 0; i < 2; i++)
                {
                    double x = random.Next(-1, 1);
                    double y = random.Next(-1, 1);

                    if (x + y == 0)
                    {
                        x = 1;
                        y = -1;
                    }

                    x += 0.1;
                    y += 0.1;

                    Asteroid asteroid = new(new(x, y), Position, Size - 1);
                    entities.Add(asteroid);
                    canvas.Children.Add(asteroid.CanvasElement);
                }
            }
        }

    }
}
