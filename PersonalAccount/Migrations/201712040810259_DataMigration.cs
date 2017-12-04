namespace PersonalAccount.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CompanyType", c => c.String());
            AddColumn("dbo.AspNetUsers", "OPF", c => c.String());
            AddColumn("dbo.AspNetUsers", "CompanyName", c => c.String());
            AddColumn("dbo.AspNetUsers", "FullCompanyName", c => c.String());
            AddColumn("dbo.AspNetUsers", "City", c => c.String());
            AddColumn("dbo.AspNetUsers", "OGRN", c => c.String());
            AddColumn("dbo.AspNetUsers", "ContactFIO", c => c.String());
            AddColumn("dbo.AspNetUsers", "PhoneNumberOne", c => c.String());
            AddColumn("dbo.AspNetUsers", "PhoneNumberTwo", c => c.String());
            AddColumn("dbo.AspNetUsers", "EmailEmployee", c => c.String());
            AddColumn("dbo.AspNetUsers", "WebSite", c => c.String());
            AddColumn("dbo.AspNetUsers", "INN", c => c.String());
            AddColumn("dbo.AspNetUsers", "KPP", c => c.String());
            AddColumn("dbo.AspNetUsers", "LawAddress", c => c.String());
            AddColumn("dbo.AspNetUsers", "DirectorFIO", c => c.String());
            AddColumn("dbo.AspNetUsers", "DirectorPost", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "DirectorPost");
            DropColumn("dbo.AspNetUsers", "DirectorFIO");
            DropColumn("dbo.AspNetUsers", "LawAddress");
            DropColumn("dbo.AspNetUsers", "KPP");
            DropColumn("dbo.AspNetUsers", "INN");
            DropColumn("dbo.AspNetUsers", "WebSite");
            DropColumn("dbo.AspNetUsers", "EmailEmployee");
            DropColumn("dbo.AspNetUsers", "PhoneNumberTwo");
            DropColumn("dbo.AspNetUsers", "PhoneNumberOne");
            DropColumn("dbo.AspNetUsers", "ContactFIO");
            DropColumn("dbo.AspNetUsers", "OGRN");
            DropColumn("dbo.AspNetUsers", "City");
            DropColumn("dbo.AspNetUsers", "FullCompanyName");
            DropColumn("dbo.AspNetUsers", "CompanyName");
            DropColumn("dbo.AspNetUsers", "OPF");
            DropColumn("dbo.AspNetUsers", "CompanyType");
        }
    }
}
