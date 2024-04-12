namespace Capstone.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CampoIdProprietarioNFT : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NFT", "IdProprietario", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NFT", "IdProprietario");
        }
    }
}
