using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace XiDach_Client.Model
{
    public class Player
    {
        public string PlayerName { get; set; }
        public bool IsHost { get; set; }
        public int Turn { get; set; }
        public bool isShowCard { get; set; }
        public int CountCard { get; set; }
        public bool ShowAlert { get; set; } = false;
        public int TotalPoint { get; set; } = 0;
        public SPECIALCASE specialCase { get; set; } = SPECIALCASE.none;
        public List<Card> cards { get; set; }
        public Socket playerSocket { get; set; }
        public int IDgame { get; set; }
        public string Result { get; set; } = "";
    }
    public enum SPECIALCASE
    {
        none = 0,
        XiDzach = 1,
        XiBang = 2,
        NguLinh = 3,
        Quoac = -1,
    }
}
