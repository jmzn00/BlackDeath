using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Game : MonoBehaviour
{
    private GameManager m_gameManager;
    private static Game m_instance;
    private void Awake()
    {
        if (m_instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        m_instance = this;
        DontDestroyOnLoad(this);

        m_gameManager = new GameManager();
        m_gameManager.Init();
    }
    private void Update()
    {
        m_gameManager.Update(Time.deltaTime);
    }
}
