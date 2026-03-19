using System;

[AttributeUsage(AttributeTargets.Class)]
public class UIComponentAttribute : Attribute
{
    public Type ViewType { get; }
    public Type[] ViewTypes { get; }
    public UIComponentAttribute(params Type[] viewTypes) 
    {
        ViewTypes = viewTypes;
    }
}
