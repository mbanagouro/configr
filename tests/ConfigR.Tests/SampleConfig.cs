namespace ConfigR.Tests;

public sealed class SampleConfig
{
    public int IntValue { get; set; } = 321;
    public short ShortValue { get; set; } = 3;
    public long LongValue { get; set; } = 9876543210;
    public string? Name { get; set; } = "FromSqlServer";
    public DateTime CreatedAt { get; set; } = new DateTime(2024, 10, 1);
    public bool IsEnabled { get; set; } = false;
    public List<string> Tags { get; set; } = new() { "sql", "integration" };
    public NestedData Details { get; set; } = new NestedData { Level = 9, Description = "From SQL" };
}

public sealed class NestedData
{
    public int Level { get; set; }
    public string? Description { get; set; }
}
