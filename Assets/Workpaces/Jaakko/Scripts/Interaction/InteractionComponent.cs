using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionComponent : MonoBehaviour, IActorComponent
{
    [SerializeField] private LayerMask m_actorLayerMask;

    private Actor m_actor;
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
    private void Update()
    {
        if (m_actor == null) return;

        InputState state = m_inputSource.GetInput();
        if (state.InteractPressedThisFrame) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, m_actorLayerMask)) 
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable == null) return;

                interactable.InteractEnter(m_actor);
            }
        }

        
    }
}
