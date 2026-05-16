using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(100)]
public class CharacterShadow : ActorComponentBase
{
    [SerializeField] private Transform   m_visualRoot;
    [SerializeField] private Transform[] m_footBones;        // optional — auto-detected if left empty
    [SerializeField, Range(0f, 1f)] private float m_shadowOpacity = 0.5f;
    [SerializeField] private float m_groundOffset = 0.01f;
    [SerializeField] private float m_blobScale    = 0.3f;    // world-unit radius of each blob
    [SerializeField] private float m_fadeHeight   = 0.5f;    // foot Y above ground at which blob fully fades
    [SerializeField] private int   m_shadowSortingOrder = -1;

    private struct BlobData { public Transform footBone; public SpriteRenderer blobSR; }
    private readonly List<BlobData> m_blobs = new();
    private Material m_blobMaterial;
    private bool m_initialized;

    // ── IActorComponent ──────────────────────────────────────────────────────

    public override bool Initialize(GameManager game)
    {
        if (m_initialized) return true; // Actor.Init calls GetComponents AND GetComponentsInChildren — guard against double-init

        m_initialized = true;

        if (m_visualRoot == null) { Debug.LogError("[CharacterShadow] visualRoot not assigned.", this); return false; }

        AutoDetectFootBones();
        BuildBlobs();
        return true;
    }

    public override bool Dispose()
    {
        foreach (var b in m_blobs)
            if (b.blobSR != null) Destroy(b.blobSR.gameObject);
        if (m_blobMaterial != null) Destroy(m_blobMaterial);
        m_blobs.Clear();
        m_initialized = false;
        return true;
    }

    // ── Setup ─────────────────────────────────────────────────────────────────

    private void AutoDetectFootBones()
    {
        if (m_footBones != null && m_footBones.Length > 0) return;

        var found = new List<Transform>();

        // 1. Names containing "foot" — catches Lfoot, Rfoot (Basselios)
        foreach (Transform t in m_visualRoot.GetComponentsInChildren<Transform>())
            if (t.name.IndexOf("foot", System.StringComparison.OrdinalIgnoreCase) >= 0)
                found.Add(t);

        if (found.Count > 0) { m_footBones = found.ToArray(); return; }

        // 2. Names containing both "leg" and "effector" — catches LeftLeg_Effector, RightLeg_Effector (Sabbah)
        foreach (Transform t in m_visualRoot.GetComponentsInChildren<Transform>())
            if (t.name.IndexOf("leg",      System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                t.name.IndexOf("effector", System.StringComparison.OrdinalIgnoreCase) >= 0)
                found.Add(t);

        if (found.Count > 0) { m_footBones = found.ToArray(); return; }

        // 3. Fallback: single central blob at Actor root
        Debug.LogWarning("[CharacterShadow] No foot bones detected — using actor root for single blob.", this);
        m_footBones = new[] { transform };
    }

    private void BuildBlobs()
    {
        var tex    = CreateBlobTexture(64);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);

        // Copy the character's sprite material so we stay on the correct URP pipeline.
        var srcSR = m_visualRoot.GetComponentInChildren<SpriteRenderer>();
        m_blobMaterial = srcSR != null
            ? new Material(srcSR.sharedMaterial) { name = "BlobShadow_Runtime" }
            : new Material(Shader.Find("Sprites/Default")) { name = "BlobShadow_Runtime" };
        if (m_blobMaterial.HasProperty("_BaseColor")) m_blobMaterial.SetColor("_BaseColor", Color.black);
        else                                          m_blobMaterial.SetColor("_Color",     Color.black);

        foreach (var foot in m_footBones)
        {
            var go = new GameObject(foot.name + "_shadow");
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // lie flat on XZ plane
            go.transform.localScale    = Vector3.one * m_blobScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite            = sprite;
            sr.material          = m_blobMaterial;
            sr.color             = new Color(0f, 0f, 0f, m_shadowOpacity);
            sr.shadowCastingMode = ShadowCastingMode.Off;
            sr.receiveShadows    = false;
            sr.sortingOrder      = m_shadowSortingOrder;

            m_blobs.Add(new BlobData { footBone = foot, blobSR = sr });
        }
    }

    // ── Per-frame ─────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        foreach (var blob in m_blobs)
        {
            if (blob.blobSR == null) continue;

            Vector3 footWorld = blob.footBone.position;

            // Ground position: snap XZ to foot, lock Y to ground.
            blob.blobSR.transform.position = new Vector3(footWorld.x, m_groundOffset, footWorld.z);

            // Fade out as the foot lifts above the ground.
            float height = Mathf.Max(0f, footWorld.y - m_groundOffset);
            float alpha  = Mathf.Clamp01(1f - height / m_fadeHeight) * m_shadowOpacity;
            Color c      = blob.blobSR.color; c.a = alpha;
            blob.blobSR.color = c;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Texture2D CreateBlobTexture(int res)
    {
        var tex    = new Texture2D(res, res, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[res * res];
        var center = new Vector2(res * 0.5f, res * 0.5f);
        float radius = res * 0.5f;
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float t = Vector2.Distance(new Vector2(x, y), center) / radius;
                float a = Mathf.Clamp01(1f - t); a *= a; // smooth quadratic falloff
                pixels[y * res + x] = new Color(0f, 0f, 0f, a);
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
