using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UserClient
{
    /// <summary>
    /// Interaction logic for ChatColour.xaml
    /// </summary>
    public partial class ChatColour : Window
    {
        private WPFBrushList mBrushes = new WPFBrushList();

        ClientForm mClientForm;

        SolidColorBrush mSelectedBrush = null;

        public ChatColour(ClientForm clientForm)
        {
            InitializeComponent();

            mClientForm = clientForm;

            Brushes.DataContext = mBrushes;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if(mSelectedBrush != null)
            {
                mClientForm.SetBrush(mSelectedBrush);
                Close();
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Brushes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            WPFBrush brush = lb.SelectedItem as WPFBrush;

            Color color = (Color)ColorConverter.ConvertFromString(brush.Name);

            SolidColorBrush brushA = new SolidColorBrush(color);

            mSelectedBrush = brushA;
        }
    }

    class WPFBrush
    {
        public string Name { get; set; }

        public WPFBrush(string name)
        {
            Name = name;
        }
    }

    class WPFBrushList : List<WPFBrush>
    {
        public WPFBrushList()
        {
            // Get type of the Brushes
            Type BrushesType = typeof(Brushes);
            // Get properties of this type
            PropertyInfo[] brushesProperty = BrushesType.GetProperties();
            // Extract Name and Hex code and add to list (binding class)
            foreach (PropertyInfo property in brushesProperty)
            {
                BrushConverter brushConverter = new BrushConverter();
                Brush brush = (Brush)brushConverter.ConvertFromString(property.Name);
                Add(new WPFBrush(property.Name));
            }
        }
    }


}
