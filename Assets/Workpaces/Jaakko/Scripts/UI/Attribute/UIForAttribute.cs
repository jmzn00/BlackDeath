using System;

[AttributeUsage(AttributeTargets.Class)]
public class UIForAttribute : Attribute
{
    public Type ActorComponentType { get; }
    public Type ViewType;

    public UIForAttribute(Type actorComponentType, Type viewType)
    {
        ActorComponentType = actorComponentType;
        ViewType = viewType;
    }
}
