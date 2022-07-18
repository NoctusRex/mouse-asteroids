using MouseAsteroids.Models;
using MouseAsteroids.Utils;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace MouseAsteroids.Windows
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private Bitmap? OriginalCursorBitmap { get; set; }
        public double Scale { get; private set; } = 1;
        private Configuration Configuration { get; set; }

        public ConfigurationWindow()
        {
            InitializeComponent();
            Configuration = ConfigurationUtils.Load();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Scale = Configuration.Scale;
            ScaleLabel.Content = $"Scale: {Math.Round(100 * Scale, 0)}%";
            UpdateCursorImage();
        }

        private void UpdateCursorImage()
        {
            if (OriginalCursorBitmap is null)
                OriginalCursorBitmap = WindowsCursorApi.CaptureCursor();
            if (OriginalCursorBitmap is null) return;

            using Bitmap bmp = ImageUtils.ScaleBitmap(OriginalCursorBitmap, Scale);
            bmp.MakeTransparent(Color.Black);

            CursorImage.Source = ImageUtils.BitmapToSource(bmp);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Scale += 0.01;
            }
            else if (e.Delta < 0)
            {
                Scale = Math.Max(0, Scale - 0.01);
            }

            Dispatcher.Invoke(() =>
            {
                ScaleLabel.Content = $"Scale: {Math.Round(100 * Scale, 0)}%";
            });

            UpdateCursorImage();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                
                Configuration.Scale = Math.Round(Scale, 2);
                ConfigurationUtils.Save(Configuration);

                Close();
                return;
            }

            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                return;
            }
        }
    }
}
