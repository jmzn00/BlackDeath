using Unity.VisualScripting;

public class TutorialUI : UIComponentBase<TutorialGroup> 
{
    private TutorialView m_tutorial;
    public TutorialUI(GameManager game, TutorialGroup group) : base(game, group)
    {
        m_tutorial = group.Get<TutorialView>();
    }
    public override void Toggle(bool show)
    {
        base.Toggle(show);

        if (show)
            m_group.ViewAll();
        else
            m_group.HideAll();
    }

    public override bool OnSubmit()
    {
        Toggle(false);
        return true;
    }
}