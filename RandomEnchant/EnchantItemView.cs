
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EnchantItemView : UdonSharpBehaviour
{
    [SerializeField]TMPro.TextMeshPro[] texts;
    [SerializeField]UniTranslateText fireElement;
    [SerializeField]UniTranslateText lightningElement;
    [SerializeField]UniTranslateText coldElement;
    [SerializeField]UniTranslateText blackElement;
    [SerializeField]UniTranslateText whiteElement;
    [SerializeField]UniTranslateText chain;
    [SerializeField]UniTranslateText explosion;
    [SerializeField]UniTranslateText penetration;
    [SerializeField]UniTranslateText bullet;
    [SerializeField]UniTranslateText homingBullet;
    [SerializeField]UniTranslateText attackSpeed;
    void Start()
    {
        
    }

    public void Show(int[] enchants){
        for(int i =0;i<3;i++){
            texts[i].text=EnchantToString(enchants[i]);
        }
    }

    string elementString(int number){
        switch(number){
            case 1:
                return fireElement.GetText();
            case 2:
                return lightningElement.GetText();
            case 3:
                return coldElement.GetText();
            case 4:
                return blackElement.GetText();
            case 5:
                return whiteElement.GetText();
            default: break;
        }
        return "";
    }

    string EnchantToString(int enchant){
        switch(enchant){
            case 3:
                return chain.GetText()+ "+1";
            case 4:
                return explosion.GetText()+" +0.3";
            case 5:
                return penetration.GetText()+" +1";
            case 6:
                return bullet.GetText()+ " +1";
            case 7:
                return homingBullet.GetText()+" +1";
            case 8:
                return attackSpeed.GetText()+" +15%";
            default: break;
        }
        if(enchant>100){
            int strong = (enchant-100)/10;
            int weak = (enchant-100)%10;
            return elementString(strong)+" +2 & "+elementString(weak)+" -1";
        }
        else if(enchant>10){
            return elementString(enchant-10)+" +1";
        }
        return "";
    }

    public void _OnEnable(){
        foreach(var text in texts){
            text.enabled=true;
        }
    }

    public void _OnDisable(){
        foreach(var text in texts){
            text.enabled=false;
        }
    }
}
