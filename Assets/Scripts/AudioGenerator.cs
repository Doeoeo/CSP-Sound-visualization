using B83.MathHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using System.Linq;

[UpdateBefore(typeof(FilterApplyBase))]
public class AudioGenerator : MonoBehaviour {
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioClip audioClip2;
    [SerializeField] private AudioSource ads1;
    [SerializeField] private AudioSource ads2;

    public static int sampleNumber = 512;
    public static float[] timeDomain = new float[sampleNumber];
    public static float[] timeDomain2 = new float[sampleNumber];
    public static float[] compareDomain = new float[sampleNumber];
    public static float[] frequncyDomain = new float[sampleNumber];
    public static float[] fullClip, fullClip2;
    public static Complex[] fft = new Complex[sampleNumber];
    
    private static AudioSource[] audioSources;
    private static AudioSource audioSource;
    private static AudioClip[] segments;
    private static AudioClip segment;
    private MyAudioPlayer[] myAudioPlayers;

    private static int i, bufferIndex = 0, overlapSample;
    private static float overlap = 0.2f;
    private static double nextEventTime;

    public static float[] sampleExt = new float[2];

    void Start() {
        Time.timeScale = 2.0f;
        i = 0;
        audioSources = new AudioSource[] {ads1, ads2};
        audioSource = ads1;
        fullClip = new float[audioClip.samples * audioClip.channels];
        fullClip2 = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(fullClip, 0);
        audioClip.GetData(fullClip2, 0);
        for (int i = 0; i < fullClip.Length; i++) fullClip[i] = (fullClip[i] + fullClip2[(i * 4) % fullClip2.Length]) / 2;
        sampleExt[0] = fullClip.Max();
        sampleExt[1] = fullClip.Min();
        audioClip.SetData(fullClip, 0);
        nextEventTime = AudioSettings.dspTime;
        segment = AudioClip.Create("Segment", timeDomain.Length, audioClip.channels, audioClip.frequency, false);

        segments = new AudioClip[] { 
            AudioClip.Create("Segment1", timeDomain.Length, audioClip.channels, audioClip.frequency, false), 
            AudioClip.Create("Segment2", timeDomain.Length, audioClip.channels, audioClip.frequency, false) 
        };
        myAudioPlayers = new MyAudioPlayer[2];
        myAudioPlayers[0] = new MyAudioPlayer();
        myAudioPlayers[1] = new MyAudioPlayer();
        myAudioPlayers[0].m_audioSource = audioSources[0];
        myAudioPlayers[1].m_audioSource = audioSources[1];

        overlapSample = (int)(overlap * audioClip.frequency);

        // TEMP
        ads1.clip = audioClip;
        ads1.Play();

        
    }

    // Update is called once per frame
    void Update() {
        /* Doesn't work yet :(
        double time = AudioSettings.dspTime;
        if((time + (segment.length - overlap) / 2 > nextEventTime) && !audioSources[bufferIndex].isPlaying) {
            Debug.Log("one execution " + " At " + time + " Til "+ (double)(nextEventTime + segment.length) + " -Next- " + nextEventTime + " --> Playing on channel " + bufferIndex);
            audioClip.GetData(timeDomain, i);
            //Array.Copy(fullClip, i, timeDomain, 0, timeDomain.Length);
            audioSources[bufferIndex].clip = segments[bufferIndex];
            audioSources[bufferIndex].clip.SetData(timeDomain, 0);
            bufferIndex = (bufferIndex + 1) % 2;
            myAudioPlayers[bufferIndex].PlayScheduled(nextEventTime);
            //audioSources[bufferIndex].PlayScheduled(nextEventTime);
            nextEventTime += (double)(segment.samples) / segment.frequency - overlap;
            myAudioPlayers[bufferIndex].m_audioSource.SetScheduledEndTime(nextEventTime);
            //myAudioPlayers[bufferIndex].m_audioSource.time = overlap;
            i += timeDomain.Length - overlapSample;
        } else { 
        //    Debug.Log("Waiting"); 
        }
        */

        // Temp remove if proper playing works
        audioSource.GetOutputData(timeDomain, 0);
        i += timeDomain.Length;


        FFT.Float2Complex(timeDomain, fft);
        fft = FFT.CalculateFFT(fft, false);
        FFT.Complex2Float(fft, false, frequncyDomain);
        Debug.Log("Before " + timeDomain.Length);
        //FFT.Float2Complex(frequncyDomain, fft);
        //fft = FFT.CalculateFFT(fft, true);
        //FFT.Complex2Float(fft, true, timeDomain);

        Debug.Log("After " + timeDomain.Length);

    }
    public static void postTimeUpdate() {
        Debug.Log(" Updating ");
        FFT.Float2Complex(timeDomain, fft);
        fft = FFT.CalculateFFT(fft, false);
        FFT.Complex2Float(fft, false, frequncyDomain);
    }

    public static int revert() {
        fft = FFT.CalculateFFT(fft, true);
        FFT.Complex2Float(fft, true, timeDomain);
        audioSource.clip.SetData(timeDomain, 0);
        return 1;
    }

}
