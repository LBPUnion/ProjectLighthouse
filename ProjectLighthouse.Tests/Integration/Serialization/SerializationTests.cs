using System;
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Integration.Serialization;

public class TestSerializable : ILbpSerializable, IHasCustomRoot
{
    public virtual string GetRoot() => "xmlRoot";
}

[Trait("Category", "Integration")]
public class SerializationTests
{
    private static IServiceProvider GetEmptyServiceProvider() => new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection());

    [Fact]
    public void ShouldNotSerializeNullObject()
    {
        TestSerializable? serializable = null;
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.True(serialized == "");
    }

    [Fact]
    public void ShouldSerializeFullEmptyTag()
    {
        TestSerializable serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.True(serialized == "<xmlRoot></xmlRoot>");
    }

    [Fact]
    public void ShouldSerializeWithCustomRoot()
    {
        TestSerializable serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.True(serialized == "<xmlRoot></xmlRoot>");
    }

    public class OverriddenRoot : TestSerializable
    {
        public override string GetRoot() => "xmlRoot2";
    }

    [Fact]
    public void ShouldSerializeWithOverriddenRoot()
    {
        OverriddenRoot serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.True(serialized == "<xmlRoot2></xmlRoot2>");
    }

    [XmlRoot("xmlRoot3")]
    public class RootAttribute : ILbpSerializable { }

    [Fact]
    public void ShouldSerializeWithRootAttribute()
    {
        RootAttribute serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.True(serialized == "<xmlRoot3></xmlRoot3>");
    }

    public class DefaultValueInt : TestSerializable
    {
        [DefaultValue(6)]
        [XmlElement("defaultValueInt")]
        public int DefaultValueTest { get; set; } = 6;
    }

    [Fact]
    public void ShouldNotSerializeDefaultValueInt()
    {
        DefaultValueInt serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot></xmlRoot>");
    }

    public class NonDefaultValueInt : TestSerializable
    {
        [DefaultValue(6)]
        [XmlElement("nonDefaultValueInt")]
        public int NonDefaultValueTest { get; set; }
    }

    [Fact]
    public void ShouldNotSerializeNonDefaultValueInt()
    {
        NonDefaultValueInt serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot><nonDefaultValueInt>0</nonDefaultValueInt></xmlRoot>");
    }

    public class DefaultNullableStringTest : TestSerializable
    {
        [DefaultValue(null)]
        [XmlElement("defaultNullableString")]
        public string? DefaultNullableString { get; set; }
    }

    [Fact]
    public void ShouldNotSerializeDefaultNullableString()
    {
        DefaultNullableStringTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot></xmlRoot>");
    }

    public class NullNullableStringTest : TestSerializable
    {
        [XmlElement("nullableString")]
        public string? NullableString { get; set; }
    }

    [Fact]
    public void ShouldNotSerializeNullNullableString()
    {
        NullNullableStringTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot></xmlRoot>");
    }

    [Fact]
    public void ShouldSerializeNonNullNullableString()
    {
        NullNullableStringTest serializable = new()
        {
            NullableString = "notNull",
        };
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot><nullableString>notNull</nullableString></xmlRoot>");
    }

    public class NonEmptyStringTest : TestSerializable
    {
        [XmlElement("stringTest")]
        public string StringTest { get; set; } = "test";
    }

    [Fact]
    public void ShouldSerializeNonNullableString()
    {
        NonEmptyStringTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot><stringTest>test</stringTest></xmlRoot>");
    }

    [XmlRoot("xmlRoot", Namespace = "test")]
    public class NameSpaceTest : TestSerializable
    {
        [XmlElement("test", Namespace = "test2")]
        public int TestValue { get; set; } = 1;
    }

    [Fact]
    public void ShouldExcludeNamespace()
    {
        NameSpaceTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot><test>1</test></xmlRoot>");
    }

    public class AttributeTest : TestSerializable
    {
        [XmlAttribute("string")]
        public string StringAttribute { get; set; } = "test";
        [XmlAttribute("int")]
        public int NumberAttribute { get; set; } = 5;
    }

    [Fact]
    public void ShouldSerializeAttributes()
    {
        AttributeTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized == "<xmlRoot string=\"test\" int=\"5\"></xmlRoot>");
    }

    public class NestingTest : TestSerializable
    {
        [XmlElement("nested")]
        public NestedType NestedType { get; set; } = new();
    }

    public class NestedType : ILbpSerializable
    {
        [XmlElement("attributeTest")]
        public AttributeTest AttributeTest { get; set; } = new();
        [XmlElement("nestedString")]
        public NonEmptyStringTest NonEmptyString { get; set; } = new();
    }

    [Fact]
    public void ShouldSerializeNestedType()
    {
        NestingTest serializable = new();
        string serialized = LighthouseSerializer.Serialize(GetEmptyServiceProvider(), serializable);
        Assert.False(string.IsNullOrWhiteSpace(serialized));
        Assert.True(serialized ==
                    "<xmlRoot><nested>" +
                    "<attributeTest string=\"test\" int=\"5\"></attributeTest>" +
                    "<nestedString><stringTest>test</stringTest></nestedString>" +
                    "</nested></xmlRoot>");
    }

}