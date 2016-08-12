using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
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
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace IrtPhotos.Source
{
    class IrtImage
    {
        //private readonly CanvasControl _canvasImage;
        //private Canvas _canvas;
        //private Canvas _transformCanvas;
        private Grid _grid;
        private double d;
        private Grid _shadowGrid;
        private Grid _shadowOnTopGrid;
        private Grid _bluredGrid;
        private Storyboard _deletingAnim;
        private Storyboard _imageAppearence;
        private Storyboard _appearence;
        private readonly OppositeDirection _direction;
        private  CompositeTransform _transform;
      
        private CompositeEffect _effect;
        private const float blurAmount = 40;
        public int I { get; set; }
        private readonly Grid _backgroundGrid;
        private const double K = 0.5;
        private string _link;
        private Ink ink;
        Image image;
        Rectangle r;
        private CloseAnimation closeAnim;

        bool isBlured = false;
        private const float borderWidth = 80;
        private float MinScale = 0.2f;

        AppearingDisapearingAnimations animation;
        const float blurConst = 15;
        Pointer remPointer;
        Pointer pointer;
        bool stop = false;
        SpriteVisual blurVisual;
        SpriteVisual shadowOnTopVisual;

        public IrtImage(Grid back)
        {
            _backgroundGrid = back;

            _grid = new Grid();
            _shadowGrid = new Grid();
            _shadowOnTopGrid = new Grid();
            _bluredGrid = new Grid();

            _grid.Children.Add(_bluredGrid);
            _bluredGrid.Children.Add(_shadowGrid);

            //animation
            animation = new AppearingDisapearingAnimations();
            _imageAppearence = animation.getImageAppearing();
            _deletingAnim = animation.getImageDeleting();
            _appearence = animation.getAppearing();

            _imageAppearence.SpeedRatio = 2;
            _appearence.SpeedRatio = 2;
            _deletingAnim.SpeedRatio = 2;
            _backgroundGrid.Children.Add(animation);

            closeAnim = new CloseAnimation();
            closeAnim.Tapped += CloseAnim_Tapped;

            //manipulaions
             _direction = new OppositeDirection();
            _transform = new CompositeTransform();
            _grid.ManipulationStarting += Canvas_ManipulationStarting;
            _grid.ManipulationCompleted += Canvas_ManipulationCompleted;
            _grid.ManipulationDelta += Canvas_ManipulationDelta;
            //_grid.DoubleTapped += Canvas_DoubleTapped;
            _grid.RenderTransform = _transform;
            _grid.ManipulationMode = ManipulationModes.All;
            _grid.RenderTransformOrigin = new Point(0.5, 0.5);
            _grid.PointerPressed += _grid_PointerPressed;
            _grid.PointerReleased += _grid_PointerReleased;

            image = new Image();
            r = new Rectangle(); //rectangle for image frame
            r.Fill = new SolidColorBrush(Colors.White);
            r.RadiusX = borderWidth/2;
            r.RadiusY = borderWidth / 2;
            _bluredGrid.Children.Add(r);
            _bluredGrid.Children.Add(image);
            _bluredGrid.Children.Add(_shadowOnTopGrid);
            _backgroundGrid.Children.Add(_grid);     
        }

        private void _grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            remPointer = pointer;
        }

        private void CloseAnim_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _deletingAnim.Stop();
            Storyboard.SetTarget(_deletingAnim.Children[0], _grid);
            Storyboard.SetTarget(_deletingAnim.Children[1], _grid);
            Storyboard.SetTarget(_deletingAnim.Children[2], _grid);

            _deletingAnim.Completed += _deletingAnim_Completed;
            var t = new CompositeTransform();
            t.TranslateX = _transform.TranslateX;
            t.TranslateY = _transform.TranslateY;
            animation.RenderTransform = t;
            _appearence.Stop();
            _appearence.Begin();
            _deletingAnim.Begin();

        }

        private void _grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointer = e.Pointer;
        }

        private void _deletingAnim_Completed(object sender, object e)
        {
            _backgroundGrid.Children.Remove(_grid);
        }

        private int realWidth;
        private int realHeight;


        public void LoadImage(string link)
        {
            _link = link;
            BitmapImage bitmap = new BitmapImage(new Uri(link));
            image.Source = bitmap;

            bitmap.ImageOpened += (sender, e) =>
            {
                realWidth = bitmap.PixelWidth;
                realHeight = bitmap.PixelHeight;

                r.Width = realWidth + borderWidth* 2;
                r.Height = realHeight + borderWidth* 2;
               
                _grid.Height = r.Height+20;
                _grid.Width = r.Width+20;

                image.Width = realWidth;
            };

            _imageAppearence.Completed += ImageAppearence_Completed;
            _imageAppearence.Stop();

            Storyboard.SetTarget(_imageAppearence.Children[0], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[1], _grid);
            Storyboard.SetTarget(_imageAppearence.Children[2], _grid);
            _appearence.Begin();
            _imageAppearence.Begin();
        }

        private void ImageAppearence_Completed(object sender, object e)
        {
            _transform = new CompositeTransform();
            _transform.ScaleX = _transform.ScaleY = 0.3;
            AddShadow();

            _grid.RenderTransform = _transform;
        }

        //private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        //{
        //    _transform.ScaleX = 0.3;
        //    _transform.ScaleY = 0.3;
        //    removeClose();
        //}
        
        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Container == null) return;

            _transform.Rotation += e.Delta.Rotation;

            double angle = 0;
            if (Math.Abs(_transform.Rotation) % 180 < 90)
            {
                angle = ((Math.Abs(_transform.Rotation) % 90) / 360 * 2 * Math.PI);
            }else
            {
                angle = ((90 - Math.Abs(_transform.Rotation) % 90) / 360 * 2 * Math.PI);
            }
 
            double distX = Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX)
                - (_grid.Width * Math.Cos(angle) + _grid.Height*Math.Sin(angle))* _transform.ScaleX / 2;

            if (distX < 0.0)
            {
                if (pointer.IsInContact && remPointer != pointer)
                {
                    return;
                }

                _direction.X = !_direction.X;
                if (_transform.TranslateX < 0)
                {
                    _transform.TranslateX -= distX;
                }
                else
                {
                    _transform.TranslateX += distX;
                }
            }

            double distY = Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY)
                - (_grid.Width * Math.Sin(angle) + _grid.Height * Math.Cos(angle)) * _transform.ScaleY / 2;

            if (distY < 0)
            {
                if (pointer.IsInContact && remPointer != pointer)
                {
                    return;
                }

                _direction.Y = !_direction.Y;
                if (_transform.TranslateY < 0)
                {
                    _transform.TranslateY -= distY;
                }
                else
                {
                    _transform.TranslateY += distY;
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

            if (_transform.ScaleX * e.Delta.Scale >= MinScale && _transform.ScaleX * e.Delta.Scale <= 1)
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
        }

        void addClose()
        {
            if (!_grid.Children.Contains(closeAnim))
            {
                _grid.Children.Add(closeAnim);
                
                addShadowOnTop();
                addBlur();
            }
        }

        void removeClose()
        {
            if (_grid.Children.Contains(closeAnim))
            {
                _grid.Children.Remove(closeAnim);
                isBlured = false;

                blurVisual.Dispose();
                shadowOnTopVisual.Dispose();
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

        void AddShadow()
        {
            var compositor = ElementCompositionPreview.GetElementVisual(_shadowGrid).Compositor;
            var spriteVisual = compositor.CreateSpriteVisual();
            spriteVisual.Size = new Vector2((float)r.RenderSize.Width, (float)r.RenderSize.Height);
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.Offset = new Vector3(20, 20, 0);
            dropShadow.Color = Color.FromArgb(127, 0, 0, 0);
            dropShadow.BlurRadius = 300;
            spriteVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(_shadowGrid, spriteVisual);
        }

        void addBlur()
        {
            if (blurVisual != null)
            {
                blurVisual.Dispose();
            }
            var gridVisual = ElementCompositionPreview.GetElementVisual(_bluredGrid);
            var compositor = gridVisual.Compositor;
            blurVisual = compositor.CreateSpriteVisual();

            blurVisual.Size = new Vector2(
              (float)_bluredGrid.ActualWidth,
              (float)_bluredGrid.ActualHeight);

            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("source"),
                BlurAmount = 10
            };

            var effectFactory = compositor.CreateEffectFactory(blurEffect);
            var effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            blurVisual.Brush = effectBrush;
            ElementCompositionPreview.SetElementChildVisual(_bluredGrid, blurVisual);
        }

        void addShadowOnTop()
        {
            if (shadowOnTopVisual != null)
            {
                shadowOnTopVisual.Dispose();
            }
            var t = _transform;
            var compositor = ElementCompositionPreview.GetElementVisual(_shadowOnTopGrid).Compositor;
            shadowOnTopVisual = compositor.CreateSpriteVisual();
            shadowOnTopVisual.Size = new Vector2((float)r.RenderSize.Width, (float)r.RenderSize.Height);
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.Offset = new Vector3(10, 10, 0);
            dropShadow.Color = Color.FromArgb(127, 0, 0, 0);
            dropShadow.Mask = r.GetAlphaMask();
            dropShadow.BlurRadius = 10;
            shadowOnTopVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(_shadowOnTopGrid, shadowOnTopVisual);
            _grid.RenderTransform = t;
        }

    }
}