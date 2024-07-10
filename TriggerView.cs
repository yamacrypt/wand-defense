
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TriggerView : UdonSharpBehaviour
{
    [SerializeField]UniTranslateText uniText;
    [SerializeField]TextMeshProUGUI popupText;

    public void ShowTemp(bool on,float duration=1){
        if(popupText!=null){
            popupText.enabled=true;
            popupText.text = on?  $"{uniText.GetText()} ON" : $"{uniText.GetText()} OFF";
            SendCustomEventDelayedSeconds("Hide",duration);
        }
    }

    public void Hide(){
        popupText.enabled=false;
    }
}
