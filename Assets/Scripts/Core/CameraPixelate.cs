using UnityEngine;

public class CameraPixelate : MonoBehaviour
{
    [SerializeField] private Material pixelateMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (pixelateMaterial != null)
            Graphics.Blit(src, dst, pixelateMaterial);
        else
            Graphics.Blit(src, dst);
    }
}
