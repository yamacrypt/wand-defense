
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BattlerStartButton : UdonSharpBehaviour
{
    [SerializeField]Transform TeleportTo;
    [SerializeField]Player player;
    [SerializeField]PopupEffect warningEffect;
    [SerializeField]GameSetting setting;

    [SerializeField]TMPro.TextMeshProUGUI explainText;
    public override void Interact()
    {
        if(player.HasYourGun()) {
            player.ResetFake();
            Teleport();
            if(Networking.LocalPlayer.isMaster) {
                if(explainText!=null)explainText.enabled=true;
            }
        } else {
            warningEffect.ShowTemp();
        }
    }

    public void Teleport(){
        Networking.LocalPlayer.TeleportTo(TeleportTo.transform.position, TeleportTo.transform.rotation);
        setting.CheckParameterAll();
    }
}
