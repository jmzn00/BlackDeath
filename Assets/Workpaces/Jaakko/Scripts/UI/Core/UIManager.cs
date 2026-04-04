using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum UIInputMode 
{
    None,
    Navigation,
    Combat
}

public class UIManager : IManager
{
    private GameManager m_game;
    private InputManager m_input;
    private UIController m_uiController;
    public UIController Controller => m_uiController;

    private UIControllerNavigation m_navigation;
    public UIControllerNavigation Navigation => m_navigation;

    private Stack<IUIInputReceiver> m_uiStack = new Stack<IUIInputReceiver>();

    public event Action OnReady;
    public bool IsReady { get; private set; }
    public UIManager(GameManager game)
    {
        m_game = game;
    }
    private void SetReady()
    {
        if (IsReady) return;

        IsReady = true;
        OnReady?.Invoke();
    }
    #region IManager
    public bool Init()
    {
        m_navigation = new UIControllerNavigation(m_game.Resolve<InputManager>());
        m_input = m_game.Resolve<InputManager>();

        m_input.OnUIInputAction += HandleUIInput;
        return true;
    }
    public void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        if (data.IsGameplay) 
        {
            m_uiController.ShowComponent<MainMenuUI>(false);
        }
        SetReady();
    }
    public bool Dispose()
    {
        if (m_uiController)
            m_uiController.Dispose();

        m_game.OnStateChanged -= OnGameStateChanged;
        m_navigation.Dispose();
        return true;
    }
    public void OnManagersInitialzied()
    {
        m_uiController = GameObject.FindFirstObjectByType<UIController>();
        if (m_uiController)
            m_uiController.Initialize(m_game);
        else
            Debug.LogWarning("No UIController Found in Scene");

        m_game.OnStateChanged += OnGameStateChanged;
        OnGameStateChanged(m_game.State);


    }
    #endregion    
    bool HandleUIInput(UIInputAction action) 
    {
        switch (action) 
        {
            case UIInputAction.Menu:
                m_uiController.ShowComponent<MainMenuUI>(true);
                return true;
            default:
                return false;
        }
    }
    private void OnGameStateChanged(GameState state)
    {
        if (m_uiController == null) return;

        switch (state)
        {
            case GameState.None:
                m_uiController.ShowComponent<CombatUI>(false);
                m_uiController.ShowComponent<DialogueUI>(false);
                break;
            case GameState.Combat:
                m_uiController.ShowComponent<CombatUI>(true);
                break;
            case GameState.Dialogue:
                m_uiController.ShowComponent<DialogueUI>(true);
                break;
        }
    }

    public void PushUI(IUIInputReceiver reciever)
    {   
        m_uiStack.Push(reciever);
    }
    public void PopUI(IUIInputReceiver reciever)
    {
        if (m_uiStack.Count > 0 && m_uiStack.Peek() == reciever)
        {
            m_uiStack.Pop();
        }
    }
    private bool m_submitConsumed;
    public void ConsumeSubmit() 
    {
        m_submitConsumed = true;
    }
    public void Update(float dt)
    {
        m_navigation.UpdateNavigation();

        if (m_uiStack.Count > 0)
        {
            var top = m_uiStack.Peek();
            ref UIInputState input = ref m_input.GetUIInputState();

            if (input.NavigateUpPressed) top.OnNavigate(Vector2.up);
            else if (input.NavigateDownPressed) top.OnNavigate(Vector2.down);
            else if (input.NavigateLeftPressed) top.OnNavigate(Vector2.left);
            else if (input.NavigateRightPressed) top.OnNavigate(Vector2.right);

            if (!m_submitConsumed && input.SubmitPressed)
            {
                top.OnSubmit();
                m_submitConsumed = true;
            }
            if (input.SubmitReleased)
                m_submitConsumed = false;
            
            if (input.CancelPressed) top.OnCancel();
        }
    }
}
public class UIControllerNavigation 
{
    private List<Selectable> m_selectables = new();
    private int m_currentIndex = 0;

    private InputManager m_input;
    public UIControllerNavigation(InputManager inputManager) 
    {
        m_input = inputManager;
    }
    public void Dispose() 
    {
        m_selectables.Clear();

    }
    public void UpdateButtons(IEnumerable<Selectable> buttons, GameObject currentSelected)
    {
        m_selectables = new List<Selectable>(buttons);

        if (currentSelected != null && m_selectables.Any(s => s.gameObject == currentSelected))
        {
            EventSystem.current.SetSelectedGameObject(currentSelected);
            m_currentIndex = m_selectables.FindIndex(s => s.gameObject == currentSelected);
        }
        else if (m_selectables.Count > 0)
        {
            m_currentIndex = 0;
            EventSystem.current.SetSelectedGameObject(m_selectables[0].gameObject);
        }
    }
    public void Clear() 
    {
        m_selectables.Clear();
    }
    public void UpdateNavigation() 
    {
        if (m_selectables.Count == 0) 
        {
            return;
        }

        ref UIInputState input = ref m_input.GetUIInputState();

        if (input.NavigateUpPressed) 
        {
            MoveDown();
        }
        else if (input.NavigateDownPressed) 
        {
            MoveUp();
        }
        else if (input.NavigateRightPressed) 
        {
            MoveRight();
        }
        else if (input.NavigateLeftPressed) 
        {
            MoveLeft();
        }
    }
    private void MoveUp() 
    {
        m_currentIndex = (m_currentIndex - 1 + m_selectables.Count) % m_selectables.Count;
        EventSystem.current.SetSelectedGameObject(m_selectables[m_currentIndex].gameObject);
    }
    private void MoveDown()
    {        
        m_currentIndex = (m_currentIndex + 1) % m_selectables.Count;
        EventSystem.current.SetSelectedGameObject(m_selectables[m_currentIndex].gameObject);
    }
    private void MoveRight()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderBy(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x > current.transform.position.x);

        if (next != null) 
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }            
    }

    private void MoveLeft()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderByDescending(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x < current.transform.position.x);

        if (next != null) 
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }        
    }

}