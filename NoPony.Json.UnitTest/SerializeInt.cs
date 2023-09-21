using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NoPony.Json.UnitTest
{
    public class SerializeInt
    {
        [Fact]
        public void Test()
        {

            TO to = new TO
            {
                Tint = 1,
            };

            string s = JSON.Serialize(to);

            Assert.Equal("{\"Tint\":1}", s);
        }

        public class TO
        {
            public int Tint { get; set; }
        }
    }
}
