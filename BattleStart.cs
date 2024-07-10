
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BattleStart : UdonSharpBehaviour
{
  [SerializeField]GameSetting setting;
  [SerializeField]AudioSource startSE;
  void Start()
  {
    localPlayer=Networking.LocalPlayer;
  }

  VRCPlayerApi localPlayer;

  [SerializeField]float spawnInterval=1f;


  public override void Interact()
  {
    if (Networking.LocalPlayer.isMaster /*&& !isStart*/)
    {
      //isStart=true;
      StartBattle();   
    }
    
  }
  public int Wave=>wave;
  int wave=0;
  [UdonSynced]int syncedWave=0;

  public int SyncedWave=>syncedWave;

  public override void OnDeserialization()
  {
    wave=syncedWave;
  }
  [SerializeField]KillEventController killEventController;

  void ResetGame(){
    wave=0;
    killEventController.Reset();
    setting.Reset();
    isContinue=false;
    SpawnCount=0;
    core.Reset();
    isEnd=false;
    // TODO show reset view temp
    ResetSync();
  }

  void ResetSync(){
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(ResetPlayer));

  }

  public void ResetPlayer(){
    player.Reset();
  }

  public void DebugGame(int wave, int spawnCount){
    this.wave=wave;
    this.SpawnCount=spawnCount;
  }

  public void KillBoss(){
    deltaLimitTime+=10000;
  }
  
  [SerializeField]CrystalCore core;

  [SerializeField]AudioSource battleBGM;
  void StartBattle(){
    if(!isWaveEnd || isEnd )return;
    /*if(isEnd){
      ResetGame();
      return;
    }*/
    isWaveEnd=false;
    bossFlag=false;
    setting.CheckParameterAll();
    ResetAllEnemyPool();
    JewelPool.Clear();
    JewelPool.Shuffle();
    itemPool.Clear();
    //SpawnStart();
    wave++;
    syncedWave=wave;
    if(explainText!=null)explainText.enabled=false;
    RequestSerialization();
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(StartWaveSync));   
    SendCustomEventDelayedSeconds(nameof(ProcessWaveStart),1);
  }

  bool isPlaying=false;

  public void ContinueWaveSync(){
    if(battleBGM!=null && !isPlaying){
      isPlaying=true;
      battleBGM.Play();
    }
    player.ResetFake();
    waveStartEffect.ShowTemp(wave,clearWave);
    gameClearView.Hide();
    treasureChest.SetActive(false);
  }

  [SerializeField]WaveStartEffect waveStartEffect;

  public void StartWaveSync(){
    //this.transform.position = this.transform.position + new Vector3(0, -100, 0);
    //setting.CheckParameter();
    //startSE.Play();
    ContinueWaveSync();

  }

  public void WaveClearShowUp(){
    //if(battleBGM!=null)battleBGM.Stop();
    waveStartEffect.ClearWave(wave);
    treasureChest.SetActive(true);
  }

  public void WaveClearShowUpSync(){
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(WaveClearShowUp));
  }

  bool isEnd=false;



  public void SpawnInterval(){
    if(isWaveEnd||isEnd)return;
    if(!isRestTime){
      Spawn();
      if(bossFlag){
        bossFlag=false;
        SpawnBoss();
      }
    }
    SendCustomEventDelayedSeconds(nameof(SpawnInterval),spawnInterval/setting.MonsterAmountMultiplier);
  }

  SyncObjectPool[] targetSpawners;

  bool bossFlag=false;

  /*void WaveStart(){
    if(isWaveEnd || isEnd)return;
    SendCustomEventDelayedSeconds(nameof(ProcessWaveStart),1);
  }*/

  int limitTime=-1;

  public void ProcessWaveStart(){
    limitTime=-1;
    targetSpawners = new SyncObjectPool[0];
    int c = wave /3;
    setting.WaveLifeMultiplier=1f+0.2f*c;
    restDuration=10;
    if(wave>=9){
      restDuration=0;
    } else if(wave>=8){
      restDuration=5;
    }
    switch(wave){
      case 1:
        if(EnemyPools.Length>=1)targetSpawners = new SyncObjectPool[]{EnemyPools[0]};
        limitTime=3;
        colorInterval=300; // 出現させない
        break;
      case 2:
        if(EnemyPools.Length>=2)targetSpawners = new SyncObjectPool[]{EnemyPools[0],EnemyPools[1]};
        limitTime=60;
        colorInterval=300; // 出現させない
        break;
      case 3:
        if(EnemyPools.Length>=3)targetSpawners = new SyncObjectPool[]{EnemyPools[0],EnemyPools[1],EnemyPools[2]};
        limitTime=70;
        colorInterval=50;
        break;
      case 4:
        if(EnemyPools.Length>=4)targetSpawners = new SyncObjectPool[]{EnemyPools[0],EnemyPools[1],EnemyPools[2],EnemyPools[3]};
        limitTime=100;
        colorInterval=40;
        break;
      case 5:
        if(EnemyPools.Length>=4)targetSpawners = new SyncObjectPool[]{EnemyPools[0],EnemyPools[1],EnemyPools[2],EnemyPools[3]};
        limitTime=110;
        colorInterval=40;
        break;
      case 6:
        if(EnemyPools.Length>=4)targetSpawners = new SyncObjectPool[]{EnemyPools[0],EnemyPools[1],EnemyPools[2],EnemyPools[3]};
        limitTime=240;
        SendCustomEventDelayedSeconds(nameof(ActivateBossFlag),30);
        colorInterval=40;
        break;
      default:
        targetSpawners = EnemyPools;
        limitTime=150;
        colorInterval=endlessColorInterval;
        break;
    }
    SpawnInterval();
  }
  [SerializeField]int endlessColorInterval=20;
  int restInterval=30;
  int restDuration=10;
  int colorInterval=60;


  void SpawnColored(){
    if(isWaveEnd||isEnd)return;
    int targetIndex = UnityEngine.Random.Range(0,coloredEnemyPools.Length);
    var targetPool = coloredEnemyPools[targetIndex];
    int targetColor = UnityEngine.Random.Range(1,6);
    EnemyAura targetAura=player.TargetAura;
    // TODO:random gene ConvertIntToEnemyAura(targetColor)

    var res = targetPool.TryToSpawn(targetAura);
    for(int i=0;i<coloredEnemyPools.Length;i++){
      if(res)break;
      targetIndex =(targetIndex+1)%coloredEnemyPools.Length;
      targetPool = coloredEnemyPools[targetIndex];
      res = targetPool.TryToSpawn(targetAura);
    }
    if(res){
      SpawnCount+=targetPool.Pool.Length;
      RequestSerialization();
    }
    //SendCustomEventDelayedSeconds(nameof(SpawnColoredInterval),colorInterval);
  
  }


  EnemyAura ConvertIntToEnemyAura(int color){
    switch(color){
      case 1:
        return EnemyAura.White;
      case 2:
        return EnemyAura.Black;
      case 3:
        return EnemyAura.Red;
      case 4:
        return EnemyAura.Blue;
      case 5:
        return EnemyAura.Yellow;
      default:
        return EnemyAura.None;
    }
  }
  bool isContinue=false;
  
  int coloredSpawnCount=0;
  int restTimeCount=0;
  float deltaLimitTime=0;
  bool isRestTime=false;
  void FixedUpdate()
  {
    if(limitTime<0){
      deltaLimitTime=0;
      coloredSpawnCount=0;
      restTimeCount=0;
      isRestTime=false;
      restTimeText.enabled=false;
      return;
    }
    if(localPlayer!=null && !localPlayer.isMaster)return;
    deltaLimitTime+=Time.deltaTime;
    restTimeText.enabled=true;
    restTimeText.text = TimeSpan.FromSeconds(RestTime).ToString(@"mm\:ss");
    if(bossBeatTime>=0)bossBeatTime+=Time.deltaTime;
    if(deltaLimitTime>=limitTime){
      deltaLimitTime=0;
      limitTime=-1;
      WaveClear();
    } else {
      if(deltaLimitTime>colorInterval*(coloredSpawnCount+1)){
        SpawnColored();
        coloredSpawnCount+=1;
      }
      if(deltaLimitTime>restInterval*(restTimeCount+1)){
        restTimeCount+=1;
        isRestTime=true;
        if(wave>clearWave){
          SendCustomEventDelayedSeconds(nameof(EndRestTime),restDuration/2);
        } else {
          SendCustomEventDelayedSeconds(nameof(EndRestTime),restDuration);
        }
      }
    }
    
  }

  [SerializeField]TMPro.TextMeshProUGUI restTimeText;

  public float RestTime => limitTime==-1 ? 0 : limitTime - deltaLimitTime;



  public void EndRestTime(){
    isRestTime=false;
  }

  public void ActivateBossFlag(){
    bossFlag=true;
  }

  bool isWaveEnd=true;


  [SerializeField]SyncObjectPool[] EnemyPools;
 
  [SerializeField]SyncObjectPool BossPool;

  [SerializeField]MagicJewelSyncedPool JewelPool;
  public void SpawnBoss(){
    if(BossPool!=null){
      bossBeatTime=0;
      BossPool.TryToSpawn();
      SpawnCount+=1;
      RequestSerialization();
    }
    
    //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(bossWarningEffect.ShowTemp));
  }

  //[SerializeField]PopupEffect bossWarningEffect;

  public void Spawn(){
    Debug.Log("Spawn ENemy "+targetSpawners.Length);
     if(targetSpawners!=null){
      for(int i=0;i<targetSpawners.Length;i++){
        if(targetSpawners[i]!=null)targetSpawners[i].TryToSpawn();
      }
    }
    SpawnCount+=targetSpawners.Length;
    RequestSerialization();
  }

  /*[UdonSynced, FieldChangeCallback(nameof(syncedSpawnCount))]*/ private int _spawnCount =0;
  public int SpawnCount{
      get=>_spawnCount;
      set{
        if(wave > clearWave){;
          _spawnCount=value;  
          setting.CalcBattleStage(value);
        }
      }
  }

  public void WaveClear(){
    isWaveEnd=true;
    ResetAllEnemyPool();
    //JewelPool.Clear();
    if(isEnd)return;
    if(wave==clearWave){
      isEnd=true;
      limitTime=-1;
      GameClearSyncDelay();
    } else {
      WaveClearShowUpSync();
    }
  }

  public void GameClearSync(){
      gameClearView.ShowForClear(bossBeatTime);
      SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(GameClear));
  }

  void GameClearSyncDelay(){
    SendCustomEventDelayedSeconds(nameof(GameClearSync),1.5f);
  }


  float bossBeatTime=-1;
  public void GameClear(){
     if(battleBGM!=null){
      battleBGM.Stop();
      isPlaying=false;
    }
    //gameClearView.ShowForClear(bossBeatTime);
    bossBeatTime=-1;
    treasureChest.SetActive(true);
  }
  
  [SerializeField]int clearWave=10;
  public int ClearWave=>clearWave;
  [SerializeField]GameObject continueView;

  public void ShowUpContinueView(){
    continueView.SetActive(true);
  }

  public void DismissContinueView(){
    if(!Networking.LocalPlayer.isMaster)return;
    ResetGame();
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(DismissContinueViewSync));
  }

  public void DismissContinueViewSync(){
    continueView.SetActive(false);
    gameClearView.Hide();
  }

  void ResetAllEnemyPool(){
    foreach(var pool in EnemyPools){
      pool.Clear();
      pool.Shuffle();
    }
    BossPool.Clear();
    foreach(var pool in coloredEnemyPools){
      pool.Clear();
    }
  }

  [SerializeField]ObjectPoolItem treasureChest;
  [SerializeField]IObjectPool itemPool;
  [SerializeField]TMPro.TextMeshProUGUI explainText;

  public void StartEndless(){
    if(!Networking.LocalPlayer.isMaster)return;
    if(clearWave!=wave)return;
    //if(isContinue)return; isContinueの廃止
    isWaveEnd=true;
    isEnd=false;
    core.Reset();
    StartBattle();
  }


  public void Continue(){
    if(clearWave<wave)return;
    Debug.Log("continue");
    if(!Networking.LocalPlayer.isMaster)return;
    if(!isWaveEnd)return;
    isWaveEnd=false;
    isEnd=false;
    bossFlag=false;
    isContinue=true;
    setting.CheckParameterAll();
    ResetAllEnemyPool();
    JewelPool.Clear();
    JewelPool.Shuffle();
    itemPool.Clear();
    core.Reset();
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(DismissContinueViewSync));
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(ContinueWaveSync));   
    SendCustomEventDelayedSeconds(nameof(ProcessWaveStart),1);
  }

  [SerializeField]ColoredEnemySyncedPool[] coloredEnemyPools;

  public void GameOver(){
    limitTime=-1;
    isEnd=true;
    isWaveEnd=true;
    //spawnCount=0;
    ResetAllEnemyPool();
    //JewelPool.Clear();

    //setting.Reset();
    //killEventController.Reset();
    //Debug.Log("GameOver");
    if(wave>clearWave){
      gameClearView.ShowForEndless(wave,clearWave);
    }
    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,nameof(GameOverSync));
  }

  [SerializeField]GameClearView gameClearView;

  [SerializeField]Player player;

  public void GameOverSync(){
    if(battleBGM!=null){
      battleBGM.Stop();
      isPlaying=false;
    }
    if(wave>clearWave){
      //gameClearView.ShowForEndless(wave,clearWave);
    } else {
      ShowUpContinueView();
    }
    //wave=0;
  }
}
