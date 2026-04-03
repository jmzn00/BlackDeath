using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class GameSaveData
{
    public List<ActorSaveData> Actors = new List<ActorSaveData>();
    public DialogueSaveData Dialogue;
}
[Serializable]
public class SaveSlotMeta 
{
    public int Slot;
    public string SceneName;
    public string TimeStamp;
    public bool HasData;
}
public class SaveManager : IManager
{
    private bool m_active;

    private ActorManager m_actorManager;
    private DialogueManager m_dialogueManager;

    private int m_currentSlot = -1;

    private int m_maxSlots = 5;

    public SaveManager(ActorManager actorManager, DialogueManager dialogueManager) 
    {
        m_actorManager = actorManager;
        m_dialogueManager = dialogueManager;
    }

    public void Update(float dt) 
    {
        if (!m_active) return;                       
    }
    public bool Init() 
    {
        m_active = true;        

        return true;
    }
    public void OnSceneLoaded(SceneData data) 
    {
        if (!data.IsGameplay) return;

        if (m_currentSlot >= 0) 
        {
            Load(m_currentSlot);
        }
    }
    public void OnManagersInitialzied()
    {

    }
    public bool Dispose() 
    {
        m_active = false;
        return true;
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
            Dialogue = m_dialogueManager.Save()
        };
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(GetSavePath(slot), json);

        WriteMeta(slot, data);
        m_currentSlot = slot;
        Debug.Log($"Saved to slot {slot}");        
    }
    public void Load(int slot) 
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path)) 
        {
            Debug.Log("No save found, starting fresh");
            return;
        }
        
        string json = File.ReadAllText(path);
        GameSaveData save = JsonUtility.FromJson<GameSaveData>(json);

        m_actorManager.LoadAllActors(save.Actors);
        m_dialogueManager.Load(save.Dialogue);

        m_currentSlot = slot;
        Debug.Log($"Loaded from slot {slot}");
    }
}
