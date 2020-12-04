using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF_UI_Tutorial02
{
    /// <summary>
    /// Converts a full path to a specific image type of a drive, folder or a file
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Get the full path
            var path = (string)value;

            if (path == null) return string.Empty;

            //Get the name
            var name = MainWindow.GetFileFolderName(path);


            //by default we presume a image
            var image = "Images/file.png";

            //if the name is blank we presume its a drive
            if (name == "") 
                image = "Images/drive.png";
            else if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory)) 
                image = "Images/folder-closed.png";

            return new BitmapImage(new Uri($"pack://application:,,, /{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
