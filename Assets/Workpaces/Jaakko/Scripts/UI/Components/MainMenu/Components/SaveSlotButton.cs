using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private Button m_button;
    [SerializeField] private Image m_snapshot; 
    [SerializeField] private TMP_Text m_sceneText;
    [SerializeField] private TMP_Text m_timeText;

    public event Action<SaveSlotMeta, int> OnPressed;

    public void Bind(SaveSlotMeta meta, int index) 
    {
        if (meta.HasData) 
        {
            m_sceneText.SetText(meta.SceneName);
            m_timeText.SetText(meta.TimeStamp);
        }
        else // empty save slot
        {
            m_sceneText.SetText("NEW SAVE");
            m_timeText.SetText("NEW SAVE");

            meta.SceneName = "Scene_Gameplay";
            meta.HasData = true;
        }
        m_button.onClick.AddListener(() =>
        {
            OnPressed?.Invoke(meta, index);
        });
    }
    private void OnDestroy()
    {
        m_button.onClick.RemoveAllListeners();
    }
}
