using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionComponent : MonoBehaviour, IActorComponent
{
    private Actor m_actor;
    private CameraManager m_cameraManager;
    private IInputSource m_inputSource;
    public void SetInputSource(IInputSource source)
    {
        m_inputSource = source;
    }
    public bool Initialize(GameManager game)
    {
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        m_actor = actor;
        
    }
    public bool Dispose()
    {
        return true;
    }
    public void LoadData(ActorSaveData data)
    {

    }
    public void SaveData(ActorSaveData data)
    {

    }
    [SerializeField] private LayerMask m_actorLayerMask;
    [SerializeField] private float m_interactionDistance = 5f;
    private void Update()
    {
        if (m_actor == null) return;

        InputState state = m_inputSource.GetInput();
        if (state.InteractPressedThisFrame) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, m_actorLayerMask)) 
            {
                Actor target = hit.collider.GetComponentInParent<Actor>();

                if (target != null && target != m_actor) 
                {
                    DialogueActor dialogueActor = target.Get<DialogueActor>();
                    if (dialogueActor != null) 
                    {
                        dialogueActor.StartDialogue(m_actor.Get<DialogueActor>()
                            , dialogueActor);
                    }
                }
            }
        }

        
    }
}
