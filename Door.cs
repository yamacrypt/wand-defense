
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Door : UdonSharpBehaviour
{
    [SerializeField]GameObject Exit;
    void Start()
    {
        
    }

  public override void Interact()
  {
    Networking.LocalPlayer.TeleportTo(Exit.transform.position, Exit.transform.rotation);
  }
}
