using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;

namespace VirtualBank.ProductAdvisor.Components
{
    public class BundleAdvisor
    {
        private readonly IReadOnlyCollection<Bundle> bundles;
        private readonly IReadOnlyCollection<Rule> defaultRules;

        public BundleAdvisor(IReadOnlyCollection<Bundle> bundles, IReadOnlyCollection<Rule> defaultRules)
        {
            this.bundles = bundles;
            this.defaultRules = defaultRules;
        }

        /// <summary>
        /// Selects bundles by the given constraints
        /// </summary>
        /// <param name="constrains"></param>
        /// <returns></returns>
        public ICollection<Bundle> SelectBy(IReadOnlyCollection<Constraint> constraints)
        {
            if (constraints == null)
                throw new ArgumentNullException(nameof(constraints));

            if (!constraints.Any())
                throw new ArgumentException("Must contain at least one item", nameof(constraints));

            if (constraints.Count != constraints.Select(x => x.Category).Distinct().Count())
                throw new ArgumentException("Multiple constraints of the same category type are not allowed!");

            var result = bundles
                .Where(bundle => constraints.SatisfyAll(bundle.GetRules(defaultRules)))
                .ToList();

            return result;
        }

        /// <summary>
        /// Validates that bundle consists of proper products
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public static bool IsValidBundle(Bundle bundle)
        {
            var products = bundle.Products;

            if (products == null || products.Count == 0) return false;

            // Check one account rule
            var accounts = products.Where(p => p.Category.IsOf(ProductCategory.Account)).ToList();

            if (accounts.Count != 1) return false;

            // Check bound products
            var boundProducstCheckFailed = products
                .Where(p => p.IsBound)
                .Any(x => products.Where(y => y != x).All(y => !x.BoundToProducts.Contains(y.Name)));

            if (boundProducstCheckFailed) return false;

            // Check rule collisions
            var grouppedRules = products
                .Where(p => p.Rules != null)
                .SelectMany(p => p.Rules)
                .GroupBy(r => r.Constraint.Category);

            foreach (var ruleGroup in grouppedRules)
            {
                var orderedRules = ruleGroup.OrderBy(r => r.Constraint.Precedence).ThenBy(r => r.ConstraintEngagement).ToList();

                if (orderedRules.Count == 1) continue; // one rule, no collisions

                var prevRule = orderedRules[0];
                var secondExactFound = true;

                foreach (var nextRule in orderedRules.Skip(1))
                {
                    if (prevRule.ConstraintEngagement != ConstraintEngagement.Exact)
                    {
                        prevRule = nextRule;
                        continue;
                    }

                    if (prevRule.ConstraintEngagement == nextRule.ConstraintEngagement)
                    {
                        if (prevRule.Constraint.Precedence != nextRule.Constraint.Precedence)
                        {
                            // e.g. "Age only <18" collides with "Age only in [18-64]"
                            return false;
                        }
                    }
                    else
                    {
                        if ((secondExactFound = !secondExactFound))
                        {
                            return false;
                        }

                        if (prevRule.Constraint.Precedence < nextRule.Constraint.Precedence)
                        {
                            // e.g. "Age only <18" collides with "Age from [18-64] or higher"
                            return false;
                        }
                    }

                    prevRule = nextRule;
                }
            }

            return true;
        }
    }
}
