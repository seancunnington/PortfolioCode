using UnityEngine;



/// <summary>
/// Simulates a sonar effect. Used with the fx_Scan material.
/// Sends a plane from the camera out into the distance via the depth texture. 
/// </summary>
/// 
[ExecuteInEditMode]
public class Scan_PostProcessing : MonoBehaviour
{

     // The material used for the scan.
     [SerializeField]
     private Material Scan = null;

     // The distance from the camera to put the scan plane at.
     // This is forwarded to the scan shader property.
     private float waveDistance = 0;

     // How opaque the scan plane is. 1 = opaque, 0 = transparent.
     // This is forwarded to the scan shader property.
     private float waveOpacity = 0;

     // Multiplied with Time.deltaTime
     [SerializeField]
     private float opacityScalar = 1;

     // The speed of how fast the scan plane is sent. 
     // Named as a Wave because it repeats. 
     [SerializeField]
     private float waveSpeed = 0;

     // Wave position reset to camera if false.
     private bool waveActive = false;

     // The maximum distance the wave can travel before it's reset via waveActive.
     [SerializeField]
     private float maxDistance = 20;

     // The position the wave starts at. 0 starts at camera position.
     [SerializeField]
     private float startDistance = 0;

     // The amount of time it takes to reactivate the wave.
     [SerializeField]
     private float pulseDelay = 0;

     // The timer that is iterated over with Time.deltaTime.
     private float pulseTimer = 0;



     /// <summary>
     /// Set's the Scan shader's properties and executes the Blit. 
     /// </summary>
     /// <param name="src"></param>
     /// <param name="dst"></param>
     /// 
     void OnRenderImage(RenderTexture src, RenderTexture dst)
     {
          // If there is an attached material for Scan, then execute.
          if (Scan != null)
          {
               // Forward these properties to the Scan shader.
               Scan.SetFloat("_Opacity", waveOpacity);
               Scan.SetFloat("_WaveDistance", waveDistance);
               Graphics.Blit(src, dst, Scan);
          }
     }



     /// <summary>
     /// Set's the camera's depth mode for the Scan shader's use of the depth texture.
     /// </summary>
     /// 
     private void Start()
     {
          // Set the camera's depth mode.
          Camera cam = GetComponent<Camera>();
          cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
     }



     /// <summary>
     /// Repeatedly send out a scan wave.
     /// </summary>
     /// 
     private void Update()
     {
          SendWave();
     }



     /// <summary>
     /// Send out a scan wave in pulses.
     /// </summary>
     /// 
     private void SendWave()
     {
          // Timer used to send out pulses of scan waves.
          // If the timer is greater than the threshold (hardcoded to 1 in this case),
          // Then reset the following variables:
          if (pulseTimer >= 1)
          {
               waveActive = true;  // This restarts the scan wave entirely, clearing the screen of the effect.
               pulseTimer = 0;     // This resets the timer itself, to begin again.
          }

          // Increase the timer multiplied by a scalar.
          pulseTimer += Time.deltaTime * pulseDelay;

          // Decrease the opacity variable over time.
          if (waveOpacity >= 0)
               waveOpacity -= Time.deltaTime * opacityScalar;


          // Send the Wave
          if (waveActive){
               // Set the wave to the starting distance at the beginning of the scan
               if (waveDistance < startDistance)
                    waveDistance = startDistance;

               waveDistance = waveDistance + waveSpeed * Time.deltaTime;   // Move the wave down camera depth

               // Once the wave reaches the maximum depth, stop the scan
               if (waveDistance >= maxDistance)
                    waveActive = false;

          } else {
               waveDistance = 0;   // If not scanning, then keep the scan line at the camera
               waveOpacity = 1;    // This makes the wave opaque again, so it's brighter at the start of the scan.
          }
     }



}
