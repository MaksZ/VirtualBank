using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Category
    {
        [Key]
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

    public static class CategoryExtensions
    {
        public static Category Get<T>(this IEnumerable<Category> source, T name) where T : struct
            => 
                source.First(x => x.IsOf(name));


        public static bool IsOf<T>(this Category obj, T category) where T : struct
            => 
                obj.Description == category.ToString();
    }
}
