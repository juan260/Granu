using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class LineFromAudio : MonoBehaviour
{
    
    LineRenderer lineRenderer;
    AudioSource audioSource;
    float [] samples;
    public int resolution = 100;

    // Start is called before the first frame update
    void Start()
    {
        var points = new Vector3[resolution];
        AnimationCurve curve = new AnimationCurve();
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
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
            curve.AddKey(i/(resolution-1), averageWidth);
            points[i] = new Vector3(0, (2*i/(resolution-1))-1, 0);
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
    }
}
