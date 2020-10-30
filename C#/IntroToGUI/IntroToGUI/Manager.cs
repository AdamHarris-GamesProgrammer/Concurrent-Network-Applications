using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntroToGUI
{
    public class Manager
    {
        TerminalTemplate m_TerminalTemplate1;
        TerminalTemplate m_TerminalTemplate2;

        public Manager()
        {
            m_TerminalTemplate1 = new TerminalTemplate(this);
            Thread formThread1 = new Thread(() => { CreateForm(m_TerminalTemplate1); });
            m_TerminalTemplate1.SetNickname("Adam");

            m_TerminalTemplate2 = new TerminalTemplate(this);
            Thread formThread2 = new Thread(() => { CreateForm(m_TerminalTemplate2); });

            formThread1.Start();
            formThread2.Start();

            for(int i = 0; i < 10; i++)
            {
                SendMessageToAll("This is a message");
                Thread.Sleep(500);
            }
            m_TerminalTemplate1.ResetOutputtedName();
            m_TerminalTemplate2.ResetOutputtedName();

            //formThread1.Join();
            //formThread2.Join();
            SendMessageToAll("ello ello");
        }

        void CreateForm(TerminalTemplate terminal)
        {
            terminal.ShowDialog();
        }

        public void SendMessageToAll(string message)
        {
            m_TerminalTemplate1.UpdateChatWindow(message);
            m_TerminalTemplate2.UpdateChatWindow(message);
        }

    }
}
