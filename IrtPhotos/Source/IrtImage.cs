using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace IrtPhotos.Source
{
    class IrtImage
    {
        private readonly CanvasControl _canvasImage;
        private Canvas _canvas;
        private Canvas _transformCanvas;
        private Storyboard _deletingAnim;
        private readonly OppositeDirection _direction;
        private  CompositeTransform _transform;
        private CanvasBitmap _bitmap;
        private CompositeEffect _effect;
        private float blurAmount = 40;
        public int I { get; set; }
        private readonly Grid _backgroundGrid;
        private const double K = 0.5;
        private float _scale = 0.3f;
        private string _link;
        private Ink ink;
        private const int DefaultWidth = 800;
        private CloseAnimation closeAnim;

        public Canvas getCanvas()
        {
            return _canvas;
        }

        public IrtImage(Grid back)
        {
            _canvasImage = new CanvasControl();
            
             _direction = new OppositeDirection();
            _transform = new CompositeTransform();

            _canvas = new Canvas { ManipulationMode = ManipulationModes.All };
            _transformCanvas = new Canvas() { ManipulationMode = ManipulationModes.All};
            closeAnim = new CloseAnimation();
            closeAnim.PointerPressed += CloseAnim_PointerPressed;

            _transformCanvas.ManipulationStarting += Canvas_ManipulationStarting;
            _transformCanvas.ManipulationCompleted += Canvas_ManipulationCompleted;
            _transformCanvas.ManipulationDelta += Canvas_ManipulationDelta;
            _transformCanvas.DoubleTapped += Canvas_DoubleTapped;
            _canvas.RenderTransform = _transform;

            _backgroundGrid = back;
        }

        private void CloseAnim_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _deletingAnim.Stop();
            Storyboard.SetTarget(_deletingAnim.Children[0], _canvas);
            Storyboard.SetTarget(_deletingAnim.Children[1], _canvas);
            Storyboard.SetTarget(_deletingAnim.Children[2], _canvas);
           
            _deletingAnim.Completed += _deletingAnim_Completed;
            _deletingAnim.Begin();
            
        }

        private void _deletingAnim_Completed(object sender, object e)
        {
            _backgroundGrid.Children.Remove(_canvas);
        }

        private CompositionImage image;
        public void LoadImage(string link, Storyboard imageAppearence, Storyboard imageDeleting)
        {
            _link = link;
            _deletingAnim = imageDeleting;
            _canvasImage.CreateResources += _canvasImage_CreateResources;
            _canvasImage.Draw += _canvasImage_Draw;
            _canvas.Children.Add(_canvasImage);
            
            //image = new CompositionImage();

            //image.Source = new Uri(_link);
            //image.BorderBrush = new SolidColorBrush(Colors.White);
            //image.BorderThickness = new Thickness(10, 10, 10, 10);
            //image.ImageOpened += Image_ImageOpened;
            //image.Width = DefaultWidth;

            //_canvas.Width = image.Width+20;

            //_canvas.Height = 300; 

            //_canvas.Background = new SolidColorBrush(Colors.White);

            //_transform.CenterX = _canvas.Width / 2;
            //_transform.CenterY = _canvas.Height / 2;
            //Canvas.SetLeft(image, 10);
            //_canvas.Children.Add(image);
            _backgroundGrid.Children.Add(_canvas);
            _canvas.RenderTransformOrigin = new Point(0.5, 0.5);
            _canvasImage.RenderTransformOrigin = new Point(0.5, 0.5);
            _transformCanvas.RenderTransformOrigin = new Point(0.5, 0.5);
            imageAppearence.Completed += ImageAppearence_Completed;
            imageAppearence.Stop();
            Storyboard.SetTarget(imageAppearence.Children[0], _canvas);
            Storyboard.SetTarget(imageAppearence.Children[1], _canvas);
            Storyboard.SetTarget(imageAppearence.Children[2], _canvas);
            imageAppearence.Begin();
        }

        private void ImageAppearence_Completed(object sender, object e)
        {
            _transform = new CompositeTransform();
            _canvas.RenderTransform = _transform;
        }

        bool isBlured = false;
        private const float borderWidth = 80;
        private void _canvasImage_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {

            var cl = new CanvasCommandList(sender);
            var clds = cl.CreateDrawingSession();
            clds.FillRoundedRectangle(0, 0, 
                (float)(_bitmap.Size.Width + borderWidth * 2), (float)(_bitmap.Size.Height  + borderWidth * 2), borderWidth/2, borderWidth / 2, Colors.White);
            clds.DrawImage(_bitmap, new Vector2(borderWidth, borderWidth));

            var scaleEffect = new ScaleEffect
            {
                Source = cl,
                Scale = new Vector2((float)_scale, (float)_scale)
            };

            var shadowEffect = new ShadowEffect
            {
                Source = scaleEffect,
                BlurAmount = blurAmount,
                ShadowColor = Color.FromArgb(127, 0, 0, 0)
            };

            if (isBlured)
            {
                var blur = new GaussianBlurEffect
                {
                    Source = scaleEffect,

                };
                _effect = new CompositeEffect
                {
                    Sources = { shadowEffect, blur }
                };
            }
            else
            {
                _effect = new CompositeEffect
                {
                    Sources = { shadowEffect, scaleEffect }
                };

            }

            args.DrawingSession.DrawImage(_effect, blurAmount*_scale* blurConst/2, blurAmount*_scale* blurConst/2);
        }

        private void _canvasImage_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _transform.ScaleX = 1;
            _transform.ScaleY = 1;
        }
        private float MinScale = 0.5f;
        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Container == null) return;
            if (Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - _canvas.Width * _transform.ScaleX / 2 < 0.0)
            {
                _direction.X = !_direction.X;
                if (_transform.TranslateX < 0)
                {
                    _transform.TranslateX -= Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - _canvas.Width * _transform.ScaleX / 2;
                }
                else
                {
                    _transform.TranslateX += Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - _canvas.Width * _transform.ScaleX / 2;
                }
            }

            if (Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - _canvas.Height * _transform.ScaleY / 2 < 0)
            {
                _direction.Y = !_direction.Y;
                if (_transform.TranslateY < 0)
                {
                    _transform.TranslateY -= Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - _canvas.Height * _transform.ScaleY / 2;
                }
                else
                {
                    _transform.TranslateY += Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - _canvas.Height * _transform.ScaleY / 2;
                }

            }
            var tX = e.Delta.Translation.X;
            var tY = e.Delta.Translation.Y;
            if (e.IsInertial)
            {
                tX *= K;
                tY *= K;
            }

            if (!_direction.X)
            {
                _transform.TranslateX += tX;
            }
            else
            {
                _transform.TranslateX -= tX;
            }

            if (!_direction.Y)
            {
                _transform.TranslateY += tY;
            }
            else
            {
                _transform.TranslateY -= tY;
            }
            if (_transform.ScaleX * e.Delta.Scale >= MinScale && _transform.ScaleX * e.Delta.Scale <= 3)
            {
               
                _transform.ScaleX *= e.Delta.Scale;
                _transform.ScaleY *= e.Delta.Scale;
                if (_transform.ScaleX <= MinScale+0.01)
                {
                    addClose();
                }
                else
                {
                    removeClose();
                }
               
            }
            
            _transform.Rotation += e.Delta.Rotation;
        }

        void addClose()
        {
            if (!_canvas.Children.Contains(closeAnim))
            {
                Canvas.SetLeft(closeAnim, _canvas.ActualWidth / 2.0 - closeAnim.ActualWidth/2);
                Canvas.SetTop(closeAnim, _canvas.ActualHeight / 2.0 - closeAnim.ActualHeight / 2);
                _canvas.Children.Add(closeAnim);
                isBlured = true;
                _canvasImage.Invalidate();
            }
        }

        void removeClose()
        {
            if (_canvas.Children.Contains(closeAnim))
            {
                _canvas.Children.Remove(closeAnim);
                isBlured = false;
                _canvasImage.Invalidate();
            }
        }

        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Container == null) return;
            _direction.X = false;
            _direction.Y = false;
        }

        private void Canvas_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Canvas.SetZIndex(e.Container, I);
            foreach (var item in _backgroundGrid.Children)
            {
                Canvas.SetZIndex(item, Canvas.GetZIndex(item) - 1);
            }
        }

        private void Image_ImageOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var shadowContainer = ElementCompositionPreview.GetElementVisual(_canvas);
            var compositor = shadowContainer.Compositor;

            var im = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(_canvas);
            var imageVisual = im.SpriteVisual;
           
            var shadow = compositor.CreateDropShadow();
            shadow.BlurRadius = blurAmount;
            imageVisual.Shadow = shadow;
        }

        const float blurConst = 15;
        async Task CreateResourcesAsync(CanvasControl sender)
        {
            _bitmap = await CanvasBitmap.LoadAsync(sender, new Uri(_link));
            _canvas.Width = (_bitmap.Size.Width  + blurAmount* blurConst + borderWidth*2)*_scale;
            _canvas.Height =( _bitmap.Size.Height  + blurAmount* blurConst + borderWidth*2)*_scale;

            _transformCanvas.Width = (_bitmap.Size.Width  + borderWidth*2)*_scale;
            _transformCanvas.Height = (_bitmap.Size.Height + borderWidth*2)*_scale;

            _transformCanvas.Background = new SolidColorBrush(Color.FromArgb(0, 0,0, 0));
            Canvas.SetLeft(_transformCanvas, blurAmount* blurConst / 2 * _scale);
            Canvas.SetTop(_transformCanvas, blurAmount* blurConst / 2*_scale);
            _canvas.Children.Add(_transformCanvas);

            _canvasImage.Width = _canvas.Width;
            _canvasImage.Height = _canvas.Height;
            _transform.CenterX = _canvas.Width / 2;
            _transform.CenterY = _canvas.Height / 2;
        }

    }
}