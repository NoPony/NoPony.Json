using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NoPony.Json.UnitTest
{
    public class SerializeString
    {
        [Fact]
        public void Test()
        {

            TO to = new TO
            {
                Tstring = "Test",
            };

            string s = JSON.Serialize(to);

            Assert.Equal("{\"Tstring\":\"Test\"}", s);
        }

        public class TO
        {
            public string Tstring { get; set; }
        }
    }
}
