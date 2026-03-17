using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private List<IUIComponent> m_uiComponents = new List<IUIComponent>();

    private HashSet<IUIComponent> m_visibleComponents = new HashSet<IUIComponent>();


    private InputManager m_inputManager;
    private GameManager m_game;

    public void Initialize(GameManager game) 
    {
        m_game = game;
        m_inputManager = m_game.Resolve<InputManager>();        

        Actor actor = m_game.Resolve<ActorManager>().CurrentControlled;

        m_inputManager.OnUIInputAction += OnUiInputAction;

        BuildModules();
        InitializeModules();

        ShowComponent<InventoryUI>(false);
        ShowComponent<DialogueUI>(false);
    }   
    public void Dispose() 
    {
        m_inputManager.OnUIInputAction -= OnUiInputAction;
    }
    public void ShowComponent<T>(bool show) where T : IUIComponent
    {
        var comp = m_uiComponents.OfType<T>().FirstOrDefault();
        if (comp != null) 
        {
            comp.Toggle(show);
            if (show) m_visibleComponents.Add(comp);
            else m_visibleComponents.Remove(comp);
        }
    }  
    public bool IsVisibe<T>() where T : IUIComponent 
    {
        var comp = m_uiComponents.OfType<T>().FirstOrDefault();
        if (comp != null) 
        {
            return comp.IsVisible();
        }
        Debug.LogWarning("Couldnt Find Component");
        return false;
    }
    void BuildModules() 
    {
        var uiTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(UIComponentBase).IsAssignableFrom(t)
            && !t.IsAbstract);

        foreach (var type in uiTypes) 
        {
            var attr = type.GetCustomAttribute<UIComponentAttribute>();
            if (attr == null) continue;

            var view = FindFirstObjectByType(attr.ViewType);
            if (view == null) 
            {
                Debug.LogWarning($"View {attr.ViewType.Name} not found");
                continue;
            }
            var instance = Activator.CreateInstance(type, m_game, view) as IUIComponent;

            if (instance != null)
                m_uiComponents.Add(instance);
        }
    }
    void InitializeModules() 
    {
        foreach (var comp in m_uiComponents)
            comp.Initialize();        
    }
    private void OnUiInputAction(UIInputAction action) 
    {
        switch (action) 
        {
            case UIInputAction.Inventory:
                ShowComponent<InventoryUI>(!IsVisibe<InventoryUI>());
                break;
        }
    }
}
