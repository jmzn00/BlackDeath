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

    private ActorManager m_actorManager;

    public SaveManager(ActorManager actorManager) 
    {
        m_actorManager = actorManager;
    }

    public void Update(float dt) 
    {
        if (!m_active) return;                       
    }
    public bool Init() 
    {
        //m_game = gameManager;
        m_savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        m_currentSave = new SaveData();
        m_active = true;        

        return true;
    }
    public void OnManagersInitialzied()
    {

    }
    public bool Dispose() 
    {
        m_active = false;
        return true;
    }
    public SaveData GetSave() 
    {
        return m_currentSave;
    }
    public void Save() 
    {       
        GameSaveData save = new GameSaveData()
        {
            Actors = m_actorManager.SaveAllActors()
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
        Debug.Log($"Game Loaded From: {m_savePath}");

        string json = File.ReadAllText(m_savePath);
        GameSaveData save = JsonUtility.FromJson<GameSaveData>(json);

        m_actorManager.LoadAllActors(save.Actors);
    }
}
