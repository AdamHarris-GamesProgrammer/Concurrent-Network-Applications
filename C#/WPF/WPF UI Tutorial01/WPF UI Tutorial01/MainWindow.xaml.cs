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

namespace WPF_UI_Tutorial01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(DescriptionText.Text);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionText.Text = "";

            WeldCheckbox.IsChecked = AssemblyCheckbox.IsChecked = PlasmaCheckbox.IsChecked = LaserCheckbox.IsChecked = PurchaseCheckbox.IsChecked 
                = LatheCheckbox.IsChecked = LaserCheckbox.IsChecked = DrillCheckbox.IsChecked = FoldCheckbox.IsChecked = RollCheckbox.IsChecked = SawCheckbox.IsChecked = false;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionText.Text = "Refreshing";

            MessageBox.Show("Honestly I have no idea what this button should do :(");
        }
    }
}
