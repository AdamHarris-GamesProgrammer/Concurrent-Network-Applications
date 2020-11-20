﻿using System;
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

        public void UpdateChatWindow(string message, HorizontalAlignment align)
        {
            //MessageWindow.Dispatcher.Invoke(() =>
            //{
            //    MessageWindow.Text += message + Environment.NewLine;
            //});
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

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputField.Text == "") return;

            mClient.SendMessage(InputField.Text);
            InputField.Text = "";
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            mClient.DisconnectedMessage();
            mClient.DisconnectFromServer();
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeNickname nicknameWindow = new ChangeNickname(mClient);
            nicknameWindow.ShowDialog();
        }
    }
}