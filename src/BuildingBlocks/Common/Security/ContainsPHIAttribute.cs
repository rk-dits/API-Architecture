namespace BuildingBlocks.Common.Security;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ContainsPHIAttribute : Attribute
{
    public string? Note { get; }
    public ContainsPHIAttribute(string? note = null) => Note = note;
}
