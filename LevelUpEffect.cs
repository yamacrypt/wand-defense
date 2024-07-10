
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LevelUpEffect : UdonSharpBehaviour
{
    [SerializeField]TextMeshProUGUI levelText;
    [SerializeField]AudioSource levelUpSound;
    void Start()
    {
        
    }

    public void ShowTemp(int level, int maxLevel){
        levelText.enabled=true;
        levelText.text = "Level Up!  "+level + " / " + maxLevel;
        levelUpSound.Stop();
        levelUpSound.Play();
        SendCustomEventDelayedSeconds("Hide",1);
    }

    public void Hide(){
        levelText.enabled=false;
    }
}
