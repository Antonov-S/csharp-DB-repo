using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.DTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<SuppliersInputObject, Supplier>();
            this.CreateMap<PartsInputObject, Part>();
            this.CreateMap<CarInputObject, Car>();
            this.CreateMap<CustoerInputObject, Customer>();
            this.CreateMap<SalesInputObject, Sale>();
        }
    }
}
