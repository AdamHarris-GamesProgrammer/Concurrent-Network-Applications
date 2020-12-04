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
using System.Windows.Shapes;

namespace UserClient
{
    /// <summary>
    /// Interaction logic for ChangeNickname.xaml
    /// </summary>
    public partial class ChangeNickname : Window
    {
        Client mOwner;

        public ChangeNickname(Client client)
        {
            InitializeComponent();

            mOwner = client;
            currentName.Content = mOwner.Nickname;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            mOwner.Nickname = nicknameInput.Text;
            Close();
        }
    }
}
