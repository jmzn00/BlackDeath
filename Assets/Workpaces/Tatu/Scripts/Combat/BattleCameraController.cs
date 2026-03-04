using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] CinemachineFollowZoom zoom;
    [SerializeField] CinemachineFollow follow;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        zoom = cam.GetComponent<CinemachineFollowZoom>();
        follow = cam.GetComponent<CinemachineFollow>();
    }

    public void SetZoom(Vector2 zoomLevel, GameObject target, float followOffset)
    {
        var currentZoom = zoom.FovRange;
        var targetZoom = zoomLevel;

        cam.Follow = target.transform;
        follow.FollowOffset = new Vector3(followOffset, 5, -10);

        StartCoroutine(ZoomToTarget(currentZoom, targetZoom));
    }

    private IEnumerator ZoomToTarget(Vector2 currentZoom, Vector2 targetZoom)
    {
        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            zoom.FovRange = Vector2.Lerp(currentZoom, targetZoom, t);
            yield return null;
        }
        zoom.FovRange = targetZoom;
    }
}
