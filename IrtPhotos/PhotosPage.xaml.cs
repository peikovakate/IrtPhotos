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


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IrtPhotos
{

    public sealed partial class PhotosPage : Page
    {
        private List<IrtImage> _images;
        private readonly string _url;
        private static string _link1 = "ms-appx:///nature.jpeg";
        private static string _link2 = "ms-appx:///colors.jpg";
        private static string _link3 = "ms-appx:///sailboat.jpg";
        private int i = 0;

        public PhotosPage()
        {
            this.InitializeComponent();
        }

        private void AddImage2_Click(object sender, RoutedEventArgs e)
        {
            AddImage(_link2);
        }

        private void AddImage1_Click(object sender, RoutedEventArgs e)
        {
            AddImage(_link1);
        }
        private void AddImage3_Click(object sender, RoutedEventArgs e)
        {
            AddImage(_link3);
        }

        private void AddImage(string link)
        {
            i++;
            var image = new IrtImage(BackgroundGrid);
            image.LoadImage(link);

        }

    }
}
