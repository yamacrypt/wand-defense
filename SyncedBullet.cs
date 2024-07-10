
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedBullet : Bullet
{
    //IObjectPool bulletSyncedPool;

    [UdonSynced] private Vector3 _syncedVelocity;
    [UdonSynced] private Vector3 _syncedPosition;

    /*[UdonSynced]byte _syncedBlackPower=0;
    [UdonSynced]byte _syncedWhitePower=0;
    [UdonSynced]byte _syncedFirePower=0;
    [UdonSynced]byte _syncedLightningPower=0;
    [UdonSynced]byte _syncedColdPower=0;*/


    /*void setPenetrationPower(byte coldPower){
        penetrationPower=(coldPower+2)/3;
    }*/

    public override void OnDeserialization() { 
        //Debug.Log("OnDeserialization"+_syncedPosition+" "+_syncedVelocity);
        rg.MovePosition(_syncedPosition);
        rg.velocity = _syncedVelocity;
        isOwner=false;
        //float eliminationTime=2f*(1f+_syncedBlackPower*0.1f);
        //SendCustomEventDelayedSeconds(nameof(ReturnToPool),eliminationTime);
    }
    //float velMag=1f;

    public override void Init(GunController gc,Vector3 velocity,Vector3 position, IObjectPool bulletPool,EnchantItem item){
        base.Init(gc,velocity,position,bulletPool,item);
        isOwner=true;
        _syncedVelocity=rg.velocity;
        _syncedPosition=position;
        RequestSerialization();
    }

    //float baseDuration=1f;


    public void _OnEnable()
    {
        //OnDeserialization();
        //ps.Play();
    }

    //GunController gc;
    //int penetration=0;


    /*public override bool TryAttack(Enemy enemy){
        if(enemy == null || !enemy.isAlive || enemy.IsInPool){
            return false;
        }
        // fakedamage無視する。
        if(gc==null)return false;
        ////Debug.Log("TryAttack: count"+enemy.name);
        dpc.InflictWeaponDamage(gc,enemy,isOwner, _syncedBlackPower>=5 ? distanceBonus : 0);
        return true;
    }*/
}
