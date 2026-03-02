using UnityEngine;

public class SaveManager : IManager
{
    public void Update(float dt) 
    {
    
    }
    public bool Init(GameManager gameManager) 
    {
        return true;
    }
    public bool Dispose(GameManager manager) 
    {
        return true;
    }
}
