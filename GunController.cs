
using System;
using BuildSoft.UdonSharp.Collection;
using DigitalRuby.LightningBolt;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
//[RequireComponent(typeof(WeaponList),typeof(EnemyList))]
public class GunController : UdonSharpBehaviour
{
    [SerializeField]NewLocalObjectPool bulletPool;
    [SerializeField]SyncObjectPool bulletSyncedPool;
    [SerializeField]SyncObjectPool explosionSyncedPool;
    [SerializeField]SyncObjectPool lightningSyncedPool;
    [SerializeField]SyncObjectPool bombSyncedPool;

    public SyncObjectPool SyncedExplosionPool => explosionSyncedPool;
    public SyncObjectPool SyncedLightningPool => lightningSyncedPool;
    public SyncObjectPool SyncedBombPool => bombSyncedPool;

    [SerializeField]Player player;

    public int BlackPower => player.BlackPower;
    public int FirePower => player.FirePower;
    public int ColdPower => player.ColdPower;
    public int LightningPower => player.LightningPower;
    public int WhitePower => player.WhitePower;

    public JewelElement mainJewelElement;

    public EnemyAura TargetEnemyAura(){
        switch(mainJewelElement){
            case JewelElement.Black:
                return EnemyAura.Black;
            case JewelElement.Fire:
                return EnemyAura.Red;
            case JewelElement.Cold:
                return EnemyAura.Blue;
            case JewelElement.Lightning:
                return EnemyAura.Yellow;
            case JewelElement.White:
                return EnemyAura.White;
            default:
                return EnemyAura.None;
        }
    }

 

    void Start()
    {
        if(bulletSyncedPool==null){
            Debug.Log("WARNING: bulletSyncedPool is null");
        }
        /*for(var i=0;i<bulletSyncedPool.Pool.Length;i++){
            bulletSyncedPool.TryToSpawn();
        }
        SendCustomEventDelayedSeconds("Clear",1);
        */
        // debug
        //OnPickupUseDown();
    }

    [SerializeField]DamageSyncByOne damageSyncByOne;

    [SerializeField]AudioSource shootAudioSource;

    void OnDisable()
    {
    }

    public override bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner){
        player.UnTap();
        return true;
    }

    /*public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player){
        Debug.Log("OnOwnershipTransferred:");
        if(player==Networking.LocalPlayer){
            if(this.player.IsYourGun(this)){
                this.player.UnTap();
            }
        }
    }*/
    [SerializeField]NewLocalObjectPool homingBulletPool;


    float ThresholdFireTime => 1.0f / FireRate();

    float FireRate(){
        float multi=0f;
        if(FirePower>=5){
            //Debug.Log("killcount: " + player.KillCount10 + "");
            multi = (float)player.KillCount10 * 0.03f;
        }
        return baseFireRate * (1f + WhitePower* 0.075f + player.EquippedItem.CalcAttackSpeed())*(1.0f+multi);
    }

    float deltaTime=0f;
    float isStuckingTime=0f;


    void FixedUpdate()
    {
        if(isPickingUp && onTrigger){
            deltaTime+=Time.deltaTime;
            if(deltaTime>ThresholdFireTime){
                deltaTime=0;
                if(player.IsEquipGun(this))Fire();
            }
        }
        if(isStacking){
            isStuckingTime+=Time.deltaTime;
            if(isStuckingTime>5){
                isStacking=false;
                isStuckingTime=0;
                if(bulletSyncedPool!=null)bulletSyncedPool.Clear();
            }
        } else{
            isStuckingTime=0;
        }
    }
     int _penetrationCount=0;
    public int PenetrationCount{
        get{
            return _penetrationCount;
        }
        set{
            _penetrationCount=Math.Max(0,value);
        }
    }
    int diffusionPoint=0;
    [SerializeField]int diffusionMin=1;
    [SerializeField]int diffusionMax=6;

    void Fire(){
        if(shootAudioSource!=null){
            shootAudioSource.Stop();
            shootAudioSource.Play();
        }
        if (diffusionPoint<diffusionMax){
            diffusionPoint+=1;
        }
        for(var i=0;i<calcBulletCount(BlackPower)+player.EquippedItem.CalcBullet();i++){
            var syncedBullet = bulletSyncedPool!=null? bulletSyncedPool.TryToSpawn() :null;
            //Debug.Log("syncedBullet: " + syncedBullet);
            if(syncedBullet!=null){
                var bc=syncedBullet.GetComponent<SyncedBullet>();
                if(bc!=null){
                    isStacking=false;
                    bc.Init(this,(getVel(bulletDirs[0].position,bulletDirs[i%5].position)).normalized*15,bulletSource.position,bulletSyncedPool,player.EquippedItem);
                }
            } else {
                isStacking=true;
                var bullet = bulletPool!=null ? bulletPool.TryToSpawn() : null;
                if(bullet!=null){
                    var bc=bullet.GetComponent<Bullet>();
                    if(bc!=null)bc.Init(this,getVel(bulletDirs[0].position,bulletDirs[i%5].position).normalized*15,bulletSource.position,bulletPool,player.EquippedItem);
                }
            }
        }
        for(var i=0;i<3;i++){
            if(PenetrationCount<1)break;
            PenetrationCount-=1;
            var bullet = homingBulletPool.TryToSpawn();
            if(bullet!=null){
                var bc=bullet.GetComponent<homingBullet>();
                if(bc!=null)bc.Init(this,(getVel(bulletDirs[0].position,bulletDirs[(i+2)%3].position)).normalized*10f,bulletSource.position,homingBulletPool,player.EquippedItem);
            }
        }
        for(var i=0;i<CalcHomingBulletCount()+player.EquippedItem.CalcHoming();i++){
            var bullet = homingBulletPool.TryToSpawn();
            if(bullet!=null){
                var bc=bullet.GetComponent<homingBullet>();
                if(bc!=null)bc.Init(this,(getVel(bulletDirs[0].position,bulletDirs[(i+1)%3].position)).normalized*10f,bulletSource.position,homingBulletPool,player.EquippedItem);
            }
        }
    }

    Vector3 getVel(Vector3 straight,Vector3 dir){
        var diff = (dir - straight); 
        return straight + diff * ((float)diffusionPoint / (float)diffusionMax) - bulletSource.position;
    }

    int calcBulletCount(int power){
        if(power>=8)return 5;
        else if(power>=6)return 4;
        else if(power>=3)return 3;
        else if(power>=1)return 2;
        else return 1;
    }

    int CalcHomingBulletCount(){
        if(WhitePower<5)return 0;
        int res=0;
        if(FirePower==0)res++;
        if(ColdPower==0)res++;
        if(LightningPower==0)res++;
        return res;
    }

    bool isStacking=false;

  
    

    bool onTrigger=false;

    public override void OnPickupUseDown(){
        if(!player.IsEquipGun(this)){
            return;
        }
        deltaTime=0f;
        diffusionPoint=diffusionMin;
        onTrigger=!onTrigger;
        triggerView.ShowTemp(onTrigger);
        if(!Networking.LocalPlayer.IsOwner(this.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,this.gameObject);
        }
    }

    [SerializeField]float baseFireRate=1f;
    [SerializeField]Transform bulletSource;
    [SerializeField]Transform[] bulletDirs;

    bool isPickingUp=false;

    public override void Interact(){
        //Debug.Log("OnInteract");
    }
    [SerializeField]TriggerView triggerView;
    public override void OnPickup(){
        if(player.IsEquipGun(this)){
            return;
        }
        Debug.Log("OnPickup");
        player.Equip(this);
        isPickingUp=true;
        deltaTime=0f;
        PenetrationCount=0;
        if(!Networking.LocalPlayer.IsOwner(this.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,this.gameObject);
            SendCustomEventDelayedSeconds(nameof(ClearSyncPool),10f);
        }
        if(bulletSyncedPool!=null && !Networking.LocalPlayer.IsOwner(bulletSyncedPool.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,bulletSyncedPool.gameObject);
            foreach(var obj in bulletSyncedPool.Pool){
               Networking.SetOwner(Networking.LocalPlayer,obj.gameObject);
            }
        }
        if(SyncedExplosionPool!=null && !Networking.LocalPlayer.IsOwner(SyncedExplosionPool.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,SyncedExplosionPool.gameObject);
            foreach(var obj in SyncedExplosionPool.Pool){
               Networking.SetOwner(Networking.LocalPlayer,obj.gameObject);
            }
        }  
        if(SyncedLightningPool!=null && !Networking.LocalPlayer.IsOwner(SyncedLightningPool.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,SyncedLightningPool.gameObject);
            foreach(var obj in SyncedLightningPool.Pool){
               Networking.SetOwner(Networking.LocalPlayer,obj.gameObject);
            }
        }  
        if(SyncedBombPool!=null && !Networking.LocalPlayer.IsOwner(SyncedBombPool.gameObject)){
            Networking.SetOwner(Networking.LocalPlayer,SyncedBombPool.gameObject);
            foreach(var obj in SyncedBombPool.Pool){
               Networking.SetOwner(Networking.LocalPlayer,obj.gameObject);
            }
        }  
        if(damageSyncByOne!=null)damageSyncByOne.TransferOwnership(Networking.LocalPlayer,TargetEnemyAura());
        else {
            Debug.Log("WARNING: damageSyncByOne is null");
        }

    }

    public void ClearSyncPool(){
        if(bulletSyncedPool!=null)bulletSyncedPool.Clear();
        if(SyncedExplosionPool!=null)SyncedExplosionPool.Clear();
        if(SyncedLightningPool!=null)SyncedLightningPool.Clear();
        if(SyncedBombPool!=null)SyncedBombPool.Clear();
    }

    /*public void SetBulletSyncPool(SyncObjectPool pool){
        bulletSyncedPool=pool;
    }*/

    public override void OnDrop(){
        if(player.IsEquipGun(this)){
            player.UnEquip();
        }
        PenetrationCount=0;
        Debug.Log("OnDrop");
        isPickingUp=false;
        onTrigger=false;
    }
    public override void OnPickupUseUp(){
        //onTrigger=false;
        //deltaTime=0f;
    }

}
