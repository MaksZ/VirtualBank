namespace VirtualBank.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        CategoryType = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CategoryItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CategoryId = c.Int(nullable: false),
                        Precedence = c.Int(),
                        BoundToProducts = c.String(),
                        CategoryType = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Bundle",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Priority = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Rule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConstraintEngagement = c.Byte(nullable: false),
                        ConstraintId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryItem", t => t.ConstraintId, cascadeDelete: true)
                .ForeignKey("dbo.CategoryItem", t => t.ProductId)
                .Index(t => t.ConstraintId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.ProductBundle",
                c => new
                    {
                        BundleId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BundleId, t.ProductId })
                .ForeignKey("dbo.Bundle", t => t.BundleId, cascadeDelete: true)
                .ForeignKey("dbo.CategoryItem", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.BundleId)
                .Index(t => t.ProductId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CategoryItem", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.Rule", "ProductId", "dbo.CategoryItem");
            DropForeignKey("dbo.Rule", "ConstraintId", "dbo.CategoryItem");
            DropForeignKey("dbo.ProductBundle", "ProductId", "dbo.CategoryItem");
            DropForeignKey("dbo.ProductBundle", "BundleId", "dbo.Bundle");
            DropIndex("dbo.ProductBundle", new[] { "ProductId" });
            DropIndex("dbo.ProductBundle", new[] { "BundleId" });
            DropIndex("dbo.Rule", new[] { "ProductId" });
            DropIndex("dbo.Rule", new[] { "ConstraintId" });
            DropIndex("dbo.CategoryItem", new[] { "CategoryId" });
            DropTable("dbo.ProductBundle");
            DropTable("dbo.Rule");
            DropTable("dbo.Bundle");
            DropTable("dbo.CategoryItem");
            DropTable("dbo.Category");
        }
    }
}
