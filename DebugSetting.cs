
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DebugSetting : UdonSharpBehaviour
{
    [SerializeField]bool teleport=false;
    [SerializeField]bool _battleStart=false;
    [SerializeField]int spawnCount=0;
    [SerializeField]GameObject InitTeleportTo;
    [SerializeField]BattleStart battleStart;
    [SerializeField]Player player;
    [SerializeField]int wave=0;
    [SerializeField]int level=0;
    void Start()
    {
        if(teleport)Networking.LocalPlayer.TeleportTo(InitTeleportTo.transform.position, InitTeleportTo.transform.rotation);
        if(Networking.LocalPlayer.isMaster){
            player.DebugReset(level);
            battleStart.DebugGame(wave,spawnCount);
            if(_battleStart){
                battleStart.Interact();
            }
        }
    }
}
