using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : IManager 
{
    private GameManager m_game;
    private UIController m_uiController;
    public UIController Controller => m_uiController;
    private UIControllerNavigation m_navigation;
    public UIControllerNavigation Navigation => m_navigation;
    public UIManager(GameManager game) 
    {
        m_game = game;
    }
    public bool Init() 
    {
        m_navigation = new UIControllerNavigation(m_game.Resolve<InputManager>());
        return true;
    }
    public bool Dispose()
    {
        if (m_uiController)
            m_uiController.Dispose();

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
    private void OnGameStateChanged(GameState state) 
    {
        if (m_uiController == null) return;

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
        m_navigation.UpdateNavigation();
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
    public void RegisterButtons(IEnumerable<Selectable> buttons) 
    {
        m_selectables = new List<Selectable>(buttons);
        m_currentIndex = 0;

        if (m_selectables.Count > 0) 
        {
            EventSystem.current
                .SetSelectedGameObject(m_selectables[0].gameObject);
        }
    }
    public void UpdateButtons(IEnumerable<Selectable> buttons, GameObject currentSelected)
    {
        m_selectables = new List<Selectable>(buttons);

        // Try to keep selection
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
        if (m_selectables.Count == 0) return;

        ref UIInputState input = ref m_input.GetUIInputState();

        if (input.NavigateUpPressed) 
        {
            MoveUp();
        }
        else if (input.NavigateDownPressed) 
        {
            MoveDown();
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
        //Debug.Log($"Selected: {EventSystem.current.currentSelectedGameObject.name}");
    }
    private void MoveDown()
    {        
        m_currentIndex = (m_currentIndex + 1) % m_selectables.Count;
        EventSystem.current.SetSelectedGameObject(m_selectables[m_currentIndex].gameObject);
        //Debug.Log($"Selected: {EventSystem.current.currentSelectedGameObject.name}");
    }
    private void MoveRight()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        // Find next selectable to the right based on position
        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderBy(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x > current.transform.position.x);

        if (next != null) 
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
            //Debug.Log($"Selected: {EventSystem.current.currentSelectedGameObject.name}");
        }
            
    }

    private void MoveLeft()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        // Find next selectable to the left based on position
        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderByDescending(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x < current.transform.position.x);

        if (next != null) 
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
            //Debug.Log($"Selected: {EventSystem.current.currentSelectedGameObject.name}");
        }
        
    }

}
