namespace TssUtils.Code.Classes;

[AttributeUsage(AttributeTargets.Field)]
public class EnumWeightAttribute(ushort weight) : Attribute
{
    public ushort Weight { get; } = weight;
}
