using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private Transform m_firstPlaceAnchor;
    [SerializeField] private Transform m_portraitAnchor;
    [SerializeField] private TMP_Text m_resultText;

    [Header("Prefabs")]
    [SerializeField] private StatPortrait m_firstPlacePortraitPrefab;
    [SerializeField] private StatPortrait m_statPortraitPrefab;    
    public void Hide() { gameObject.SetActive(false); }
    public void View() { gameObject.SetActive(true); }

    private List<StatPortrait> m_portraits = new();
    public void Init() 
    {
        
    }
    public void DisplayResults(List<CombatActorStats> stats,
        CombatResult result) 
    {
        m_resultText.text = result.ToString();

        for (int i = 0; i < stats.Count; i++) 
        {
            if (i == 0) 
            {
                StatPortrait first = Instantiate(m_firstPlacePortraitPrefab
                    , m_firstPlaceAnchor);
                first.Bind(stats[i]);
                m_portraits.Add(first);
            }
            else if (i < 5) 
            {
                StatPortrait portrait = Instantiate(m_statPortraitPrefab, m_portraitAnchor);
                portrait.Bind(stats[i]);
                m_portraits.Add(portrait);
            }
        }
    }
    public void ClearPortraits() 
    {
        foreach (var portrait in m_portraits)
        {
            Destroy(portrait.gameObject);
        }
        m_portraits.Clear();
    }
}
