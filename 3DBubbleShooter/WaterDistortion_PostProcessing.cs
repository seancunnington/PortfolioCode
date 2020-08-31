using UnityEngine;

/// <summary>
/// Simulates a water distortion effect via post processing. 
/// </summary>
/// 
[ExecuteInEditMode]
public class WaterDistortion_PostProcessing : MonoBehaviour
{
    [SerializeField]
     private Material waterDistortion = null;

     void OnRenderImage(RenderTexture src, RenderTexture dst)
     {
          // If there is an attached material for this effect, then execute.
          if (waterDistortion != null)
               Graphics.Blit(src, dst, waterDistortion);
     }
}