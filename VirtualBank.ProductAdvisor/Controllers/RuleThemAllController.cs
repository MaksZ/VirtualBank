using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IDataModel dataModel = GetDataModel();

        [HttpGet]
        public HttpResponseMessage Welcome()
        {
            var response = new HttpResponseMessage();
            var page = System.Web.HttpContext.Current.Request.MapPath("~/Welcome.html");
            response.Content = new StringContent(System.IO.File.ReadAllText(page));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


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

        [Route("api/products")]
        public IEnumerable<ProductDto> GetProducts()
        {
            return dataModel.ProductCategories
                .SelectMany(x => x.Items.OfType<Product>())
                .Select(Converter.AsProductDto);
        }

        [Route("api/bundles/advise")]
        [ResponseType(typeof(AdviseDto))]
        public IHttpActionResult GetAdvise(int age = -1, int student = -1, int income = -1)
        {
            try
            {
                var constraints = GetConstraintsBy(age, student, income);

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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // It's ok for demo purposes
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/bundles/advise")]
        public IHttpActionResult ValidateAdvise(AdviseDto advise)
        {
            try
            {
                var t = advise.Answers;
                var constraints = GetConstraintsBy(t.Age, t.Student, t.Income);

                if (advise.SelectedBundle == null)
                    return BadRequest("Bundle is not included");

                var bundle = advise.SelectedBundle.AsAttachedTo(dataModel);

                var validationResult = Components.BundleAdvisor.Validate(bundle, verbose: true);

                if (validationResult.Any())
                    return BadRequest(string.Join(Environment.NewLine, validationResult.Select(x => x.ErrorMessage)));

                var bundleAdvisor = new Components.BundleAdvisor(new[] { bundle }, dataModel.DefaultRules);

                bundle = bundleAdvisor
                    .SelectBy(constraints)
                    .OrderByDescending(b => b.Priority)
                    .FirstOrDefault();

                if (bundle == null) return BadRequest("Bundle doesn't match given constraints");

                return Ok();
            }
            catch (Exception ex)
            {
                if (ex is Converter.AttachmentException || ex is ArgumentException)
                    return BadRequest(ex.Message);

                // It's ok for demo purposes
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //db.Dispose();
            base.Dispose(disposing);
        }

        private List<Constraint> GetConstraintsBy(int age = -1, int student = -1, int income = -1)
        {
            var constraints = new List<Constraint>();

            if (age != -1)
                constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Age).GetItem(age));

            if (student != -1)
                constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Student).GetItem(student));

            if (income != -1)
                constraints.Add(dataModel.ConstraintCategories.Get(ConstraintCategory.Income).GetItem(income));

            if (!constraints.Any())
                throw new ArgumentException("At least one answer is expected!");

            return constraints;
        }

        private static IDataModel GetDataModel()
        {
            try
            {
                var repo = new VirtualBankRepo();

                if (!repo.SelfCheck())
                    return ModelBuilder.Build();

                return repo;
            }
            catch
            {
                // In case hosted database doesn't respond, let's return at least anything
                return ModelBuilder.Build();
            }
        }
    }
}
