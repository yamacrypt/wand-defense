
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Explosion : UdonSharpBehaviour
{
    int power;
    IObjectPool pool;
    ParticleSystem particle{
        get{
            if(_particle==null){
                _particle = GetComponent<ParticleSystem>();
            }
            return _particle;
        }
    }
    ParticleSystem _particle;
    DamageProcessController dpc;
    /*AudioSource audioSource{
        get{
            if(_audioSource==null){
                _audioSource = GetComponent<AudioSource>();
            }
            return _audioSource;
        }
    }
    AudioSource _audioSource;*/
    CapsuleCollider _col;

    CapsuleCollider col{
        get{
            if(_col==null){
                _col = GetComponent<CapsuleCollider>();
            }
            return _col;
        }
    }

    protected bool isOwner=false;

    EnemyAura weakAura=EnemyAura.None;

    float distanceBonus=1f;
    bool isHomingDamageBonus=false;

    public virtual void Activate(Vector3 pos,int power,float distanceBonus, DamageProcessController dpc, IObjectPool pool,bool isHomingDamageBonus,EnchantItem item)
    {
        this.isOwner=true;
        this.isHomingDamageBonus=isHomingDamageBonus;
        this.distanceBonus=distanceBonus;
        this.dpc=dpc;
        this.pool=pool;
        this.transform.position =pos;
        this.power = power;
        // if level <3 一瞬で消えるバグ発生
        this.transform.localScale = Vector3.one*((1.0f+this.power/10.0f) *1.5f +  item.CalcExpRange());
        //thresholdTime = particle.main.duration;
        timer=0f;
        //audioSource.Play();
        // trigger判定の正常化
        col.enabled=false;
        SendCustomEventDelayedFrames("EnableCollider",2);
    }

    public void EnableCollider(){
        col.enabled=true;
    }

    string GunTarget="GunTarget";

    void OnTriggerEnter(Collider other)
    {
        if(other.name!=GunTarget || !isOwner)return;
        ////Debug.Log("explode");
        var enemy = other.transform.parent.GetComponent<Enemy>();
        if(enemy==null || enemy.IsInPool)return;
        Explode(enemy);
    }
    void Explode(Enemy enemy){
        if(!isOwner)return;
        dpc.InflictExplosionDamage(enemy,power,distanceBonus,isHomingDamageBonus);
    }

    float timer=-1f;
    [SerializeField]float thresholdTime=0.5f;
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
