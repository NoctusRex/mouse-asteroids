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
        private Bitmap? CursorBitmap { get; set; }
        private int UpdateTickSpeed => 1000 / 30;
        private DateTime LastUpdateTick { get; set; }
        private Player Player { get; set; } = new();
        private bool ForwardDown { get; set; }
        private bool RotateLeftDown { get; set; }
        private bool RotateRightDown { get; set; }
        private Vector LastPlayerDirection { get; set; }
        private Image CursorImage { get; set; } = new Image();
        private Configuration Configuration { get; set; } 

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

            CursorBitmap = WindowsCursorApi.CaptureCursor();
            if (CursorBitmap != null)
            {
                CursorBitmap = ImageUtils.ScaleBitmap(CursorBitmap, Configuration.Scale);
                CursorBitmap.MakeTransparent(System.Drawing.Color.Black);
            }

            GameTask.Start();
        }

        private void GameLoop()
        {
            Dispatcher.Invoke(() =>
            {
                SetupGame();

                while (Run)
                {
                    if (LastUpdateTick.AddMilliseconds(UpdateTickSpeed) < DateTime.Now)
                    {
                        UpdatePlayerPosition();

                        LastUpdateTick = DateTime.Now;
                    }

                    DrawGameCursor();
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

            CursorImage = new Image();
            GameCanvas.Children.Add(CursorImage);

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
                    if (CursorBitmap is null) return;

                    CursorBitmap = ImageUtils.ScaleBitmap(CursorBitmap, Player.Scale);
                    CursorBitmap.MakeTransparent(System.Drawing.Color.Black);

                    break;
                case Key.Escape:
                    if (CursorBitmap != null)
                    {
                        System.Windows.Point playerPositionOnScreen = PointToScreen(Player.Position);
                        int xOffset = (int)Player.Direction.X * CursorBitmap.Width;
                        int yOffset = (int)Player.Direction.Y * CursorBitmap.Height;

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
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Run = false;
        }

        private void DoEvents()
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            catch { }
        }

        private void UpdatePlayerPosition()
        {
            if (ForwardDown)
            {
                Player.Speed = Math.Min(Player.Speed + Player.Acceleration, Player.MaxSpeed);
            }

            if (RotateLeftDown)
            {
                Player.Rotation -= Player.RotationSpeed;
            }

            if (RotateRightDown)
            {
                Player.Rotation += Player.RotationSpeed;
            }

            if (Player.Speed <= 0) return;

            double x = Player.Position.X + (Math.Round(ForwardDown ? Player.Direction.X : LastPlayerDirection.X, 2) * Player.Speed);
            double y = Player.Position.Y + (Math.Round(ForwardDown ? Player.Direction.Y : LastPlayerDirection.Y, 2) * Player.Speed);

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x > Width - CursorBitmap?.Width) x = Width;
            if (y > Height - CursorBitmap?.Height) y = Height;

            Player.Position = new((int)x, (int)y);

            if (ForwardDown)
                LastPlayerDirection = Player.Direction;
            else
                Player.Speed = Math.Max(Player.Speed - Player.Deceleration, 0);
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

        private void DrawGameCursor()
        {
            if (CursorBitmap is null) return;

            using Bitmap copy = new(CursorBitmap.Width + 20, CursorBitmap.Height + 20);
            using Graphics graphics = Graphics.FromImage(copy);
            graphics.DrawImage(CursorBitmap, 10, 10);

            if (ForwardDown)
            {
                Font font = new("Tahoma", (int)(8 * Player.Scale));
                string thruster = $"X";

                graphics.DrawString(thruster,
                                    font,
                                    Brushes.OrangeRed,
                                    new PointF(copy.Width / 2 - (graphics.MeasureString(thruster, font).Width / 2), CursorBitmap.Height));

                Font font2 = new("Tahoma", (int)(5 * Player.Scale));

                graphics.DrawString(thruster,
                                    font2,
                                    Brushes.Yellow,
                                    new PointF(
                                        copy.Width / 2 - (graphics.MeasureString(thruster, font2).Width / 2),
                                        CursorBitmap.Height)
                                    );
            }

            // I cheated here with the +20, the cursor icon is not facing upwards by default
            CursorImage.Source = ImageUtils.BitmapToSource(ImageUtils.RotateBitmap(copy, Player.Rotation + 20));

            Canvas.SetLeft(CursorImage, Player.Position.X);
            Canvas.SetTop(CursorImage, Player.Position.Y);
        }

    }
}
