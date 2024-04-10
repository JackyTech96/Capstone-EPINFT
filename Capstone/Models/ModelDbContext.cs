using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Capstone.Models
{
    public partial class ModelDbContext : DbContext
    {
        public ModelDbContext()
            : base("name=ModelDbContext")
        {
        }

        public virtual DbSet<Categorie> Categorie { get; set; }
        public virtual DbSet<Collezioni> Collezioni { get; set; }
        public virtual DbSet<FileNFT> FileNFT { get; set; }
        public virtual DbSet<NFT> NFT { get; set; }
        public virtual DbSet<Operazioni> Operazioni { get; set; }
        public virtual DbSet<Transazioni> Transazioni { get; set; }
        public virtual DbSet<Utenti> Utenti { get; set; }
        public virtual DbSet<Wallets> Wallets { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categorie>()
                .HasMany(e => e.Collezioni)
                .WithRequired(e => e.Categorie)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Collezioni>()
                .Property(e => e.Royalties)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Collezioni>()
                .HasMany(e => e.NFT)
                .WithRequired(e => e.Collezioni)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FileNFT>()
                .HasMany(e => e.NFT)
                .WithRequired(e => e.FileNFT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NFT>()
                .Property(e => e.Prezzo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<NFT>()
                .HasMany(e => e.Transazioni)
                .WithRequired(e => e.NFT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Operazioni>()
                .Property(e => e.Importo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Transazioni>()
                .Property(e => e.Importo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Collezioni)
                .WithRequired(e => e.Utenti)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.NFT)
                .WithRequired(e => e.Utenti)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Operazioni)
                .WithRequired(e => e.Utenti)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Transazioni)
                .WithRequired(e => e.Utenti)
                .HasForeignKey(e => e.IdAcquirente)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Transazioni1)
                .WithRequired(e => e.Utenti1)
                .HasForeignKey(e => e.IdVenditore)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Wallets)
                .WithRequired(e => e.Utenti)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Wallets>()
                .Property(e => e.Saldo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Wallets>()
                .HasMany(e => e.Operazioni)
                .WithRequired(e => e.Wallets)
                .WillCascadeOnDelete(false);

        }
    }
}
