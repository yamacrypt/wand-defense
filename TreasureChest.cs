
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TreasureChest : UdonSharpBehaviour
{
    [SerializeField]Animator animator;
    [SerializeField]AudioSource audioSource;
    [SerializeField]BattleStart battlerStart;
    [SerializeField]IObjectPool itemPool;
    void Start()
    {
      self.SetActive(false);
    }
  
  [SerializeField]ObjectPoolItem self;

  public override void Interact()
  {
    if(!animator.GetBool("isOpen")){
      SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"Open");
      if(audioSource!=null)audioSource.Play();
      int wave=battlerStart.SyncedWave;
      if(wave==battlerStart.ClearWave){
        for(int i=0;i<3;i++){
          var item =itemPool.TryToSpawn();
          if(item==null)return;
          var enchant=item.GetComponent<EnchantItem>();
          if(enchant==null)return;
          enchant.Init(ItemGrade.Rare);
        }
        return;
      }
      int count=0;
      if(wave>battlerStart.ClearWave){
        count=3;
      } else if(wave>battlerStart.ClearWave/2){
        count=2;
      } else {
        count=1;
      }
      for(int i=0;i<count;i++){
        var item =itemPool.TryToSpawn();
        if(item==null)break;
        var enchant=item.GetComponent<EnchantItem>();
        if(enchant==null)continue;
        int normal=6;
        int magic=3 + wave/2;
        int rare=1 + wave;
        int val = Random.Range(0,normal+magic+rare);
        if(val<normal){
          enchant.Init(ItemGrade.Normal);
        } else if(val<normal+magic){
          enchant.Init(ItemGrade.Magic);
        } else {
          enchant.Init(ItemGrade.Rare);
        }
      }
      SendCustomEventDelayedSeconds(nameof(Return),3f);
    }
  }

  public void Return(){
    self.SetActive(false);
  }

  public void _OnDisable(){
    animator.SetBool("isOpen",false);
  }

  public void Open(){
    animator.SetBool("isOpen",true);
  }

  public void Close(){
    animator.SetBool("isOpen",false);
  }
}
