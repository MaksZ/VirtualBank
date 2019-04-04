using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data.Entities
{
    public class Rule
    {
        // [Key]
        public int Id { get; set; }

        /// <summary>
        /// Describes, how the constraint is engaged to the rule
        /// </summary>
        public ConstraintEngagement ConstraintEngagement { get; set; }

        // [NotMapped]
        public string DisplayName 
            => 
                $"{Constraint?.Category?.Description}: {Constraint?.DisplayText} {(ConstraintEngagement == ConstraintEngagement.IncludingAbove ? "or higher" : "")}";

        /// <summary>
        /// Reference to related constraint
        /// </summary>
        public int ConstraintId { get; set; }

        /// <summary>
        /// Related constraint
        /// </summary>
        public virtual Constraint Constraint { get; set; }

        /// <summary>
        /// Reference to related product
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Related product
        /// </summary>
        public virtual Product Product { get; set; }
    }

    public static class RuleExtentions
    {
        /// <summary>
        /// Shows if given constraint belongs to the same category as one in the rule
        /// </summary>
        public static bool Correlates(this Rule obj, Constraint constraint)
        {
            return obj.Constraint.Category == constraint.Category;
        }

        /// <summary>
        /// Shows if rule is violated by given constraint; 
        /// IMPORTANT: call method only for constraint that this rule correlates with
        /// </summary>
        public static bool IsViolatedBy(this Rule obj, Constraint constraint)
        {
            switch (obj.ConstraintEngagement)
            {
                case ConstraintEngagement.Exact:
                    return obj.Constraint.Precedence != constraint.Precedence;

                case ConstraintEngagement.IncludingAbove:
                    return obj.Constraint.Precedence > constraint.Precedence;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds such rule which constraint is included in all other rules
        /// </summary>
        public static Rule GetCommonRule(this IEnumerable<Rule> rules)
            =>
                rules.OrderBy(r => r.ConstraintEngagement).ThenByDescending(r => r.Constraint.Precedence).First();

        public static bool NotIn(this Rule self, IEnumerable<Rule> rules)
            =>
                !rules.Any(rule => rule.Constraint.Category == self.Constraint.Category);

        public static bool SatisfyAll(this IEnumerable<Constraint> constraints, IEnumerable<Rule> rules)
            =>
                constraints.All(constraint => constraint.Satisfies(rules));

        public static bool Satisfies(this Constraint constraint, IEnumerable<Rule> rules) 
            => 
                !rules.Any(rule => rule.Correlates(constraint) && rule.IsViolatedBy(constraint));
    }
}
