using System;

[AttributeUsage(AttributeTargets.Class)]
public class UIComponentAttribute : Attribute
{
    public Type ViewType { get; }
    public UIComponentAttribute(Type viewType) 
    {
        ViewType = viewType;
    }
}
