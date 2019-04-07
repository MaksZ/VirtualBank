using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualBank.Data.Entities;

namespace VirtualBank.ProductAdvisor.DTO
{
    internal class Converter
    {
        public static Func<Product, ProductDto> AsProductDto =
            obj => new ProductDto
            {
                Id = obj.Id,
                Name = obj.DisplayText
            };

        public static Func<Bundle, BundleDto> AsBundleDto = 
            obj => new BundleDto
            {
                Id = obj.Id,
                Name = obj.Name,
                Products = obj.Products.Select(AsProductDto).ToList()
            };

        public static Func<Constraint, AnswerDto> AsAnswerDto =
            obj => new AnswerDto
            {
                Id = obj.Precedence,
                Text = obj.DisplayText
            };

        public static Func<Category, QuestionDto> AsQuestionDto =
            obj => new QuestionDto
            {
                Category = obj.Description,
                PossibleAnswers = obj.Items
                    .OfType<Constraint>()
                    .OrderBy(x => x.Precedence)
                    .Select(AsAnswerDto)
                    .ToList()
            };
    }
}