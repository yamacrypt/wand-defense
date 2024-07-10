
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Bullet : UdonSharpBehaviour
{
    Rigidbody _rg;
    protected Rigidbody rg{
        get{
            if(_rg==null)_rg = this.GetComponent<Rigidbody>();
            return _rg;
        }
    }
    IObjectPool bulletPool;
    
    void setPenetrationPower(int coldPower){
        penetrationPower=calcPeneCount(coldPower) + item.CalcPene();
    }

    int calcPeneCount(int power){
        if(power>=8)return 4;
        else if(power>=6)return 3;
        else if(power>=3)return 2;
        else if(power>=1)return 1;
        else return 0;
    }

    float velMag;

    protected bool isOwner=false;
    EnchantItem item;

    public virtual void Init(
        GunController gc,Vector3 velocity,Vector3 position, IObjectPool bulletPool,EnchantItem item){
        this.bulletPool=bulletPool;
        this.item=item;
        isOwner=true;
        if(gc==null)return;
        this.gc=gc;
        rg.MovePosition(position);
        velMag = (1+gc.ColdPower*0.1f);
        rg.velocity = velocity * velMag * 0.9f;
        setPenetrationPower(gc.ColdPower);
        penetration=penetrationPower;
        float eliminationTime=baseDuration*(1f+gc.BlackPower*0.1f);
        //SendCustomEventDelayedSeconds(nameof(ReturnToPool),eliminationTime);
        thresholdTime=eliminationTime;
        timer=0f;
        distanceBonus=0f;
        ////Debug.Log("bullet Init");
    }

    float baseDuration=0.6f;

    float distanceBonus=0f;

    float timer=-1f;
    float thresholdTime;
    void FixedUpdate()
    {
        _FixedUpdate();
    }

    protected virtual void _FixedUpdate()
    {
        if(timer<0f || !isOwner)return;
        timer   += Time.deltaTime;
        distanceBonus = timer / baseDuration * velMag;
        if(timer>thresholdTime){
            timer=-1f;
            distanceBonus=0f;
            //Debug.Assert(pool!=null);
            ReturnToPool();
        }
    }

    int penetrationPower=0;

    public void ReturnToPool(){
        if(isOwner){
            isOwner=false;
            timer=-1f;
            rg.MovePosition(this.transform.position + Vector3.down*100f);
            SendCustomEventDelayedFrames(nameof(Return),2);
        }
    }

    public void Return(){
       bulletPool.Return(this.gameObject);
    }

    DamageProcessController _dpc;
    DamageProcessController dpc{
        get{
            if(_dpc==null){
                _dpc = GameObject.Find("DPC").GetComponent<DamageProcessController>();
                Debug.Assert(_dpc!=null,"DPC Find Failed");
            }
            return _dpc;
        }
    }
    protected virtual bool IsHoming(){
        return false;
    }
    GunController gc;
    int penetration=0;
    string GunTarget="GunTarget";
    string Stage="Stage";
    string BulletStage="BulletStage";

    private void OnTriggerEnter(Collider col)
    {
        _OnTriggerEnter(col);
    }

    protected virtual void _OnTriggerEnter(Collider col)
    {
        if(!isOwner)return;
        if(col.name==BulletStage){
            ReturnToPool();
            return;
        }
        if(col.name==Stage){
            //Debug.Log("Gene Bomb");
            if(gc!=null && gc.FirePower>=10){
                dpc.GenerateBomb(transform.position,gc,distanceBonus,gc.WhitePower>=10? IsHoming() : false);
            }
            penetration--;
            if(penetration<0){
                ReturnToPool();
            }
            return;
        }
        var obj = col.transform.parent.gameObject;
        if (obj==null) return;
        var enemy = obj.GetComponent<Enemy>();
        if(enemy==null || !enemy.isAlive)return;
        //Debug.Log("WeaponEnter: " + col.gameObject.name);
        if(penetration>=0){
            penetration--;
            if(TryAttack(enemy)){
                if(penetration<0 || IsHoming()){;
                    ReturnToPool();
                } else{
                    if(gc.ColdPower>=10)gc.PenetrationCount+=1;   
                }
            }
        }
    }

    [SerializeField]NewLocalObjectPool splitPool;

    bool TryAttack(Enemy enemy){
        if(enemy == null || !enemy.isAlive || enemy.IsInPool){
            return false;
        }
        ////Debug.Log("TryAttack: count"+enemy.name);
        if(gc==null || !isOwner)return false;
        dpc.InflictWeaponDamage(gc,enemy,gc.BlackPower >=5 ? distanceBonus : 0f,isHomingDamageBonus: gc.WhitePower>=10? IsHoming() : false);
        return true;
    }
}
