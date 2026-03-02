using UnityEngine;

public class Player : MonoBehaviour
{
    private void Awake()
    {
        Services.Register(this);
    }
    public void Load(SaveData saveData) 
    {
        transform.position = saveData.PlayerPosition;
    }


}
