using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Devices
{
    public partial class CropImage : Window
    {
        private readonly BitmapSource _source;
        public BitmapSource? CroppedImage { get; private set; }

        private bool _isDragging;
        private Point _dragStart;
        private double _startPanX;
        private double _startPanY;

        private double _baseScale = 1.0;
        private double _zoomScale = 1.0;
        private double _panX = 0.0;
        private double _panY = 0.0;

        public CropImage(BitmapSource source)
        {
            InitializeComponent();
            _source = source;
            Loaded += CropImage_Loaded;
        }

        private void CropImage_Loaded(object sender, RoutedEventArgs e)
        {
            imgMain.Source = _source;
            imgPreview.Source = _source;

            // صبر بیشتر برای لود کامل布局
            Dispatcher.BeginInvoke(new Action(InitializeImage), DispatcherPriority.Render);
        }

        private void InitializeImage()
        {
            if (CropCanvas == null || imgMain == null || zoomSlider == null || CropGuide == null)
                return;

            double vw = CropCanvas.ActualWidth;
            double vh = CropCanvas.ActualHeight;

            if (vw <= 10 || vh <= 10 || _source.PixelWidth <= 0 || _source.PixelHeight <= 0)
            {
                // اگر هنوز اندازه درست نبود، دوباره امتحان کن
                Dispatcher.BeginInvoke(new Action(InitializeImage), DispatcherPriority.Render);
                return;
            }

            _baseScale = Math.Max(vw / _source.PixelWidth, vh / _source.PixelHeight) * 0.85; // کمی کوچک‌تر برای حاشیه
            _zoomScale = 1.0;
            _panX = 0.0;
            _panY = 0.0;

            zoomSlider.Value = 1.0;

            UpdateImageLayout();
            UpdatePreview();
        }

        private void UpdateImageLayout()
        {
            if (CropCanvas == null || imgMain == null || _source == null)
                return;

            double displayWidth = _source.PixelWidth * _baseScale * _zoomScale;
            double displayHeight = _source.PixelHeight * _baseScale * _zoomScale;

            imgMain.Width = displayWidth;
            imgMain.Height = displayHeight;

            // محافظت از Null
            double canvasWidth = CropCanvas.ActualWidth > 0 ? CropCanvas.ActualWidth : 800;
            double canvasHeight = CropCanvas.ActualHeight > 0 ? CropCanvas.ActualHeight : 600;

            double centerX = canvasWidth / 2.0 + _panX;
            double centerY = canvasHeight / 2.0 + _panY;

            Canvas.SetLeft(imgMain, centerX - (displayWidth / 2.0));
            Canvas.SetTop(imgMain, centerY - (displayHeight / 2.0));
        }

        // بقیه متدها بدون تغییر (فقط MouseWheel و ValueChanged رو هم ایمن‌تر کردم)
        private void CropCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _isDragging = true;
            _dragStart = e.GetPosition(CropCanvas);
            _startPanX = _panX;
            _startPanY = _panY;
            CropCanvas.CaptureMouse();
        }

        private void CropCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point current = e.GetPosition(CropCanvas);
            Vector delta = current - _dragStart;

            _panX = _startPanX + delta.X;
            _panY = _startPanY + delta.Y;

            UpdateImageLayout();
            UpdatePreview();
        }

        private void CropCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            CropCanvas?.ReleaseMouseCapture();
        }

        private void CropCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double next = _zoomScale + (e.Delta > 0 ? 0.1 : -0.1);
            next = Math.Max(0.5, Math.Min(4.0, next)); // محدوده وسیع‌تر

            _zoomScale = next;
            zoomSlider.Value = _zoomScale;

            UpdateImageLayout();
            UpdatePreview();
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == _zoomScale) return;
            _zoomScale = e.NewValue;
            UpdateImageLayout();
            UpdatePreview();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            CroppedImage = CreateCroppedBitmap();
            DialogResult = CroppedImage != null;
            Close();
        }

        private void UpdatePreview()
        {
            var crop = CreateCroppedBitmap();
            if (crop != null)
                imgPreview.Source = crop;
        }

        private BitmapSource? CreateCroppedBitmap()
        {
            if (_source == null || CropCanvas == null || imgMain == null || CropGuide == null)
                return null;

            if (CropCanvas.ActualWidth <= 0 || CropCanvas.ActualHeight <= 0)
                return null;

            double guideSize = CropGuide.Width;

            double guideLeft = (CropCanvas.ActualWidth - guideSize) / 2.0;
            double guideTop = (CropCanvas.ActualHeight - guideSize) / 2.0;

            double imgLeft = Canvas.GetLeft(imgMain);
            double imgTop = Canvas.GetTop(imgMain);

            double displayScale = _baseScale * _zoomScale;
            if (displayScale <= 0) return null;

            double sourceX = (guideLeft - imgLeft) / displayScale;
            double sourceY = (guideTop - imgTop) / displayScale;
            double sourceSize = guideSize / displayScale;

            int x = Math.Max(0, (int)Math.Round(sourceX));
            int y = Math.Max(0, (int)Math.Round(sourceY));
            int size = (int)Math.Round(sourceSize);

            size = Math.Min(size, _source.PixelWidth - x);
            size = Math.Min(size, _source.PixelHeight - y);

            if (size <= 0) return null;

            var rect = new Int32Rect(x, y, size, size);
            var cropped = new CroppedBitmap(_source, rect);
            cropped.Freeze();
            return cropped;
        }
    }
}