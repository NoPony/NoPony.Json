using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NoPony.Json.UnitTest
{
    // if we have a class that references itself will the metadata collection go into an infinite loop?
    
    public class InfiniteLoop
    {
        [Fact]
        public void Test()
        {

            TORoot to = new TORoot
            {
                Child = new TORoot()
            };

            string s = JSON.Serialize(to);
        }

        public class TORoot
        {
            public TORoot Child { get; set; }
        }
    }
}
