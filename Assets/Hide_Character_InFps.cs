using UnityEngine;

public class HideCharacterInFPS : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hideInPlayMode = true;
    [SerializeField] private bool keepShadow = true;

    [Header("What to Hide")]
    [SerializeField] private Renderer[] characterRenderers;

    void Start()
    {
        if (hideInPlayMode)
        {
            HideCharacterMeshes();
        }
    }

    void HideCharacterMeshes()
    {
        // Ha nincs kézzel beállítva, automatikusan megkeresi
        if (characterRenderers == null || characterRenderers.Length == 0)
        {
            characterRenderers = GetComponentsInChildren<Renderer>();
        }

        foreach (Renderer renderer in characterRenderers)
        {
            if (renderer != null)
            {
                if (keepShadow)
                {
                    // Csak a mesh láthatatlan, de az árnyék megmarad
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
                else
                {
                    // Teljesen láthatatlan (árnyék is)
                    renderer.enabled = false;
                }
            }
        }

        Debug.Log("Karakter mesh-ek elrejtve FPS módban!");
    }

    // Editor-ban láthatóvá teszi újra (ha Stop-olod a játékot)
    void OnDisable()
    {
        if (characterRenderers != null)
        {
            foreach (Renderer renderer in characterRenderers)
            {
                if (renderer != null)
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.enabled = true;
                }
            }
        }
    }
}