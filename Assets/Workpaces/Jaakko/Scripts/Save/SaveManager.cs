using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public List<ActorSaveData> Actors = new List<ActorSaveData>();
    public DialogueSaveData Dialogue;
    public CombatSaveData Combat;
}
[Serializable]
public class SaveSlotMeta 
{
    public int Slot;
    public string SceneName;
    public string TimeStamp;
    public bool HasData;
}
public class SaveManager : ManagerBase
{
    private ActorManager m_actorManager;
    private DialogueManager m_dialogueManager;
    private CombatManager m_combatManager;

    private GameSaveData m_loadedSave;

    private int m_currentSlot = -1;
    private int m_maxSlots = 5;

    public SaveManager(ActorManager actorManager, DialogueManager dialogueManager, CombatManager combatManager) 
    {
        m_actorManager = actorManager;
        m_dialogueManager = dialogueManager;
        m_combatManager = combatManager;
    }
    public override void OnSceneLoaded(SceneData data)
    {
        IsReady = false;

        if (m_currentSlot >= 0 && data.IsGameplay)
        {
            Load(m_currentSlot);
        }

        SetReady();
    }   
    public void SetCurrentSlot(int slot) 
    {
        m_currentSlot = slot;
    }
    private string GetSavePath(int slot) 
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }
    private void WriteMeta(int slot, SceneData data) 
    {
        var meta = new SaveSlotMeta()
        {
            Slot = slot,
            SceneName = data.Name,
            TimeStamp = DateTime.Now.ToString(),
            HasData = true
        };

        string json = JsonUtility.ToJson(meta, true);
        File.WriteAllText(GetMetaPath(slot), json);
    }
    private string GetMetaPath(int slot) 
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.meta.json");
    }
    public List<SaveSlotMeta> GetAllSlots() 
    {
        var slots = new List<SaveSlotMeta>();

        for (int i = 0; i < m_maxSlots; i++) 
        {
            string path = GetMetaPath(i);

            if (File.Exists(path)) 
            {
                string json = File.ReadAllText(path);
                slots.Add(JsonUtility.FromJson<SaveSlotMeta>(json));
            }
            else 
            {
                slots.Add(new SaveSlotMeta()
                {
                    Slot = i,
                    HasData = false
                });
            }
        }
        return slots;
    }
    public void Save(int slot, SceneData data) 
    {
        var save = new GameSaveData()
        {
            Actors = m_actorManager.SaveAllActors(),
            Dialogue = m_dialogueManager.Save(),
            Combat = m_combatManager.Save()
        };
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(GetSavePath(slot), json);

        WriteMeta(slot, data);
        m_currentSlot = slot;
    }
    public void Load(int slot   ) 
    {
        m_currentSlot = slot;
        string path = GetSavePath(slot);

        if (!File.Exists(path)) 
        {
            m_loadedSave = new GameSaveData();
        }
        else 
        {
            string json = File.ReadAllText(path);
            m_loadedSave = JsonUtility.FromJson<GameSaveData>(json);
        }

        m_currentSlot = slot;
    }
    public void RestoreAfterSceneLoad() 
    {
        if (m_loadedSave == null) return;

        m_actorManager.LoadAllActors(m_loadedSave.Actors);
        m_dialogueManager.Load(m_loadedSave.Dialogue);
        m_combatManager.Load(m_loadedSave.Combat);
    }
}
