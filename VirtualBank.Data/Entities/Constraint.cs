using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBank.Data.Entities
{
    public class Constraint : CategoryItem
    {
        // [Key]
        public int Id { get; set; }

        /// <summary>
        /// Denotes a precedence of a constraint in a category
        /// </summary>
        public int Precedence { get; set; }

        /// <summary>
        /// Describes the constraint in human-friendly way
        /// </summary>
        public string DisplayText { get; set; }
    }
}
