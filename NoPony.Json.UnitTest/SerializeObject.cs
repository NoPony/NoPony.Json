using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NoPony.Json.UnitTest
{
    public class SerializeObject
    {
        [Fact]
        public void Test()
        {
            Cake c = new Cake
            {
                Id = "0001",
                Type = "Donut",
                Name = "Cake",
                Ppu = 0.55M,
                Batters = new BatterList
                {
                    Batter = new List<Batter>
                    {
                        new Batter { Id = "1001", Type = "Regular" },
                        new Batter { Id = "1002", Type = "Chocolate" },
                        new Batter { Id = "1003", Type = "Blueberry" },
                        new Batter { Id = "1004", Type = "Devil's Food" },
                    },
                },
                Topping = new List<Topping>
                {
                    new Topping{ Id = "5001", Type = "None" },
                    new Topping{ Id = "5002", Type = "Glazed" },
                    new Topping{ Id = "5005", Type = "Sugar" },
                    new Topping{ Id = "5007", Type = "Powdered Sugar" },
                    new Topping{ Id = "5006", Type = "Chocolate with Sprinkles" },
                    new Topping{ Id = "5003", Type = "Chocolate" },
                    new Topping{ Id = "5004", Type = "Maple" },
                }
            };

            string s = JSON.Serialize(c);

            Assert.Equal("{\"Id\":\"0001\",\"Type\":\"Donut\",\"Name\":\"Cake\",\"Ppu\":0.55,\"Batters\":{\"Batter\":[{\"Id\":\"1001\",\"Type\":\"Regular\"},{\"Id\":\"1002\",\"Type\":\"Chocolate\"},{\"Id\":\"1003\",\"Type\":\"Blueberry\"},{\"Id\":\"1004\",\"Type\":\"Devil's Food\"}]},\"Topping\":[{\"Id\":\"5001\",\"Type\":\"None\"},{\"Id\":\"5002\",\"Type\":\"Glazed\"},{\"Id\":\"5005\",\"Type\":\"Sugar\"},{\"Id\":\"5007\",\"Type\":\"Powdered Sugar\"},{\"Id\":\"5006\",\"Type\":\"Chocolate with Sprinkles\"},{\"Id\":\"5003\",\"Type\":\"Chocolate\"},{\"Id\":\"5004\",\"Type\":\"Maple\"}]}", s);
        }

        public class Cake
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public decimal Ppu { get; set; }
            public BatterList Batters { get; set; }
            public List<Topping> Topping { get; set; }
            //public Topping[] Topping { get; set; }
        }

        public class BatterList
        {
            public List<Batter> Batter { get; set; }
        }

        public class Batter
        {
            public string Id { get; set; }
            public string Type { get; set; }
        }

        public class Topping
        {
            public string Id { get; set; }
            public string Type { get; set; }
        }
    }
}