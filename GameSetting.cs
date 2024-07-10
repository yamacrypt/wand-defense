
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameSetting : UdonSharpBehaviour
{
    float _monsterLifeMultiplier=1;
    float _monsterAmountMultiplier=1;
    public float MonsterLifeMultiplier=>_monsterLifeMultiplier*WaveLifeMultiplier; //* Difficulty;
    public float MonsterAmountMultiplier => _monsterAmountMultiplier * battleStageAmountMultiplier;//* (1.0f+(float)Math.Sqrt((double)battleStage-1.0)/3.0f);//* Difficulty;
    public float MonsterSpeedMultiplier => battleStageSpeedMultiplier;

    float stageMonsterDamageReduction=1f;

    public float StageMonsterDamageReduction => Mathf.Clamp01(stageMonsterDamageReduction);


    public float PartyAmountMultiplier=>_monsterAmountMultiplier;
    public float Difficulty=1;

    [UdonSynced] float _waveLifeMultiplier=1;
    public float WaveLifeMultiplier{
        get{
            return _waveLifeMultiplier;
        }
        set{
            _waveLifeMultiplier=value;
            RequestSerialization();
        }
    }
    void Start()
    {
        if(Networking.LocalPlayer!=null){
            isMaster=new bool[1]{Networking.LocalPlayer.isMaster};
        } else {
            isMaster=new bool[1]{true};
        }
        CheckParameter();
        //CheckParameterInterval();
    }
    /*public void CheckParameterInterval(){
        CheckParameter();
        SendCustomEventDelayedSeconds("CheckParameterInterval",30);
    }*/

    public override void OnPlayerJoined(VRC.SDKBase.VRCPlayerApi player){
        VRCPlayerApi.GetPlayers(players);
    }


    [SerializeField]Player myPlayer;
    public Player MyPlayer=>myPlayer;
    public override void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player){
        VRCPlayerApi.GetPlayers(players);
        CheckParameterAll();
        // todo: ここで呼び出して問題ないか検証
        //Debug.Log("isMaster before:"+isMaster[0]);
        if(Networking.LocalPlayer!=null)isMaster[0] = Networking.LocalPlayer.isMaster;
        //Debug.Log("isMaster new:"+isMaster[0]);
    }

    int playerCount=1;
    VRCPlayerApi[] gamePlayers= new VRCPlayerApi[16];
    VRCPlayerApi[] players= new VRCPlayerApi[16];

    public int GamePlayerCount=>playerCount;
    public VRCPlayerApi[] GamePlayers=>gamePlayers;
    public VRCPlayerApi[] Players=>players;

    public void CheckParameterAll(){
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"CheckParameter");
    }


    public void CheckParameter(){
        playerCount=VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi.GetPlayers(gamePlayers);
        var multiplier=Math.Sqrt(playerCount);
        var sqrt=Math.Sqrt(multiplier);
        _monsterAmountMultiplier = (float)(multiplier);
        _monsterLifeMultiplier = (float)(multiplier);
        StageUpCondition=(int)(stageUpCondition*_monsterAmountMultiplier);
        //stageUpFirstCondition=(int)(stageUpFirstCondition*sqrt);
    }

    // used by owner
    [UdonSynced, FieldChangeCallback(nameof(syncedBattleStage))]int _battleStage=0;
    public int syncedBattleStage{
        get=>_battleStage;
        set{
            if(_battleStage!=value){
                _battleStage=value;
                var multi=calcBattleStageMultipler(value);
                stageMonsterDamageReduction=1.0f / multi;
                float sqrt=(float)Math.Sqrt(multi);
                battleStageAmountMultiplier=sqrt;//Mathf.Sqrt(sqrt);
                battleStageSpeedMultiplier=sqrt ;//* Mathf.Sqrt(sqrt);//Mathf.Sqrt(battleStageLifeMultiplier);
                Debug.Log("stage:"+value+" multi:"+multi+" amount:"+battleStageAmountMultiplier+" speed:"+battleStageSpeedMultiplier);
            }
        }
    }

    float calcBattleStageMultipler(int stage){
        return 1.0f+stage/20.0f;
    }
    // used by owner
    // public int BattleStage=>battleStage;
    float battleStageAmountMultiplier=1;
    float battleStageSpeedMultiplier=1;

    // used by owner
    int _killedCount=0;
    [SerializeField]int stageUpCondition=50;
    int StageUpCondition=50;

    

    // used by owner
    public int KilledCount{
      get=>_killedCount;
      set{
        _killedCount=value;
        
      }
   }

   public void CalcBattleStage(int spawnCount){
        int stage=spawnCount/StageUpCondition;
        //Debug.Log("stage:"+stage+" spawnCount:"+spawnCount+" stageUpCondition:"+StageUpCondition);
        if(syncedBattleStage!=stage){
            syncedBattleStage=stage;
            RequestSerialization();
        }
   }

    public void Reset(){
        KilledCount=0;
        syncedBattleStage=0;
        WaveLifeMultiplier=1f;
        RequestSerialization();
    }

    

    

    bool[] isMaster;

    public bool[] IsMaster=>isMaster;

   
    
}
