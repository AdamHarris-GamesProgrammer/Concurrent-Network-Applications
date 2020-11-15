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



namespace UserClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
        private Client mClient;

        public ClientForm(Client client)
        {
            InitializeComponent();
            mClient = client;
        }

        public void UpdateChatWindow(string message)
        {
            MessageWindow.Dispatcher.Invoke(() =>
            {
                MessageWindow.Text += message + Environment.NewLine;
                //MessageWindow.ScrollToEnd();
            });
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            
            mClient.SendMessage(InputField.Text);
        }
    }
}
