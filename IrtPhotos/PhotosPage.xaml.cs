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
using IrtPhotos.Source;
using Windows.UI.Xaml.Media.Animation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IrtPhotos
{

    public sealed partial class PhotosPage : Page
    {
        private List<IrtImage> _images;
        private readonly string _url;
        private static string[] _link = { "ms-appx:///nature.jpeg", "ms-appx:///colors.jpg", "ms-appx:///sailboat.jpg" };



        public PhotosPage()
        {
            this.InitializeComponent();
           
            addImButton._backgroundGrid = BackgroundGrid;
            Canvas.SetZIndex(addImButton, 1000);
            addImButton.DoubleTapped += AddImButton_DoubleTapped;
            
            var transform = (CompositeTransform)(addImButton.RenderTransform);
            transform.TranslateX = -1000;

            AddImage(_link[0]);

        }

        private void AddImButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Random r = new Random();
            
            AddImage(_link[r.Next(0, 2)]);
        }

        private void AddImage(string link)
        {    
            var image = new IrtImage(PhotosGrid);
            image.LoadImage(link);

        }



    }
}
