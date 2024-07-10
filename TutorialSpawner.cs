
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TutorialSpawner : UdonSharpBehaviour
{
  [SerializeField]SyncObjectPool EnemyPool;
  [SerializeField]GameSetting testSetting;
  void Start()
  {
    if (Networking.IsOwner(Networking.LocalPlayer,EnemyPool.gameObject) /*&& !isStart*/)
    {
      //isStart=true;
      StartBattle();   
    }
  }

  [SerializeField]float spawnInterval=1f;

  public void StartBattle(){
    EnemyPool.Clear();
    EnemyPool.Shuffle();
    testSetting.CheckParameterAll();
    SpawnStart();
  }
  public void SpawnStart()
  {
    EnemyPool.TryToSpawn();
    SendCustomEventDelayedSeconds("SpawnStart",spawnInterval);
  }

}
