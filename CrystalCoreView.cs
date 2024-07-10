
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CrystalCoreView : UdonSharpBehaviour
{
    [SerializeField]TMPro.TextMeshProUGUI lifeText;
    void Start()
    {
        
    }
    

    public void Notify(int life){
        lifeText.text="Life: "+life;
    }
}
