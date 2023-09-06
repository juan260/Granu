using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GranularSynth))]
public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    public Slider GainSlider;
    public Slider MinimumGrainSizeSlider;
    public Slider MaximumGrainSizeSlider;
    public GranularSynth synth;
    private bool on;

    void Start(){
        synth = GetComponent<GranularSynth>();
        on = menu.activeSelf;
        GainSlider.value = synth.gain;
    }

    public void MenuSwitch(){
        on = !on;
        menu.SetActive(on);
    }
}
