namespace HomeAutomation.apps.Common.Base;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class AreaKeyAttribute(string areaKey) : Attribute
{
    public string AreaKey { get; } = areaKey;
}
