
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LevelBonusPopup : UdonSharpBehaviour
{
    [SerializeField]TextMeshProUGUI popupText;
    [SerializeField]AudioSource popupSound;

    [SerializeField]AbilityDetailView abilityDetailView;

    [SerializeField]Player player;

    public void ShowTemp(JewelElement type,float duration=3){
        if(popupText!=null){
            popupText.enabled=true;
            popupText.text=getText(type);
            SendCustomEventDelayedSeconds("Hide",duration);
        }
        if(popupSound!=null){
            popupSound.Stop();
            popupSound.Play();
        }
    }

    string getText(JewelElement type){
        switch(type){
            case JewelElement.Fire:
                if(player.FirePower>=10)return abilityDetailView.FireText3;
                else if(player.FirePower>=5)return abilityDetailView.FireText2;
                return "";
            case JewelElement.Lightning:
                if(player.LightningPower>=10)return abilityDetailView.LightningText3;
                else if(player.LightningPower>=5)return abilityDetailView.LightningText2;
                return "";
            case JewelElement.Cold:
                if(player.ColdPower>=10)return abilityDetailView.ColdText3;
                else if(player.ColdPower>=5)return abilityDetailView.ColdText2;
                return "";
            case JewelElement.Black:
                if(player.BlackPower>=10)return abilityDetailView.BlackText3;
                else if(player.BlackPower>=5)return abilityDetailView.BlackText2;
                return "";
            case JewelElement.White:
                if(player.WhitePower>=10)return abilityDetailView.WhiteText3;
                else if(player.WhitePower>=5)return abilityDetailView.WhiteText2;
                return "";
            default:
                return "";
        }
    }

    public void Hide(){
        popupText.enabled=false;
    }
}
