
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedExplosion : Explosion
{
    [UdonSynced] private Vector3 _syncedPosition;

    public override void OnDeserialization() { 
        this.transform.position=_syncedPosition;
        isOwner=false;
    }

    public override void Activate(Vector3 pos,int power,float distanceBonus, DamageProcessController dpc, IObjectPool pool,bool isHomingDamageBonus, EnchantItem item)
    {
        base.Activate(pos,power,distanceBonus,dpc,pool,isHomingDamageBonus,item);
        isOwner=true;
        _syncedPosition=pos;
        RequestSerialization();
    }
}
