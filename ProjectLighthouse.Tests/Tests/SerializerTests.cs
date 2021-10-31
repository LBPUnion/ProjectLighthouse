using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Serialization;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class SerializerTests : LighthouseTest
    {
        [Fact]
        public void BlankElementWorks()
        {
            Assert.Equal("<test></test>", LbpSerializer.BlankElement("test"));
        }

        [Fact]
        public void StringElementWorks()
        {
            Assert.Equal("<test>asd</test>", LbpSerializer.StringElement("test", "asd"));
            Assert.Equal("<test>asd</test>", LbpSerializer.StringElement(new KeyValuePair<string, object>("test", "asd")));
        }

        [Fact]
        public void TaggedStringElementWorks()
        {
            Assert.Equal("<test foo=\"bar\">asd</test>", LbpSerializer.TaggedStringElement("test", "asd", "foo", "bar"));
            Assert.Equal
            (
                "<test foo=\"bar\">asd</test>",
                LbpSerializer.TaggedStringElement(new KeyValuePair<string, object>("test", "asd"), new KeyValuePair<string, object>("foo", "bar"))
            );
        }

        [Fact]
        public void ElementsWorks()
        {
            Assert.Equal
            (
                "<test>asd</test><foo>bar</foo>",
                LbpSerializer.Elements(new KeyValuePair<string, object>("test", "asd"), new KeyValuePair<string, object>("foo", "bar"))
            );
        }
    }
}