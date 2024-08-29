namespace ShardingCore.MultiField.Demo;

public class Player
{
    public long Id { get; set; }
    public required string AppCode { get; set; }
    public required string GroupCode { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }

    public string SplitTableKey => $"{AppCode}_{GroupCode}";
}