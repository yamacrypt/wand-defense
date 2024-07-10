
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedBomb : Bomb
{
    [UdonSynced] private Vector3 _syncedPosition;
    public override void OnDeserialization() { 
        this.transform.position=_syncedPosition;
        isOwner=false;
    }

    public override void Activate(Vector3 pos,GunController gc,float distanceBonus, DamageProcessController dpc, IObjectPool pool,bool isHomingDamageBonus)
    {
        base.Activate(pos,gc,distanceBonus,dpc,pool,isHomingDamageBonus);
        isOwner=true;
        _syncedPosition=pos;
        RequestSerialization();
    }
}
