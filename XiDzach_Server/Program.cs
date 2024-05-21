using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using XiDzach_Server.Model;
namespace XiDzach_Server
{
    class Program
    {
        private static Socket serverSocket;
        private static Socket client;
        private static Thread clientThread;
        private static List<Player> connectedPlayers = new List<Player>();
        private static List<Card> lstCard = new List<Card>(); 
        private static List<Card> lstCardInGame = new List<Card>(); 
        private static int currentturn = 1;
        private static int Game = 0;
        static void Main(string[] args)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8989);
                serverSocket.Bind(serverEP);
                serverSocket.Listen(4);
                Console.WriteLine("[ Waiting for connection from players ... ]");
                while (true)
                {
                    client = serverSocket.Accept();
                    Console.WriteLine(">> Connection from " + client.RemoteEndPoint);
                    clientThread = new Thread(() => readingClientSocket(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kết nối lỗi: " + ex.ToString());
                Console.Read();
            }
        }
        public static void readingClientSocket(Socket client)
        {
            Player p = new Player();
            p.playerSocket = client;
            connectedPlayers.Add(p);
            byte[] buffer = new byte[1024];
            while (p.playerSocket.Connected)
            {
                if (p.playerSocket.Available > 0)
                {
                    string msg = "";

                    while (p.playerSocket.Available > 0)
                    {
                        int bRead = p.playerSocket.Receive(buffer);
                        msg += Encoding.UTF8.GetString(buffer, 0, bRead);
                    }

                    Console.WriteLine(p.playerSocket.RemoteEndPoint + ": " + msg);
                    AnalyzingMessage(msg, p);
                }
            }
        }
        public static void AnalyzingMessage(string msg, Player p)
        {
            string[] arrPayload = msg.Split(';');
            switch (arrPayload[0])
            {
                case "CONNECT":
                    {
                        p.PlayerName = arrPayload[1];
                        if(connectedPlayers.Count == 1)
                        {
                            p.IsHost = true;
                        }
                        else if (connectedPlayers.Count >= 2 && connectedPlayers.Count <= 4)
                        {
                            p.Turn = connectedPlayers.Count - 1;
                            Player host = connectedPlayers.Where(x => x.IsHost == true).Single(); // lấy 1 Player trong list<Player> connectedPlayers có giá trị IsHost == true
                            host.Turn = connectedPlayers.Count;
                        }
                        foreach (var player in connectedPlayers) // rà soát tất cả người chơi đang tham gia
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes("LOBBYINFO;" + player.PlayerName + (connectedPlayers.Count == 1 ? ";true" : ""));
                            p.playerSocket.Send(buffer); // Gửi cho người mới vào các thông tin của người vào trc đó
                            Thread.Sleep(100); //delay trong khoảng này
                        }

                        foreach (var player in connectedPlayers) // rà soát tất cả người chơi đang tham gia
                        {
                            if (player.playerSocket != p.playerSocket)
                            {
                                byte[] buffer = Encoding.UTF8.GetBytes("LOBBYINFO;" + p.PlayerName + (connectedPlayers.Count == 1 ? ";true" : ""));
                                player.playerSocket.Send(buffer); // Gửi cho các người chơi cũ thông tin của người mới vào
                                Thread.Sleep(100);
                            }
                        }
                    }
                    break;
                case "DISCONNECT": 
                    {
                        foreach (var player in connectedPlayers)
                        {
                            if (player.PlayerName == arrPayload[1])
                            {
                                player.playerSocket.Shutdown(SocketShutdown.Both);
                                player.playerSocket.Close();
                                connectedPlayers.Remove(player);
                            }
                        }
                    }
                    break;
                case "DISCONNECTGameRoom":
                    {
                        foreach (var player in connectedPlayers)
                        {
                            string makemsg1 = "CloseAll;";
                            byte[] buffer = Encoding.UTF8.GetBytes(makemsg1);
                            player.playerSocket.Send(buffer);
                            Console.WriteLine("Sendback: " + makemsg1);
                            Thread.Sleep(100);
                        }
                    }
                    break;
                case "Start":
                    {
                        if (Game == 0)
                            DaoBai();
                        lstCardInGame = lstCard;
                        Game++;  
                        ChiaBai();
                        //GỬI THÔNG TIN CỦA CÁC NGƯỜI CHƠI KHÁC CHO BẢN THÂN MÌNH
                        foreach (var player in connectedPlayers) //Nam;Minh;Hoang;Long 
                        {
                            foreach (var player_ in connectedPlayers) //Nam;Minh;Hoang;Long
                            {
                                if (player.PlayerName != player_.PlayerName) 
                                {
                                    string arrIDCard = "";
                                    foreach(Card item in player_.cards)
                                    {
                                        arrIDCard += item.ID + "$$";
                                    }
                                    string makemsg1 = "OTHERINFO;" + player_.PlayerName + ";" + player_.Turn + ";" + player_.cards.Count + ";" + player_.isShowCard + ";" + arrIDCard + ";" + player_.IsHost + ";" + player_.TotalPoint + ";" + player_.specialCase;
                                    byte[] buffer = Encoding.UTF8.GetBytes(makemsg1);
                                    player.playerSocket.Send(buffer);
                                    Console.WriteLine("Sendback: " + makemsg1);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        foreach (var player in connectedPlayers)
                        {
                            string makemsg = "SETUP;" + player.PlayerName;
                            byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                            player.playerSocket.Send(buffer);
                            Console.WriteLine("Sendback: " + makemsg);
                            Thread.Sleep(500);
                        }
                        foreach (var player in connectedPlayers) //check người chơi xì dách xì bàng, nếu host là xì dách hoặc xì bàng thì nó currentTurn == 4, nếu người chơi đầu xì dách thì bỏ qua lượt, tới lượt tiếp theo
                        {
                            bool isXiBangXiDach = CheckNextTurnSpecialCase(player);
                            if (isXiBangXiDach && player.IsHost)
                            {
                                currentturn = connectedPlayers.Count;
                                break;
                            }
                            else if (player.Turn == 1 && isXiBangXiDach)
                                currentturn++;
                        }
                        foreach (var player in connectedPlayers)
                        {
                            string makemsg_ = "TURN;" + connectedPlayers.Where(s=>s.Turn == currentturn).Single().PlayerName + ";"+ currentturn;
                            byte[] buffer_ = Encoding.UTF8.GetBytes(makemsg_);
                            player.playerSocket.Send(buffer_);
                            Console.WriteLine("Sendback: " + makemsg_);
                            Thread.Sleep(500);
                        }
                        foreach (var player in connectedPlayers)
                        {
                            string makemsg_ = "SetResult;";
                            byte[] buffer_ = Encoding.UTF8.GetBytes(makemsg_);
                            player.playerSocket.Send(buffer_);
                            Console.WriteLine("Sendback: " + makemsg_);
                            Thread.Sleep(500);
                        }
                    }
                    break;
                case "DrawCard":
                    {
                        RutBai(p);
                    }
                    break;
                case "Show":
                    {
                        Player player_ = new Player();
                        foreach (var player in connectedPlayers)
                        {
                            if (player.playerSocket == p.playerSocket)
                            {
                                player_ = player;
                                player.isShowCard = true;
                                string makemsg = "ShowSuccess;" + player.PlayerName + ";" + player.Turn;
                                byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                                player.playerSocket.Send(buffer);
                                Console.WriteLine("Sendback: " + makemsg);
                                Thread.Sleep(200);
                            }
                        }
                        foreach (var player in connectedPlayers)
                        {
                            if (player.playerSocket != player_.playerSocket)
                            {
                                string makemsg1 = "ShowBy;" + player_.PlayerName + ";" + player_.Turn;
                                byte[] buffer = Encoding.UTF8.GetBytes(makemsg1);
                                player.playerSocket.Send(buffer);
                                Console.WriteLine("Sendback: " + makemsg1);
                                Thread.Sleep(200);
                            }
                        }
                    }
                    break;
                case "Block":
                    {
                        Player player_ = getPlayerByTurn();
                        foreach (var player in connectedPlayers) 
                        {
                            string makemsg = "NextTurn;" + currentturn + ";" + player_.PlayerName;
                            byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                            player.playerSocket.Send(buffer);
                            Console.WriteLine("Sendback: " + makemsg);
                            Thread.Sleep(200);
                        }
                    }
                    break;
                case "ReStart":
                    {
                        currentturn = 1;
                        lstCard.Clear();
                        lstCardInGame.Clear();
                        DaoBai();
                        lstCardInGame = lstCard;// 52 lá
                        Game++; //ván 
                        ChiaBai("ReStart");
                        foreach (var player in connectedPlayers) //Nam/Minh/Lam/Meo
                        {
                            foreach (var player_ in connectedPlayers) //Nam/Minh/Lam/Meo
                            {
                                if (player.PlayerName != player_.PlayerName)
                                {
                                    string arrIDCard = "";
                                    foreach (Card item in player_.cards)
                                    {
                                        arrIDCard += item.ID + "$$";
                                    }
                                    string makemsg1 = "OTHERINFORestart;" + player_.PlayerName + ";" + player_.Turn + ";" + player_.cards.Count + ";" + player_.isShowCard + ";" + arrIDCard + ";" + player_.IsHost + ";" + player_.TotalPoint + ";" + player_.specialCase;
                                    byte[] buffer = Encoding.UTF8.GetBytes(makemsg1);
                                    player.playerSocket.Send(buffer);
                                    Console.WriteLine("Sendback: " + makemsg1);
                                    Thread.Sleep(300);
                                }
                            }
                        }

                        foreach (var player in connectedPlayers)
                        {
                            string makemsg = "ReSetup;" + player.PlayerName;
                            byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                            player.playerSocket.Send(buffer);
                            Console.WriteLine("Sendback: " + makemsg);
                            Thread.Sleep(500);
                        }
                        foreach (var player in connectedPlayers) //check người chơi xì dách xì bàng, nếu host là xì dách hoặc xì bàng thì nó currentTurn == 4, nếu người chơi đầu xì dách thì bỏ qua lượt, tới lượt tiếp theo
                        {
                            bool isXiBangXiDach = CheckNextTurnSpecialCase(player);
                            if (isXiBangXiDach && player.IsHost)
                            {
                                currentturn = connectedPlayers.Count;
                                break;
                            }
                            else if (player.Turn == 1 && isXiBangXiDach)
                                currentturn++;
                        }
                        foreach (var player in connectedPlayers)
                        {
                            string makemsg_ = "TURN;" + connectedPlayers.Where(s => s.Turn == currentturn).Single().PlayerName + ";" + currentturn;
                            byte[] buffer_ = Encoding.UTF8.GetBytes(makemsg_);
                            player.playerSocket.Send(buffer_);
                            Console.WriteLine("Sendback: " + makemsg_);
                            Thread.Sleep(300);
                        }

                        foreach (var player in connectedPlayers)
                        {
                            string makemsg_ = "SetResult;";
                            byte[] buffer_ = Encoding.UTF8.GetBytes(makemsg_);
                            player.playerSocket.Send(buffer_);
                            Console.WriteLine("Sendback: " + makemsg_);
                            Thread.Sleep(500);
                        }
                    }
                    break;
                case "Check":
                    {   
                        Player player_ = new Player();
                        player_ = connectedPlayers.Where(s => s.Turn == Convert.ToInt32(arrPayload[2]) && s.PlayerName == arrPayload[1] + "").FirstOrDefault();
                        string result = "";
                        if (player_ != null)
                        {
                            result = CheckPointHost(p, player_);
                            string makemsg = "Result;" + result + ";" + player_.PlayerName + ";" + player_.Turn;
                            byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                            p.playerSocket.Send(buffer); // Gửi cái
                            Console.WriteLine("Sendback: " + makemsg);
                            Thread.Sleep(200);

                            if (result == "Thắng")
                            {
                                result = "Thua";
                            }
                            else if (result == "Thua")
                            {
                                result = "Thắng";
                            }
                            string makemsg1 = "Result;" + result + ";" + player_.PlayerName + ";" + player_.Turn;
                            byte[] buffer1 = Encoding.UTF8.GetBytes(makemsg1);
                            player_.playerSocket.Send(buffer1); // Gửi người chơi bị xét
                            Console.WriteLine("Sendback: " + makemsg1);
                            Thread.Sleep(200);
                        }
                        foreach (var player in connectedPlayers) // rà soát tất cả người chơi đang tham gia
                        {
                            if (player.playerSocket != p.playerSocket && player.playerSocket != player_.playerSocket)
                            {
                                string makemsg = "ResultOther;" + result + ";" + player_.PlayerName + ";" + player_.Turn;
                                byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                                player.playerSocket.Send(buffer);
                                Console.WriteLine("Sendback: " + makemsg);
                                Thread.Sleep(200);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void DaoBai()
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
        private static void ChiaBai(string action = "INIT")
        {
            try
            {
                foreach (var player in connectedPlayers.OrderBy(x=>x.Turn).ToList()) // turn từ lớn đến bé 
                {
                    if (action == "ReStart")
                    {
                        player.specialCase = SPECIALCASE.none;
                        player.isShowCard = false;
                        player.TotalPoint = 0;
                        player.Result = "";
                    }
                    Random rand = new Random();
                    string idArr = ""; 
                    player.cards = new List<Card>();
                    for (int i = 0; i < 2; i++)
                    {
                        int toSkip = rand.Next(0, lstCardInGame.Count());
                        Card card = lstCardInGame.Skip(toSkip).Take(1).Single(); 
                        player.cards.Add(card); 
                        idArr += card.ID + "$$"; 
                        lstCardInGame.Remove(card); 
                    }
                    player.CountCard = 2;
                    player.IDgame = Game;
                    CheckSpecialCase(player);
                    string makemsg = action +";" + player.PlayerName + ";" + player.Turn + ";" + player.IDgame + ";" + player.IsHost + ";" + idArr + ";" + player.TotalPoint + ";" + player.specialCase ;
                    byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                    player.playerSocket.Send(buffer);
                    Console.WriteLine("Sendback: " + makemsg);
                    Thread.Sleep(2000);
                }
            }
            catch(Exception ex)
            {

            }
        }
        private static void RutBai(Player p)
        {
            try
            {
                Random rand = new Random();
                string idArr = "";
                Player player_ = new Player();
                foreach (var player in connectedPlayers) 
                {
                    if (player.playerSocket == p.playerSocket)
                    {
                        int toSkip = rand.Next(0, lstCardInGame.Count());
                        Card card = lstCardInGame.Skip(toSkip).Take(1).Single(); 
                        player.cards.Add(card); 
                        idArr += card.ID; 
                        lstCardInGame.Remove(card); 
                        player.CountCard++;
                        CheckSpecialCase(player);
                        player_ = player;
                        string makemsg = "UpdateCard;" + idArr + ";" + player.TotalPoint + ";" + player.specialCase;
                        byte[] buffer = Encoding.UTF8.GetBytes(makemsg);
                        player.playerSocket.Send(buffer);
                        Console.WriteLine("Sendback: " + makemsg);
                        Thread.Sleep(200);
                    }
                }

                foreach (var player in connectedPlayers) // rà soát tất cả người chơi đang tham gia
                {
                    if (player.playerSocket != player_.playerSocket)
                    {
                        string makemsg1 = "UpdateCardForOtherPlayer;" + player_.PlayerName + ";" + player_.Turn + ";" + idArr + ";" + player_.TotalPoint + ";" + player_.specialCase;
                        byte[] buffer = Encoding.UTF8.GetBytes(makemsg1);
                        player.playerSocket.Send(buffer);
                        Console.WriteLine("Sendback: " + makemsg1);
                        Thread.Sleep(200);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        private static void CheckSpecialCase(Player p) 
        {
            try
            {
                switch (p.CountCard)
                {
                    case 2:// A: 1,10,11
                        {
                            if ((p.cards[0].Value + p.cards[1].Value) == 11 && (p.cards[0].NameCard == "A" || p.cards[1].NameCard == "A"))
                            {
                                p.specialCase = SPECIALCASE.XiDzach;
                                p.TotalPoint = 21;
                                p.isShowCard = true;
                            }
                            else if (p.cards[0].NameCard == "A" && p.cards[1].NameCard == "A")
                            {
                                p.specialCase = SPECIALCASE.XiBang;
                                p.TotalPoint = 21;
                                p.isShowCard = true;
                            }
                            else if (p.cards[0].NameCard == "A" || p.cards[1].NameCard == "A")
                            {
                                if (p.cards[0].NameCard == "A") // A;7    
                                    p.TotalPoint = checkPointMax(1 + p.cards[1].Value, 10 + p.cards[1].Value, 11 + p.cards[1].Value);
                                else if (p.cards[1].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[0].Value, 10 + p.cards[0].Value, 11 + p.cards[0].Value);
                            }
                            else
                                p.TotalPoint = p.cards[0].Value + p.cards[1].Value;
                        }
                        break;
                    case 3:// A: 1,10,11
                        {
                            if (p.cards[0].NameCard == "A" || p.cards[1].NameCard == "A" || p.cards[2].NameCard == "A") // 1,2,1
                            {
                                if (p.cards[0].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[1].Value + p.cards[2].Value, 10 + p.cards[1].Value + p.cards[2].Value, 11 + p.cards[1].Value + p.cards[2].Value);
                                if (p.cards[1].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[0].Value + p.cards[2].Value, 10 + p.cards[0].Value + p.cards[2].Value, 11 + p.cards[0].Value + p.cards[2].Value);
                                if (p.cards[2].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[0].Value + p.cards[1].Value, 10 + p.cards[0].Value + p.cards[1].Value, 11 + p.cards[0].Value + p.cards[1].Value);
                            }
                            else
                                p.TotalPoint = p.cards[0].Value + p.cards[1].Value + p.cards[2].Value;
                            CheckOtherSpecialCase(p); // Check Quoac
                        }
                        break;
                    case 4://A: 1,10,11
                        {
                            if (p.cards[0].NameCard == "A" || p.cards[1].NameCard == "A" || p.cards[2].NameCard == "A" || p.cards[3].NameCard == "A" || p.cards[3].NameCard == "A") 
                            {
                                if (p.cards[0].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[1].Value + p.cards[2].Value + p.cards[3].Value, 10 + p.cards[1].Value + p.cards[2].Value + p.cards[3].Value, 11 + p.cards[1].Value + p.cards[2].Value + p.cards[3].Value);
                                if (p.cards[1].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[0].Value + p.cards[2].Value + p.cards[3].Value, 10 + p.cards[0].Value + p.cards[2].Value + p.cards[3].Value, 11 + p.cards[0].Value + p.cards[2].Value + p.cards[3].Value);
                                if (p.cards[2].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[1].Value + p.cards[0].Value + p.cards[3].Value, 10 + p.cards[1].Value + p.cards[0].Value + p.cards[3].Value, 11 + p.cards[1].Value + p.cards[0].Value + p.cards[3].Value);
                                if (p.cards[3].NameCard == "A")
                                    p.TotalPoint = checkPointMax(1 + p.cards[1].Value + p.cards[2].Value + p.cards[0].Value, 10 + p.cards[1].Value + p.cards[2].Value + p.cards[0].Value, 11 + p.cards[1].Value + p.cards[2].Value + p.cards[0].Value);
                            }
                            else
                                p.TotalPoint = p.cards[0].Value + p.cards[1].Value + p.cards[2].Value + p.cards[3].Value;
                            CheckOtherSpecialCase(p);
                        }
                        break;
                    case 5://A: 1
                        {
                            p.TotalPoint = p.cards[0].Value + p.cards[1].Value + p.cards[2].Value + p.cards[3].Value + p.cards[4].Value;
                            if (p.TotalPoint <= 21)
                                p.specialCase = SPECIALCASE.NguLinh;
                            CheckOtherSpecialCase(p);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private static int checkPointMax(int a, int b, int c = 0) 
        {    
            List<int> lst = new List<int>();
            lst.Add(a);
            lst.Add(b);
            if( c > 0 )
                lst.Add(c);
            int max = lst.Max();
            int min = lst.Min();
            if (max <= 21)
                return max;
            else
            {
                lst.Remove(max);
                max = lst.Max();
                if (max <= 21)
                    return max;
                else 
                    return min;
            }
        }
        private static void CheckOtherSpecialCase(Player p)
        {
            if (p.TotalPoint > 21)
            {
                p.specialCase = SPECIALCASE.Quoac;
            }
        }
        private static string CheckPointHost(Player host, Player player)
        {
            string rs = "";
            switch (host.specialCase)
            {
                case SPECIALCASE.XiBang:
                    {
                        if (player.specialCase == SPECIALCASE.XiBang)
                            return "Hòa";
                        else
                            return "Thắng";
                    }
                case SPECIALCASE.XiDzach:
                    {
                        if (player.specialCase == SPECIALCASE.XiBang)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.XiDzach)
                            return "Hòa";
                        else
                            return "Thắng";
                    }
                case SPECIALCASE.NguLinh:
                    {
                        if (player.specialCase == SPECIALCASE.XiBang)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.XiDzach)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.NguLinh)
                            return "Hòa";
                        else
                            return "Thắng";
                    }
                case SPECIALCASE.Quoac:
                    {
                        if (player.specialCase == SPECIALCASE.Quoac)
                            return "Hòa";
                        else
                            return "Thua";
                    }
                default:
                    {
                        if (player.specialCase == SPECIALCASE.XiBang)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.XiDzach)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.NguLinh)
                            return "Thua";
                        else if (player.specialCase == SPECIALCASE.Quoac)
                            return "Thắng";
                        else if (host.TotalPoint == player.TotalPoint)
                            return "Hòa";
                        else if (host.TotalPoint < player.TotalPoint)
                            return "Thua";
                        else if (host.TotalPoint > player.TotalPoint)
                            return "Thắng";
                    }
                    break;
            }
            return rs;
        }
        private static bool CheckNextTurnSpecialCase(Player player)
        {
            bool isRS = false;
            if (player.specialCase == SPECIALCASE.XiBang || player.specialCase == SPECIALCASE.XiDzach)
                isRS = true;
            return isRS;
        }
        private static Player getPlayerByTurn()
        {
            currentturn++;
            Player player_ = connectedPlayers.Where(s => s.Turn == currentturn).FirstOrDefault();
            bool isXiBangXiDach = CheckNextTurnSpecialCase(player_);
            if (isXiBangXiDach)
            {
                player_ = getPlayerByTurn();
            }
            return player_;
        }
    }
}
