using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Models
{
    public class CarrelloItem
    {
        public NFT NFTItem { get; set; }
        public int Quantita { get; set; }
    }

    public class Carrello
    {
        public List<CarrelloItem> Items { get; set; }

        public Carrello()
        {
            Items = new List<CarrelloItem>();
        }
    }

}