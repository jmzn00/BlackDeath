using UnityEngine;
using UnityEngine.UI;

public class ActorSwitch : MonoBehaviour
{
    [SerializeField] private Button m_switchActorButton;

    private void Start()
    {
        m_switchActorButton.onClick.AddListener(() =>
        {
            Services.Get<ActorManager>().SwitchToNextActor();
        });
    }
}
