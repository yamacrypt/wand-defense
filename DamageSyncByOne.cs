
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DamageSyncByOne : UdonSharpBehaviour
{
    [SerializeField ]IObjectPool[] enemyPools;
    Enemy[] enemies;
    [UdonSynced]byte[] syncedSumDamages= new byte[200]; 
    byte[] sumDamages= new byte[200]; 

    /*bool IsOwnerOfEnemyPool(){
        return Networking.IsOwner(localPlayer,enemyPools[0].gameObject);
    }*/
    public override void OnDeserialization()
    {
        Debug.Log("OnDeserialization DamageSyncController");
        if(setting.GamePlayerCount>=2 && setting.IsMaster[0]){
            //Debug.Log(syncedSumDamages[0]);
            for(var i=0;i<enemies.Length;i++){
                var enemy = enemies[i];
                enemy.TakeSyncDamage(syncedSumDamages[i]);
            }
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner,"ResetSumDamages");
        }
    }

    public override void OnPreSerialization(){
        Debug.Log("OnPreSerialization DamageSyncController");
        for(var i=0;i<enemies.Length;i++){
            syncedSumDamages[i]=sumDamages[i];
            sumDamages[i]=0;
        }
    }

    [SerializeField]GameSetting setting;

    VRCPlayerApi localPlayer;
    void Start()
    {
        localPlayer = Networking.LocalPlayer; 
        var enemyCount=0;
        for(var i=0;i<enemyPools.Length;i++){
            enemyCount+=enemyPools[i].Pool.Length;
        }
        enemies = new Enemy[enemyCount];
        if(enemyCount>200){
            Debug.LogError("WARN enemyCount <=200");
        }
        /*if(Networking.IsOwner(Networking.LocalPlayer,this.gameObject)){
            sumDamages = new byte[enemyCount];
            RequestSerialization();
        }*/
        int k=0;
        foreach(var poolC in enemyPools){
            for(var i=0;i<poolC.Pool.Length;i++){
                var enemy = poolC.Pool[i].GetComponent<Enemy>();
                Debug.Assert(enemy!=null);
                enemies[k]=enemy;
                k++;
            }
        }
    }

    EnemyAura weakAura;

    public void TransferOwnership(VRCPlayerApi player,EnemyAura aura){
        if(Networking.IsOwner(player,this.gameObject))return;
        Networking.SetOwner(player,this.gameObject);
        this.player=player;
        weakAura=aura;
    }

    [SerializeField]float intervalTime=1f;

    private float _timeElapsed;
    VRCPlayerApi player;

    //TODO: player leftじに更新
   //bool setting.IsMaster[0];

    private void FixedUpdate()
    {
        if(localPlayer != player || setting.IsMaster[0])return;
        // all pool are owned by instance owner
        //if(!Networking.IsOwner(Networking.LocalPlayer,enemyPools[0].gameObject)) return;
        _timeElapsed += Time.deltaTime;
        if (_timeElapsed  > intervalTime)
        {
            if(setting.GamePlayerCount >= 2 &&player!=null) {
                Debug.Log("damage sync by"+setting.IsMaster[0]);
                SyncDamageAll();
            }
            _timeElapsed = 0f;
        }
    }

    public void SyncDamageAll(){
        if(!Networking.IsOwner(localPlayer,this.gameObject)){
            Debug.Log("damage sync failed");
            return;
        }
        for(var j=0;j<enemies.Length;j++){
            SyncDamage(j);
        }
    }

    void SyncDamage(int index){
        var enemy = enemies[index];
        //if(enemy.IsInPool) return; 敵倒したときにsetActive(false)するため
        var damage = enemy.CheckLocalDamage();
        if(damage>0){
            sumDamages[index]+=damage;
            RequestSerialization();
        }
    }
}
