namespace ProductFormApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedCategoryId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Products", "Category_CategoryId", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "Category_CategoryId" });
            AddColumn("dbo.Products", "CategoryId", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "Category_CategoryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Category_CategoryId", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "CategoryId");
            CreateIndex("dbo.Products", "Category_CategoryId");
            AddForeignKey("dbo.Products", "Category_CategoryId", "dbo.Categories", "CategoryId", cascadeDelete: true);
        }
    }
}
