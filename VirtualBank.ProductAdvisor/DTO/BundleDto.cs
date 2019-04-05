using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualBank.ProductAdvisor.DTO
{
    public class BundleDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<ProductDto> Products { get; set; }
    }
}