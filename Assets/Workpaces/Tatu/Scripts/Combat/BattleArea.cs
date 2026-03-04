using UnityEngine;
using UnityEngine.Events;

public class BattleArea : MonoBehaviour
{
    [Header("Battle")]
    public UnityEvent onPlayerEnter;

    // list of enemies to spawn in the battle area
    [SerializeField] public GameObject[] enemiesToSpawn;
    // spawn points for the enemies and the player
    [SerializeField] public Transform[] enemySpawnPoints;
    [SerializeField] public Transform playerSpawnPoint;

    [Header("Optional")]
    [SerializeField] private BattleManager battleManager;

    private void Reset()
    {
        // Try to auto-find BattleManager in scene
        if (battleManager == null)
            battleManager = FindAnyObjectByType<BattleManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayerEnter.Invoke();
            if (battleManager != null)
            {
                battleManager.StartBattle(this);
            }
        }
    }
}