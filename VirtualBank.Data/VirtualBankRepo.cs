using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data
{
    public class VirtualBankRepo : IDataModel, IDisposable
    {
        private readonly VirtualBankContext db = new VirtualBankContext();

        private IReadOnlyCollection<Rule> defaultRules;

        public IReadOnlyCollection<Bundle> Bundles
        {
            get
            {
                if (db.Bundles.Local.Count == 0)
                {
                    EnsureProducts();

                    db.Bundles.Load();
                }

                return db.Bundles.Local;
            }
        }

        public IReadOnlyCollection<Category> ConstraintCategories
        {
            get
            {
                EnsureConstraints();

                return db.Categories.Local.Where(c => c.CategoryType == CategoryType.Constraint).ToList();
            }
        }

        public IReadOnlyCollection<Category> ProductCategories
        {
            get
            {
                EnsureProducts();

                return db.Categories.Local.Where(c => c.CategoryType == CategoryType.Product).ToList();
            }
        }

        private void EnsureConstraints()
        {
            if (db.Categories.Local.Count > 0) return;

            db.Categories.Load();
            db.Constraints.Load();
        }

        private void EnsureProducts()
        {
            if (db.Products.Local.Count > 0) return;

            EnsureConstraints();

            db.Products.Load();
            db.Rules.Load();
        }

        public IReadOnlyCollection<Rule> DefaultRules
        {
            get
            {
                if (defaultRules == null)
                {
                    EnsureConstraints();

                    defaultRules = ConstraintCategories.Select(ToDefaultRule).ToList();
                }

                return defaultRules;
            }
        }

        private static Rule ToDefaultRule(Category category)
        {
            ConstraintCategory type;
            if (!Enum.TryParse(category.Description, out type))
            {
                throw new ArgumentException($"Unexpected constraint category: {category.Description}");
            }

            var ce = type == ConstraintCategory.Student
                ? ConstraintEngagement.Exact
                : ConstraintEngagement.IncludingAbove;

            var constraint = category.Items.OfType<Constraint>().OrderBy(x => x.Precedence).First();

            return new Rule
            {
                Constraint = constraint,
                ConstraintEngagement = ce
            };
        }

        public void Dispose()
        {
            db?.Dispose();
        }

        public bool SelfCheck()
        {
            return db.Categories.Count() > 0;
        }
    }
}
