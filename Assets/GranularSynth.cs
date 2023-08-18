using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


class GranularNote
{
    int startSample;
    int head = 0;
    int fadeIn;
    int fadeOut;
    int grainSize;
    float[] samples;
    float gain;
    float internalGain = 0f;
    int samplesPlayed = 0;
    public bool done = false;
    
    public GranularNote(int StartSample, int FadeIn, int FadeOut, int GrainSize, float [] Samples, float Gain){
        startSample = StartSample;
        fadeIn = FadeIn;
        fadeOut = FadeOut;
        grainSize = GrainSize;
        samples = Samples;
        gain = Gain;
    }

    public void play(float [] data, int channels){
        if(!done){
            for (int i = 0; i < data.Length; i += channels)
                {  
                    if(samplesPlayed < fadeIn){
                        internalGain = ((float)samplesPlayed)/fadeIn;
                    } 
                    internalGain=1.0f;
                    if(samplesPlayed > grainSize-fadeOut){
                        internalGain = ((float)grainSize-samplesPlayed)/fadeOut;
                    }
                    if(startSample + samplesPlayed > samples.Length-fadeOut){
                        internalGain = ((float)samples.Length-(startSample+samplesPlayed))/fadeOut;
                    }

                    data[i] = samples[head+startSample]* gain * internalGain;
                    if (channels > 1)
                    {
                        data[i + 1] = samples[head + startSample + 1] * gain * internalGain;
                    }
                    head += channels;
                    samplesPlayed++;
                    if (head + startSample >= samples.Length || head >= grainSize) 
                    {
                        done = true;
                        break;
                    }
                }
        }

    }
}

[RequireComponent(typeof(AudioSource))]
public class GranularSynth : MonoBehaviour
{   
    float[] samples;
    bool playing = false;
    int head = 0;
    public float gain = 1.0f;
    public Transform rightHand;
    public int grainSize = 48000;
    public int fadeIn = 480;
    public int fadeOut = 480;
    public int grainFreq = 10;
    float basePosition;
    AudioSource audioSource;
    int samplePosition;
    List<GranularNote> grainList = new List<GranularNote>();
    float timer=0;

    // Testing
    public double frequency = 440.0;
    private double increment;
    private double phase;
    private double sampling_frequency = 48000;
    public TMP_Text debugText;
    


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);
        
    }

    // Update is called once per frame
    void Update()
    {

        // Update the Hand Position
            basePosition = (float)(rightHand.transform.InverseTransformPoint(gameObject.transform.position)).x;

            basePosition += (gameObject.transform.localScale.y);
            basePosition /= (gameObject.transform.localScale.y)*2;
            basePosition = Mathf.Clamp(basePosition, 0.0f, 1f);
            
            basePosition *= (float)samples.Length;
            samplePosition = (int)Mathf.Round(basePosition);

        if(timer<0){
            // Clean the grain list
            for(int i = 0; i < grainList.Count;){
                if(grainList[i].done) grainList.RemoveAt(i);
                else i++;
            }
            

            // Add grain to the list
            grainList.Add(new GranularNote(samplePosition, fadeIn, fadeOut, grainSize, samples, gain));
            timer = 1.0f/grainFreq;
        }else{
            timer -= Time.deltaTime;
        }
    }

    public void BeginInteraction(){
        playing = true;
        audioSource.Play();
        
    }

    public void EndInteraction(){
        playing = false;
        audioSource.Stop();
    }

    void PlayTone(float [] data, int channels){
        increment = frequency * 2.0 * Mathf.PI / sampling_frequency;
        for(int i = 0; i< data.Length; i+= channels){
            phase += increment;
            data[i]=(float)(gain*Mathf.Sin((float)phase));
            if(channels==2)
            data[i+1]= data[i];
            if(phase >(Mathf.PI*2)){
                phase = 0.0f;
            }
        }
    }

    void OnAudioFilterRead(float [] data, int channels){
        if(playing){
            foreach(GranularNote note in grainList){
                note.play(data, channels);
            }
        }
    }

    void OnAudioFilterReadold(float [] data, int channels){
        if (playing)
         {
             for (int i = 0; i < data.Length; i += channels)
             {

                 data[i] = samples[head+samplePosition];
                 //TODO What if there's more than 2
                 if (channels > 1)
                 {
                     data[i + 1] = samples[head+samplePosition];
                 }
                 head += 2;
                if (head + samplePosition >= samples.Length || head >= grainSize) 
                 {
                     head = 0;
                 }
             }

         }

         
        
    }
}
