using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCAController : MonoBehaviour
{
    private FMOD.Studio.VCA VCA_Controller;
    public string VCA_Name;

    [SerializeField] private float VCA_Vol;

    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        VCA_Controller = FMODUnity.RuntimeManager.GetVCA("vca:/" + VCA_Name);
        slider = GetComponent<Slider>();
        VCA_Controller.getVolume(out VCA_Vol);
    }

    public void SetVolume(float volume)
    {
        VCA_Controller.setVolume(volume);
        VCA_Controller.getVolume(out VCA_Vol);
    }
}
