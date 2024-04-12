namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NFT")]
    public partial class NFT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NFT()
        {
            Transazioni = new HashSet<Transazioni>();
        }

        [Key]
        public int IdNFT { get; set; }

        public int IdUtente { get; set; }

        public int IdCollezione { get; set; }

        public int IdProprietario { get; set; }

        public string NomeFile { get; set; }

        public string TipoFile { get; set; }

        [Required]
        [StringLength(50)]
        public string NomeNFT { get; set; }

        public string Descrizione { get; set; }

        public decimal Prezzo { get; set; }

        public DateTime DataCreazione { get; set; }

        public bool IsDisponibile { get; set; }

        public virtual Collezioni Collezioni { get; set; }

        public virtual Utenti Utenti { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transazioni> Transazioni { get; set; }
    }
}
