using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XiDach_Client.Core;
using XiDach_Client.Model;


namespace XiDach_Client
{
    public partial class GameRoom : Form
    {
        public PublicFunction publicFunction;
        public int IDGame;
        public Player thisPlayer;
        public List<Card> lstCard = new List<Card>();
        public DataTable result = new DataTable();
        public List<Player> otherplayers;
        public int currentTurn;
        public int countCheckPlayer = 0;
        public string PlayerNextTurn;
        private int ck1 = 0;
        private int ck2 = 0;
        private int ck3 = 0;
        public GameRoom()
        {
            InitializeComponent();
            btnRut.Enabled = false;
            btnChot.Enabled = false;
            btnCheck1.Enabled = false;
            btnCheck2.Enabled = false;
            btnCheck3.Enabled = false;
            btnShowCard.Enabled = true;
        }
        public void InitDisplay() 
        {   
            lblPoint.Text = thisPlayer.TotalPoint.ToString();
            txtGame.Text = IDGame + "";
            this.Text = thisPlayer.PlayerName;
            if (thisPlayer.IsHost)
            {
                txtHost.Text = thisPlayer.PlayerName;
            }
            else
            {
                Player host = otherplayers.Where(s => s.IsHost == true).SingleOrDefault();
                if(host != null)
                {
                    txtHost.Text = host.PlayerName;
                }
            }

            if (thisPlayer.Turn == 1) 
            {
                txtCurrTurn.Text = thisPlayer.PlayerName;
                EnableBTN();
            }
            else 
            {
                Player curTurn = otherplayers.Where(s => s.Turn == 1).Single();
                {
                    txtCurrTurn.Text = curTurn.PlayerName;
                }
            }
            groupBox1.Text = thisPlayer.PlayerName;
            SetCardForThisPlayer();
            ShowCardForOtherPlayers();
        }
        private void EnableBTN()
        {
            btnRut.Enabled = true;
            btnChot.Enabled = true;
        }
        private void btnShowCard_Click(object sender, EventArgs e)
        {
            btnShowCard.Enabled = false;
            publicFunction.Send("Show;");
        }
        private void btnChot_Click(object sender, EventArgs e)
        {
            btnRut.Enabled = false;
            btnChot.Enabled = false;
            publicFunction.Send("Block;");

        }
        private void btnRut_Click(object sender, EventArgs e)
        {   
            if (thisPlayer.CountCard < 5)
            {
                publicFunction.Send("DrawCard;");
            }
            else
            {
                MessageBox.Show("Số lá bài của bạn đã ở mức tối đa !!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void SetImageControl(PictureBox box, string strPathImage = "")
        {
            FileStream file = new FileStream("..\\..\\Asset\\Pictures\\" + (!string.IsNullOrEmpty(strPathImage) ? strPathImage : "BackCard.jpg"), FileMode.Open, FileAccess.Read);
            box.Image = Image.FromStream(file);
            box.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        private Control GetControlByName(GroupBox groupBox, string Name)
        {
            foreach (Control c in groupBox.Controls)
                if (c.Name == Name)
                    return c;

            return null;
        }
        public void UpdateCardDraw()
        {
            try
            {
                if (thisPlayer.IsHost)
                {
                    if (thisPlayer.TotalPoint >= 15 || (thisPlayer.TotalPoint < 15 && thisPlayer.CountCard == 5))
                    {
                        btnCheck1.Enabled = true;
                        btnCheck2.Enabled = true;
                        btnCheck3.Enabled = true;
                    }
                    else
                    {
                        btnCheck1.Enabled = false;
                        btnCheck2.Enabled = false;
                        btnCheck3.Enabled = false;
                    }
                }
                else
                {
                    if (thisPlayer.TotalPoint >= 16 || (thisPlayer.TotalPoint < 16 && thisPlayer.CountCard == 5))
                    {
                        btnChot.Enabled = true;
                    }
                    else
                    {
                        btnChot.Enabled = false;
                    }
                }
                if (thisPlayer.CountCard == 5 || thisPlayer.specialCase == SPECIALCASE.Quoac)
                {
                    btnRut.Enabled = false;
                }
                lblPoint.Text = thisPlayer.TotalPoint.ToString();
                SetCardForThisPlayer();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void UpdateCardDrawOtherPlayer(List<Player> lstPlayer)
        {
            try
            {
                otherplayers = lstPlayer;
                ShowCardForOtherPlayers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void SetCardForThisPlayer()
        {   try
            {
                for (int i = 0; i < thisPlayer.cards.Count; i++)
                {
                    Control control = GetControlByName(groupBox1, "pic_" + (i + 1).ToString("0") + "_Player1");
                    PictureBox pic = (System.Windows.Forms.PictureBox)control;
                    SetImageControl(pic, thisPlayer.cards[i].ImageFront);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
        }
        public void SetTurn()
        {   
            txtCurrTurn.Text = PlayerNextTurn;
            
            if ( thisPlayer.IsHost == true)
            {
                if (thisPlayer.Turn == currentTurn)
                {
                    btnRut.Enabled = true;
                    if (thisPlayer.TotalPoint >= 15 || (thisPlayer.TotalPoint < 15 && thisPlayer.CountCard == 5))
                    {
                        btnCheck1.Enabled = true;
                        btnCheck2.Enabled = true;
                        btnCheck3.Enabled = true;
                    }
                    else
                    {
                        btnCheck1.Enabled = false;
                        btnCheck2.Enabled = false;
                        btnCheck3.Enabled = false;
                    }
                }
                else
                {
                    btnRut.Enabled = false;
                    btnCheck1.Enabled = false;
                    btnCheck2.Enabled = false;
                    btnCheck3.Enabled = false;
                }
            }
            else
            {
                if (thisPlayer.Turn == currentTurn)
                {
                    if (thisPlayer.TotalPoint >= 16 || (thisPlayer.TotalPoint < 16 && thisPlayer.CountCard == 5))
                    {
                        btnChot.Enabled = true;
                    }
                    else
                    {
                        btnChot.Enabled = false;
                    }
                    btnRut.Enabled = true;
                }
                else
                {
                    btnRut.Enabled = false;
                    btnChot.Enabled = false;
                }
            }
        }
        public void ShowCardForOtherPlayers()
        {
            try
            {
                switch (otherplayers.Count)
                {
                    case 1:
                        {
                            groupBox2.Text = otherplayers[0].PlayerName;
                            if (otherplayers[0].isShowCard)
                            {
                                
                                for (int i = 0; i < otherplayers[0].cards.Count; i++)
                                {
                                    Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                    PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                    SetImageControl(pic, otherplayers[0].cards[i].ImageFront);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < otherplayers[0].cards.Count; i++)
                                {
                                    Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                    PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                    SetImageControl(pic);
                                }
                            }
                            
                        }
                        break;
                    case 2:
                        {
                            otherplayers = otherplayers.OrderBy(s => s.Turn).ToList();
                            for (int p = 0; p < otherplayers.Count; p++)
                            {       
                                if (p == 0)
                                {
                                    groupBox2.Text = otherplayers[p].PlayerName;
                                    if (otherplayers[p].isShowCard)
                                    {
                                        
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic, otherplayers[p].cards[i].ImageFront);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic);
                                        }
                                    }
                                   
                                }
                                else
                                {   
                                    groupBox3.Text = otherplayers[p].PlayerName;
                                    if (otherplayers[p].isShowCard)
                                    {
                                        
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox3, "pic_" + (i + 1).ToString("0") + "_Player3");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic, otherplayers[p].cards[i].ImageFront);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox3, "pic_" + (i + 1).ToString("0") + "_Player3");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic);
                                        }
                                    }
                                    
                                }
                            }
                        }
                        break;
                    case 3:
                        {
                            otherplayers = otherplayers.OrderBy(s => s.Turn).ToList();
                            for (int p = 0; p < otherplayers.Count; p++)
                            {
                                if (p == 0)
                                {
                                    groupBox2.Text = otherplayers[p].PlayerName;
                                    if (otherplayers[p].isShowCard)
                                    {
                                        
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic, otherplayers[p].cards[i].ImageFront);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox2, "pic_" + (i + 1).ToString("0") + "_Player2");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic);
                                        }
                                    }
                                   
                                }
                                else if (p == 1)
                                {
                                    groupBox3.Text = otherplayers[p].PlayerName;
                                    if (otherplayers[p].isShowCard) 
                                    {
                                        
                                        otherplayers[p].ShowAlert = false;
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox3, "pic_" + (i + 1).ToString("0") + "_Player3");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic, otherplayers[p].cards[i].ImageFront);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox3, "pic_" + (i + 1).ToString("0") + "_Player3");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic);
                                        }
                                    }
                                }
                                else
                                {
                                    groupBox4.Text = otherplayers[p].PlayerName;
                                    if (otherplayers[p].isShowCard)
                                    {
                                        
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox4, "pic_" + (i + 1).ToString("0") + "_Player4");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic, otherplayers[p].cards[i].ImageFront);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < otherplayers[p].cards.Count; i++)
                                        {
                                            Control control = GetControlByName(groupBox4, "pic_" + (i + 1).ToString("0") + "_Player4");
                                            PictureBox pic = (System.Windows.Forms.PictureBox)control;
                                            SetImageControl(pic);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void Restart() 
        {
            btnRut.Enabled = false;
            btnChot.Enabled = false;
            btnCheck1.Enabled = false;
            btnCheck2.Enabled = false;
            btnCheck3.Enabled = false;
            btnShowCard.Enabled = true;
            ck1 = 0;
            ck2 = 0;
            ck3 = 0;
            lblPoint.Text = thisPlayer.TotalPoint.ToString();
            txtGame.Text = IDGame + "";
            if (thisPlayer.IsHost)
            {
                txtHost.Text = thisPlayer.PlayerName;
            }
            else
            {
                if (otherplayers != null)
                {
                    Player host = otherplayers.Where(s => s.IsHost == true).SingleOrDefault();
                    if (host != null)
                    {
                        txtHost.Text = host.PlayerName;
                    }
                }
            }

            if (thisPlayer.Turn == 1) 
            {
                txtCurrTurn.Text = thisPlayer.PlayerName;
                EnableBTN();
            }
            else 
            {
                if (otherplayers != null)
                {
                    Player curTurn = otherplayers.Where(s => s.Turn == 1).Single();
                    {
                        txtCurrTurn.Text = curTurn.PlayerName;
                    }
                }
            }

            SetCardForThisPlayer();
            ShowCardForOtherPlayers();
           
        }
        public void clearData()
        {
            GroupBox group = groupBox1;
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 1:
                        group = groupBox2;
                        break;
                    case 2:
                        group = groupBox3;
                        break;
                    case 3:
                        group = groupBox4;
                        break;
                    default:
                        break;
                }
                for (int p = 0; p < 5; p++)
                {
                    Control control = GetControlByName(group, "pic_" + (p + 1).ToString("0") + "_Player" + (i + 1).ToString("0"));
                    PictureBox pic = (System.Windows.Forms.PictureBox)control;
                    SetNoneImage(pic);
                }
            }
        }
        private void SetNoneImage(PictureBox box)
        {
            box.Image = null;
        }
        public void SetDataResult()
        {
            try
            {
                if (thisPlayer.IsHost)
                {
                    if (result.Columns.Count == 0)
                    {
                        result.Columns.Add("Game");
                        for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                        {
                            result.Columns.Add(otherplayers[i].PlayerName);
                        }
                        DataRow row = result.NewRow();
                        row[0] = IDGame;
                        for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                        {
                            row[i + 1] = otherplayers[i].Result;//""
                        }
                        result.Rows.Add(row);
                    }
                    else
                    {
                        DataRow row = result.NewRow();
                        row[0] = IDGame;
                        for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                        {
                            row[i + 1] = otherplayers[i].Result;
                        }
                        result.Rows.Add(row);
                    }
                }
                else
                {
                    List<Player> lstnothost = new List<Player>();
                    if (result.Columns.Count == 0)
                    {

                        result.Columns.Add("Game");
                        result.Columns.Add(thisPlayer.PlayerName);
                        for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                        {
                            if (!otherplayers[i].IsHost)
                            {
                                result.Columns.Add(otherplayers[i].PlayerName);
                                lstnothost.Add(otherplayers[i]);
                            }

                        }
                        lstnothost = lstnothost.OrderBy(s => s.Turn).ToList();
                        DataRow row = result.NewRow();
                        row[0] = 1;
                        row[1] = thisPlayer.Result;
                        for (int i = 0; i < lstnothost.Count(); i++)
                        {
                            row[i + 2] = lstnothost[i].Result;
                        }
                        result.Rows.Add(row);
                    }
                    else
                    {
                        for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                        {
                            if (!otherplayers[i].IsHost)
                            {
                                lstnothost.Add(otherplayers[i]);
                            }

                        }
                        lstnothost = lstnothost.OrderBy(s => s.Turn).ToList();
                        DataRow row = result.NewRow();
                        row[0] = IDGame;
                        row[1] = thisPlayer.Result;
                        for (int i = 0; i < lstnothost.Count(); i++)
                        {
                            row[i + 2] = lstnothost[i].Result;
                        }
                        result.Rows.Add(row);
                    }
                }
                dgv_ShowResult.DataSource = result;
                dgv_ShowResult.Update();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            } 
        }
        public void UpdateDataResult()
        {
            if (thisPlayer.IsHost)
            {
                int rowUpdate = result.Rows.Count - 1;
                DataRow row = result.Rows[rowUpdate];
                for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                {
                    row[i + 1] = otherplayers[i].Result;
                }
                result.AcceptChanges();
            }
            else
            {
                List<Player> lstnothost = new List<Player>();
                for (int i = 0; i < otherplayers.OrderBy(s => s.Turn).Count(); i++)
                {
                    if (!otherplayers[i].IsHost)
                    {
                        lstnothost.Add(otherplayers[i]);
                    }

                }
                lstnothost = lstnothost.OrderBy(s => s.Turn).ToList();
                int rowUpdate = result.Rows.Count - 1;
                DataRow row = result.Rows[rowUpdate];
                row[1] = thisPlayer.Result;
                for (int i = 0; i < lstnothost.Count(); i++)
                {
                    row[i + 2] = lstnothost[i].Result;
                }
                result.AcceptChanges();
            }
            dgv_ShowResult.DataSource = result;
            dgv_ShowResult.Update();
        }
        public void ShowFormRestart()
        {
            if(countCheckPlayer == otherplayers.Count)
            {
                DialogResult rs = MessageBox.Show("Bạn đã chơi xong ván. Bạn có muốn bắt đầu lại!", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult.Yes == rs)
                {
                    publicFunction.Send("ReStart;");
                }
                else
                {
                    publicFunction.Send("DISCONNECTGameRoom;");
                }
            }
        }
        private void btnCheck1_Click(object sender, EventArgs e)
        {
            
            if(ck1 == 0)
            {
                btnShowCard.Enabled = false;
                if (thisPlayer.isShowCard == false)
                {
                    publicFunction.Send("Show;");
                }
                countCheckPlayer++;
                publicFunction.Send("Check" + ";" + otherplayers[0].PlayerName + ";" + otherplayers[0].Turn);
                btnCheck1.Enabled = false;
                ShowFormRestart();
                ck1 = 1;
            }
            else if( ck1  == 1)
            {
                btnCheck1.Enabled = false;
                MessageBox.Show("Bạn đã kiểm tra bài người chơi này!!!","Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnCheck2_Click(object sender, EventArgs e)
        {
          
            if (ck2 == 0 )
            {
                btnShowCard.Enabled = false;
                if (thisPlayer.isShowCard == false)
                {
                    publicFunction.Send("Show;");
                }
                countCheckPlayer++;
                publicFunction.Send("Check" + ";" + otherplayers[1].PlayerName + ";" + otherplayers[1].Turn);
                btnCheck2.Enabled = false;
                ShowFormRestart();
                ck2 = 1;
            }
            else if (ck2  == 1)
            {
                btnCheck2.Enabled = false;
                MessageBox.Show("Bạn đã kiểm tra bài người chơi này!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
           
        }
        private void btnCheck3_Click(object sender, EventArgs e)
        {
           
            if (ck3 == 0)
            {
                btnShowCard.Enabled = false;
                if (thisPlayer.isShowCard == false)
                {
                    publicFunction.Send("Show;");
                }
                countCheckPlayer++;
                publicFunction.Send("Check" + ";" + otherplayers[2].PlayerName + ";" + otherplayers[2].Turn);
                btnCheck3.Enabled = false;
                ShowFormRestart();
                ck3 = 1;
            }
            else if(ck3 == 1) 
            {
                btnCheck3.Enabled = false;
                MessageBox.Show("Bạn đã kiểm tra bài người chơi này!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }
        public void ProcessingResults()
        {
            if(thisPlayer.Result == "Thắng")
            {
                MessageBox.Show("Chiến thắng!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if(thisPlayer.Result == "Thua")
            {
                MessageBox.Show("Thất bại!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if(thisPlayer.Result == "Hòa")
            {
                MessageBox.Show("Hòa!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void GameRoom_Load(object sender, EventArgs e)
        {

        }
    }
}
