using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBank.Data.Entities
{
    public abstract class CategoryItem
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Describes the object in human-friendly way
        /// </summary>
        [NotMapped]
        public virtual string DisplayText => Name;

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
