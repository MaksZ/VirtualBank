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
        /// <remarks>
        /// Expected to be any non-negative integer;
        /// Must be unique for all items of the same category
        /// </remarks>
        public int Precedence { get; set; }
    }

    public static class ConstraintExtensions
    {
        public static Constraint GetItem(this Category obj, int precedence)
        {
            var item = (precedence >= 0) ? obj.Items.OfType<Constraint>().FirstOrDefault(x => x.Precedence == precedence) : null;

            if (item == null)
            {
                var values = obj.Items.OfType<Constraint>().Select(c => c.Precedence).ToArray();
                var value_min = values.Min();
                var value_max = values.Max();

                var message =
                    (value_min <= precedence && precedence <= value_max)
                    ? $"{obj.Description} = {precedence} is not valid in range [{value_min}, {value_max}]"
                    : $"{obj.Description} must be in range [{values.Min()}, {values.Max()}], now is {precedence}";

                throw new ArgumentOutOfRangeException(message);
            }

            return item;
        }
    }
}
