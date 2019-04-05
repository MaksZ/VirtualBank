using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;
using VirtualBank.ProductAdvisor.Components;

namespace VirtualBank.Tests.ProductAdvisor.Components
{
    [TestClass]
    public class BundleAdvisorTests
    {
        private static IDataModel dataModel;
        private static IReadOnlyCollection<Rule> defaultRules;
        private BundleAdvisor sut;

        [ClassInitialize]
        public static void Initialize(TestContext _)
        {
            dataModel = ModelBuilder.Build();

            defaultRules = dataModel.DefaultRules;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            sut = new BundleAdvisor(dataModel.Bundles, defaultRules);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BundleAdvisor_ShouldNotAccept_MultipleConstraintsOfTheSameCategory()
        {
            var constraints = FromCategory(ConstraintCategory.Age).Items
                .OfType<Constraint>()
                .ToList();

            var result = sut.SelectBy(constraints);

            Assert.Fail();
        }

        [TestMethod]
        public void BundleAdvisor_ShouldSelect_JuniorSaverBundle()
        {
            var constraints = new List<Constraint>
            {
                FromCategory(ConstraintCategory.Age).GetItem(ModelBuilder.Constants.Age_0_17)
            };

            var result = sut.SelectBy(constraints);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(ModelBuilder.Constants.Bundle_junior_saver, result.First().Name);
        }

        [TestMethod]
        public void BundleAdvisor_ShouldSelect_StudentBundle()
        {
            var constraints = new List<Constraint>
            {
                FromCategory(ConstraintCategory.Student).GetItem(ModelBuilder.Constants.Student_yes)
            };

            var result = sut.SelectBy(constraints);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(ModelBuilder.Constants.Bundle_student, result.First().Name);
        }

        [TestMethod]
        public void BundleAdvisor_ShouldNotSelect_StudentBundle()
        {
            var constraints = new List<Constraint>
            {
                FromCategory(ConstraintCategory.Student).GetItem(ModelBuilder.Constants.Student_no)
            };

            var result = sut.SelectBy(constraints);

            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result.All(b => b.Name != ModelBuilder.Constants.Bundle_student));
        }

        [TestMethod]
        public void BundleAdvisor_ShouldNotValidate_EmptyBundle()
        {
            var bundle = new Bundle { Products = null };

            Assert.IsFalse(BundleAdvisor.IsValidBundle(bundle));

            bundle.Products = new List<Product>(0);

            Assert.IsFalse(BundleAdvisor.IsValidBundle(bundle));
        }

        [TestMethod]
        public void BundleAdvisor_ShouldValidate_BundleWithOnlyOneAccount()
        {
            var cardCategory = FromCategory(ProductCategory.Card);

            var bundle = new Bundle { Products = new List<Product>() };

            bundle.Products.Add(new Product { Category = cardCategory });
            bundle.Products.Add(new Product { Category = cardCategory });

            Assert.IsFalse(BundleAdvisor.IsValidBundle(bundle));

            var accountCategory = FromCategory(ProductCategory.Account);
            bundle.Products.Add(new Product { Category = accountCategory });

            Assert.IsTrue(BundleAdvisor.IsValidBundle(bundle));

            bundle.Products.Add(new Product { Category = accountCategory });

            Assert.IsFalse(BundleAdvisor.IsValidBundle(bundle));
        }

        [TestMethod]
        public void BundleAdvisor_ShouldValidate_BundleWithBoundProducts()
        {
            var bundle = new Bundle { Products = new List<Product>() };

            var accountCategory = FromCategory(ProductCategory.Account);
            var someProduct = new Product { Name = "SomeProduct", BoundToProducts = "OtherProduct", Category = accountCategory };

            var cardCategory = FromCategory(ProductCategory.Card);
            var otherProduct = new Product { Name = "OtherProduct", Category = cardCategory };

            var otherOther = new Product { Name = "OtherOther", Category = cardCategory };

            // Compose bundle 
            bundle.Products.Add(someProduct);

            Assert.IsFalse(BundleAdvisor.IsValidBundle(bundle));

            // Add missing product
            bundle.Products.Add(otherProduct);

            Assert.IsTrue(BundleAdvisor.IsValidBundle(bundle));

            // Replace one dependence with other and include it to bound product list
            someProduct.BoundToProducts = $"{otherProduct.Name};{otherOther.Name}";
            bundle.Products.Remove(otherProduct);
            bundle.Products.Add(otherOther);

            Assert.IsTrue(BundleAdvisor.IsValidBundle(bundle));

            otherProduct.Name = "NewProduct";
            bundle.Products.Add(otherProduct);

            Assert.IsTrue(BundleAdvisor.IsValidBundle(bundle));
        }

        [TestMethod]
        public void BundleAdvisor_ShouldValidate_BundlesFromDataModel()
        {
            foreach (var bundle in dataModel.Bundles)
            {
                Assert.IsTrue(BundleAdvisor.IsValidBundle(bundle));
            }
        }

        private static Category FromCategory(ConstraintCategory cc) 
            => 
                dataModel.ConstraintCategories.Get(cc);

        private static Category FromCategory(ProductCategory cc)
            =>
                dataModel.ProductCategories.Get(cc);
    }
}
