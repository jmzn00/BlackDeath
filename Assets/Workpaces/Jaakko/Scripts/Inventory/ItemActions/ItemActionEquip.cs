using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemActions/Equippable")]
public class ItemActionEquip : ItemActionSO, IEquippable
{
    public override string ActionName => "Equip";
    public void OnEquip(Actor actor) 
    {
    
    }
    public void OnUnequip(Actor actor) 
    {

    }
    public override void Execute(Actor actor, ItemDefinition itemDef)
    {
        throw new System.NotImplementedException();
    }
    public override bool CanExecute(Actor actor, ItemDefinition itemDef)
    {
        return true;
    }
}
