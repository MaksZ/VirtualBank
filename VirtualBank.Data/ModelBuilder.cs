using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;

namespace VirtualBank.Data
{
    public interface IDataModel
    {
        IReadOnlyCollection<Category> ConstraintCategories { get; }

        IReadOnlyCollection<Category> ProductCategories { get; }

        IReadOnlyCollection<Bundle> Bundles { get; }

        IReadOnlyCollection<Rule> DefaultRules { get; }
    }

    /// <summary>
    /// Helper class to create a model
    /// </summary>
    public static class ModelBuilder
    {
        private static int keyId = 0;

        public static IDataModel Build()
        {
            keyId = 0;

            var dm = new DataModel();
            return dm;
        }

        public static Category Get<T>(this IEnumerable<Category> source, T name) => source.First(x => x.Description == name.ToString());

        public static Constraint GetItem(this Category category, string name) => category.Items.OfType<Constraint>().First(x => x.DisplayText == name);

        private static int GenId() => ++keyId;

        private static Category CreateCategory<T>(T value, CategoryType type) 
            => new Category
            {
                Description = value.ToString(),
                CategoryType = type,
                Id = GenId()
            };

        private static Constraint CreateConstraint(string name, int precedence, Category category)
            => new Constraint
            {
                DisplayText = name,
                Precedence = precedence,
                Id = GenId(),
                CategoryId = category.Id,
                Category = category
            };

        private static Product CreateProduct(string name, Category category)
            => new Product
            {
                Name = name,
                Id = GenId(),
                CategoryId = category.Id,
                Category = category
            };

        private static Rule CreateRule(Constraint constraint, ConstraintEngagement ce, Product product)
            => new Rule
            {
                ConstraintEngagement = ce,
                ConstraintId = constraint.Id,
                Constraint = constraint,
                ProductId = product.Id,
                Product = product
            };

        private static Bundle CreateBundle(string name, int value)
            => new Bundle
            {
                Id = GenId(),
                Name = name,
                Value = value,
                Products = new List<Product>()
            };

        private static List<Category> CreateConstraintCategories()
        {
            var age = CreateCategory(ConstraintCategory.Age, CategoryType.Constraint);
            var student = CreateCategory(ConstraintCategory.Student, CategoryType.Constraint);
            var income = CreateCategory(ConstraintCategory.Income, CategoryType.Constraint);

            age.Items = new List<CategoryItem>
            {
                CreateConstraint(Constants.Age_0_17, 0, age),
                CreateConstraint(Constants.Age_18_64, 1, age),
                CreateConstraint(Constants.Age_65_plus, 2, age),
            };

            student.Items = new List<CategoryItem>
            {
                CreateConstraint(Constants.Student_no, 0, student),
                CreateConstraint(Constants.Student_yes, 1, student),
            };

            income.Items = new List<CategoryItem>
            {
                CreateConstraint(Constants.Income_0, 0, income),
                CreateConstraint(Constants.Income_1_12k, 1, income),
                CreateConstraint(Constants.Income_12k1_40k, 2, income),
                CreateConstraint(Constants.Income_40k1_plus, 3, income),
            };

            return new List<Category>
            {
                age,
                student,
                income
            };
        }

        private static List<Category> CreateProductCategories(IReadOnlyCollection<Category> constraintCategories)
        {
            var constraints = constraintCategories.SelectMany(x => x.Items).OfType<Constraint>().ToList();

            var account = CreateCategory(ProductCategory.Account, CategoryType.Product);
            var card = CreateCategory(ProductCategory.Card, CategoryType.Product);

            account.Items = CreateAccountProducts(account, constraints);
            card.Items = CreateCardProducts(card, constraints);

            return new List<Category>
            {
                account,
                card,
            };
        }

        private static IReadOnlyCollection<Rule> CreateDefaultRules(IReadOnlyCollection<Category> cc)
        {
            return new List<Rule>
            {
                // For Age default rule is "any age"
                GetDefaultRule(cc.Get(ConstraintCategory.Age), Constants.Age_0_17, ConstraintEngagement.IncludingAbove),
                // For Student default rule is "No"
                GetDefaultRule(cc.Get(ConstraintCategory.Student), Constants.Student_no, ConstraintEngagement.Exact),
                // For Income default rule is "any income"
                GetDefaultRule(cc.Get(ConstraintCategory.Income), Constants.Income_0, ConstraintEngagement.IncludingAbove),
            };
        }

        private static Rule GetDefaultRule(Category category, string value, ConstraintEngagement ce)
            => new Rule
            {
                ConstraintEngagement = ce,
                Constraint = category.GetItem(value)
            };

        private static Constraint Get(this IEnumerable<Constraint> source, string name) => source.First(x => x.DisplayText == name);
        
        private static ICollection<CategoryItem> CreateAccountProducts(Category category, List<Constraint> constraints)
        {
            var junior = CreateProduct(Constants.Product_junior, category);
            var student = CreateProduct(Constants.Product_student, category);
            var current = CreateProduct(Constants.Product_current, category);
            var current_plus = CreateProduct(Constants.Product_current_plus, category);
            var pensioner = CreateProduct(Constants.Product_pensioner, category);

            junior.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Age_0_17), ConstraintEngagement.Exact, junior)
            };

            student.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Student_yes), ConstraintEngagement.Exact, student),
                CreateRule(constraints.Get(Constants.Age_18_64), ConstraintEngagement.IncludingAbove, student)
            };

            current.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Income_1_12k), ConstraintEngagement.IncludingAbove, current),
                CreateRule(constraints.Get(Constants.Age_18_64), ConstraintEngagement.IncludingAbove, current)
            };

            current_plus.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Income_40k1_plus), ConstraintEngagement.Exact, current_plus),
                CreateRule(constraints.Get(Constants.Age_18_64), ConstraintEngagement.IncludingAbove, current_plus)
            };

            pensioner.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Income_1_12k), ConstraintEngagement.IncludingAbove, pensioner),
                CreateRule(constraints.Get(Constants.Age_65_plus), ConstraintEngagement.Exact, pensioner)
            };

            return new List<CategoryItem>
            {
                junior,
                student,
                current,
                current_plus,
                pensioner
            };
        }

        private static ICollection<CategoryItem> CreateCardProducts(Category category, List<Constraint> constraints)
        {
            var debit = CreateProduct(Constants.Product_debit, category);
            var credit = CreateProduct(Constants.Product_credit, category);
            var goldCredit = CreateProduct(Constants.Product_gold_credit, category);

            debit.BoundToProducts = string.Join(";", new[] 
            {
                Constants.Product_student,
                Constants.Product_current,
                Constants.Product_current_plus,
                Constants.Product_pensioner,
            });

            credit.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Income_12k1_40k), ConstraintEngagement.IncludingAbove, credit),
                CreateRule(constraints.Get(Constants.Age_18_64), ConstraintEngagement.IncludingAbove, credit)
            };

            goldCredit.Rules = new List<Rule>
            {
                CreateRule(constraints.Get(Constants.Income_40k1_plus), ConstraintEngagement.Exact, goldCredit),
                CreateRule(constraints.Get(Constants.Age_18_64), ConstraintEngagement.IncludingAbove, goldCredit)
            };

            return new List<CategoryItem>
            {
                debit,
                credit,
                goldCredit,
            };
        }

        private static List<Bundle> CreateBundles(IReadOnlyCollection<Category> productCategories)
        {
            var products = productCategories.SelectMany(x => x.Items).OfType<Product>().ToList();

            var juniorSaver = CreateBundle(Constants.Bundle_junior_saver, 0);

            ToBundle(juniorSaver, Constants.Product_junior, products);

            var student = CreateBundle(Constants.Bundle_student, 0);

            ToBundle(student, Constants.Product_student, products);
            ToBundle(student, Constants.Product_debit, products);
            ToBundle(student, Constants.Product_credit, products);

            var classic = CreateBundle(Constants.Bundle_classic, 1);

            ToBundle(classic, Constants.Product_current, products);
            ToBundle(classic, Constants.Product_debit, products);

            var classicPlus = CreateBundle(Constants.Bundle_classic_plus, 1);

            ToBundle(classicPlus, Constants.Product_current, products);
            ToBundle(classicPlus, Constants.Product_debit, products);
            ToBundle(classicPlus, Constants.Product_credit, products);

            var gold = CreateBundle(Constants.Bundle_gold, 2);

            ToBundle(gold, Constants.Product_current_plus, products);
            ToBundle(gold, Constants.Product_debit, products);
            ToBundle(gold, Constants.Product_gold_credit, products);

            return new List<Bundle>
            {
                juniorSaver,
                student,
                classic,
                classicPlus,
                gold
            };
        }

        private static void ToBundle(Bundle bundle, string productName, IReadOnlyCollection<Product> products)
        {
            var product = products.First(x => x.Name == productName);

            bundle.Products.Add(product);

            if (product.Bundles == null) product.Bundles = new List<Bundle>();

            product.Bundles.Add(bundle);
        }

        private class DataModel : IDataModel
        {
            public IReadOnlyCollection<Category> ConstraintCategories { get; }

            public IReadOnlyCollection<Category> ProductCategories { get; }

            public IReadOnlyCollection<Bundle> Bundles { get; }

            public IReadOnlyCollection<Rule> DefaultRules { get; }

            internal DataModel()
            {
                ConstraintCategories = CreateConstraintCategories();
                ProductCategories = CreateProductCategories(ConstraintCategories);
                Bundles = CreateBundles(ProductCategories);
                DefaultRules = CreateDefaultRules(ConstraintCategories);
            }
        }

        public static class Constants
        {
            public const string Age_0_17 = "0-17";
            public const string Age_18_64 = "18-64";
            public const string Age_65_plus = "65+";
            public const string Student_yes = "Yes";
            public const string Student_no = "No";
            public const string Income_0 = "0";
            public const string Income_1_12k = "1-12000";
            public const string Income_12k1_40k = "12001-40000";
            public const string Income_40k1_plus = "40001+";

            public const string Product_junior = "Junior Saver";
            public const string Product_student = "Student";
            public const string Product_pensioner = "Pensioner";
            public const string Product_current = "Current";
            public const string Product_current_plus = "Current Plus";

            public const string Product_debit = "Debit";
            public const string Product_credit = "Credit";
            public const string Product_gold_credit = "Gold Credit";

            public const string Bundle_junior_saver = "Junior Saver";
            public const string Bundle_student = "Student";
            public const string Bundle_classic = "Classic";
            public const string Bundle_classic_plus = "Classic Plus";
            public const string Bundle_gold = "Gold";
        }
    }
}
