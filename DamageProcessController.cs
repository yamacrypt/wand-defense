
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public enum DamageType{
    Normal,
    Explosion,
    LightningChain,
    ProjectileSlash
}
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DamageProcessController : UdonSharpBehaviour
{
    [SerializeField]NewLocalObjectPool lightningChainPool;
    [SerializeField]NewLocalObjectPool explosionPool;
    [SerializeField]Player player;

    void Start()
    {
        
    }

   
    Vector3 upOne(Vector3 v){
        return new Vector3(v.x,v.y+1,v.z);
    }

    public void InflictWeaponDamage(GunController gc,Enemy enemy,float distanceBonus,bool isHomingDamageBonus){
        if(gc==null || !enemy.isAlive)return;
        int blackPower=gc.BlackPower;
        int whitePower=gc.WhitePower;
        int firePower=gc.FirePower;
        int lightningPower=gc.LightningPower;
        int coldPower=gc.ColdPower;
        var weakAura=gc.TargetEnemyAura();

        int damage = (int)((whitePower*2+10) * (1.0f+distanceBonus));
        float abilityDistanceBonus=blackPower>=10 ? distanceBonus : 0f;
        ////Debug.Log(damage);
        enemy.TakeDamage(damage);
        if(enemy.isAlive){
            if(coldPower>=5) {
                enemy.Slow50Sync();
            }
        }
        ////Debug.Log(enemy.transform.position);
        if(!enemy.isAlive){
            player.AddKillCount();
        }
        if(!enemy.isAlive && firePower>0 ){
            Debug.Log("Explosion");
            GenerateExplosion(gc.SyncedExplosionPool,enemy.transform.position,firePower,abilityDistanceBonus,isHomingDamageBonus);
            /*GameObject obj;
            if(gc.SyncedExplosionPool!=null ){
                obj = gc.SyncedExplosionPool.TryToSpawn();
                if(obj!=null){
                    var explosion = obj.GetComponent<SyncedExplosion>();
                    if(explosion!=null)explosion.Activate(enemy.transform.position,firePower,abilityDistanceBonus,this,gc.SyncedExplosionPool);
                }
            } else {
                obj = explosionPool.TryToSpawn();
                if(obj!=null){
                    var explosion = obj.GetComponent<Explosion>();
                    if(explosion!=null)explosion.Activate(enemy.transform.position,firePower,abilityDistanceBonus,this,explosionPool);
                    
                }
            }*/
        }
        ////Debug.Log("chainPower: "+lightningPower);
        if(lightningPower>0){
            Enemy startEnemy=enemy;
            Enemy[] endEnemy_valArray=enemy.NearestEnemy_valArray;
            int count = calcLightningChainCount(lightningPower) + player.EquippedItem.CalcChain();
            if(lightningPower>=10){
                count += calcLightningChainCount(coldPower);
            }
            ////Debug.Log("endEnemy: "+endEnemy);
            int i=0;
            float bonus = blackPower >=10 ? distanceBonus : 0f;
            for(i=0;i<count;i++){
                if(endEnemy_valArray[0]==null || endEnemy_valArray[0].IsInPool)break;
                InflictLightningDamage(endEnemy_valArray[0],gc,abilityDistanceBonus,isHomingDamageBonus,count);
                if(lightningPower>=5){
                    endEnemy_valArray[0].BecomeVulnerableSync();
                }
                if(gc.SyncedLightningPool!=null){
                    var chainObj = gc.SyncedLightningPool.TryToSpawn();
                    if(chainObj!=null){
                        var start = upOne(startEnemy.transform.position);
                        var end =upOne(endEnemy_valArray[0].transform.position);
                        startEnemy=endEnemy_valArray[0];
                        endEnemy_valArray = endEnemy_valArray[0].NearestEnemy_valArray;
                        var chain = chainObj.GetComponent<SyncedLightningChain>();
                        if(chain!=null)chain.Activate(gc.SyncedLightningPool,start,end);
                    } else {
                        startEnemy=endEnemy_valArray[0];
                        endEnemy_valArray = endEnemy_valArray[0].NearestEnemy_valArray;
                    }
                } else {
                    var chainObj = lightningChainPool.TryToSpawn();
                    if(chainObj!=null){
                        var start = upOne(startEnemy.transform.position);
                        var end =upOne(endEnemy_valArray[0].transform.position);
                        startEnemy=endEnemy_valArray[0];
                        endEnemy_valArray = endEnemy_valArray[0].NearestEnemy_valArray;
                        var chain = chainObj.GetComponent<LightningChain>();
                        if(chain!=null)chain.Activate(lightningChainPool,start,end);
                    } else {
                        startEnemy=endEnemy_valArray[0];
                        endEnemy_valArray = endEnemy_valArray[0].NearestEnemy_valArray;
                    }
                }
            }
        }
    }

    int calcLightningChainCount(int power){
        if(power>=8)return 4;
        else if(power>=6)return 3;
        else if(power>=3)return 2;
        else if(power>=1)return 1;
        else return 0;
    }

    float HomingDamageBonus(bool on){
        if(on)return 1.2f;
        else return 0;
    }

    public void InflictExplosionDamage(Enemy enemy, int power,float distanceBonus, bool isHomingDamageBonus){
        if(!enemy.isAlive)return;
        int damage=(int)((power*3+10) * (1.0f+distanceBonus) * (1.0f+HomingDamageBonus(isHomingDamageBonus)));
        enemy.TakeDamage(damage);
        if(!enemy.isAlive){
            player.AddKillCount();
        }
    }


    void InflictLightningDamage(Enemy enemy, GunController gc,float distanceBonus, bool isHomingDamageBonus,int chain){
        if(gc==null || !enemy.isAlive){
            // TODO: fakeDamageの実装
            return;
        }
        float chainBonus = gc.LightningPower>=10 ? 0.2f * chain : 0f;
        int damage=(int)((gc.LightningPower+5) * (1.0f+distanceBonus) * (1.0f+HomingDamageBonus(isHomingDamageBonus)*(1.0f+chainBonus)));
        Debug.Log("ilghtning daamge "+ damage);
        enemy.TakeDamage(damage);
        if(gc.LightningPower>=10){
            int coldPower=gc.ColdPower;
            if(enemy.isAlive){
                if(coldPower>=5) {
                    enemy.Slow50Sync();
                }
            }
            int firePower=gc.FirePower;
            if(!enemy.isAlive && firePower>0){
                Debug.Log("Explosion");
                GenerateExplosion(gc.SyncedExplosionPool,enemy.transform.position,firePower,distanceBonus,false);
                /*var obj = explosionPool.TryToSpawn();
                if(obj!=null){
                    var explosion = obj.GetComponent<Explosion>();
                    explosion.Activate(enemy.transform.position,firePower,distanceBonus,this,explosionPool);
                }*/
            }
        }
        if(!enemy.isAlive){
            player.AddKillCount();
        }
    }
    [SerializeField]NewLocalObjectPool bombPool;
    public void GenerateBomb(Vector3 pos,GunController gc, float distanceBonus, bool isHomingDamageBonus){
        if(gc.SyncedBombPool!=null){
            var obj = gc.SyncedBombPool.TryToSpawn();
            if(obj!=null){
                var bomb = obj.GetComponent<SyncedBomb>();
                bomb.Activate(pos,gc,distanceBonus,this,gc.SyncedBombPool,isHomingDamageBonus);
            }
            }else {
                var obj = bombPool.TryToSpawn();
                if(obj!=null){
                    var bomb = obj.GetComponent<Bomb>();
                    bomb.Activate(pos,gc,distanceBonus,this,bombPool,isHomingDamageBonus);
            }
        }
    }

    public void GenerateExplosion(IObjectPool syncPool,Vector3 pos,int firePower,float abilityDistanceBonus, bool isHomingDamageBonus){
        if(syncPool!=null ){
            var obj = syncPool.TryToSpawn();
            if(obj!=null){
                var explosion = obj.GetComponent<SyncedExplosion>();
                if(explosion!=null)explosion.Activate(pos,firePower,abilityDistanceBonus,this,syncPool,isHomingDamageBonus,player.EquippedItem);
            }
        } else {
            var obj = explosionPool.TryToSpawn();
            if(obj!=null){
                var explosion = obj.GetComponent<Explosion>();
                if(explosion!=null)explosion.Activate(pos,firePower,abilityDistanceBonus,this,explosionPool,isHomingDamageBonus,player.EquippedItem);
                
            }
        }
    }

}
