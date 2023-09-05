using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GranularSynth))]
public class LineFromAudio : MonoBehaviour {

    AudioSource audioSource;
    public int resolution = 30;
    float [] samples;
    public GameObject barPrefab;
    GranularSynth granularSynth;


    void Start(){
        audioSource = GetComponent<AudioSource>();
        //var points = new Vector3[resolution];
        var widths = new float[resolution];
        samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);
        granularSynth = GetComponent<GranularSynth>();
        float roundEdges = 0.3f;

        int samplesPerBlock = (int)((float)samples.Length/(float)resolution);
        float averageWidth;
        for(int i = 0; i<resolution;i++){
            averageWidth = 0f;
            for(int j = 0; j < samplesPerBlock && i*samplesPerBlock + j < samples.Length; j++){
                averageWidth += Mathf.Abs(samples[i*samplesPerBlock + j]);
            }
            averageWidth /= samplesPerBlock;

            // Create new prefab

            GameObject newBar = Instantiate(barPrefab, this.transform, worldPositionStays:false);
            newBar.transform.localRotation = Quaternion.Euler(0,0,90);
            //Adjust for the fade outs
            if((float)i/resolution < granularSynth.lengthEndLineFade){
                //curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth * Mathf.Pow((float)(i)/(resolution*granularSynth.lengthEndLineFade),roundEdges));
                newBar.transform.localPosition = new Vector3(0f, 1f-2*(float)i/(float)(resolution-1), 0f);
                newBar.transform.localScale = new Vector3(1f/resolution, averageWidth * Mathf.Pow((float)(i)/(resolution*granularSynth.lengthEndLineFade),roundEdges), 1f/resolution);
            } else if((float)i/resolution > 1.0f-granularSynth.lengthEndLineFade){
                //curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth * Mathf.Pow((float)(resolution-i)/(float)(resolution*granularSynth.lengthEndLineFade), roundEdges));
                newBar.transform.localPosition = new Vector3(0f, 1f-2*(float)i/(float)(resolution-1), 0f);
                newBar.transform.localScale = new Vector3(1f/resolution, averageWidth * Mathf.Pow((float)(resolution-i)/(float)(resolution*granularSynth.lengthEndLineFade), roundEdges), 1f/resolution);
            }   else {
                //curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth);
                newBar.transform.localPosition = new Vector3(0f, 1f-2*(float)i/(float)(resolution-1), 0f);
                newBar.transform.localScale = new Vector3(1f/resolution, averageWidth, 1f/resolution);
            }
             //debugText += averageWidth.ToString();
            //points[i] = new Vector3(0f, ((float)2*i/(float)(resolution-1))-1, 0f);
        }
        
    }
}


/*
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GranularSynth))]
public class LineFromAudio : MonoBehaviour
{
    
    LineRenderer lineRenderer;
    AudioSource audioSource;
    float [] samples;
    public int resolution = 100;
    string debugText = "";
    public TMP_Text debugDisplay;
    GranularSynth granularSynth;
    float roundEdges = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        var points = new Vector3[resolution];
        AnimationCurve curve = new AnimationCurve();
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        granularSynth = GetComponent<GranularSynth>();
        samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);

        lineRenderer.positionCount = resolution;
        int samplesPerBlock = (int)((float)samples.Length/(float)resolution);
        float averageWidth;
        for(int i = 0; i<resolution-1;i++){
            averageWidth = 0f;
            for(int j = 0; j < samplesPerBlock ; j++){
                averageWidth += Mathf.Abs(samples[i*samplesPerBlock + j]);
            }
            averageWidth /= samplesPerBlock;

            //Adjust for the fade outs
            if((float)i/resolution < granularSynth.lengthEndLineFade){
                curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth * Mathf.Pow((float)(i)/(resolution*granularSynth.lengthEndLineFade),roundEdges));

            } else if((float)i/resolution > 1.0f-granularSynth.lengthEndLineFade){
                curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth * Mathf.Pow((float)(resolution-i)/(float)(resolution*granularSynth.lengthEndLineFade), roundEdges));

            }   else {
                curve.AddKey(1f-(float)i/(float)(resolution-1), averageWidth);
            }
             debugText += averageWidth.ToString();
            points[i] = new Vector3(0, ((float)2*i/(float)(resolution-1))-1, 0);
        }
        averageWidth = 0f;
        for(int j = (resolution-1)*samplesPerBlock; j < samples.Length ; j++){
                    averageWidth += Mathf.Abs(samples[j]);
            }
        
        
        averageWidth /= samplesPerBlock;
        curve.AddKey(1, averageWidth);
        points[resolution-1] = new Vector3(0, 1, 0);

        lineRenderer.SetPositions(points);
        lineRenderer.widthCurve = curve;
        
           

        if(debugDisplay)
                debugDisplay.text=debugText;
    }
}
*/