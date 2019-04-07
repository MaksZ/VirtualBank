using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBank.Data.Entities
{
    public class Bundle
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    public static class BundleExtensions
    {
        /// <summary>
        /// Generates rules for bundle - a subset of product rules, 
        /// such as if some constraint doesn't violate it, 
        /// it won't violate all bundle's product rules, applied to each product's rules separately
        /// </summary>
        /// <param name="bundle">A valid bundle, i.e. composed with products having no collisions between their rules</param>
        /// <param name="defaultRules">Default rules for misssed constraint categories</param>
        public static ICollection<Rule> GetRules(this Bundle bundle, IReadOnlyCollection<Rule> defaultRules)
        {
            // Find common rule for each constraint category
            var rules = bundle.Products
                .Where(p => p.Rules != null)
                .SelectMany(p => p.Rules)
                .GroupBy(x => x.Constraint.Category)
                .Select(g => g.GetCommonRule())
                .ToList();

            // We complete the set with rules for categories, that the set doesn't contain
            rules.AddRange(defaultRules.Where(dr => dr.NotIn(rules)).ToList());

            return rules;
        }

        /// <summary>
        /// Finds common rule; has sense only for rules without collisions 
        /// </summary>
        /// <remarks>
        /// A rule with exact engagement is stronger than having 'include above' one;
        /// A rule with higher precedence is stronger  that with lower one;
        /// E.g.: common rule for Rule(age > 17) and Rule(age in [18-64]) is Rule(age in [18-64]),
        /// common rule for Rule(income > 12000) and Rule(income > 40000) is Rule(income > 40000)
        /// </remarks>
        internal static Rule GetCommonRule(this IEnumerable<Rule> rules)
            =>
                rules.OrderBy(r => r.ConstraintEngagement).ThenByDescending(r => r.Constraint.Precedence).First();
    }
}
