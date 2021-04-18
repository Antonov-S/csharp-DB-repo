using CarDealer.Data;
using CarDealer.DataTransferObjects.Output;
using CarDealer.DTO;
using CarDealer.DTO.Input;
using CarDealer.DTO.Output;
using CarDealer.Models;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext context = new CarDealerContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var suppliersXml = File.ReadAllText("./Datasets/suppliers.xml");
            var partsXml = File.ReadAllText("./Datasets/parts.xml");
            var carsXml = File.ReadAllText("./Datasets/cars.xml");
            var customersXml = File.ReadAllText("./Datasets/customers.xml");
            var salesXml = File.ReadAllText("./Datasets/sales.xml");

            ImportSuppliers(context, suppliersXml);
            ImportParts(context, partsXml);
            ImportCars(context, carsXml);
            ImportCustomers(context, customersXml);
            ImportSales(context, salesXml);

            System.Console.WriteLine(GetSalesWithAppliedDiscount(context));
            

        }


        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var allSales = context
                .Sales
                .Select(s => new SalesWithDiscounts()
                {
                    Car = new CarOutputObject()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(p => p.Part.Price),
                    PriceWithDiscount =
                    s.Car.PartCars.Sum(p => p.Part.Price) - (s.Car.PartCars.Sum(p => p.Part.Price) * s.Discount / 100)

                })
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SalesWithDiscounts[]), new XmlRootAttribute("sales"));

            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            var textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, allSales, nameSpace);

            var result = textWriter.ToString();

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithTheirListOfParts = context.Cars
                .Select(x => new CarsWithTheirListOfPartsOutputObject
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars.Select(pc => new PartOutputObject
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price
                    })
                    .OrderByDescending(x => x.Price)
                    .ToArray()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(CarsWithTheirListOfPartsOutputObject[]), new XmlRootAttribute("cars"));
            var textWriter = new StringWriter();
            var nameSpace = new XmlSerializerNamespaces();
            nameSpace.Add("", "");

            serializer.Serialize(textWriter, carsWithTheirListOfParts, nameSpace);

            var result = textWriter.ToString();
            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customersWithAtLeastOneCar = context.Customers
                .Where(x => x.Sales.Count >= 1)
                .Select(x => new CustomerOutputObject
                {
                    FullName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales.Select(x => x.Car).SelectMany(x => x.PartCars).Sum(x => x.Part.Price)
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(CustomerOutputObject[]), new XmlRootAttribute("customers"));
            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(textWriter, customersWithAtLeastOneCar, ns);
            var result = textWriter.ToString();
            return result;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new LocalSupplierOutputObject
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartCount = x.Parts.Count()
                })
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(LocalSupplierOutputObject[]), new XmlRootAttribute("suppliers"));
            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(textWriter, localSuppliers, ns);
            var result = textWriter.ToString();
            return result;
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.Make == "BMW")
                .Select(x => new CarMakeByBMWoutputObject
                {
                    Id = x.Id,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(CarMakeByBMWoutputObject[]), new XmlRootAttribute("cars"));
            var textWriter = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(textWriter, cars, ns);
            var result = textWriter.ToString();
            return result;
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.TravelledDistance > 2_000_000)
                .Select(x => new CarOutputObject
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .ToArray();

            

            XmlSerializer serializer = new XmlSerializer(typeof(CarOutputObject[]), new XmlRootAttribute("cars"));

            var textWriter = new StringWriter();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            serializer.Serialize(textWriter, cars, ns);
            var result = textWriter.ToString();
            
            return result;
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var carIds = context.Cars.Select(x => x.Id).ToList();
            

            XmlSerializer serializer = new XmlSerializer(typeof(SalesInputObject[]), new XmlRootAttribute("Sales"));
            var textReader = new StringReader(inputXml);
            var salesDTO = serializer.Deserialize(textReader) as SalesInputObject[];
            
            var sales = salesDTO
                .ToList()
                .Where(x => carIds.Contains(x.CarId))
                .Select(y => new Sale
                {
                    CarId = y.CarId,
                    CustomerId = y.CustomerId,
                    Discount = y.Discount
                });
                

            context.Sales.AddRange(sales);
            context.SaveChanges();


            return $"Successfully imported {sales.Count()}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CustomerInputObject[]), new XmlRootAttribute("Customers"));
            var textRead = new StringReader(inputXml);
            var customerDTO = serializer.Deserialize(textRead) as CustomerInputObject[];

            var customers = customerDTO
                .Select(x => new Customer
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate,
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToList();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var allParts = context.Parts
                .Select(x => x.Id)
                .ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(CarInputObject[]), new XmlRootAttribute("Cars"));
            var textRead = new StringReader(inputXml);
            var carsDTO = serializer.Deserialize(textRead) as CarInputObject[];

            var cars = carsDTO
                .Select(x => new Car
                {
                    Make = x.Meke,
                    Model = x.Model,
                    TravelledDistance = x.TraveledDistance,
                    PartCars = x.CarPartsInputModel.Select(x => x.Id)
                    .Distinct()
                    .Intersect(allParts)
                    .Select(pc => new PartCar
                    {
                        PartId = pc
                    })
                    .ToList()
                })
                .ToList();

            context.Cars.AddRange(cars);
                context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportParts(CarDealerContext context, string partsXml)
        {
            var suppliers = context.Suppliers
                .Select(x => x.Id)
                .ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(PartInputObject[]), new XmlRootAttribute("Parts"));

            var textRead = new StringReader(partsXml);

            var partsDto = serializer.Deserialize(textRead) as PartInputObject[];

            var parts = partsDto
                .Where(s => suppliers.Contains(s.SupplierId))
                .Select(x => new Part
                {
                    Name = x.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    SupplierId = x.SupplierId
                })
                .ToList();

            context.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        public static string ImportSuppliers(CarDealerContext context, string suppliersXml)
        {
            // 1. Създаваме нов rootAttribute и указваме роо-та на файла.
            var root = new XmlRootAttribute("Suppliers");

            // 2. Създаваме нов XmlSerializer, указваме в какво DTO да ни парсне обекта и викаме rootAttribute-а. rootAttribute-а може да се създаде дирекно в параметрите.
            XmlSerializer serializer = new XmlSerializer(typeof(SupplierInputObject[]), root);

            // 3. Трябва ни textReader, но понеже е абстрактен клас викаме наследник - String Reader с параметър xml-ския файл.
            var textRead = new StringReader(suppliersXml);

            // 4. Десериализира ни обектите (връща object) и ги кастваме към масив от DTO-та.
            var suppliersDto = serializer.Deserialize(textRead) as SupplierInputObject[];

            // 5. Ръчно си мапваме обектите към Supplier и ги добавяме в базата.
            var suppliers = suppliersDto.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            })
                .ToList();

            context.AddRange(suppliers);
            context.SaveChanges();



            return $"Successfully imported {suppliers.Count}";
        }
    }
}