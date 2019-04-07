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
        public void Constraint_ShouldBeApplicable_ToRuleOfTheSameCategory()
        {
            var constraints = GetConstraints(ConstraintCategory.Age).ToList();

            var rule = new Rule { Constraint = constraints.First() };

            constraints.ForEach(constraint => Assert.IsTrue(constraint.IsApplicableTo(rule)));
        }

        [TestMethod]
        public void Constraint_ShouldNotBeApplicable_ToRuleOfOtherCategory()
        {
            var age = GetConstraints(ConstraintCategory.Age).ToList();
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var rule = new Rule { };

            foreach (var ageConstraint in age)
                foreach (var incomeConstraint in income)
                {
                    rule.Constraint = ageConstraint;
                    Assert.IsFalse(incomeConstraint.IsApplicableTo(rule));

                    rule.Constraint = incomeConstraint;
                    Assert.IsFalse(ageConstraint.IsApplicableTo(rule));
                }
        }

        [TestMethod]
        public void RuleWithExactEngagement_ShouldNotBeViolatedBy_TheSamePrecedenceConstraint()
        {
            var student = GetConstraints(ConstraintCategory.Student).ToList();

            var rule = new Rule { Constraint = student[0], ConstraintEngagement = ConstraintEngagement.Exact };

            Assert.IsFalse(student[0].Violates(rule));

            var weak = new Constraint { Precedence = student[0].Precedence };
            Assert.IsFalse(weak.Violates(rule));
        }

        [TestMethod]
        public void RuleWithExactEngagement_ShouldBeViolatedBy_OtherPrecedenceConstraint()
        {
            var student = GetConstraints(ConstraintCategory.Student).ToList();

            var rule = new Rule { Constraint = student[0], ConstraintEngagement = ConstraintEngagement.Exact };

            Assert.IsTrue(student[1].Violates(rule));

            var weak = new Constraint { Precedence = student[1].Precedence };
            Assert.IsTrue(weak.Violates(rule));
        }

        [TestMethod]
        public void RuleWithIncludingEngagement_ShouldNotBeViolatedBy_TheSameOrHigherPrecedenceConstraint()
        {
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var rule = new Rule { Constraint = income[0], ConstraintEngagement = ConstraintEngagement.IncludingAbove };

            Assert.IsFalse(income[0].Violates(rule));
            Assert.IsFalse(income[1].Violates(rule));
            Assert.IsFalse(income[2].Violates(rule));
        }

        [TestMethod]
        public void RuleWithIncludingEngagement_ShouldBeViolatedBy_LowerPrecedenceConstraint()
        {
            var income = GetConstraints(ConstraintCategory.Income).ToList();

            var rule = new Rule { Constraint = income[3], ConstraintEngagement = ConstraintEngagement.IncludingAbove };

            Assert.IsTrue(income[0].Violates(rule));
            Assert.IsTrue(income[1].Violates(rule));
        }

        [TestMethod]
        public void RuleExact_IsStronger_ThanIncludingAbove()
        {
            var age = GetConstraints(ConstraintCategory.Age).ToList();

            var rules = new[]
            {
                new Rule { Constraint = age[1], ConstraintEngagement = ConstraintEngagement.IncludingAbove },
                new Rule { Constraint = age[1], ConstraintEngagement = ConstraintEngagement.Exact },
            };

            Assert.AreEqual(ConstraintEngagement.Exact, rules.GetCommonRule().ConstraintEngagement);

            rules = new[]
            {
                new Rule { Constraint = age[1], ConstraintEngagement = ConstraintEngagement.Exact },
                new Rule { Constraint = age[0], ConstraintEngagement = ConstraintEngagement.IncludingAbove },
            };

            Assert.AreEqual(ConstraintEngagement.Exact, rules.GetCommonRule().ConstraintEngagement);
        }

        [TestMethod]
        public void RuleWithHigherPrecedence_IsStronger_ThanWithLower()
        {
            var income = GetConstraints(ConstraintCategory.Income).OfType<Constraint>().ToList();

            var data = new[]
            {
                new int [] { 0, 2 },
                new int [] { 1, 3, 2 },
                new int [] { 2, 0, 1 }
            };

            foreach (var sample in data)
            {
                var rules = sample
                    .Select(val => new Rule
                    {
                        Constraint = income.First(c => c.Precedence == val),
                        ConstraintEngagement = ConstraintEngagement.IncludingAbove
                    })
                    .ToList();

                var commonRule = rules.GetCommonRule();

                Assert.AreEqual(sample.Max(), commonRule.Constraint.Precedence);
            }
        }

        private static IEnumerable<Constraint> GetConstraints(ConstraintCategory cc)
            =>
                dataModel.ConstraintCategories.Get(cc).Items.OfType<Constraint>();
    }
}
