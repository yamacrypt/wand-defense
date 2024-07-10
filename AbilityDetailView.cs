
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AbilityDetailView : UdonSharpBehaviour
{
    [SerializeField]UniTranslateText fireText;
    [SerializeField]UniTranslateText lightningText;
    [SerializeField]UniTranslateText coldText;
    [SerializeField]UniTranslateText blackText;
    [SerializeField]UniTranslateText whiteText;
     [SerializeField]UniTranslateText defaultText;

    [SerializeField]UniTranslateText fireText2;
    [SerializeField]UniTranslateText lightningText2;
    [SerializeField]UniTranslateText coldText2;
    [SerializeField]UniTranslateText blackText2;
    [SerializeField]UniTranslateText whiteText2;
    [SerializeField]UniTranslateText defaultText2;

    [SerializeField]UniTranslateText fireText3;
    [SerializeField]UniTranslateText lightningText3;
    [SerializeField]UniTranslateText coldText3;
    [SerializeField]UniTranslateText blackText3;
    [SerializeField]UniTranslateText whiteText3;
    [SerializeField]UniTranslateText defaultText3;
    public string FireText=>fireText.GetText();
    public string DefaultText=>defaultText.GetText();
    public string LightningText=>lightningText.GetText();
    public string ColdText=>coldText.GetText();
    public string BlackText=>blackText.GetText();
    public string WhiteText=>whiteText.GetText();
    public string DefaultText2=>defaultText2.GetText();
    public string FireText2=>fireText2.GetText();
    public string LightningText2=>lightningText2.GetText();
    public string ColdText2=>coldText2.GetText();
    public string BlackText2=>blackText2.GetText();
    public string WhiteText2=>whiteText2.GetText();
    public string DefaultText3=>defaultText3.GetText();
    public string FireText3=>fireText3.GetText();
    public string LightningText3=>lightningText3.GetText();
    public string ColdText3=>coldText3.GetText();
    public string BlackText3=>blackText3.GetText();
    public string WhiteText3=>whiteText3.GetText();


    [SerializeField]TMPro.TextMeshProUGUI ablityDetailText;

    [SerializeField]ToggleGroup toggleGroup;

    void Start()
    {
       ablityDetailText.text=DefaultText;
    }
  
    public void ShowupFire(){
        ShowupText(
            FireText, 
            player.FirePower>=5?FireText2: DefaultText2,
            player.FirePower>=10?FireText3: DefaultText3
        );
    }

    void ShowupText(string txt, string txt2, string txt3){
        var on = toggleGroup.AnyTogglesOn();
        if(!on){
            ablityDetailText.text=DefaultText;
            //ablityDetailText.text+="\n"+DefaultText2;
            //ablityDetailText.text+="\n"+DefaultText3;
        } else {
            ablityDetailText.text=txt;
            ablityDetailText.text+="\n"+txt2;
            ablityDetailText.text+="\n"+txt3;
        }
    }

    [SerializeField]Player player;

    public void ShowupLightning(){
        ShowupText(
            LightningText, 
            player.LightningPower>=5?LightningText2: DefaultText2,
            player.LightningPower>=10?LightningText3: DefaultText3
        );
    }

    public void ShowupCold(){
        ShowupText(
            ColdText, 
            player.ColdPower>=5?ColdText2: DefaultText2,
            player.ColdPower>=10?ColdText3: DefaultText3
        );
    }

    public void ShowupBlack(){
        ShowupText(
            BlackText, 
            player.BlackPower>=5?BlackText2: DefaultText2,
            player.BlackPower>=10?BlackText3: DefaultText3
        );
    }

    public void ShowupWhite(){
        ShowupText(
            WhiteText, 
            player.WhitePower>=5?WhiteText2: DefaultText2,
            player.WhitePower>=10?WhiteText3: DefaultText3
        );  
    }
}
