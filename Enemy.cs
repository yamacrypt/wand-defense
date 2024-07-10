
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using BuildSoft.UdonSharp.Collection;
using System;

public enum EnemyAura{
    None,White,Black,Red,Blue,Yellow
}
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Enemy : UdonSharpBehaviour
{
    [SerializeField] Transform gunTarget;
    public Vector3 GunTarget=>gunTarget!=null ? gunTarget.position : this.transform.position;
    public bool IsBoss=>isBoss;
    [SerializeField]int Life=10;
    [SerializeField]int Attack=5;

    [SerializeField]IObjectPool pool;

    public IObjectPool myPool=>pool;

    [SerializeField]LifeBar lifeBar;


    bool isInPool=true;
    public bool IsInPool=>isInPool;

    [SerializeField]GameSetting setting;

    [UdonSynced, FieldChangeCallback(nameof(syncedLife))] private byte _syncedLife =10;

    public byte syncedLife{
        get{return _syncedLife;}
        set{
            _syncedLife=value;
            if(Networking.LocalPlayer!=null && Networking.LocalPlayer.isMaster){
                localSyncedLife=value;
            } else {
                if(syncedLife>0){
                    localSyncedLife = syncedLife;
                }
                //else DelayDeath();
            }
        }
    }


    [SerializeField]int experience=1;

    byte _damagePerSync=0;
    byte damagePerSync{
        get{return _damagePerSync;}
        set{
            _damagePerSync=value;
        }
    }
    
    [SerializeField]int _localSyncedLife=10;
    public int localSyncedLife{
        get{return _localSyncedLife;}
        set{
            _localSyncedLife=value;
            if(lifeBar!=null)lifeBar.Percentage=(float)CurrentLife/(float)MaxLife;
        }
    }

    int CurrentLife=>Math.Max(0,localSyncedLife-damagePerSync);

    //[SerializeField]GameObject VisibleObj;

    public byte CheckLocalDamage(){
        var damage = damagePerSync;
        damagePerSync=0;
        
        return damage;
    }

    /*public void Slow75(){
        slowMultiplier=1.0f/4.0f;
        PlaySlowEffect();
    }

    public void Slow100(){
        slowMultiplier=0f;
        PlaySlowEffect();
    }

    public void Slow100Sync(){
        if(slowMultiplier>0f)
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Slow100));
    }*/

    [SerializeField]SpriteRenderer shockEffect;
    bool isVulnerable=false;
    public void BecomeVulnerable(){
        isVulnerable=true;
        if(!IsInPool){
            if(shockEffect!=null)shockEffect.enabled=true;
        }
        Debug.Log("become vulnerable");
    }

     public void BecomeVulnerableSync(){
        if(!isVulnerable){
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(BecomeVulnerable));
        }
    }

    /*public void Slow75Sync(){
        if(slowMultiplier>1.0f/4.0f)
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Slow75));
    }*/

    void PlaySlowEffect(){
        if(iceParticles!=null){
            foreach(var p in iceParticles){
                p.Play();
            }
        }
    }

    public void Slow50(){
        slowMultiplier=1.0f/2.0f;
        PlaySlowEffect();
    }

    public void Slow50Sync(){
        if(slowMultiplier>1.0f/2.0f)
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Slow50));
    }


    float slowMultiplier=1f ;
    EnemyAura playerAura=>setting.MyPlayer.TargetAura;
    public void TakeDamage(int damage){
        if(IsInPool || damage==0)return;
        float v=isVulnerable ? 1.5f : 1;
        float fd=isBoss ? damage*bossDamageReduction : damage;
        fd *= v;
        fd *= setting.StageMonsterDamageReduction;
        if(this.auraType != EnemyAura.None){
            if(this.auraType == playerAura){
                fd*=2;
            } else {
                fd/=2;
            }
        }
        int damageInt=(int)fd;
        if(damageInt>byte.MaxValue)damageInt=byte.MaxValue;
        byte d = (byte)damageInt;
        if(Networking.LocalPlayer.isMaster){
            int overflowCheck=syncedLife;
            overflowCheck-=d;
            if(overflowCheck>byte.MinValue){
                syncedLife-=d;
            } else {
                syncedLife=0;
                BeKilled(); 
            }
            RequestSerialization();
        } else {
            //currentLife-=d;
            int overflowCheck=damagePerSync;
            overflowCheck+=d;
            if(overflowCheck<byte.MaxValue){
                damagePerSync+=d;
            } else {
                damagePerSync=byte.MaxValue;
            }
            if(lifeBar!=null)lifeBar.Percentage=(float)CurrentLife/(float)MaxLife;
            if(!isAlive){
                BeKilled(); 
            }
        }
    }

    public void TakeSyncDamage(byte damage){
        if(!Networking.LocalPlayer.isMaster)return;
        if(IsInPool || damage==0)return;
        byte d=damage;
        int overflowCheck=syncedLife;
        overflowCheck-=d;
        if(overflowCheck>byte.MinValue){
            syncedLife-=d;
        } else {
            syncedLife=0;
            BeKilled(); 
        }
        RequestSerialization();
    }
    bool isTriggerKillEvent=false;
    public void BeKilled(){
        if(isTriggerKillEvent || IsInPool)return;
        isTriggerKillEvent=true;
        Debug.Log("Death");
        if(Networking.LocalPlayer.isMaster){
           BeKilledProcess();
        } else {
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(BeKilledProcess));
            DelayDeath();
            //VisibleObj.SetActive(false);
        }
    }
    [SerializeField]EnemyAura _auraType=EnemyAura.None;
    public EnemyAura auraType{
        get{return _auraType;}
        set{
            _auraType=value;
            //RequestSerialization();
        }
    }

    [SerializeField]bool isBoss=false;

    [SerializeField]KillEventController killEventController;
    public void BeKilledProcess(){
        DelayDeath();
        if(killEventController!=null)killEventController.ProcessKillEvent(experience,this);
    }

    public void DelayDeath(){
        SendCustomEventDelayedFrames(nameof(Death),2);
    }

    [SerializeField]AudioSource deathSound;
    [SerializeField]AudioSource appearSound;

    public void Death(){
        if(isBoss&& deathSound!=null){
            deathSound.Stop();
            deathSound.Play();
        }
        pool.Return(this.gameObject);
    }

    public bool isAlive=>CurrentLife>0 ;

    //gc対策
    public Enemy[] NearestEnemy_valArray;

    [SerializeField]float bossDamageReduction=0.2f;

    Vector3 target;

    Rigidbody rg;
    CharacterController cc;

    //bool Networking.LocalPlayer.isMaster=false;//Networking.IsOwner(Networking.LocalPlayer, gameObject);
    void Start()
    {
        //Networking.LocalPlayer.isMaster=Networking.LocalPlayer.isMaster[0];
        if(NearestEnemy_valArray==null || NearestEnemy_valArray.Length<1){
            NearestEnemy_valArray= new Enemy[1]{null};
        } else {
            NearestEnemy_valArray[0]=null;
        }
        //isOwner=Networking.IsOwner(Networking.LocalPlayer, gameObject);
        rg=GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        target = core ? core.transform.position : Vector3.zero;//Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        if(core!=null)CalcDirInterval();
    }

    [SerializeField]SkinnedMeshRenderer auraSkin;


    byte MaxLife=>(Life * setting.MonsterLifeMultiplier * 0.85f) > byte.MaxValue ? byte.MaxValue : (byte)(Life * setting.MonsterLifeMultiplier* 0.85f);

    public void _OnEnable()
    {
        //Debug.Log("Enemy OnEnable");
        damagePerSync=0;
        slowMultiplier=1f;
        isInPool=false;
        isTriggerKillEvent=false;
        isVulnerable=false;
        //VisibleObj.SetActive(true);
        if(Networking.LocalPlayer!=null && !Networking.LocalPlayer.isMaster){
            //Debug.Log(Life * setting.MonsterLifeMultiplier);
            localSyncedLife= MaxLife;
            //preSyncedLife=currentLife;
        } else {
            // WARNING BYTE value
            syncedLife= MaxLife;
        }
        if(NearestEnemy_valArray==null || NearestEnemy_valArray.Length<1){
            NearestEnemy_valArray= new Enemy[1]{null};
        } else {
            NearestEnemy_valArray[0]=null;
        }
        if(core!=null)CalcDir();
        searchNearEnemy.enabled=false;
        CheckNearEnemy(false);
        SendCustomEventDelayedFrames(nameof(ActiveSearchNearEnemy),2);
        if(isBoss&& appearSound!=null){
            appearSound.Stop();
            appearSound.Play();
        }
        if(iceParticles!=null){
            foreach(var p in iceParticles){
                p.Stop();
            }
        }
        if(shockEffect!=null)shockEffect.enabled=false;
        if(auraSkin!=null){
            if(auraType!=EnemyAura.None){
                auraSkin.enabled=true;
                if(auraType==EnemyAura.Black){
                    if(auraSkin.material.HasProperty("_Color"))auraSkin.material.SetColor("_Color",Color.black);
                    if(auraSkin.material.HasProperty("_EmissionColor"))auraSkin.material.SetColor("_EmissionColor",Color.gray);
                } else {
                    if(auraSkin.material.HasProperty("_Color"))auraSkin.material.SetColor("_Color",Color.white);
                    if(auraSkin.material.HasProperty("_EmissionColor"))auraSkin.material.SetColor("_EmissionColor",GetAuraColor()*AuraColorIntensity);
                }
            } else {
                auraSkin.enabled=false;
            }
        }
    }

    [SerializeField]int AuraColorIntensity=20;

    void CheckNearEnemy(bool toggle){
        int dir = toggle ? 1: -1;
        searchNearEnemy.transform.position+=Vector3.down*100f *dir;
    }

    Color GetAuraColor(){
        Debug.Log("AuraType:"+ auraType);
        switch(auraType){
            case EnemyAura.Red:
                return Color.red ;
            case EnemyAura.Blue:
                return Color.blue;
            case EnemyAura.Yellow:
                return Color.yellow;
            case EnemyAura.Black:
                return Color.black;
            case EnemyAura.White:
                return Color.white;
            default:
                return new Color(1f,1f,1f,0f);
        }
    }

    [SerializeField]ParticleSystem[] iceParticles;


    /*public override void OnDeserialization(){
        if(auraSkin!=null){
            if(auraType!=EnemyAura.None){
                auraSkin.enabled=true;
                if(auraType==EnemyAura.Black){
                    if(auraSkin.material.HasProperty("_Color"))auraSkin.material.SetColor("_Color",Color.black);
                    if(auraSkin.material.HasProperty("_EmissionColor"))auraSkin.material.SetColor("_EmissionColor",Color.gray);
                } else {
                    if(auraSkin.material.HasProperty("_Color"))auraSkin.material.SetColor("_Color",Color.white);
                    if(auraSkin.material.HasProperty("_EmissionColor"))auraSkin.material.SetColor("_EmissionColor",GetAuraColor()*24);
                }
            } else {
                auraSkin.enabled=false;
            }
        }
    }*/

    public void _OnDisable()
    {
        //Debug.Log("Enemy OnDisable");
        //damagePerSync=0;  damgesync対策でコメントアウトしてる
        slowMultiplier=1f;
        isInPool=true;
        searchNearEnemy.enabled=false;
        isTriggerKillEvent=true;
        if(iceParticles!=null){
            foreach(var p in iceParticles){
                p.Stop();
            }
        }
        if(shockEffect!=null)shockEffect.enabled=false;
        if(auraSkin!=null)auraSkin.enabled=false;
    }

    public void AttackCore(){
        if(!Networking.LocalPlayer.isMaster)return;
        if(core==null || core.isGameOver || !isAlive)return;
        //core.TakeDamage(Attack);
        SendCustomEventDelayedSeconds(nameof(DelayAttack),1.5f);
    }

    public void DelayAttack(){
        if(core==null || core.isGameOver || !isAlive)return;
        core.TakeDamage(Attack);
        if(!isBoss){
            Death();
        } else {
            // TODO gameoverじ確実に止める
            SendCustomEventDelayedSeconds(nameof(DelayAttack),3f);
        }
    }

    [SerializeField]CrystalCore core;

    [SerializeField]SearchNearEnemy searchNearEnemy;


    Vector3 targetDir=Vector3.zero;

    [SerializeField]float velocityModifier=1f;

    public void CalcDir(){
        if(!IsInPool){
            targetDir = (target-transform.position).normalized * velocityModifier * slowMultiplier * setting.MonsterSpeedMultiplier * 0.95f;
            //Debug.Log(target+" "+ transform.position+" "+targetDir);
            if(NearestEnemy_valArray[0]!=null && NearestEnemy_valArray[0].IsInPool){
                NearestEnemy_valArray[0]=null;
                searchNearEnemy.enabled=false;
                CheckNearEnemy(false);
                SendCustomEventDelayedFrames(nameof(ActiveSearchNearEnemy),2);
            }/* else if(NearestEnemy_valArray[0]==null){
                searchNearEnemy.enabled=false;
                SendCustomEventDelayedFrames("ActiveSearchNearEnemy",2);
            }*/
        }
    }
    public void CalcDirInterval(){
        CalcDir();
        SendCustomEventDelayedSeconds("CalcDirInterval",1f);
    }
    void FixedUpdate()
    {
        if(core==null || IsInPool)return;
        //var dir = (target-transform.position).normalized;
        if(cc !=null){
            cc.SimpleMove(targetDir);
        } else if(rg!=null){
            rg.velocity=targetDir;
        }
    }
    public void ActiveSearchNearEnemy(){
        CheckNearEnemy(true);
        searchNearEnemy.enabled=true;
    }
    //[SerializeField]VRCObjectPool dropPool;
}
