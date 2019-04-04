using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Product : CategoryItem
    {
        // [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        // [NotMapped]
        public string DisplayName => $"{Name} {Category?.Description}";

        /// <summary>
        /// Describe what other products this one depends on
        /// </summary>
        /// <remarks>
        /// For simplicity, we keep here just product names separated by semicolon
        /// </remarks>
        public string BoundToProducts { get; set; }

        // [NotMapped]
        public bool IsBound => !string.IsNullOrEmpty(BoundToProducts);
        
        /// <summary>
        /// Set of rules for product possession
        /// </summary>
        public virtual ICollection<Rule> Rules { get; set; }

        /// <summary>
        /// Bundles that product is included in
        /// </summary>
        public virtual ICollection<Bundle> Bundles { get; set; }
    }

    public static class ProductExtentions
    {
        public static bool HasRuleThatViolates(this Product obj, Constraint constraint)
        {
            return obj.Rules != null && obj.Rules.Any(r => r.IsViolatedBy(constraint));
        }

        public static bool IsOf(this Product product, ProductCategory category)
            =>
                product.Category.Description == category.ToString();
    }
}
