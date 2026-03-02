using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public static InputManager InputManager { get; internal set; }
    private readonly List<IManager> m_managers = new();

    public InputManager Input { get; private set; }

    private void Awake()
    {
        if (I != null) return;

        I = this;
        CreateManagers();
        InitManagers();
    }
    private void OnDisable()
    {
        DisposeManagers();
    }
    private void OnDestroy()
    {
        if (I != this) return;

        I = null;        
    }

    private void CreateManagers() 
    {
        Input = new InputManager();
        m_managers.Add(Input);
    }
    private void InitManagers() 
    {
        foreach (var m in m_managers)
            m.Init(this);
    }
    private void DisposeManagers() 
    {
        foreach (var m in m_managers)
            m.Dispose(this);
    }
}
