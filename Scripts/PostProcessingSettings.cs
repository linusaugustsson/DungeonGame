using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingSettings : MonoBehaviour
{
    public static PostProcessingSettings instance;
    private ColorGrading colorGrading;
    private float saturationDefaultValue;
    private float postExposureDefaultValue;


    private Bloom bloom;
    private Grain grain;
    private AmbientOcclusion ambientOcclusion;
    private Vignette vignette;
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);


        PostProcessVolume ppVolume = GetComponent<PostProcessVolume>();
        ppVolume.profile.TryGetSettings(out colorGrading);
        ppVolume.profile.TryGetSettings(out bloom);
        ppVolume.profile.TryGetSettings(out grain);
        ppVolume.profile.TryGetSettings(out ambientOcclusion);
        ppVolume.profile.TryGetSettings(out vignette);


        saturationDefaultValue = colorGrading.saturation.value;
        postExposureDefaultValue = colorGrading.postExposure.value;
    }

    public void SetSpectatorLook()
    {
        colorGrading.saturation.value = -100.0f;
        colorGrading.postExposure.value = 1.7f;
    }

    public void SetAliveLook()
    {
        colorGrading.saturation.value = saturationDefaultValue;
        colorGrading.postExposure.value = postExposureDefaultValue;
    }

    public void ToggleBloom()
    {
        
    }

    public void ToggleGrain()
    {

    }

    public void ToggleAmbientOcclusion()
    {

    }

    public void ToggleVignette()
    {

    }


}
