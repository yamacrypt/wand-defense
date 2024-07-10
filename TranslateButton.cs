
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TranslateButton : UdonSharpBehaviour
{
    [SerializeField]UdonLab.TranslateManager translateManager;
    void Start()
    {
        
    }

    public override void Interact(){
        translateManager.switchLanguage();
    }
}
