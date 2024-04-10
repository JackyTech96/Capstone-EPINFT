namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Utenti")]
    public partial class Utenti
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Utenti()
        {
            Collezioni = new HashSet<Collezioni>();
            NFT = new HashSet<NFT>();
            Operazioni = new HashSet<Operazioni>();
            Transazioni = new HashSet<Transazioni>();
            Transazioni1 = new HashSet<Transazioni>();
            Wallets = new HashSet<Wallets>();
        }

        [Key]
        public int IdUtente { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        public string FotoUtente { get; set; }

        public bool IsArtista { get; set; }

        [Required]
        [StringLength(50)]
        public string Ruolo { get; set; } = "user";


        [NotMapped]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La password deve essere lunga almeno {2} caratteri.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Le password non corrispondono.")]
        public string ConfirmNewPassword { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Collezioni> Collezioni { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NFT> NFT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Operazioni> Operazioni { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transazioni> Transazioni { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transazioni> Transazioni1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Wallets> Wallets { get; set; }
    }
}
