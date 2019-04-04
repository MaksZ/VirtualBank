using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBank.Data;

namespace VirtualBank.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataModel = ModelBuilder.Build();

            foreach (var bundle in dataModel.Bundles)
            {
                Console.WriteLine($"BUNDLE: {bundle.Name}");

                foreach (var product in bundle.Products)
                {
                    Console.WriteLine($"  product: {product.DisplayName}");

                    if (product.Rules != null)
                        foreach (var rule in product.Rules)
                        {
                            Console.WriteLine($"    {rule.DisplayName}");
                        }

                    if (!string.IsNullOrEmpty(product.BoundToProducts))
                    {
                        Console.WriteLine($"    {product.BoundToProducts}");
                    }
                }
            }

            Console.WriteLine("Program is finished. Press any key to close...");
            Console.ReadKey();
        }
    }
}
