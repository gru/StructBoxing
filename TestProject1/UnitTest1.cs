using JetBrains.dotMemoryUnit;
using Xunit.Abstractions;

[assembly: EnableDotMemoryUnitSupport]

namespace TestProject1;

public class UnitTest1
{
    public UnitTest1(ITestOutputHelper output)
    {
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }
    
    [Fact]
    [DotMemoryUnit(CollectAllocations = true)]
    public void StructMemTest()
    {
        const int objectCount = 1_000_000;
        
        var dictionary = new Dictionary<Struct, string>(objectCount);
        for (var i = 0; i < objectCount; i++)
            dictionary.Add(new Struct(i), string.Empty);
        
        var before = dotMemory.Check();

        for (var i = 0; i < objectCount; i++)
            Assert.True(dictionary.ContainsKey(new Struct(i)));

        dotMemory.Check(memory =>
            Assert.True(memory.GetTrafficFrom(before).AllocatedMemory.ObjectsCount > objectCount));
    }
    
    [Fact]
    [DotMemoryUnit(CollectAllocations = true)]
    public void EquatableStructMemTest()
    {
        const int objectCount = 1_000_000;
        
        var dictionary = new Dictionary<EquatableStruct, string>(objectCount);
        for (var i = 0; i < objectCount; i++)
            dictionary.Add(new EquatableStruct(i), string.Empty);
        
        var before = dotMemory.Check();

        for (var i = 0; i < objectCount; i++)
            Assert.True(dictionary.ContainsKey(new EquatableStruct(i)));

        dotMemory.Check(memory =>
            Assert.True(memory.GetTrafficFrom(before).AllocatedMemory.ObjectsCount < 200));
    }
}

internal readonly struct Struct
{
    public Struct(int value)
    {
        Value = value;
    }

    public int Value { get; }
}

internal readonly struct EquatableStruct : IEquatable<EquatableStruct>
{
    public EquatableStruct(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public bool Equals(EquatableStruct other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value;
    }
}