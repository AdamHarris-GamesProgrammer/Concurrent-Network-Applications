﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;



namespace UserClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
        private Client mClient;
        private string mLastNicknameRecieved = "";

        private SolidColorBrush mPersonalColour = Brushes.AliceBlue;

        private string mSelectedClient = "";       

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
                    item.Background = mPersonalColour;
                }
                else
                {
                    item.Background = Brushes.LightGray;
                }


                item.FontWeight = FontWeights.SemiBold;


                item.HorizontalAlignment = align;

                item.Content = message;

                MessageWnd.Items.Add(item);
            });
        }

        public void UpdateClientListWindow(string[] users)
        {
            ClientList.Dispatcher.Invoke(() =>
            {
                ClientList.Items.Clear();

                foreach (string user in users)
                {
                    if (user == mClient.Nickname) continue;

                    var item = new ListViewItem();
                    item.Content = user;

                    ClientList.Items.Add(item);
                }
            });


        }

        public void SendPrivateMessage(string recipient, string message)
        {
            var test = new TabItem();

            
        }


        public void SendNicknameToWindow(string nickname, HorizontalAlignment align = HorizontalAlignment.Left)
        {
            if (mLastNicknameRecieved == nickname) return;

            MessageWnd.Dispatcher.Invoke(() =>
            {
                var item = new ListViewItem();

                item.HorizontalAlignment = align;
                item.FontStyle = FontStyles.Italic;
                item.Content = nickname;

                MessageWnd.Items.Add(item);
            });

            mLastNicknameRecieved = nickname;
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
            SendMessage();
        }

        private void SendMessage()
        {
            if (InputField.Text == "") return;


            if (mSelectedClient == "")
            {
                mClient.SendMessage(InputField.Text);

                SetButtonText("Send Message");
            }
            else
            {
                mClient.SendPrivateMessage(mSelectedClient, InputField.Text);

                ClientList.SelectedItem = null;

                ClientList.UnselectAll();

                SetButtonText("Send Message");
            }



            InputField.Text = "";

            mSelectedClient = "";

            if (mLastNicknameRecieved != "You" && mLastNicknameRecieved != "You -> " + mSelectedClient) mLastNicknameRecieved = "";
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            mClient.DisconnectFromServer();

            InputField.IsEnabled = false;
            InputField.Text = "You can no longer send messages.";
            SubmitButton.IsEnabled = false;
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeNickname nicknameWindow = new ChangeNickname(mClient);
            nicknameWindow.ShowDialog();
        }

        private void SetButtonText(string text)
        {
            SubmitButton.Content = text;
        }

        private void ClientList_Selected(object sender, RoutedEventArgs e)
        {
            var item = ClientList.SelectedItem as ListViewItem;

            if (item == null) return;

            mSelectedClient = (string)item.Content;
            SetButtonText("Send Message to "+ System.Environment.NewLine + mSelectedClient);

        }

        private void InputField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                SendMessage();
            }
        }

        private void HangmanButton_Click(object sender, RoutedEventArgs e)
        {
            mClient.StartGame();
        }

        private void MessageColour_Click(object sender, RoutedEventArgs e)
        {
            ChatColour chatColourWindow = new ChatColour(this);

            chatColourWindow.ShowDialog();
        }

        public void SetBrush(SolidColorBrush mSelectedBrush)
        {
            mPersonalColour = mSelectedBrush;
        }

    }
}
