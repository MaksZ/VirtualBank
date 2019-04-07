using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using VirtualBank.Data;
using VirtualBank.Data.Entities;
using VirtualBank.Data.Enums;
using VirtualBank.ProductAdvisor.DTO;

namespace VirtualBank.ProductAdvisor.Controllers
{
    public class RuleThemAllController : ApiController
    {
        private readonly IDataModel dataModel = ModelBuilder.Build();

        [Route("api/bundles")]
        public IEnumerable<BundleDto> Get()
        {
            return dataModel.Bundles.Select(Converter.AsBundleDto);
        }

        [Route("api/questions")]
        public IEnumerable<QuestionDto> GetConstraints()
        {
            return dataModel.ConstraintCategories.Select(Converter.AsQuestionDto);
        }

        [Route("api/bundles/advise")]
        [ResponseType(typeof(AdviseDto))]
        public IHttpActionResult GetAdvise(int age = -1, int student = -1, int income = -1)
        {
            try
            {
                var constraints = new List<Constraint>();

                if (age != -1)
                    constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Age).GetItem(age));

                if (student != -1)
                    constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Student).GetItem(student));

                if (income != -1)
                    constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Income).GetItem(income));

                if (!constraints.Any())
                    return BadRequest("At least one answer is expected!");

                var bundleAdvisor = new Components.BundleAdvisor(dataModel.Bundles, dataModel.DefaultRules);

                var bundle = bundleAdvisor
                    .SelectBy(constraints)
                    .OrderByDescending(b => b.Priority)
                    .Select(Converter.AsBundleDto)
                    .FirstOrDefault();

                if (bundle == null) return NotFound();

                return Ok(new AdviseDto
                {
                    SelectedBundle = bundle,
                    Answers= new AnswerDigest
                    {
                        Age = age,
                        Student = student,
                        Income = income
                    }
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("api/bundles/advise")]
        public IHttpActionResult ValidateAdvise(AdviseDto advise)
        {
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            //db.Dispose();
            base.Dispose(disposing);
        }
    }
}
