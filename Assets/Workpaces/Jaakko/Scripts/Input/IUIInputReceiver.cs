using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IUIInputReceiver 
{
    bool OnSubmit();
    bool OnCancel();
    bool OnNavigate(Vector2 direction);

    List<Selectable> GetSelectables();
}