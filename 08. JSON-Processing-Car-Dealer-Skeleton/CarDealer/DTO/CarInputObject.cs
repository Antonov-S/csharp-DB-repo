using System.Collections.Generic;

namespace CarDealer.DTO
{
    public class CarInputObject
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int TravelledDistance { get; set; }
        public IEnumerable<int> PartsId { get; set; }
    }

    //"make": "Opel",
    //"model": "Omega",
    //"travelledDistance": 176664996,
    //"partsId": [ints]
    
}
