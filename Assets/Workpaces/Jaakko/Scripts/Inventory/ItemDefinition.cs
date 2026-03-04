using UnityEngine;
public interface IItemAction 
{
    void Execute(Actor actor, ItemDefinition itemDef);
    string ActionName { get; }
    bool CanExecute(Actor actor, ItemDefinition itemDef);
}
public interface IEquippable
{
    void OnEquip(Actor actor);
    void OnUnequip(Actor actor);
}
public abstract class ItemActionSO : ScriptableObject, IItemAction 
{
    public abstract string ActionName { get; }

    public abstract void Execute(Actor actor, ItemDefinition itemDef);
    public abstract bool CanExecute(Actor actor, ItemDefinition itemDef);    
}
public enum ItemType 
{
    Default,
    Consumeable,
    Weapon,
    Armor,
    Quest
}
public enum EquipmentSlot 
{
    Weapon,
    Gloves,
    Gem,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string ItemID;
    public string DisplayName;
    public Sprite Icon;
    public ItemType Type;

    [TextArea] public string Description;
    public int MaxStack = 1;

    public IItemAction[] Actions;


}
