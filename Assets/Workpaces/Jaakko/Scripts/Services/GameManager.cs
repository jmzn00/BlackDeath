using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private readonly List<IManager> m_managers = new();

    private InputManager Input;

    // There Should Be Only One GameManager
    // GameManager Should Be Created In A
    // Bootstrap scene before anything else

    private void Awake()
    {
        DontDestroyOnLoad(this);

        CreateManagers();
        InitManagers();
    }
    private void OnDestroy()
    {
        DisposeManagers();
        Services.Clear();
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < m_managers.Count; i++) 
            m_managers[i].Update(dt);
    }

    private void CreateManagers() 
    {
        Input = new InputManager();
        m_managers.Add(Input);

        Services.Register(Input);
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
