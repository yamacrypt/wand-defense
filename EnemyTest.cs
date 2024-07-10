
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using BuildSoft.UdonSharp.Collection;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EnemyTest : UdonSharpBehaviour
{
    [SerializeField]int Life=10;
    [SerializeField]int Attack=5;

    [SerializeField]VRCObjectPool pool;


    public bool IsInPool=>!this.gameObject.activeSelf;

    [UdonSynced, FieldChangeCallback(nameof(syncedLife))] private byte _syncedLife =10;

    public byte syncedLife{
        get{return _syncedLife;}
        set{
            _syncedLife=value;
            if(isOwner){
                localSyncedLife=value;
            } else {
                if(syncedLife>0)localSyncedLife = syncedLife;
            }
        }
    }

    bool needSync;
    byte _damagePerSync=0;
    byte damagePerSync{
        get{return _damagePerSync;}
        set{
            _damagePerSync=value;
            if(_localSyncedLife - damagePerSync<=0  )
            {
                BeKilled(); 
            }
        }
    }
    
    [SerializeField]int _localSyncedLife=10;
    public int localSyncedLife{
        get{return _localSyncedLife;}
        set{
            _localSyncedLife=value;
            if(_localSyncedLife - damagePerSync<=0  )
            {
                BeKilled(); 
            }
        }
    }

    int currentLife=>localSyncedLife-damagePerSync;

    [SerializeField]GameObject VisibleObj;

    int fakeDamage=0;

    public byte CheckLocalDamage(){
        var damage = damagePerSync;
        damagePerSync=0;
        return damage;
    }


    public void TakeDamage(byte damage,bool IsOwnerOfDamageSource){
        if(IsInPool || damage==0)return;
        if(IsOwnerOfDamageSource){
            //Debug.Log("TakeDamage: "+damage);
            if(isOwner){
                int overflowCheck=syncedLife;
                overflowCheck-=damage;
                if(overflowCheck>byte.MinValue){
                    syncedLife-=damage;
                } else {
                    syncedLife=0;
                }
                RequestSerialization();
            } else {
                //currentLife-=damage;
                int overflowCheck=damagePerSync;
                overflowCheck+=damage;
                if(overflowCheck<byte.MaxValue){
                    damagePerSync+=damage;
                } else {
                    damagePerSync=byte.MaxValue;
                }
            }
        } else {
            fakeDamage+=damage;
        }
    }
    bool isTriggerKillEvent=false;
    public void BeKilled(){
        if(isTriggerKillEvent)return;
        isTriggerKillEvent=true;
        //Debug.Log("Death");
        if(isOwner && !IsInPool){
           BeKilledProcess();
        } else {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "DeathProcess");
            VisibleObj.SetActive(false);
        }
    }

    [SerializeField]int dropThreshold=10;

    public void BeKilledProcess(){
        DelayDeath();
        player.GetExperience(1);
    }

    public void DelayDeath(){
        SendCustomEventDelayedFrames("Death",2);
    }

    public void Death(){
        pool.Return(this.gameObject);
    }

    public bool isAlive=>currentLife>0 ;
    public bool isFakeAlive=>currentLife-fakeDamage>0 ;

    Enemy nearestEnemy;

    public void SetNearestEnemy(Enemy enemy){
        nearestEnemy=enemy;
    }

    public Enemy NearestEnemy => nearestEnemy;

    Vector3 target;

    Rigidbody rg;
    CharacterController cc;

    bool isOwner=>Networking.IsOwner(Networking.LocalPlayer, gameObject);
    void Start()
    {
        //isOwner=Networking.IsOwner(Networking.LocalPlayer, gameObject);
        rg=GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        target = core.position;//Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
    }

    void OnEnable()
    {
        fakeDamage=0;
        isTriggerKillEvent=false;
        VisibleObj.SetActive(true);
        if(!isOwner){
            localSyncedLife= (int)(Life);
            //preSyncedLife=currentLife;
            needSync=false;
        } else {
            // WARNING BYTE value
            syncedLife= (byte)(Life);
        }

    }

    void OnDisable()
    {
        isTriggerKillEvent=true;
    }

    [SerializeField]Transform core;

    [SerializeField]SearchNearEnemy searchNearEnemy;
    void FixedUpdate()
    {
        var dir = (target-transform.position).normalized;
        if(cc !=null){
            cc.SimpleMove(dir);
        } else if(rg!=null){
            rg.velocity=dir;
        }
        if(nearestEnemy!=null && nearestEnemy.IsInPool){
            nearestEnemy=null;
            searchNearEnemy.enabled=false;
            SendCustomEventDelayedFrames("ActiveSearchNearEnemy",2);
        }
    }
    public void ActiveSearchNearEnemy(){
        searchNearEnemy.enabled=true;
    }
    //[SerializeField]VRCObjectPool dropPool;
 

    [SerializeField]Player player;        
}
