using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Constraint : CategoryItem
    {
        /// <summary>
        /// Denotes a precedence of a constraint in a category
        /// </summary>
        public int Precedence { get; set; }
    }

    public static class ConstraintExtensions
    {
        public static Constraint GetItem(this Category obj, int precedence)
        {
            var item = (precedence >= 0) ? obj.Items.OfType<Constraint>().FirstOrDefault(x => x.Precedence == precedence) : null;

            if (item == null)
                throw new ArgumentOutOfRangeException($"{obj.Description} must be in range [0, {obj.Items.Count - 1}], actual value: {precedence}");

            return item;
        }
    }
}
