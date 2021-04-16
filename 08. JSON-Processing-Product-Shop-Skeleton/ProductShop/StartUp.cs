using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DataTransferObjects;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        static IMapper mapper;

        public static void Main(string[] args)
        {
            var productShopContext = new ProductShopContext();
            // productShopContext.Database.EnsureDeleted();
            // productShopContext.Database.EnsureCreated();

            // string usersJson = File.ReadAllText("../../../Datasets/users.json");
            // string productJson = File.ReadAllText("../../../Datasets/products.json");
            // string categoriesJson = File.ReadAllText("../../../Datasets/categories.json");
            // string categoriesProductsJson = File.ReadAllText("../../../Datasets/categories-products.json");


            //ImportUsers(productShopContext, usersJson);
            //ImportProducts(productShopContext, productJson);
            //ImportCategories(productShopContext, categoriesJson);
            //ImportCategoryProducts(productShopContext, categoriesProductsJson);

            var result = GetUsersWithProducts(productShopContext);
            Console.WriteLine(result);
                        
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(x => x.ProductsSold)
                .ToList()
                .Where(x => x.ProductsSold.Any(b => b.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Where(x => x.BuyerId != null).Count(),
                        products = u.ProductsSold.Where(x => x.BuyerId != null).Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                    }
                })
                .OrderByDescending(x => x.soldProducts.count)
                .ToList();

            var resultObject = new
            {
                usersCount = users.Count(),
                users = users
            };


            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var result = JsonConvert.SerializeObject(resultObject, Formatting.Indented, jsonSerializerSettings);

            return result;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categoriesInfo = context.Categories
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoryProducts.Count,
                    averagePrice = c.CategoryProducts.Count == 0 ? 0.ToString("F2") : (c.CategoryProducts.Sum(p => p.Product.Price) / c.CategoryProducts.Count).ToString("F2"),
                    totalRevenue = c.CategoryProducts.Sum(p => p.Product.Price).ToString("F2")
                })
                .OrderByDescending(x => x.productsCount)
                .ToArray();

            var result = JsonConvert.SerializeObject(categoriesInfo, Formatting.Indented);

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldсProducts = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(user => new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    soldProducts = user.ProductsSold.Where(p => p.BuyerId != null).Select(b => new
                    {
                        name = b.Name,
                        price = b.Price,
                        buyerFirstName = b.Buyer.FirstName,
                        buyerLastName = b.Buyer.LastName
                    })
                    .ToList()
                })
                .OrderBy(x => x.lastName)
                .ThenBy(x => x.firstName)
                .ToList();

            var result = JsonConvert.SerializeObject(soldсProducts, Formatting.Indented);

            return result;
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = p.Seller.FirstName + " " + p.Seller.LastName

                })
                .OrderBy(x => x.price)
                .ToArray();

            var result = JsonConvert.SerializeObject(products, Formatting.Indented);

            return result;
        }

        public static string ImportCategoryProducts(ProductShopContext context, string categoriesProductsJson)
        {
            ConfigMapper();
            var dtoCategoryProduct = JsonConvert.DeserializeObject<IEnumerable<CategorieProductInputObject>>(categoriesProductsJson);
            var categoriesProducts = mapper.Map < IEnumerable<CategoryProduct>>(dtoCategoryProduct);

            context.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string categoriesJson)
        {
            ConfigMapper();

            var dtoCategories = JsonConvert.DeserializeObject<IEnumerable<CategoryInputModel>>(categoriesJson)
                .Where(x => x.Name != null)
                .ToList();

            var categories = mapper.Map<IEnumerable<Category>>(dtoCategories);

            context.AddRange(categories);
            context.SaveChanges();
            
            return $"Successfully imported {categories.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string productJson)
        {
            ConfigMapper();

            var dtoProducts = JsonConvert.DeserializeObject<IEnumerable<ProductInputModel>>(productJson);
            var products = mapper.Map<IEnumerable<Product>>(dtoProducts);

            context.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        private static void ConfigMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            mapper = config.CreateMapper();
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            ConfigMapper();

            var dtoUsers = JsonConvert.DeserializeObject<IEnumerable<UserInputModel>>(inputJson);

            var users = mapper.Map<IEnumerable<User>>(dtoUsers);

            context.AddRange(users);
            context.SaveChanges();
            

            return $"Successfully imported {users.Count()}";
        }
    }
}