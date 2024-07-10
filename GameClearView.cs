using System;

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameClearView : UdonSharpBehaviour
{
    [SerializeField]TMPro.TextMeshProUGUI clearWaveText;
    [SerializeField]GameObject continueButton;
    [SerializeField]TipesView tipsView;

    [UdonSynced]int syncedWave;
    [UdonSynced]int syncedClearWave;
    [UdonSynced]float syncedTime;

    [SerializeField]UniTranslateText bossBeatText;
    [SerializeField]UniTranslateText reachingWaveText;
    ObjectPoolItem _self;
    ObjectPoolItem self{
        get{
            if(_self==null)_self=GetComponent<ObjectPoolItem>();
            return _self;
        }
    }
    void Start()
    {
        self.SetActive(false);
    }

    public override void OnDeserialization(){
        if(syncedTime ==-1f){
            _ShowForEndless(syncedWave,syncedClearWave);
        } else if(syncedWave==-1 && syncedClearWave==-1){
            _ShowForClear(syncedTime);
        } else {
            Debug.LogWarning("GameClearView OnDeserialization Error");
        }
    }

    void _ShowForEndless(int wave,int clearWave){
        clearWaveText.text=reachingWaveText.GetText()+wave;
        if(wave>clearWave){
            continueButton.SetActive(false);
        } else {
           continueButton.SetActive(true);
        }
        self.SetActive(true);
        if(tipsView!=null)tipsView.Show();
    }

    public void ShowForEndless(int wave,int clearWave){
        syncedWave=wave;
        syncedClearWave=clearWave;
        syncedTime=-1f;
        RequestSerialization();
        if(tipsView!=null)tipsView.RandomSelect();
        _ShowForEndless(wave,clearWave);
    }

    public void _ShowForClear(float time){
        clearWaveText.text=bossBeatText.GetText()+TimeSpan.FromSeconds(Math.Max(0f,time)).ToString(@"mm\:ss");
        continueButton.SetActive(true);
        self.SetActive(true);
        if(tipsView!=null)tipsView.Show();
    }

    public void ShowForClear(float time){
        syncedTime=time;
        syncedWave=-1;
        syncedClearWave=-1;
        RequestSerialization();
        if(tipsView!=null)tipsView.RandomSelect();
        _ShowForClear(time);
    }

    public void Hide(){
        self.SetActive(false);
        if(tipsView!=null)tipsView.Hide();
    }

    [SerializeField]TMPro.TextMeshProUGUI[] texts;
    [SerializeField]Image[] images;


    public void _OnEnable(){
        foreach(var image in images){
            image.enabled=true;
        }
        foreach(var text in texts){
            text.enabled=true;
        }
    }

    public void _OnDisable(){
        foreach(var image in images){
            image.enabled=false;
        }
        foreach(var text in texts){
            text.enabled=false;
        }
    }
}
