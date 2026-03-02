using UnityEngine;
using UnityEngine.UI;

public class SaveUI : MonoBehaviour
{
    [SerializeField] private Button m_saveButton;
    [SerializeField] private Button m_loadButton;

    private SaveManager m_saveManager;
    private void Start()
    {
        m_saveManager = Services.Get<SaveManager>();

        m_saveButton.onClick.AddListener(() =>
        {
            m_saveManager.Save();    
        });
        m_loadButton.onClick.AddListener(() =>
        {
            m_saveManager.Load();
        });
    }
}
