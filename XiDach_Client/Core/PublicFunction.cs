using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using XiDach_Client.Model;

namespace XiDach_Client.Core
{
    public class PublicFunction
    {
        public static Socket clientSocket;
        public static Thread recvThread;
        public Player thisPlayer;
        private List<Card> lstCard = new List<Card>();
        public void Connect(IPEndPoint serverEP)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(serverEP);
                recvThread = new Thread(() => Receive());
                recvThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void close()
        {
            clientSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            clientSocket.Close();
        }
        public void Send(string strInput)
        {
            byte[] msg = Encoding.UTF8.GetBytes(strInput);
            clientSocket.Send(msg);
        }
        public void Receive()
        {
            while (clientSocket.Connected)
            {
                if (clientSocket.Available > 0)
                {
                    string msg = "";
                    byte[] buffer = new byte[1024];
                    while (clientSocket.Available > 0)
                    {
                        int bRead = clientSocket.Receive(buffer);
                        msg = Encoding.UTF8.GetString(buffer, 0, bRead);
                    }
                    AnalyzingReturnMessage(msg);
                }
            }
        }

        public static GameRoom gametable;
        public static List<Player> otherplayers;
        public void AnalyzingReturnMessage(string msg)
        {
            string[] arrPayload = msg.Split(';');

            switch (arrPayload[0])
            {
                case "LOBBYINFO":
                    {
                        if (arrPayload.Length == 3)
                            WelcomeForm.lobby.DisplayConnectedPlayer(arrPayload[1], true);
                        else
                            WelcomeForm.lobby.DisplayConnectedPlayer(arrPayload[1]);
                    }
                    break;
                case "INIT":
                    {
                        DaoBai();
                        thisPlayer.Turn = int.Parse(arrPayload[2]);
                        thisPlayer.IsHost = Convert.ToBoolean((arrPayload[4]).ToLower());
                        string[] arrIDCard = (arrPayload[5]).Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
                        thisPlayer.cards = new List<Card>();
                        foreach (string itemID in arrIDCard)
                        {
                            thisPlayer.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(itemID)).Single());
                        }
                        thisPlayer.TotalPoint = Convert.ToInt32(arrPayload[6]);
                        if (arrPayload[7] == SPECIALCASE.none.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.none;
                        }
                        else if (arrPayload[7] == SPECIALCASE.XiBang.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.XiBang;
                        }
                        else if (arrPayload[7] == SPECIALCASE.XiDzach.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.XiDzach;
                        }
                        gametable = new GameRoom();
                        gametable.lstCard = lstCard;
                        gametable.IDGame = int.Parse(arrPayload[3]);
                        gametable.thisPlayer = thisPlayer;
                        WelcomeForm.lobby.Invoke((MethodInvoker)delegate ()
                        {
                            gametable.publicFunction = this;
                            gametable.Show();
                        }
                        );
                    }
                    break;
                case "OTHERINFO":
                    {
                        Player otherplayer = new Player();
                        otherplayer.PlayerName = arrPayload[1];
                        otherplayer.Turn = Convert.ToInt32(arrPayload[2]);
                        otherplayer.CountCard = Convert.ToInt32(arrPayload[3]);
                        otherplayer.isShowCard = Convert.ToBoolean((arrPayload[4]).ToLower());
                        otherplayer.IsHost = Convert.ToBoolean((arrPayload[6]).ToLower());
                        string[] arrIDCard = (arrPayload[5]).Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
                        otherplayer.cards = new List<Card>();
                        foreach (string itemID in arrIDCard)
                        {
                            otherplayer.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(itemID)).Single());
                        }
                        otherplayer.TotalPoint = Convert.ToInt32(arrPayload[7]);
                        if (arrPayload[8] == SPECIALCASE.none.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.none;
                        }
                        else if (arrPayload[8] == SPECIALCASE.XiBang.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.XiBang;
                            otherplayer.ShowAlert = true;
                        }
                        else if (arrPayload[8] == SPECIALCASE.XiDzach.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.XiDzach;
                            otherplayer.ShowAlert = true;
                        }
                        if (otherplayers == null)
                            otherplayers = new List<Player>();
                        otherplayers.Add(otherplayer);
                        gametable.otherplayers = otherplayers;
                    }
                    break;
                case "OTHERINFORestart":
                    {
                        if (otherplayers.Count == 0)
                            otherplayers = new List<Player>();

                        Player otherplayer = new Player();
                        otherplayer.PlayerName = arrPayload[1];
                        otherplayer.Turn = Convert.ToInt32(arrPayload[2]);
                        otherplayer.CountCard = Convert.ToInt32(arrPayload[3]);
                        otherplayer.isShowCard = Convert.ToBoolean((arrPayload[4]).ToLower());
                        otherplayer.IsHost = Convert.ToBoolean((arrPayload[6]).ToLower());
                        string[] arrIDCard = (arrPayload[5]).Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
                        otherplayer.cards = new List<Card>();
                        foreach (string itemID in arrIDCard)
                        {
                            otherplayer.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(itemID)).Single());
                        }
                        otherplayer.TotalPoint = Convert.ToInt32(arrPayload[7]);
                        if (arrPayload[8] == SPECIALCASE.none.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.none;
                        }
                        else if (arrPayload[8] == SPECIALCASE.XiBang.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.XiBang;
                            otherplayer.ShowAlert = true;
                        }
                        else if (arrPayload[8] == SPECIALCASE.XiDzach.ToString())
                        {
                            otherplayer.specialCase = SPECIALCASE.XiDzach;
                            otherplayer.ShowAlert = true;
                        }

                        otherplayers.Add(otherplayer);
                        gametable.otherplayers = otherplayers;
                    }
                    break;
                case "SETUP":
                    {
                        gametable.Invoke((MethodInvoker)delegate()
                        {
                            gametable.InitDisplay();
                        }
                      );
                    }
                    break;
                case "TURN":
                     {
                        gametable.currentTurn = Convert.ToInt32(arrPayload[2]);
                        gametable.PlayerNextTurn = arrPayload[1];
                        gametable.SetTurn();
                     }
                      break;
                case "SetResult":
                    {
                        gametable.Invoke((MethodInvoker)delegate ()
                        {
                            gametable.SetDataResult();
                        }
                       );
                    }
                    break;
                case "NextTurn":
                    {
                        gametable.currentTurn = Convert.ToInt32(arrPayload[1]);
                        gametable.PlayerNextTurn = arrPayload[2];
                        gametable.SetTurn();
                    }
                    break;
                case "UpdateCard":
                    {
                        thisPlayer.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(arrPayload[1])).Single());
                        if (arrPayload[3] == SPECIALCASE.none.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.none;
                        }
                        else if (arrPayload[3] == SPECIALCASE.Quoac.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.Quoac;
                        }
                        else if (arrPayload[3] == SPECIALCASE.NguLinh.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.NguLinh;
                            thisPlayer.ShowAlert = true;
                        }
                        thisPlayer.TotalPoint = Convert.ToInt32(arrPayload[2]);
                        gametable.UpdateCardDraw();
                    }
                    break;
                case "UpdateCardForOtherPlayer":
                    {
                        foreach (Player _player in otherplayers)
                        {
                            if (_player.Turn == Convert.ToInt32(arrPayload[2] + "") && arrPayload[1] == _player.PlayerName)
                            {
                                _player.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(arrPayload[3])).Single());
                                _player.CountCard = _player.cards.Count;
                                if (arrPayload[5] == SPECIALCASE.none.ToString())
                                {
                                    _player.specialCase = SPECIALCASE.none;
                                }

                                else if (arrPayload[5] == SPECIALCASE.Quoac.ToString())
                                {
                                    _player.specialCase = SPECIALCASE.Quoac;
                                    _player.ShowAlert = true;
                                }
                                else if (arrPayload[5] == SPECIALCASE.NguLinh.ToString())
                                {
                                    _player.specialCase = SPECIALCASE.NguLinh;
                                    _player.ShowAlert = true;
                                }
                                _player.TotalPoint = Convert.ToInt32(arrPayload[4]);
                            }
                        }
                        gametable.UpdateCardDrawOtherPlayer(otherplayers);

                    }
                    break;
                case "ShowBy":
                    {
                        foreach (Player _player in otherplayers)
                        {
                            if (_player.Turn == Convert.ToInt32(arrPayload[2]) && arrPayload[1] == _player.PlayerName)
                            {
                                _player.isShowCard = true;
                            }
                        }
                        gametable.ShowCardForOtherPlayers();
                    }
                    break;
                case "ShowSuccess":
                    {

                        thisPlayer.isShowCard = true;
                    }
                    break;
                case "ReStart":
                    {
                        thisPlayer.isShowCard = false;
                        thisPlayer.Turn = int.Parse(arrPayload[2]);
                        thisPlayer.IsHost = Convert.ToBoolean((arrPayload[4]).ToLower());
                        string[] arrIDCard = (arrPayload[5]).Split(new string[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);// 1$$5$$
                        thisPlayer.cards = new List<Card>();
                        thisPlayer.Result = "";
                        foreach (string itemID in arrIDCard)
                        {
                            thisPlayer.cards.Add(lstCard.Where(s => s.ID == Convert.ToInt32(itemID)).Single());
                        }
                        thisPlayer.TotalPoint = Convert.ToInt32(arrPayload[6]);
                        if (arrPayload[7] == SPECIALCASE.none.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.none;
                        }
                        else if (arrPayload[7] == SPECIALCASE.XiBang.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.XiBang;
                            thisPlayer.ShowAlert = true;
                        }
                        else if (arrPayload[7] == SPECIALCASE.XiDzach.ToString())
                        {
                            thisPlayer.specialCase = SPECIALCASE.XiDzach;
                            thisPlayer.ShowAlert = true;
                        }
                        gametable.thisPlayer = thisPlayer;
                        gametable.IDGame = int.Parse(arrPayload[3]);
                        otherplayers.Clear();
                        gametable.currentTurn++;
                        gametable.countCheckPlayer = 0;
                        gametable.clearData();
                    }
                    break;
                case "ReSetup":
                    {
                        gametable.Restart();
                    }
                    break;
                case "Result":
                    {
                        string result = arrPayload[1];
                        foreach (Player _player in otherplayers)
                        {
                            if (_player.Turn == Convert.ToInt32(arrPayload[3]) && arrPayload[2] == _player.PlayerName)
                            {

                                if (result == "Thắng")
                                {
                                    _player.Result = "Thua";
                                }
                                else if (result == "Thua")
                                {
                                    _player.Result = "Thắng";
                                }
                                else
                                    _player.Result = result;
                                _player.isShowCard = true;
                            }
                        }
                        thisPlayer.Result = result;
                        gametable.ProcessingResults();
                        gametable.ShowCardForOtherPlayers();
                        gametable.UpdateDataResult();
                    }
                    break;
                case "ResultOther":
                    {
                        string result = arrPayload[1];
                        foreach (Player _player in otherplayers)
                        {
                            if (_player.Turn == Convert.ToInt32(arrPayload[3]) && arrPayload[2] == _player.PlayerName)
                            {
                                _player.Result = result;
                                _player.isShowCard = true;
                            }
                        }
                        gametable.ShowCardForOtherPlayers();
                        gametable.UpdateDataResult();
                    }
                    break;
                case "CloseAll":
                    {
                        gametable.Close();
                    }
                    break;
                default:
                    break;
            }  
        }
        public void DaoBai()
        {
            lstCard.Add(new Card() { ID = 1, NameCard = "A", Value = 1, ImageFront = "Aco.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 2, NameCard = "2", Value = 2, ImageFront = "2co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 3, NameCard = "3", Value = 3, ImageFront = "3co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 4, NameCard = "4", Value = 4, ImageFront = "4co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 5, NameCard = "5", Value = 5, ImageFront = "5co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 6, NameCard = "6", Value = 6, ImageFront = "6co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 7, NameCard = "7", Value = 7, ImageFront = "7co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 8, NameCard = "8", Value = 8, ImageFront = "8co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 9, NameCard = "9", Value = 9, ImageFront = "9co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 10, NameCard = "10", Value = 10, ImageFront = "10co.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 11, NameCard = "J", Value = 10, ImageFront = "Jco.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 12, NameCard = "Q", Value = 10, ImageFront = "Qco.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 13, NameCard = "K", Value = 10, ImageFront = "Kco.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 14, NameCard = "A", Value = 1, ImageFront = "Aro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 15, NameCard = "2", Value = 2, ImageFront = "2ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 16, NameCard = "3", Value = 3, ImageFront = "3ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 17, NameCard = "4", Value = 4, ImageFront = "4ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 18, NameCard = "5", Value = 5, ImageFront = "5ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 19, NameCard = "6", Value = 6, ImageFront = "6ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 20, NameCard = "7", Value = 7, ImageFront = "7ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 21, NameCard = "8", Value = 8, ImageFront = "8ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 22, NameCard = "9", Value = 9, ImageFront = "9ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 23, NameCard = "10", Value = 10, ImageFront = "10ro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 24, NameCard = "J", Value = 10, ImageFront = "Jro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 25, NameCard = "Q", Value = 10, ImageFront = "Qro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 26, NameCard = "K", Value = 10, ImageFront = "Kro.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 27, NameCard = "A", Value = 1, ImageFront = "Achuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 28, NameCard = "2", Value = 2, ImageFront = "2chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 29, NameCard = "3", Value = 3, ImageFront = "3chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 30, NameCard = "4", Value = 4, ImageFront = "4chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 31, NameCard = "5", Value = 5, ImageFront = "5chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 32, NameCard = "6", Value = 6, ImageFront = "6chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 33, NameCard = "7", Value = 7, ImageFront = "7chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 34, NameCard = "8", Value = 8, ImageFront = "8chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 35, NameCard = "9", Value = 9, ImageFront = "9chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 36, NameCard = "10", Value = 10, ImageFront = "10chuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 37, NameCard = "J", Value = 10, ImageFront = "Jchuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 38, NameCard = "Q", Value = 10, ImageFront = "Qchuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 39, NameCard = "K", Value = 10, ImageFront = "Kchuon.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 40, NameCard = "A", Value = 1, ImageFront = "Abich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 41, NameCard = "2", Value = 2, ImageFront = "2bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 42, NameCard = "3", Value = 3, ImageFront = "3bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 43, NameCard = "4", Value = 4, ImageFront = "4bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 44, NameCard = "5", Value = 5, ImageFront = "5bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 45, NameCard = "6", Value = 6, ImageFront = "6bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 46, NameCard = "7", Value = 7, ImageFront = "7bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 47, NameCard = "8", Value = 8, ImageFront = "8bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 48, NameCard = "9", Value = 9, ImageFront = "9bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 49, NameCard = "10", Value = 10, ImageFront = "10bich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 50, NameCard = "J", Value = 10, ImageFront = "Jbich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 51, NameCard = "Q", Value = 10, ImageFront = "Qbich.jpg", IsOpen = false });
            lstCard.Add(new Card() { ID = 52, NameCard = "K", Value = 10, ImageFront = "Kbich.jpg", IsOpen = false });
        }
    }
}
