namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Transazioni")]
    public partial class Transazioni
    {
        [Key]
        public int IdTransazione { get; set; }

        public int IdNFT { get; set; }

        public int IdAcquirente { get; set; }

        public int IdVenditore { get; set; }

        public decimal Importo { get; set; }

        public DateTime DataTransazione { get; set; }

        public virtual NFT NFT { get; set; }

        public virtual Utenti Utenti { get; set; }

        public virtual Utenti Utenti1 { get; set; }
    }
}
