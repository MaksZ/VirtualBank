using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;

namespace VirtualBank.Tests.Data
{
    [TestClass]
    public class ExtensionTests
    {
        private static IDataModel dataModel;

        [ClassInitialize]
        public static void Initialize(TestContext _)
        {
            dataModel = ModelBuilder.Build();
        }

        [TestMethod]
        public void Rule_ShouldCorrelate_WithConstraintOfTheSameCategory()
        {
            var constraints = GetConstraints(ConstraintCategory.Age).ToList();

            var sut = new Rule { Constraint = constraints.First() };

            constraints.ForEach(constraint => Assert.IsTrue(sut.Correlates(constraint)));
        }

        [TestMethod]
        public void Rule_ShouldNotCorrelate_WithConstraintOfOtherCategory()
        {
            var age = GetConstraints(ConstraintCategory.Age).ToList();
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var sut = new Rule { };

            foreach (var ageConstraint in age)
                foreach (var incomeConstaint in income)
                {
                    sut.Constraint = ageConstraint;
                    Assert.IsFalse(sut.Correlates(incomeConstaint));

                    sut.Constraint = incomeConstaint;
                    Assert.IsFalse(sut.Correlates(ageConstraint));
                }
        }

        [TestMethod]
        public void RuleWithExactEngagement_ShouldNotBeViolatedBy_TheSamePrecedenceConstraint()
        {
            var student = GetConstraints(ConstraintCategory.Student).ToList();

            var sut = new Rule { Constraint = student[0], ConstraintEngagement = ConstraintEngagement.Exact };

            Assert.IsFalse(sut.IsViolatedBy(student[0]));

            var weak = new Constraint { Precedence = student[0].Precedence };
            Assert.IsFalse(sut.IsViolatedBy(weak));
        }

        [TestMethod]
        public void RuleWithExactEngagement_ShouldBeViolatedBy_OtherPrecedenceConstraint()
        {
            var student = GetConstraints(ConstraintCategory.Student).ToList();

            var sut = new Rule { Constraint = student[0], ConstraintEngagement = ConstraintEngagement.Exact };

            Assert.IsTrue(sut.IsViolatedBy(student[1]));

            var weak = new Constraint { Precedence = student[1].Precedence };
            Assert.IsTrue(sut.IsViolatedBy(weak));
        }

        [TestMethod]
        public void RuleWithIncludingEngagement_ShouldNotBeViolatedBy_TheSameOrHigherPrecedenceConstraint()
        {
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var sut = new Rule { Constraint = income[0], ConstraintEngagement = ConstraintEngagement.IncludingAbove };

            Assert.IsFalse(sut.IsViolatedBy(income[0]));
            Assert.IsFalse(sut.IsViolatedBy(income[1]));
            Assert.IsFalse(sut.IsViolatedBy(income[2]));
        }

        [TestMethod]
        public void RuleWithIncludingEngagement_ShouldBeViolatedBy_LowerPrecedenceConstraint()
        {
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var sut = new Rule { Constraint = income[3], ConstraintEngagement = ConstraintEngagement.IncludingAbove };

            Assert.IsTrue(sut.IsViolatedBy(income[0]));
            Assert.IsTrue(sut.IsViolatedBy(income[1]));
        }

        private static IEnumerable<Constraint> GetConstraints(ConstraintCategory cc)
            =>
                dataModel.ConstraintCategories.Get(cc).Items.OfType<Constraint>();
    }
}
