using UnityEngine;
using UnityEngine.UI;

public class NextTurnUI : MonoBehaviour
{
    [SerializeField] private Button m_nextTurnButton;

    private void Start()
    {
        m_nextTurnButton.onClick.AddListener(() =>
        {
            //Services.Get<CombatManager>().NextTurn();
        });
    }
}
