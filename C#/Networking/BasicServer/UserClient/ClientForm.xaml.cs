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

        private string lastNicknameRecieved = "";

        public ClientForm(Client client)
        {
            InitializeComponent();
            mClient = client;

            ChangeNickname nicknameWindow = new ChangeNickname(mClient);
            nicknameWindow.ShowDialog();
        }

        public void SetWindowTitle(string title)
        {
            chatWindow.Title = title;
        }

        public void SendMessageToWindow(string message, HorizontalAlignment align)
        {
            MessageWnd.Dispatcher.Invoke(() =>
            {
                var item = new ListViewItem();

                if (align == HorizontalAlignment.Right)
                {
                    item.Background = Brushes.AliceBlue;
                }
                else
                {
                    item.Background = Brushes.LightGray;
                }

                item.HorizontalAlignment = align;
 
                item.Content = message;

                MessageWnd.Items.Add(item);
            });
        }

        public void SendNicknameToWindow(string nickname, HorizontalAlignment align = HorizontalAlignment.Left)
        {
            if (lastNicknameRecieved == nickname/* || nickname == "You"*/) return;

            MessageWnd.Dispatcher.Invoke(() =>
            {
                var item = new ListViewItem();

                item.HorizontalAlignment = align;
                item.FontStyle = FontStyles.Italic;
                item.Content = nickname;

                MessageWnd.Items.Add(item);
            });

            lastNicknameRecieved = nickname;
        }

        public void DisconnectMessage(string disconnectedNickname)
        {

            MessageWnd.Dispatcher.Invoke(() =>
            {
                var item = new ListViewItem();

                item.HorizontalAlignment = HorizontalAlignment.Center;
                item.FontStyle = FontStyles.Italic;
                item.Content = disconnectedNickname + " has disconnected";

                MessageWnd.Items.Add(item);
            });
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputField.Text == "") return;

            mClient.SendMessage(InputField.Text);
            InputField.Text = "";

            if(lastNicknameRecieved != "You") lastNicknameRecieved = "";
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            mClient.DisconnectFromServer();
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeNickname nicknameWindow = new ChangeNickname(mClient);
            nicknameWindow.ShowDialog();
        }
    }
}
