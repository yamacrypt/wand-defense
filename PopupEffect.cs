
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PopupEffect : UdonSharpBehaviour
{
    [SerializeField]TextMeshProUGUI popupText;
    [SerializeField]AudioSource popupSound;

    public void ShowTemp(float duration=1){
        if(popupText!=null){
            popupText.enabled=true;
            SendCustomEventDelayedSeconds("Hide",duration);
        }
        if(popupSound!=null){
            popupSound.Stop();
            popupSound.Play();
        }
    }

    public void Hide(){
        popupText.enabled=false;
    }
}
