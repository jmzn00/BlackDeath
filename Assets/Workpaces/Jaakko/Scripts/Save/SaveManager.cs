using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class SaveData
{
    public Vector3 PlayerPosition;
    public string[] InventoryItems;
    public int QuestProgress;
}
[Serializable]
public class GameSaveData
{
    public List<ActorSaveData> Actors = new List<ActorSaveData>();
}
public class SaveManager : IManager
{
    private bool m_active;
    private string m_savePath;
    private SaveData m_currentSave;

    public void Update(float dt) 
    {
        if (!m_active) return;                       
    }
    public bool Init(GameManager gameManager) 
    {
        m_savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        m_currentSave = new SaveData();
        m_active = true;

        return m_active;
    }
    public bool Dispose(GameManager manager) 
    {
        m_active = false;
        return m_active;
    }
    public SaveData GetSave() 
    {
        return m_currentSave;
    }
    public void Save() 
    {
        var actorManager = Services.Get<ActorManager>();
        GameSaveData save = new GameSaveData()
        {
            Actors = actorManager.SaveAllActors()
        };
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(m_savePath, json);

        Debug.Log($"Game Saved To: {m_savePath}");
    }
    public void Load() 
    {
        if (!File.Exists(m_savePath)) 
        {
            Debug.Log("No save found, starting fresh");
            return;
        }
        
        string json = File.ReadAllText(m_savePath);
        GameSaveData save = JsonUtility.FromJson<GameSaveData>(json);

        var actorManager = Services.Get<ActorManager>();
        actorManager.LoadAllActors(save.Actors);

        Debug.Log($"Game Loaded From: {m_savePath}");
    }
}
