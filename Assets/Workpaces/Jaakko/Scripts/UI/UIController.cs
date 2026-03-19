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

        //ShowComponent<InventoryUI>(false);
        //ShowComponent<DialogueUI>(false);
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
        var groups = FindObjectsByType<UIViewGroup>(FindObjectsSortMode.None)
            .ToDictionary(g => g.ComponentType, g => g);
        
        var uiTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IUIComponent).IsAssignableFrom(t));

        foreach (var type in uiTypes) 
        {
            if (!groups.TryGetValue(type, out var group)) 
            {
                Debug.LogWarning($"No UIViewGroup found for {type.Name}");
                continue;
            }

            var ctor = type.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 2 &&
                    p[0].ParameterType == typeof(GameManager) &&
                    p[1].ParameterType.IsAssignableFrom(group.GetType());
                });
            if (ctor == null) 
            {
                Debug.LogWarning($"No valid constructor for {type.Name}");
                continue;
            }
            var instance = ctor.Invoke(new object[] { m_game, group }) as IUIComponent;

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
                //ShowComponent<InventoryUI>(!IsVisibe<InventoryUI>());
                break;
        }
    }
}
