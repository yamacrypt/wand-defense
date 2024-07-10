
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DisappearedUI : UdonSharpBehaviour
{
    void Start()
    {

    }

    public override void Interact()
    {
        this.gameObject.SetActive(false);
    }
}
