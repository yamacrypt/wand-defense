
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TipesView : UdonSharpBehaviour
{
    [SerializeField]UniTranslateText[] tips;
    [SerializeField]TMPro.TextMeshProUGUI tipText;
    void Start()
    {
        
    }

    [UdonSynced]int index=0;
    public override void OnDeserialization()
    {
        tipText.text = tips[index].GetText();
    }

    // master only
    public void RandomSelect(){
        index = Random.Range(0,tips.Length);
        tipText.text = tips[index].GetText();
        RequestSerialization();
    }
    public void Show(){
        tipText.enabled=true;
    }

    public void Hide(){
        tipText.enabled=false;
    }
}
