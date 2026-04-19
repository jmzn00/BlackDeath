using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIInputMode 
{
    None,
    Navigation,
    Combat
}

public class UIManager : ManagerBase
{
    private GameManager m_game;
    private InputManager m_input;
    private UIController m_uiController;

    private UIControllerNavigation m_navigation;
    public UIControllerNavigation Navigation => m_navigation;

    private Stack<IUIInputReceiver> m_uiStack = new Stack<IUIInputReceiver>();
    private IUIInputReceiver m_currentReceiver;

    private bool m_submitConsumed;
    public UIManager(GameManager game)
    {
        m_game = game;
    }
    #region IManager
    public override bool Init()
    {        
        m_input = m_game.Resolve<InputManager>();
        m_navigation = new UIControllerNavigation(m_input);

        m_input.OnUIInputAction += HandleUIInput;

        return true;
    }
    public override void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        m_uiController.SceneChanged(data);
        SetReady();
    }
    public override bool Dispose()
    {
        if (m_uiController)
            m_uiController.Dispose();

        m_game.OnStateChanged -= OnGameStateChanged;
        m_navigation.Dispose();
        return true;
    }
    public override void OnManagersInitialzied()
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
                if (m_game.State == GameState.Combat) return true;

                m_uiController.ShowComponent<MainMenuUI>(true);
                return true;
            default:
                return false;
        }
    }
    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.None:
                ShowComponent<DialogueUI>(false);
                ShowComponent<CombatUI>(false);
                break;
            case GameState.Combat:
                ShowComponent<CombatUI>(true);
                break;
            case GameState.Dialogue:
                ShowComponent<DialogueUI>(true);
                break;
        }
    }
    private IUIComponent previousUIComponent;
    private void ShowComponent<T>(bool show) where T : IUIComponent
    {
        if (previousUIComponent != null)
            m_uiController.ShowComponent<T>(show);

        m_uiController.ShowComponent<T>(show);
    }
    public void PushUI(IUIInputReceiver reciever)
    {
        if (m_currentReceiver == reciever)
            return;

        m_currentReceiver = reciever;
        Debug.Log($"Push UI {reciever}");

        m_uiStack.Push(reciever);        
    }
    public void PopUI(IUIInputReceiver reciever)
    {
        if (m_currentReceiver != reciever)
            return;

        m_currentReceiver = null;
        Debug.Log($"Pop UI {reciever}");

        if (m_uiStack.Count > 0 && m_uiStack.Peek() == reciever)
        {
            m_uiStack.Pop();
        }
    }
    public void ConsumeSubmit() 
    {
        m_submitConsumed = true;
    }
    public override void Update(float dt)
    {
        m_navigation.UpdateNavigation();

        if (m_uiStack.Count > 0)
        {
            
            var top = m_uiStack.Peek();
            ref UIInputState input = ref m_input.GetUIInputState();

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