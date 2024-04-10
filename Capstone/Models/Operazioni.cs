namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Operazioni")]
    public partial class Operazioni
    {
        [Key]
        public int IdOperazione { get; set; }

        public int IdWallet { get; set; }

        public int IdUtente { get; set; }

        [StringLength(50)]
        public string Tipo { get; set; }

        public decimal Importo { get; set; }

        public DateTime DataOperazione { get; set; }

        public virtual Utenti Utenti { get; set; }

        public virtual Wallets Wallets { get; set; }
    }
}
