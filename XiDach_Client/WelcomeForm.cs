using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XiDach_Client.Core;
using XiDach_Client.Model;

namespace XiDach_Client
{
    public partial class WelcomeForm : Form
    {
        PublicFunction publicFunction = new PublicFunction();
        private Player player;
        public static Lobby lobby;
        public WelcomeForm()
        {
            InitializeComponent();
        }
        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPlayerName.Text)){
                
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(txtIpAddress.Text), 8989);
                publicFunction.Connect(serverEP);
                player = new Player();
                player.PlayerName = txtPlayerName.Text;
                publicFunction.thisPlayer = player;
                lobby = new Lobby(); 
                lobby.publicFunction = publicFunction;
                publicFunction.Send("CONNECT;"+txtPlayerName.Text);
                lobby.FormClosed += new FormClosedEventHandler(lobby_FormClosed);
                this.Hide();
                lobby.Show();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập tên người chơi!");
            }
        }
        void lobby_FormClosed(object sender, EventArgs e)
        {
            publicFunction.Send("DISCONNECT;" + player.PlayerName);
            this.Show();
        }
        private void WelcomeForm_Load(object sender, EventArgs e)
        {

        }
    }
}
