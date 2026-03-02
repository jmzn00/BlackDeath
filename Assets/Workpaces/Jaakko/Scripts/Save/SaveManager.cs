using System;
using System.IO;
using UnityEngine;
[Serializable]
public class SaveData
{
    public Vector3 PlayerPosition;
    public string[] InventoryItems;
    public int QuestProgress;
}
public class SaveManager : IManager
{
    private bool m_active;
    private string m_savePath;
    private SaveData m_currentSave;
    private Player m_player;

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
    public void Save() 
    {
        Player player = Services.Get<Player>();
        if (player == null) 
        {
            Debug.LogError("Player is NULL");
            return;
        }
        m_currentSave = new SaveData()
        {
            PlayerPosition = player.transform.position
        };
        try 
        {
            string json = JsonUtility.ToJson(m_currentSave, true);
            File.WriteAllText(m_savePath, json);
            Debug.Log($"Game Saved to {m_savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    public void Load() 
    {
        if (!File.Exists(m_savePath)) 
        {
            Debug.Log("No save found, starting fresh");
            return;
        }

        try 
        {
            string json = File.ReadAllText(m_savePath);
            m_currentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Game loaded from {m_savePath}");            
        }
        catch (Exception e) 
        {
            Debug.Log($"Failed to load save: {e.Message}");
        }
        Services.Get<Player>().Load(m_currentSave);
    }
}
