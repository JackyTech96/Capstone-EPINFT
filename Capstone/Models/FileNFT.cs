namespace Capstone.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FileNFT")]
    public partial class FileNFT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FileNFT()
        {
            NFT = new HashSet<NFT>();
        }

        [Key]
        public int IdFileNFT { get; set; }

        [Required]
        public string NomeFile { get; set; }

        [Required]
        public string TipoFile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NFT> NFT { get; set; }
    }
}
