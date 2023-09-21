using System;
using Xunit;

namespace NoPony.Json.UnitTest
{
    public class SerializeAnonymousObject
    {
        [Fact]
        public void Test()
        {
            string s = JSON.Serialize(
                new
                {
                    Name = "Name",
                    Value = "Value",
                    Children = new[]
                    {
                        new {Name = "One",  Value = 1, },
                        new {Name = "Two",  Value = 2, },
                        new {Name = "Three",  Value = 3, },
                    }
                });

            Assert.Equal("{\"Name\":\"Name\",\"Value\":\"Value\",\"Children\":[{\"Name\":\"One\",\"Value\":1},{\"Name\":\"Two\",\"Value\":2},{\"Name\":\"Three\",\"Value\":3}]}", s);
        }
    }
}
