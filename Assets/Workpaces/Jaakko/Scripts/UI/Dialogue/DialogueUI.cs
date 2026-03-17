[UIComponent(typeof(DialogueView))]
public class DialogueUI : UIComponentBase
{
    private DialogueView m_view;
    private DialogueManager m_dialogue;

    public DialogueUI(GameManager game, DialogueView view) : base(game) 
    {
        m_view = view;
        m_dialogue = game.Resolve<DialogueManager>();
    }
    public override void Initialize() 
    {
        m_view.Initialize(m_dialogue);
        m_view.Init();        
    }    
    public override void Dispose()
    {

    }
    public override bool IsVisible()
    {
        return m_view.gameObject.activeInHierarchy;
    }
    public override void Toggle(bool toggle) 
    {
        if (toggle)
            m_view.View();
        else 
            m_view.Hide();
    }
}
