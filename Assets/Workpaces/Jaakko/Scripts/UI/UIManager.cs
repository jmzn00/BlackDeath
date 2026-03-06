using UnityEngine;

public class UIManager : IManager 
{
    private GameManager m_game;
    private UIController m_uiController;
    public UIController Controller => m_uiController;
    public UIManager(GameManager game) 
    {
        m_game = game;
    }
    public bool Init() 
    {        
        return true;
    }
    public bool Dispose()
    {
        return true;
    }
    public void OnManagersInitialzied() 
    {
        m_uiController = GameObject.FindFirstObjectByType<UIController>();
        if (m_uiController)
            m_uiController.Inject(m_game);
        m_game.OnStateChanged += OnGameStateChanged;
        OnGameStateChanged(m_game.State);
    }
    private void OnGameStateChanged(GameState state) 
    {
        switch (state) 
        {
            case GameState.None:
                m_uiController.ShowComponent<CombatUI>(false);
                break;
            case GameState.Combat:
                m_uiController.ShowComponent<CombatUI>(true);
                break;            
        }
    }
    public void Update(float dt) 
    {
    
    }
}
