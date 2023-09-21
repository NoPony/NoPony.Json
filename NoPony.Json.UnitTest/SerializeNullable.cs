using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NoPony.Json.UnitTest
{
    public class SerializeNullable
    {
        [Fact]
        public void Test()
        {

            TORoot to = new TORoot
            {
                Tint = 123,
            };

            string s = JSON.Serialize(to);

            Assert.Equal("{\"Tint\":123}", s);

            TORoot to2 = JSON.Deserialize<TORoot>(s);

            Assert.Equal(123, to2.Tint);
        }

        public class TORoot
        {
            public int? Tint { get; set; }
        }
    }
}
