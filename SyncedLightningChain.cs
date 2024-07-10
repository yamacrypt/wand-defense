
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedLightningChain :  LightningChain
{
    [UdonSynced] private Vector3 _syncedStart;
    [UdonSynced] private Vector3 _syncedEnd;

    public override void OnDeserialization() { 
        Init(_syncedStart,_syncedEnd);
        isOwner=false;
    }
    public override  void Activate(IObjectPool pool,Vector3 start,Vector3 end){
        base.Activate(pool,start,end);
        _syncedStart=start;
        _syncedEnd=end;
        RequestSerialization();
        isOwner=true;
    }
}
