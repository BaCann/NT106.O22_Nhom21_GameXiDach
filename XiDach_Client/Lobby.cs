using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XiDach_Client.Core;
using XiDach_Client.Model;

namespace XiDach_Client
{
    public partial class Lobby : Form
    {
        public Lobby lobby;
        public PublicFunction publicFunction;
        public List<Label> PlayerName = new List<Label>();
        public int connectedPlayer = 0;
        public Lobby()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            btnStart.Enabled = false;
        }
        public void DisplayConnectedPlayer(string name, bool isShow = false)
        {
            try
            {
                connectedPlayer++;
                if (isShow)
                    btnStart.Enabled = true;
                switch (connectedPlayer)
                {
                    case 1:
                        labelP1.Text = name;
                        break;
                    case 2:
                        labelP2.Text = name;
                        break;
                    case 3:
                        labelP3.Text = name;
                        break;
                    case 4:
                        labelP4.Text = name;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void btnLeave_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnStart_Click_2(object sender, EventArgs e)
        {
            publicFunction.Send("Start;");
        }
        private void Lobby_Load(object sender, EventArgs e)
        {

        }
    }
}
