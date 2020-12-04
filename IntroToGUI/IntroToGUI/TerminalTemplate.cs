using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IntroToGUI
{
    public partial class TerminalTemplate : Form
    {
        public delegate void UpdateChatWindowDelegate(string MessageRecieved);
        public UpdateChatWindowDelegate updateChatWindowDelegate;

        Manager manager;
        bool outputtedName = false;
        string nickname;

        public TerminalTemplate(Manager manager)
        {
            this.manager = manager;
            InitializeComponent();
            updateChatWindowDelegate += new UpdateChatWindowDelegate(UpdateChatWindow);
            nickname = "Unnamed: ";
        }

        private void TerminalTemplate_Load(object sender, EventArgs e)
        {
           
        }

        public void UpdateChatWindow(string MessageRecieved)
        {
           
            if (!outputtedName)
            {
                messageBox.Text += nickname += Environment.NewLine;
                outputtedName = true;
            }

            if (messageBox.InvokeRequired)
            {
                Invoke(updateChatWindowDelegate, MessageRecieved);
            }
            else
            {
                messageBox.Text += MessageRecieved += Environment.NewLine;
                messageBox.SelectionStart = messageBox.Text.Length;
                messageBox.ScrollToCaret();
            }
        }

        public void ResetOutputtedName()
        {
            outputtedName = false;
        }
        public void SetNickname(string name)
        {
            nickname = name += ": ";
            TerminalTemplate template = this;
            template.AccessibleName = nickname;
        }

        public string GetNickname()
        {
            return nickname;
        }
    }
}
