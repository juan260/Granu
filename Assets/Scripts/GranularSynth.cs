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
    float internalGain = 0f;
    int samplesPlayed = 0;
    public bool done = false;
    int offset;
    List<GranularNote> grainList;
    
    
    public GranularNote(int StartSample, int FadeIn, int FadeOut, int GrainSize, float [] Samples, int Offset, List<GranularNote> GrainList){
        startSample = StartSample;
        fadeIn = FadeIn;
        fadeOut = FadeOut;
        grainSize = GrainSize;
        samples = Samples;
        offset = Offset;
        grainList = GrainList;
    }

    public void play(float [] data, int channels, float gain){
        if(!done){
            for (int i = 0; i < data.Length; i += channels)
                {  
                    if(offset>0){
                        offset--;
                    } else {
                        if (head + startSample >= samples.Length || samplesPlayed >= grainSize || head + startSample + channels >= samples.Length) 
                        {
                            done = true;
                            grainList.Remove(this);
                            break;
                        }
                        if(samplesPlayed < fadeIn){
                            internalGain = ((float)samplesPlayed)/fadeIn;
                        } else { internalGain=1.0f; }
                        
                        if(samplesPlayed > grainSize-fadeOut){
                            internalGain = ((float)grainSize-samplesPlayed)/fadeOut;
                        }
                        // TODO Test  this:
                        if(startSample + samplesPlayed > samples.Length-fadeOut){
                            internalGain = ((float)samples.Length-(startSample+samplesPlayed))/fadeOut;
                        }

                        data[i] += samples[head+startSample]* internalGain * gain;
                        if (channels > 1)
                        {
                            // TODO This sometimes does index out of range
                            data[i + 1] += samples[head + startSample + 1] * internalGain * gain;
                        }
                        head += channels;
                        samplesPlayed++;
                    }
                }
        }

    }
}

public class GranularSynth : MonoBehaviour
{   
    float[] samples;
    public float gain = 1.0f;
    public Transform hand;
    [Range(480,4800)]
    public int grainSizeMin = 1000;
    [Range(4800,96000)]
    public int grainSizeMax = 90000;
    int grainSize;
    [Range(1,48000)]
    public int fadeIn = 480;
    [Range(1,48000)]
    public int fadeOut = 480;
    [Range(0.01f,10f)]
    public float minGrainFreq = 2;
    [Range(10f,1000f)]
    public float maxGrainFreq = 100;
    float grainFreq;
    public int maxGrains = 20;
    float basePosition;
    AudioSource audioSource;
    int samplePosition;
    List<GranularNote> grainList = new List<GranularNote>();

    [Range(0f, 10f)]
    public float maxDistance =5f;
    float distance=1f;
    public float lengthEndLineFade = 0.2f;

    public TMP_Text debugDisplay;
    string debugText = "";
    float lastAudioFrameTime;
    public float averageTimeBetweenAudioFramesSmoothingFactor = 0.01f;
    public float averageTimeBetweenAudioFrames = 0.02f;
    float elapsedTime;
    int samplesPerSecond = 48000;
    float currentTime = 0f;

    public bool deletePreviousAudioData = true;
    


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        samples = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(samples, 0);
        audioSource.Pause();
        lastAudioFrameTime = 0f;
    }
   
    public void setGain(float gain){
        this.gain = gain;
    }
    // Update is called once per frame
    void Update()
    {

        // Update the Hand Position
        
            Transform handPosition = hand.transform;
            Transform myTransform = gameObject.transform;
            myTransform.localScale.Set(1,1,1);
            handPosition.Rotate(myTransform.rotation.eulerAngles*(-1));
            // Y = left, X = up, Z = Forward
            //UnityEngine.Debug.Log(handPosition.position);
            Vector3 relativePositionRightHand = myTransform.InverseTransformPoint(handPosition.position);
            //UnityEngine.Debug.Log(relativePositionRightHand);
            basePosition = (float)(relativePositionRightHand).y*(-1f);
            basePosition = (float)(relativePositionRightHand).y*(-1f);
            basePosition += 1f;
            basePosition /= 2f;
            basePosition = Mathf.Clamp(basePosition, 0.0f, 1f);

            samplePosition = (int)Mathf.Round(basePosition*(float)samples.Length);

            // Distance between 0 and 1 (minimum and maximum)
            distance = Mathf.Sqrt(Mathf.Pow(relativePositionRightHand.x,2f)+Mathf.Pow(relativePositionRightHand.z,2f))/maxDistance;
            distance = Mathf.Clamp(distance, 0f, 1f);

            
            if(basePosition > 1f-lengthEndLineFade){
                distance = Mathf.Max(distance, 1f-((1f-basePosition)/lengthEndLineFade));
            }
            if(basePosition < lengthEndLineFade){
                distance = Mathf.Max(distance, 1f-(basePosition/lengthEndLineFade));
            }

            grainSize = (int)Mathf.Round(Mathf.Lerp(grainSizeMin, grainSizeMax, 
                Mathf.InverseLerp(maxDistance, 0f, Mathf.Abs(relativePositionRightHand.x))));
            
            grainFreq = (int)Mathf.Round(Mathf.Lerp(minGrainFreq, maxGrainFreq, 
                Mathf.InverseLerp(maxDistance, 0f, Mathf.Abs(relativePositionRightHand.z))));
            

        
        if(elapsedTime>1.0/grainFreq){
            
            // Clean the grain list
          /*  int i = 0;
            foreach(GranularNote grain in grainList.ToList()){
                if(grain.done) grainList.RemoveAt(i);
                i++;
            }
            */
            if(grainList.Count<maxGrains){
                // Add grain to the list
                //debugText = ((int)elapsedTime * samplesPerSecond).ToString();
                grainList.Add(new GranularNote(samplePosition, fadeIn, fadeOut, grainSize, samples, (int)elapsedTime * samplesPerSecond, grainList)); 
                //UnityEngine.Debug.Log(grainSize);
                //UnityEngine.Debug.Log(relativePositionRightHand.x);
            }
            elapsedTime=0;
        }else{
            elapsedTime += Time.deltaTime;
        }
        //if(debugDisplay)
        //debugDisplay.text=debugText;
        currentTime = Time.time;
    }

    void OnAudioFilterRead(float [] data, int channels){
            if(deletePreviousAudioData){
                for(int i = 0;i<data.Length;i++){
                    data[i] = 0;
                }
            }
            foreach(GranularNote note in new List<GranularNote>(grainList)){
                if(!note.done) note.play(data, channels, gain*(1-distance));
            }
            
            averageTimeBetweenAudioFrames = averageTimeBetweenAudioFrames * (1-averageTimeBetweenAudioFramesSmoothingFactor) +
                 (currentTime - lastAudioFrameTime) * averageTimeBetweenAudioFramesSmoothingFactor;
            
            lastAudioFrameTime = currentTime;

    }   


}
