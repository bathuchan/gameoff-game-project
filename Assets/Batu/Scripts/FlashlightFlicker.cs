using UnityEngine;
using System.Collections;
using SmoothShakeFree;

public class FlashlightFlicker : MonoBehaviour
{
    public Light flashlight;               // The flashlight light component
    public float minFlickerInterval = 0.1f; // Minimum time between flickers
    public float maxFlickerInterval = 1f;   // Maximum time between flickers
    private float minIntensity = 0.5f;      // Minimum intensity for flicker
    private float maxIntensity = 1f;        // Maximum intensity for flicker
    public float flickerChance = 0.2f;     // Chance of the light turning off completely
    public float shutdownTime = 3f;        // Time for the flashlight to stay off before turning back on

    private bool isFlickering = false;
    private bool isLightOff = false;
    private Animator anim;
    private SmoothShake smoothShake;
    private SmoothShakeFreePreset preset;

    void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
            minIntensity=flashlight.intensity*0.8f;
            maxIntensity = flashlight.intensity;
        }
        if (anim ==null ) 
        {
            anim=GetComponent<Animator>();
            if(TryGetComponent<SmoothShake>(out smoothShake))
                preset = smoothShake.preset;

        }

        // Start the flickering effect when the game begins
        StartCoroutine(FlickerLight());
    }

    private IEnumerator FlickerLight()
    {
        bool shakeActive=true;
        while (true)
        {
            if (isLightOff)
            {
                // Wait for the shutdown time before turning the light back on
                anim.SetTrigger("Reload");
                
                
                shakeActive = false;
                yield return new WaitForSeconds(shutdownTime*Random.Range(.8f,1.2f));
                
                flashlight.enabled = true;
                isLightOff = false;
            }
            
            
            // Randomly decide if the light should turn off
            if (Random.value < flickerChance)
            {
                //if(smoothShake!=null)smoothShake.ForceStop();
                flashlight.enabled = false;
                isLightOff = true;
                continue;  // Skip to the next flicker cycle if the light is off
            }
            //if (anim.GetCurrentAnimatorStateInfo(0).IsName("FlashIdle") && !shakeActive)
            //{
            //    Debug.Log("shakestardet");
            //    if (smoothShake != null) smoothShake.StartShake();
            //    shakeActive = true;
            //}

            // Randomize the flicker interval and intensity
            float flickerInterval = Random.Range(minFlickerInterval, maxFlickerInterval);
            flashlight.intensity = Random.Range(minIntensity, maxIntensity);

            // Wait for the next flicker
            yield return new WaitForSeconds(flickerInterval);
        }
    }
}
