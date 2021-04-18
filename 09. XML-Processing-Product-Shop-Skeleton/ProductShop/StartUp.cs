using ProductShop.Data;
using ProductShop.Models;
using ProductShop.Dtos.Import;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using ProductShop.Dtos.Export;
using ProductShop.DataTransferObjects.Output.GetSoldProductsDto;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var usersXml = File.ReadAllText("./Datasets/users.xml");
            //var productsXml = File.ReadAllText("./Datasets/products.xml");
            //var categoriesXml = File.ReadAllText("./Datasets/categories.xml");
            //var categoriesProductsXml = File.ReadAllText("./Datasets/categories-products.xml");

            //ImportUsers(context, usersXml);
            //ImportProducts(context, productsXml);
            //ImportCategories(context, categoriesXml);
            //ImportCategoryProducts(context, categoriesProductsXml);
            
            
            System.Console.WriteLine(GetUsersWithProducts(context));
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            const string root = "Users";

            var textWriter = new StringWriter();
            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            var users = new UserRootDto()
            {
                Count = context.Users.Count(u => u.ProductsSold.Any(p => p.Buyer != null)),
                Users = context
                .Users
                .ToArray()
                .Where(u => u.ProductsSold.Count >= 1)
                .OrderByDescending(x => x.ProductsSold.Count())
                .Take(10)
                .Select(u => new GetUsersWithProductsModel()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new SoldProductsModel()
                    {
                        Count = u.ProductsSold.Count(ps => ps.Buyer != null),
                        Products = u.ProductsSold
                        .ToArray()
                          .Where(ps => ps.Buyer != null)
                                .Select(ps => new ProductModel()
                                {
                                    Name = ps.Name,
                                    Price = ps.Price
                                })
                                  .OrderByDescending(x => x.Price)
                                  .ToArray()
                    }
                })
                .ToArray()
            };

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserRootDto), new XmlRootAttribute(root));
            xmlSerializer.Serialize(textWriter, users, nameSpace);

            return textWriter.ToString();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            // Get all categories. For each category select its name, the number of products, the average price of those products and the total revenue (total price sum) of those products (regardless if they have a buyer or not). Order them by the number of products (descending) then by total revenue.

            var allCategories = context.Categories
                .Select(x => new CategoryOutputModel
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count,
                    AveragePrice = x.CategoryProducts.Average(p => p.Product.Price),
                    TotalRevenue = x.CategoryProducts.Sum(p => p.Product.Price)

                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToArray();

            const string root = "Categories";

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryOutputModel[]), new XmlRootAttribute(root));

            var textWriter = new StringWriter();

            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            xmlSerializer.Serialize(textWriter, allCategories, nameSpace);

            var result = textWriter.ToString();

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            //Get all users who have at least 1 sold item. Order them by last name, then by first name. Select the person's first and last name. For each of the sold products, select the product's name and price. Take top 5 records.

            const string root = "Users";

            var users = context
                .Users
                .Where(u => u.ProductsSold.Any(b => b.Buyer != null))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .Select(u => new GetSoldProductsModel()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold
                        .Where(b => b.Buyer != null)
                            .Select(sp => new ProductModel()
                            {
                                Name = sp.Name,
                                Price = sp.Price
                            }).ToArray()
                })
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetSoldProductsModel[]), new XmlRootAttribute(root));

            var textWriter = new StringWriter();

            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            xmlSerializer.Serialize(textWriter, users, nameSpace);

            var result = textWriter.ToString();

            return result;
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            const string root = "Products";

            var productsInRange = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new ProductOutputModel
                {
                    Name = x.Name,
                    Price = x.Price,
                    BuyerFullName = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .OrderBy(x => x.Price)
                .Take(10)
                .ToArray();


            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductOutputModel[]), new XmlRootAttribute(root));

            var textWriter = new StringWriter();

            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            xmlSerializer.Serialize(textWriter, productsInRange, nameSpace);

            var result = textWriter.ToString();

            return result;
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var root = new XmlRootAttribute("CategoryProducts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryProductInputModel[]), root);
            var textReader = new StringReader(inputXml);
            var categoryProductsDTO = xmlSerializer.Deserialize(textReader) as CategoryProductInputModel[];

            var categoryProducts = categoryProductsDTO
                .Select(x => new CategoryProduct
                {
                    CategoryId = x.CategoryId,
                    ProductId = x.ProductId
                })
                .ToArray();

            context.AddRange(categoryProducts);
            context.SaveChanges();


            return $"Successfully imported {categoryProducts.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string categoriesXml)
        {
            var root = new XmlRootAttribute("Categories");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryInputModel[]), root);
            var textReader = new StringReader(categoriesXml);
            var categoriesDTO = xmlSerializer.Deserialize(textReader) as CategoryInputModel[];

            var categories = categoriesDTO
                .Select(x => new Category
                {
                    Name = x.Name
                })
                .ToArray();

            context.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string productsXml)
        {
            var root = new XmlRootAttribute("Products");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductInputModel[]), root);
            var textReader = new StringReader(productsXml);
            var productsDTO = xmlSerializer.Deserialize(textReader) as ProductInputModel[];

            var products = productsDTO
                .Select(x => new Product
                {
                    Name = x.Name,
                    Price = x.Price,
                    SellerId = x.SellerId,
                    BuyerId = x.BuyerId
                })
                .ToArray();

            context.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportUsers(ProductShopContext context, string usersXml)
        {
            var root = new XmlRootAttribute("Users");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserinputModel[]), root);

            var textRead = new StringReader(usersXml);

            var usersDto = xmlSerializer.Deserialize(textRead) as UserinputModel[];

            var users = usersDto.Select(x => new User
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = x.Age
            })
            .ToArray();

            context.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";


            //// 1. Създаваме нов rootAttribute и указваме роо-та на файла.
            //var root = new XmlRootAttribute("Suppliers");

            //// 2. Създаваме нов XmlSerializer, указваме в какво DTO да ни парсне обекта и викаме rootAttribute-а. rootAttribute-а може да се създаде дирекно в параметрите.
            //XmlSerializer serializer = new XmlSerializer(typeof(SupplierInputObject[]), root);

            //// 3. Трябва ни textReader, но понеже е абстрактен клас викаме наследник - String Reader с параметър xml-ския файл.
            //var textRead = new StringReader(suppliersXml);

            //// 4. Десериализира ни обектите (връща object) и ги кастваме към масив от DTO-та.
            //var suppliersDto = serializer.Deserialize(textRead) as SupplierInputObject[];

            //// 5. Ръчно си мапваме обектите към Supplier и ги добавяме в базата.
            //var suppliers = suppliersDto.Select(x => new Supplier
            //{
            //    Name = x.Name,
            //    IsImporter = x.IsImporter
            //})
            //    .ToList();

            //context.AddRange(suppliers);
            //context.SaveChanges();


        }
    }
}