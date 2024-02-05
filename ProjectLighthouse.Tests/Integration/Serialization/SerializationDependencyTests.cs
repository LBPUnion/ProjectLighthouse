using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Integration.Serialization;

[Trait("Category", "Integration")]
public class SerializationDependencyTests
{
    private static IServiceProvider GetTestServiceProvider(params object[] dependencies)
    {
        ServiceCollection collection = new();
        foreach (object o in dependencies)
        {
            ServiceDescriptor descriptor = new(o.GetType(), o);
            collection.Add(descriptor);
        }
        return new DefaultServiceProviderFactory().CreateServiceProvider(collection);
    }

    public class TestDependency
    {
        public TestDependency(string secret)
        {
            this.Secret = secret;
        }
        public string Secret { get; }
    }

    public class DependencyTest : TestSerializable, INeedsPreparationForSerialization
    {
        [DefaultValue("")]
        [XmlElement("secret")]
        public string Secret { get; set; } = "";

        public void PrepareSerialization(TestDependency testDependency)
        {
            this.Secret = testDependency.Secret;
        }
    }

    [Fact]
    public void ShouldInjectDependency()
    {
        DependencyTest serializable = new();
        TestDependency testDependency = new("bruh");
        string serialized = LighthouseSerializer.Serialize(GetTestServiceProvider(testDependency), serializable);
        Assert.True(serialized == "<xmlRoot><secret>bruh</secret></xmlRoot>");
    }

    public class RecursiveDependencyTest : TestSerializable
    {
        [XmlElement("dependency")]
        public DependencyTest Dependency { get; set; } = new();
    }

    [Fact]
    public void ShouldInjectDependencyIntoNestedClass()
    {
        RecursiveDependencyTest serializable = new();
        TestDependency testDependency = new("bruh");
        string serialized = LighthouseSerializer.Serialize(GetTestServiceProvider(testDependency), serializable);
        Assert.True(serialized == "<xmlRoot><dependency><secret>bruh</secret></dependency></xmlRoot>");
    }

    public class RecursiveDependencyTestWithPreparation : TestSerializable, INeedsPreparationForSerialization
    {
        [XmlElement("dependency")]
        public DependencyTest Dependency { get; set; } = new();

        [XmlElement("prepared")]
        public string PreparedField { get; set; } = "";

        public void PrepareSerialization()
        {
            this.PreparedField = "test";
        }
    }

    public class DependencyOrderTest : TestSerializable, INeedsPreparationForSerialization
    {
        [XmlElement("dependency")]
        public DependencyTest Dependency { get; set; } = null!;

        [XmlElement("nestedDependency")]
        public RecursiveDependencyTestWithPreparation RecursiveDependency { get; set; } = null!;

        public void PrepareSerialization(TestDependency testDependency)
        {
            this.Dependency = new DependencyTest();
            this.RecursiveDependency = new RecursiveDependencyTestWithPreparation();
        }
    }

    [Fact]
    public void ShouldDependenciesBePreparedInOrder()
    {
        DependencyOrderTest serializable = new();
        TestDependency testDependency = new("bruh");
        string serialized = LighthouseSerializer.Serialize(GetTestServiceProvider(testDependency), serializable);
        Assert.True(serialized ==
                    "<xmlRoot>" +
                    "<dependency><secret>bruh</secret></dependency>" +
                    "<nestedDependency><dependency><secret>bruh</secret></dependency><prepared>test</prepared></nestedDependency>" +
                    "</xmlRoot>");
    }

    public class CounterDependency
    {
        private int counter;
        public int Counter => this.counter++;

        public CounterDependency()
        {
            this.counter = 0;
        }

    }

    public class ListDependencyItem : ILbpSerializable, INeedsPreparationForSerialization
    {
        [XmlElement("counter")]
        public int Counter { get; set; } = -1;

        public void PrepareSerialization(CounterDependency counter)
        {
            this.Counter = counter.Counter;
        }
    }

    public class ListDependencyTest : TestSerializable
    {
        [XmlElement("item")]
        public List<ListDependencyItem> Items { get; set; } = new();
    }

    [Fact]
    public void ShouldPrepareItemsInList()
    {
        ListDependencyTest serializable = new()
        {
            Items = new List<ListDependencyItem>
            {
                new(),
                new(),
                new(),
            },
        };
        CounterDependency counterDependency = new();
        string serialized = LighthouseSerializer.Serialize(GetTestServiceProvider(counterDependency), serializable);
        Assert.True(serialized == "<xmlRoot><item><counter>0</counter></item><item><counter>1</counter></item><item><counter>2</counter></item></xmlRoot>");
    }

}