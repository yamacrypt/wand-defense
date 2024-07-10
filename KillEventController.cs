
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class KillEventController : UdonSharpBehaviour
{
    [SerializeField]GameSetting setting;
    [SerializeField]Player player;
    //[SerializeField]int dropThreshold=10;
    
    public void ProcessKillEvent(int experience,Enemy enemy){
        if(player!=null)player.GetExperience(experience);
        var dropPos=enemy.transform.position;
        setting.KilledCount+=1;
        killTemp+=1;
        var dropThreshold = player.requiredExp /5+2;
        var drop = ChanceToDrop(dropThreshold);
        if(drop!=null){
            drop.GetComponent<MagicJewel>().setPosition(dropPos+halfY);
        }
        if(enemy.IsBoss){
        battleStart.KillBoss();
        }
        
        /*if(enemy.auraType!=EnemyAura.None&& IsColoredClear(enemy.myPool)){
            if(treasurePool!=null){
                var treasure =treasurePool.TryToSpawn();
                if(treasure!=null)treasure.transform.position=dropPos;
            }
        }*/
        
    }

    Vector3 halfY= new Vector3(0,0.5f,0);
    public void Reset(){
        thresholdCount=0;
        dropPool2.Clear();
        //dropPool.Shuffle();
    }

    bool IsColoredClear(IObjectPool pool){
        Debug.Log("IsColoredClear");
        foreach(var item in pool.Pool){
            if(item.ActiveMode!=ActiveMode.Deactive){
                return true;
            }
        }
        return true;
    }
     int calcDropCount(){
        int count=0;
        foreach(var item in dropPool2.Pool){
            count += item.ActiveMode!=ActiveMode.Deactive ? 1 :0;
        }
        return count;
    }        
    // used by owner
    int thresholdCount=0;
    [SerializeField]MagicJewelSyncedPool dropPool2;
    [SerializeField]IObjectPool treasurePool;

    GameObject ChanceToDrop(int thresHold){
        if(!setting.IsMaster[0])return null;
        if(setting.KilledCount-thresholdCount>=thresHold && dropPool2!=null){
            thresholdCount+=thresHold;
            //if(calcDropCount()>=setting.MaxDropLimit)return null;
            return dropPool2.TryToSpawn();
        }
        return null;
    }


    void Start()
    {
        /*if(Networking.LocalPlayer!=null && Networking.LocalPlayer.isMaster){
            CheckDifficultyInterval();
        }*/
    }
    [SerializeField]BattleStart battleStart;

    int killTemp=0;
    int spawnTemp=0;

    /*public void CheckDifficultyInterval(){
        var spawnCount=battleStart.spawnCount-spawnTemp;
        spawnTemp=battleStart.spawnCount;
        if(spawnCount>0){
            float prop = (float)killTemp / (float)spawnCount;
            setting.Difficulty=Mathf.Clamp(prop,0.8f,1.2f);
        }
        killTemp=0;
        SendCustomEventDelayedSeconds(nameof(CheckDifficultyInterval),30);
    }*/
}
