
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Bomb : UdonSharpBehaviour
{
    [SerializeField]DamageProcessController dpc;
    void Start()
    {
        
    }

    int power;

    GunController gc;

    IObjectPool pool;

    float distanceBonus=1f;

    protected bool isOwner=false;
    bool isHomingDamageBonus=false;
    public virtual void Activate(Vector3 pos,GunController gc,float distanceBonus, DamageProcessController dpc, IObjectPool pool,bool isHomingDamageBonus)
    {
        this.isHomingDamageBonus=isHomingDamageBonus;
        this.dpc=dpc;
        this.pool=pool;
        this.distanceBonus=distanceBonus; 
        this.transform.position =pos;
        this.gc = gc;
        this.power =  gc.FirePower;
        col.enabled=false;
        isOwner=true;
        timer=0f;
        SendCustomEventDelayedFrames("EnableCollider",2);
    }

    public void EnableCollider(){
        col.enabled=true;
    }



    SphereCollider _col;

    SphereCollider col{
        get{
            if(_col==null){
                _col = GetComponent<SphereCollider>();
            }
            return _col;
        }
    }
    string GunTarget="GunTarget";
    void OnTriggerEnter(Collider other)
    {
        if(!isOwner || other.name!=GunTarget)return;
        var enemy = other.transform.parent.GetComponent<Enemy>();
        if(enemy==null || enemy.IsInPool)return;
        dpc.GenerateExplosion(gc.SyncedExplosionPool,enemy.transform.position,power,distanceBonus,isHomingDamageBonus);
        isOwner=false;
        if(pool!=null)pool.Return(this.gameObject);
    }

    float timer=-1f;
    [SerializeField]float thresholdTime=3f;
    void FixedUpdate()
    {
        if(timer<0f || !isOwner)return;
        timer   += Time.deltaTime;
        if(timer>thresholdTime){
            timer=-1f;
            //particle.Stop();
            //audioSource.Stop();
            //Debug.Assert(pool!=null);
            isOwner=false;
            if(pool!=null)pool.Return(this.gameObject);
        }
    }
}
