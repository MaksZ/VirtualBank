using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBank.Data.Entities
{
    public abstract class CategoryItem
    {
        /// <summary>
        /// Reference to related category
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Related category
        /// </summary>
        public virtual Category Category { get; set; }
    }
}
