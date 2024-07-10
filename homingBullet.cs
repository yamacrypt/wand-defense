
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class homingBullet : Bullet
{
    string NearEnemy="NearEnemy";
    Enemy target;
    public override void Init(GunController gc,Vector3 velocity,Vector3 position, IObjectPool bulletPool,EnchantItem item){
        base.Init(gc,velocity,position,bulletPool,item);
        isOwner=true;
        target=null;
        magnitude=rg.velocity.magnitude;
    }

    float magnitude;
    protected override void _OnTriggerEnter(Collider col)
    {
        if(col.name==NearEnemy){
            if(target==null){
                var enemy = col.transform.parent.GetComponent<Enemy>();
                if(enemy.isAlive)target=enemy;
            }
        } else {
            base._OnTriggerEnter(col);
        }
    }

    protected override bool IsHoming(){
        return true;
    }

    [SerializeField]CapsuleCollider col;

    protected override void _FixedUpdate()
    {
        base._FixedUpdate();
        if(isOwner&&target!=null){
            if(!target.isAlive){
                if(target.NearestEnemy_valArray!=null && target.NearestEnemy_valArray.Length==1){
                    target=target.NearestEnemy_valArray[0];
                }
                if(target==null||!target.isAlive){
                    target=null;
                    col.enabled=false;
                    SendCustomEventDelayedFrames(nameof(EnableCol),2);
                    return;
                }
            }
            rg.velocity=(target.GunTarget-this.transform.position).normalized*magnitude;
        }
    }

    public void EnableCol(){
        col.enabled=true;
    }
}
