using System;

public class MainMenuGroup : UIViewGroup
{
    public override Type ComponentType => typeof(MainMenuUI);

    public LoadView LoadView;
}
