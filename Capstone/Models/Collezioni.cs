namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Collezioni")]
    public partial class Collezioni
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Collezioni()
        {
            NFT = new HashSet<NFT>();
        }

        [Key]
        public int IdCollezione { get; set; }

        public int IdUtente { get; set; }

        public int IdCategoria { get; set; }

        [Required]
        [Display(Name = "Nome Collezione")]
        [StringLength(50)]
        public string NomeCollezione { get; set; }

        public string Descrizione { get; set; }

        [Display(Name = "Data Creazione")]
        public DateTime DataCreazione { get; set; }

        public decimal? Royalties { get; set; }

        [Display(Name = "Foto Collezione")]
        public string FotoCollezione { get; set; }

        public virtual Categorie Categorie { get; set; }

        public virtual Utenti Utenti { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NFT> NFT { get; set; }
    }
}
