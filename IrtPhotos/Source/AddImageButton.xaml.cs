using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IrtPhotos.Source
{
    public sealed partial class AddImageButton : UserControl
    {
        private OppositeDirection _direction;
        private CompositeTransform _transform;
        public Grid _backgroundGrid;
        private double K = 0.5;

        public AddImageButton()
        {
            this.InitializeComponent();
            _direction = new OppositeDirection();
            _transform = new CompositeTransform();
            this.ManipulationMode = ManipulationModes.All;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = _transform;
            this.ManipulationDelta += AddImageButton_ManipulationDelta;
        }

        private void AddImageButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Container == null) return;
            if (Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - this.Width * _transform.ScaleX / 2 < 0.0)
            {
                _direction.X = !_direction.X;
                if (_transform.TranslateX < 0)
                {
                    _transform.TranslateX -= Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - this.Width * _transform.ScaleX / 2;
                }
                else
                {
                    _transform.TranslateX += Math.Abs(_backgroundGrid.ActualWidth / 2) - Math.Abs(_transform.TranslateX) - this.Width * _transform.ScaleX / 2;
                }
            }

            if (Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - this.Height * _transform.ScaleY / 2 < 0)
            {
                _direction.Y = !_direction.Y;
                if (_transform.TranslateY < 0)
                {
                    _transform.TranslateY -= Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - this.Height * _transform.ScaleY / 2;
                }
                else
                {
                    _transform.TranslateY += Math.Abs(_backgroundGrid.ActualHeight / 2) - Math.Abs(_transform.TranslateY) - this.Height * _transform.ScaleY / 2;
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
        }

        private void addPressed(object sender, PointerRoutedEventArgs e)
        {
            turnToQr.Begin();
        }

        private void turnToQr_Completed(object sender, object e)
        {
            scanAnimation.Begin();
        }

        

    }
}
