using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data
{
    public class VirtualBankContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Constraint> Constraints { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Bundle> Bundles { get; set; }

        public VirtualBankContext() : base("name=VirtualBankDb")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Category>()
                .HasMany(x => x.Items)
                .WithRequired(x => x.Category)
                .HasForeignKey(x => x.CategoryId);

            // We use Table per Hierarchy strategy for category items because of objects triviality
            // details: https://weblogs.asp.net/manavi/inheritance-mapping-strategies-with-entity-framework-code-first-ctp5-part-1-table-per-hierarchy-tph 
            modelBuilder.Entity<CategoryItem>()
                        .Map<Constraint>(m => m.Requires(nameof(CategoryType)).HasValue((byte)CategoryType.Constraint))
                        .Map<Product>(m => m.Requires(nameof(CategoryType)).HasValue((byte)CategoryType.Product));

            modelBuilder.Entity<Rule>()
                .HasRequired(x => x.Product)
                .WithMany(x => x.Rules)
                .HasForeignKey(x => x.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rule>()
                .HasRequired(x => x.Constraint);

            modelBuilder.Entity<Bundle>()
                            .HasMany(x => x.Products)
                            .WithMany(x => x.Bundles)
                            .Map(cs =>
                            {
                                cs.MapLeftKey($"{nameof(Bundle)}Id");
                                cs.MapRightKey($"{nameof(Product)}Id");
                                cs.ToTable($"{nameof(Product)}{nameof(Bundle)}");
                            });


            /*
             

 
        modelBuilder.Entity<CreditCard>().Map(m =>
        {
            m.MapInheritedProperties();
            m.ToTable("CreditCards");
        }); 
             */
        }
    }
}
