using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using System.Windows.Interop;
using MouseAsteroids.Models;
using System.Drawing.Drawing2D;
using Brushes = System.Drawing.Brushes;
using MouseAsteroids.Utils;
using MouseAsteroids.Windows;

namespace MouseAsteroids
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task GameTask { get; set; }
        private bool Run { get; set; } = true;
        private static int UpdateTickSpeed => 1000 / 30;
        private DateTime LastUpdateTick { get; set; }

        private bool ForwardDown { get; set; }
        private bool RotateLeftDown { get; set; }
        private bool RotateRightDown { get; set; }
        private bool SpaceDown { get; set; }

        private Configuration Configuration { get; set; }

        private Player Player { get; set; } = new();
        private List<Entity> Entities { get; set; } = new();

        private Random Random { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            Configuration = ConfigurationUtils.Load();
            GameTask = new Task(() => GameLoop());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            Left = screenLeft;
            Top = screenTop;
            Width = screenWidth;
            Height = screenHeight;

            MainGrid.Width = screenWidth;
            MainGrid.Height = screenHeight;

            SetupGame();

            GameTask.Start();
        }

        private void GameLoop()
        {
            Dispatcher.Invoke(() =>
            {
                while (Run)
                {
                    if (LastUpdateTick.AddMilliseconds(UpdateTickSpeed) < DateTime.Now)
                    {
                        Update();

                        if (Entities.Where(x => x is Asteroid).Count() < 3)
                        {
                            double x = Random.Next(-1, 1);
                            double y = Random.Next(-1, 1);

                            if (x + y == 0)
                            {
                                x = 1;
                                y = -1;
                            }

                            x += 0.1;
                            y += 0.1;

                            Asteroid asteroid = new(new(x, y), new(Random.Next(0, (int)Width), -300), 5);
                            Entities.Add(asteroid);
                            GameCanvas.Children.Add(asteroid.CanvasElement);
                        }

                        LastUpdateTick = DateTime.Now;
                    }

                    Draw();
                    DoEvents();
                    Thread.Sleep(5);
                }
            });
        }

        private void SetupGame()
        {
            var cursorPos = System.Windows.Forms.Cursor.Position;

            Player = new()
            {
                Deceleration = 0.1,
                Acceleration = 0.2,
                Position = PointFromScreen(new(cursorPos.X, cursorPos.Y)),
                Speed = 0,
                MaxSpeed = 10,
                RotationSpeed = 5,
                Scale = Configuration.Scale
            };

            Player.CursorBitmap = WindowsCursorApi.CaptureCursor();
            if (Player.CursorBitmap != null)
            {
                Player.CursorBitmap = ImageUtils.ScaleBitmap(Player.CursorBitmap, Configuration.Scale);
                Player.CursorBitmap.MakeTransparent(System.Drawing.Color.Black);
            }
            Player.IsNormalCursor = WindowsCursorApi.IsNormalCursor();

            Player.CanvasElement = new Image();
            GameCanvas.Children.Add(Player.CanvasElement);
            Entities.Add(Player);

            LastUpdateTick = DateTime.Now;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    ConfigurationWindow window = new();
                    bool? dialogResult = window.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        Player.Scale = window.Scale;
                    }
                    if (Player.CursorBitmap is null) return;

                    Player.CursorBitmap = ImageUtils.ScaleBitmap(Player.CursorBitmap, Player.Scale);
                    Player.CursorBitmap.MakeTransparent(System.Drawing.Color.Black);

                    break;
                case Key.Escape:
                    if (Player.CursorBitmap != null)
                    {
                        System.Windows.Point playerPositionOnScreen = PointToScreen(Player.Position);
                        int xOffset = (int)Player.Direction.X * Player.CursorBitmap.Width;
                        int yOffset = (int)Player.Direction.Y * Player.CursorBitmap.Height;

                        System.Windows.Forms.Cursor.Position = new((int)playerPositionOnScreen.X + xOffset, (int)playerPositionOnScreen.Y + yOffset);
                    }

                    Close();
                    break;
                case Key.W:
                    ForwardDown = true;
                    break;
                case Key.D:
                    RotateRightDown = true;
                    RotateLeftDown = false;
                    break;
                case Key.A:
                    RotateLeftDown = true;
                    RotateRightDown = false;
                    break;
                case Key.Space:
                    if (Player.CursorBitmap is null) return;
                    if (!SpaceDown)
                    {
                        Projectile projectile = new(Player.Direction, new(Player.Position.X + (Player.CursorBitmap.Width / 2), Player.Position.Y + (Player.CursorBitmap.Height / 2)), Player.Scale);
                        Entities.Add(projectile);
                        GameCanvas.Children.Add(projectile.CanvasElement);
                        SpaceDown = true;
                    }
                    break;
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.W:
                    ForwardDown = false;
                    break;
                case Key.D:
                    RotateRightDown = false;
                    break;
                case Key.A:
                    RotateLeftDown = false;
                    break;
                case Key.Space:
                    SpaceDown = false;
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Run = false;
        }

        private static void DoEvents()
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            catch { }
        }

        private void Update()
        {
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Dictionary<string, object> parameters = new();

                parameters.Add("Width", Width);
                parameters.Add("Height", Height);

                if (Entities[i] is Player)
                {
                    parameters.Add("ForwardDown", ForwardDown);
                    parameters.Add("RotateLeftDown", RotateLeftDown);
                    parameters.Add("RotateRightDown", RotateRightDown);
                }

                Entities[i].Update(parameters);

                foreach (Entity entity in Entities.ToList())
                {
                    if (Entities[i] == entity) continue;
                    parameters.TryAdd("Entities", Entities);
                    parameters.TryAdd("Canvas", GameCanvas);

                    if (Collides(Entities[i], entity))
                        Entities[i].OnCollision(entity, parameters);
                }

                if (Entities[i].Destroyed)
                {
                    GameCanvas.Children.Remove(Entities[i].CanvasElement);
                    Entities.Remove(Entities[i]);
                }
            }
        }

        private void Draw()
        {
            foreach (Entity entity in Entities)
            {
                Dictionary<string, object> parameters = new();

                parameters.Add("Width", Width);
                parameters.Add("Height", Height);

                if (entity is Player)
                {
                    parameters.Add("ForwardDown", ForwardDown);
                }

                entity.Draw(parameters);
            }
        }

        private void DrawRectangle(System.Windows.Media.Color stroke, System.Windows.Media.Color fill, int width, int height, int thickness, double x, double y, double opacity = 1)
        {
            Rectangle rectangle = new()
            {
                Stroke = new SolidColorBrush(stroke) { Opacity = opacity },
                Fill = new SolidColorBrush(fill) { Opacity = opacity },
                Width = width,
                Height = height,
                StrokeThickness = thickness
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            GameCanvas.Children.Add(rectangle);
        }

        private void DrawText(string text, int x, int y, System.Windows.Media.Color color)
        {
            TextBlock textBlock = new()
            {
                Text = text,
                Foreground = new SolidColorBrush(color),
            };

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            GameCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// https://social.msdn.microsoft.com/Forums/en-US/8abf1a61-cf81-4b6f-ad83-1d71cb9c7353/how-to-create-a-game-with-ellipse-collision?forum=csharpgeneral
        /// </summary>
        private bool Collides(Entity entity1, Entity entity2)
        {
            if (entity1.CanvasElement is Ellipse ellipse1 && entity2.CanvasElement is Ellipse ellipse2)
            {
                using GraphicsPath graphicsPath = new();
                using GraphicsPath graphicsPath2 = new();
                graphicsPath.AddEllipse((int)entity1.Position.X, (int)entity1.Position.Y, (int)ellipse1.Width, (int)ellipse1.Height);
                graphicsPath2.AddEllipse((int)entity2.Position.X, (int)entity2.Position.Y, (int)ellipse2.Width, (int)ellipse2.Height);

                using Region region = new(graphicsPath);
                region.Intersect(graphicsPath2);
                return region.GetRegionScans(new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0)).Length > 0;
            }


            return false;
        }
    }
}
