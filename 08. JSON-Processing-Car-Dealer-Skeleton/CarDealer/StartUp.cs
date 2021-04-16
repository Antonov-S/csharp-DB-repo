using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.DTO.CarsListOfPartsDto;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{

    public class StartUp
    {
    static IMapper mapper;

        public static void Main(string[] args)
        {
            var carDealerContext = new CarDealerContext();
            //carDealerContext.Database.EnsureDeleted();
            //carDealerContext.Database.EnsureCreated();

            //string suppliersJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //string partsJson = File.ReadAllText("../../../Datasets/parts.json");
            //string carsJson = File.ReadAllText("../../../Datasets/cars.json");
            //string customersJson = File.ReadAllText("../../../Datasets/customers.json");
            //string salesJson = File.ReadAllText("../../../Datasets/sales.json");

            //ImportSuppliers(carDealerContext, suppliersJson);
            //ImportParts(carDealerContext, partsJson);
            //ImportCars(carDealerContext, carsJson);
            //ImportCustomers(carDealerContext, customersJson);
            //ImportSales(carDealerContext, salesJson);

            var result = GetSalesWithAppliedDiscount(carDealerContext);
            Console.WriteLine(result);

        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(s => new CarInfoDTO()
                {
                    Car = new CarDTO()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    CustomerName = s.Customer.Name,
                    Discount = s.Discount.ToString("F2"),
                    Price = s.Car.PartCars.Sum(p => p.Part.Price).ToString("F2"),
                    PriceWithDiscount = (s.Car.PartCars.Sum(p => p.Part.Price) - (s.Car.PartCars.Sum(p => p.Part.Price) * s.Discount / 100)).ToString("F2")
                })
                .Take(10)
                .ToList();

            var result = JsonConvert.SerializeObject(sales, Formatting.Indented);

            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customerWithAtLeastOneCar = context.Customers
                .Where(x => x.Sales.Count >= 1)
                .Select(x => new
                {
                    fullName = x.Name,
                    boughtCars = x.Sales.Count,
                    spentMoney = x.Sales
                    .Select(car => car.Car.PartCars
                    .Select(y => y.Part)
                    .Sum(p => p.Price))
                    .Sum()
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToList();

            var result = JsonConvert.SerializeObject(customerWithAtLeastOneCar, Formatting.Indented);

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var partsCars = context
                .Cars
                .Select(c => new CarsListDto()
                {
                    Car = new CarsWithPartsListDto()
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TravelledDistance = c.TravelledDistance
                    },
                    Parts = c.PartCars.Select(p => new PartsList()
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price.ToString("F2")
                    }).ToList()

                })
                .ToList();

            var json = JsonConvert.SerializeObject(partsCars, Formatting.Indented);

            return json;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count()
                })
                .ToList();

            var result = JsonConvert.SerializeObject(localSuppliers, Formatting.Indented);

            return result;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var carsFromToyota = context.Cars
                .Where(x => x.Make == "Toyota")
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .Select(x => new
                {
                    Id = x.Id,
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .ToList();

            var result = JsonConvert.SerializeObject(carsFromToyota, Formatting.Indented);

            return result;
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(x => new
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToArray();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        public static string ImportSales(CarDealerContext context, string salesJson)
        {
            ConfigMapper();

            var dtoSales = JsonConvert.DeserializeObject<IEnumerable<SalesInputObject>>(salesJson);

            var sales = mapper.Map<IEnumerable<Sale>>(dtoSales);

            context.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count()}.";
        }

        public static string ImportCustomers(CarDealerContext context, string customersJson)
        {
            ConfigMapper();

            var dtoCustomers = JsonConvert.DeserializeObject<IEnumerable<CustoerInputObject>>(customersJson);

            var customers = mapper.Map<IEnumerable<Customer>>(dtoCustomers);

            context.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count()}.";
        }

        public static string ImportCars(CarDealerContext context, string carsJson)
        {
            
            var dtoCars = JsonConvert.DeserializeObject<IEnumerable<CarInputObject>>(carsJson);

            var listOfCars = new List<Car>();

            foreach (var car in dtoCars)
            {
                var currentCar = new Car
                {
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance.ToString()
                };

                foreach (var partId in car?.PartsId.Distinct())
                {
                    currentCar.PartCars.Add(new PartCar
                    {
                        PartId = partId
                    });
                }

                listOfCars.Add(currentCar);

            }

            context.AddRange(listOfCars);
            context.SaveChanges();

            return $"Successfully imported {listOfCars.Count()}.";
        }

        public static string ImportParts(CarDealerContext context, string partsJson)
        {
            ConfigMapper();

            var suppliedIds = context.Suppliers
                .Select(x => x.Id)
                .ToArray();

            var dtoParts = JsonConvert.DeserializeObject<IEnumerable<PartsInputObject>>(partsJson)
                .Where(s => suppliedIds.Contains(s.supplierId))
                .ToList();

            var parts = mapper.Map<IEnumerable<Part>>(dtoParts);

            context.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}.";
        }

        public static string ImportSuppliers(CarDealerContext context, string suppliersJson)
        {
            ConfigMapper();

            var dtoSuppliers = JsonConvert.DeserializeObject<IEnumerable<SuppliersInputObject>>(suppliersJson);

            var suppliers = mapper.Map<IEnumerable<Supplier>>(dtoSuppliers);
            context.AddRange(suppliers);
            context.SaveChanges();


            return $"Successfully imported {suppliers.Count()}.";
        }

        private static void ConfigMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            mapper = config.CreateMapper();
        }
    }
}