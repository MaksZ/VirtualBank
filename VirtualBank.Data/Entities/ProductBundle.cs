using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualBank.Data.Entities
{
    public class Bundle
    {
        // Key
        public int Id { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    public static class BundleExtensions
    {
        /// <rem>
        /// Generates rules for bundle
        /// </summary>
        /// <param name="bundle">A valid bundle, i.e. composed with products having no collisions between their rules</param>
        /// <param name="defaultRules">Default rules for misssed constraint cateogries</param>
        /// <returns></returns>
        public static ICollection<Rule> GetRules(this Bundle bundle, IReadOnlyCollection<Rule> defaultRules)
        {
            var allRules = bundle.Products.Where(p => p.Rules != null).SelectMany(p => p.Rules);

            // Find common rule for each constraint category
            var rules = allRules
                .GroupBy(x => x.Constraint.Category)
                .Select(g => g.GetCommonRule())
                .ToList();

            // Complete missing cateogries with defaults
            rules.AddRange(defaultRules.Where(dr => dr.NotIn(rules)).ToList());

            return rules;
        }
    }
}
