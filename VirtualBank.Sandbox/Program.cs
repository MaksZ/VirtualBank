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
            Console.WriteLine("Select action: (d) : Display data model, (i) : Init database");

            var choice = Console.ReadKey(true).Key;

            switch (choice)
            {
                case ConsoleKey.D: new Program().DisplayDataModel(); break;
                case ConsoleKey.I: new Program().InitDatabase(); break;
            }

            Console.WriteLine("Program is finished. Press any key to close...");
            Console.ReadKey();
        }

        private void DisplayDataModel()
        {
            var dataModel = ModelBuilder.Build();

            foreach (var bundle in dataModel.Bundles)
            {
                Console.WriteLine($"BUNDLE: {bundle.Name}");

                foreach (var product in bundle.Products)
                {
                    Console.WriteLine($"  product: {product.DisplayText}");

                    if (product.Rules != null)
                        foreach (var rule in product.Rules)
                        {
                            Console.WriteLine($"    {rule.Description}");
                        }

                    if (!string.IsNullOrEmpty(product.BoundToProducts))
                    {
                        Console.WriteLine($"    {product.BoundToProducts}");
                    }
                }
            }
        }

        private void InitDatabase()
        {
            using (var db = new VirtualBankContext())
            {
                if (db.Categories.Count() > 0) return;

                // Override generator not to set Id prop
                ModelBuilder.GenId = () => 0;
                ModelBuilder.SkipBundleToProduct = true;

                var dataModel = ModelBuilder.Build();

                db.Categories.AddRange(dataModel.ConstraintCategories);
                db.Categories.AddRange(dataModel.ProductCategories);
                db.Bundles.AddRange(dataModel.Bundles);

                db.SaveChanges();
            }
        }
    }
}
