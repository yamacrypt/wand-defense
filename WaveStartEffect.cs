
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WaveStartEffect : UdonSharpBehaviour
{
    [SerializeField]TextMeshProUGUI waveStartText;
    [SerializeField]AudioSource waveStartSound;
    void Start()
    {
        
    }

    public void ShowTemp(int wave, int maxWave){
        waveStartText.enabled=true;
        if(wave==maxWave){
            waveStartText.text="Final Wave";
        } else if(wave>maxWave) {
            waveStartText.text="Extra Wave "+ wave;
        }
        else {
            waveStartText.text="Wave "+wave + " / " + maxWave;
        }
        waveStartSound.Stop();
        waveStartSound.Play();
        SendCustomEventDelayedSeconds("Hide",1);
    }

    public void ClearWave(int wave){
        waveStartText.enabled=true;
        waveStartText.text="Wave "+wave + " Clear !!";
        waveStartSound.Stop();
        waveStartSound.Play();
        SendCustomEventDelayedSeconds("Hide",1);
    }

    public void Hide(){
        waveStartText.enabled=false;
    }
}
