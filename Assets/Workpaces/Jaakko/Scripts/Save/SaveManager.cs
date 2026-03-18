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
public class SaveManager : IManager
{
    private bool m_active;
    private string m_savePath;

    private ActorManager m_actorManager;
    private DialogueManager m_dialogueManager;

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
        //m_game = gameManager;
        m_savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
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
    public void Save() 
    {
        GameSaveData save = new GameSaveData()
        {
            Actors = m_actorManager.SaveAllActors(),
            Dialogue = m_dialogueManager.Save()
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
        m_dialogueManager.Load(save.Dialogue);
    }
}
