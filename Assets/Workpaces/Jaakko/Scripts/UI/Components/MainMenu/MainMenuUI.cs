using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : UIComponentBase<MainMenuGroup> 
{
    private SaveManager m_save;
    private LoadView m_loadView;

    private List<SaveSlotButton> m_slots;
    public MainMenuUI (GameManager game, MainMenuGroup group) : base (game, group) 
    {
        m_save = m_game.Resolve<SaveManager> ();
        m_loadView = group.LoadView;
    }    
    public override void Initialize() 
    {
        m_slots = new();
        m_loadView.Init();

        List<SaveSlotMeta> slots = m_save.GetAllSlots();
        for (int i = 0; i < slots.Count; i++) 
        {
            SaveSlotMeta meta = slots[i];

            SaveSlotButton s = m_loadView.CreateSlot(meta);
            s.Bind(meta);
            s.OnPressed += Load;

            m_slots.Add(s);
        }
    }
    public override void Dispose() 
    {
        foreach (var s in m_slots) 
        {
            GameObject.Destroy(s.gameObject);
        }
    }
    public override bool IsVisible()
    {
        return false;    
    }
    public override void Toggle(bool show)
    {
        
    }
    private void Load(SaveSlotMeta meta) 
    {
        if (meta.HasData) 
        {
            m_game.LoadGame(meta);
        }        
    }
}