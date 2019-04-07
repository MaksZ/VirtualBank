using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Product : CategoryItem
    {
        [NotMapped]
        public override string DisplayText => $"{Name} {Category?.Description}";

        /// <summary>
        /// Describe what other products this one depends on
        /// </summary>
        /// <remarks>
        /// For simplicity, we keep here just product names separated by semicolon
        /// </remarks>
        public string BoundToProducts { get; set; }

        [NotMapped]
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
}
