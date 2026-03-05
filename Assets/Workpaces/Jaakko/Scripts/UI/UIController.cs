using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Actor m_actor;    
    private List<IUIComponent> m_uiComponents = new List<IUIComponent>();

    private HashSet<IUIComponent> m_visibleComponents = new HashSet<IUIComponent>();

    private bool m_initialized = false;

    private InputManager m_inputManager;
    private GameManager m_game;

    public void Inject(GameManager game) 
    {
        m_game = game;
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
    public void Init(Actor actor) 
    {
        m_initialized = true;
        m_actor = actor;

        BuildModules();
        InitializeModules(actor);

        ShowComponent<HealthUI>(true);
        ShowComponent<InventoryUI>(false);
        ShowComponent<CombatUI>(false);
    }

    public void ChangeActor(Actor actor) 
    {
        if (!m_initialized)
            Init(actor);

        m_actor = actor;
        foreach (var comp in m_uiComponents)
            comp.OnActorChanged(actor);
    }
    void BuildModules() 
    {
        var uiTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(UIComponentBase)
            .IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in uiTypes) 
        {
            var attr = type.GetCustomAttribute<UIForAttribute>();
            if (attr == null) continue;

            var actorComponent = m_actor.Get(attr.ActorComponentType);
            if (actorComponent == null) 
            {
                Debug.LogWarning($"Actor component of type {attr.ActorComponentType.Name} not found for UI component {type.Name}");
                continue;
            }

            var view = GetComponentInChildren(attr.ViewType, true);
            if (view == null) 
            {
                Debug.LogWarning($"Could not find view {attr.ViewType.Name} in children");
                continue;
            }

            var instance = Activator.CreateInstance(type, actorComponent, view) as IUIComponent;
            if (instance != null)
                m_uiComponents.Add(instance);
        }
    }
    void InitializeModules(Actor actor) 
    {
        foreach (var comp in m_uiComponents)
            comp.Initialize(actor);        
    }
    private void Start()
    {
        m_inputManager = Services.Get<InputManager>();
        m_inputManager.OnUIInputAction += OnUiInputAction;
    }
    private void OnUiInputAction(UIInputAction action) 
    {
        switch (action) 
        {
            case UIInputAction.Inventory:
                ShowComponent<InventoryUI>(true);
                break;
        }
    }
}
