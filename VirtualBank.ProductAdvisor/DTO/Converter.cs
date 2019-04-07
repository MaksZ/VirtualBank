using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualBank.Data;
using VirtualBank.Data.Entities;

namespace VirtualBank.ProductAdvisor.DTO
{
    internal static class Converter
    {
        public static Func<Product, ProductDto> AsProductDto =
            obj => new ProductDto
            {
                Id = obj.Id,
                Name = obj.DisplayText,
                RuleDescription = 
                    !string.IsNullOrEmpty(obj.BoundToProducts)
                    ? $"Depends on : {obj.BoundToProducts}"
                    : obj.Rules.ToDescription()
            };

        public static Func<Bundle, BundleDto> AsBundleDto = 
            obj => new BundleDto
            {
                Id = obj.Id,
                Name = obj.Name,
                RuleDescription = obj.GetRules().ToDescription(),
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


        internal static Bundle AsAttachedTo(this BundleDto obj, IDataModel dataModel)
        {
            var bundle = new Bundle
            {
                Products = new List<Product>()
            };

            if (obj.Products == null) return bundle;

            var items = dataModel.ProductCategories
                    .SelectMany(x => x.Items).ToList();

            var products = items.OfType<Product>()
                    .ToList();

            foreach (var productDto in obj.Products)
            {
                var product = products
                    .FirstOrDefault(x => x.Id == productDto.Id);

                if (product == null)
                    throw new AttachmentException($"Unknown product, Id: {productDto.Id}");

                bundle.Products.Add(product);
            }

            return bundle;
        }

        public class AttachmentException : Exception
        {
            public AttachmentException(string message) : base(message) { }
        }

    }
}