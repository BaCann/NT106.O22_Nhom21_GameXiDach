using System;
namespace XiDach_Client.Model
{
    public class Card
    {
        public int ID { get; set; }
        public string NameCard { get; set; }
        public int Value { get; set; }
        public string ImageFront { get; set; }
        public bool IsOpen { get; set; }
    }
}
