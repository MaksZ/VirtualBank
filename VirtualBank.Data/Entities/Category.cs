using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Category
    {
        // [Key]
        public int Id { get; set; }

        /// <summary>
        /// Category description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category type
        /// </summary>
        public CategoryType CategoryType { get; set; }

        /// <summary>
        /// Related constraints
        /// </summary>
        public virtual ICollection<CategoryItem> Items { get; set; }
    }
}
