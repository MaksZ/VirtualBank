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
        /// Generates rules for bundle
        /// </summary>
        /// <param name="bundle">A valid bundle, i.e. composed with products having no collisions between their rules</param>
        /// <param name="defaultRules">Default rules for misssed constraint cateogries</param>
        public static ICollection<Rule> GetRules(this Bundle bundle, IReadOnlyCollection<Rule> defaultRules)
        {
            // Find common rule for each constraint category
            var rules = bundle.Products
                .Where(p => p.Rules != null)
                .SelectMany(p => p.Rules)
                .GroupBy(x => x.Constraint.Category)
                .Select(g => g.GetCommonRule())
                .ToList();

            // Complete missing cateogries with defaults
            rules.AddRange(defaultRules.Where(dr => dr.NotIn(rules)).ToList());

            return rules;
        }

        /// <summary>
        /// Finds common rule; has sense only for rules without collisions 
        /// </summary>
        private static Rule GetCommonRule(this IEnumerable<Rule> rules)
            =>
                rules.OrderBy(r => r.ConstraintEngagement).ThenByDescending(r => r.Constraint.Precedence).First();
    }
}
