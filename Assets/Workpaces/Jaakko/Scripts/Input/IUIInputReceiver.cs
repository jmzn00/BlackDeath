using UnityEngine;

public interface IUIInputReceiver 
{
    bool OnSubmit();
    bool OnCancel();
    bool OnNavigate(Vector2 direction);
}