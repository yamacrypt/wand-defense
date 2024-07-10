
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ColoredEnemySyncedPool : IObjectPool
{
    void Start()
    {
        for(int i=0;i<Pool.Length;i++){
            Pool[i].SetActive(false);
        }
         if(Pool.Length>15){
            Debug.LogError("WARN Pool.Length<15");
        }
    }
    [UdonSynced]bool[] actives=new bool[15]; 
    [UdonSynced]bool[] syncedChanges=new bool[15];
    [UdonSynced]EnemyAura syncedAura;
    bool[] changes=new bool[15];

    public override void OnDeserialization() { 
        //.Log("OnDeserialization Pool");
        for(int i=0;i<Pool.Length;i++){
            if(actives[i] && syncedChanges[i]){
                var enemy = Pool[i].GetComponent<Enemy>();
                if(enemy!=null)enemy.auraType=syncedAura;
            }
            Pool[i].SetActive(actives[i],syncedChanges[i]);
        }
    }

    public override void OnPreSerialization() { 
        //Debug.Log("OnPreSerialization Pool");
        for(int i=0;i<Pool.Length;i++){
            syncedChanges[i]=changes[i];
            changes[i]=false;
        }
    }
    public bool TryToSpawn(EnemyAura aura){
        //Debug.Log("TrytoSpawn: " );
        if(!Networking.IsOwner(Networking.LocalPlayer,gameObject))return false;
        foreach(var active in actives){
            if(active)return false;
        }
        for(int i=0;i<Pool.Length;i++){
            var item = Pool[i];
            var enemy = item.GetComponent<Enemy>();
            if(enemy==null)continue;
            enemy.auraType=aura;
            //Debug.Assert(enemy.auraType!=EnemyAura.None);
            var res =item.SetActive(true);
            if(res){
                actives[i]=true;
                changes[i]=true;
            }
        }
        syncedAura=aura;
        RequestSerialization();
        return true;
    }
    public override void Return(GameObject obj){
        //if(!Networking.IsOwner(gameObject))return;
        if(obj==null)return;
        for(int i=0;i<Pool.Length;i++){
            if(Pool[i].gameObject==obj){
                // TODO: ensure taht pool and obj is same owner
                if(Networking.IsOwner(Networking.LocalPlayer,gameObject)){
                    var res =Pool[i].SetActive(false);
                    if(res){
                        actives[i]=false;
                        changes[i]=true;
                        RequestSerialization();
                    }
                } else {
                    Pool[i]._SetActive(false);
                }
                return;
            }
        }
        Debug.Log("Return failed");
    }

    public override void Shuffle(){}

    public override void Clear(){
        if(!Networking.IsOwner(Networking.LocalPlayer,gameObject))return;
        for(int i=0;i<Pool.Length;i++){
            Pool[i].SetActive(false);
            actives[i]=false;
            changes[i]=true;
        }
        RequestSerialization();
    }


}
