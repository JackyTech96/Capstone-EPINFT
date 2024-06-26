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

        [Display(Name = "Nome File")]
        public string NomeFile { get; set; }

        [Display(Name = "Tipo File")]
        public string TipoFile { get; set; }

        [Required]
        [Display(Name = "Nome NFT")]
        [StringLength(50)]
        public string NomeNFT { get; set; }

        public string Descrizione { get; set; }

        public decimal Prezzo { get; set; }

        [Display(Name = "Data Creazione")]
        public DateTime DataCreazione { get; set; }

        [Display(Name = "Disponibile?")]
        public bool IsDisponibile { get; set; }

        public virtual Collezioni Collezioni { get; set; }

        public virtual Utenti Utenti { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transazioni> Transazioni { get; set; }
    }
}
