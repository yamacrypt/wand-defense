
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MagicJewelSyncedPool : IObjectPool
{
    [SerializeField]int maxSpawn=6;
    void Start()
    {
        for(int i=0;i<Pool.Length;i++){
            Pool[i].SetActive(false);
        }
         if(Pool.Length>20){
            Debug.LogError("WARN Pool.Length<20");
        }
    }
    [UdonSynced]bool[] actives=new bool[20];
    [UdonSynced]bool[] syncedChanges=new bool[20];
    bool[] changes=new bool[20];

    int[] _shuffles=null;
    int[] shuffles{
        get{
            if(_shuffles==null){
                _shuffles=new int[Pool.Length];
                for(int i=0;i<Pool.Length;i++){
                    _shuffles[i]=i;
                }
            }
            return _shuffles;
        }
        set{
            _shuffles=value;
        }
    }

    public override void OnDeserialization() { 
        //.Log("OnDeserialization Pool");
        int spawn=0;
        for(int i=0;i<Pool.Length;i++){
            if(actives[i] && !syncedChanges[i])spawn++;
        }
        for(int i=0;i<Pool.Length;i++){
            if(actives[i]&& syncedChanges[i]){
                if(spawn<maxSpawn){
                    Pool[i].SetActive(true,true);
                    spawn++;
                } else {
                    continue;
                }
            }
            Pool[i].SetActive(actives[i],syncedChanges[i]);
        }
    }

    public override void OnPreSerialization() { 
        //Debug.Log("OnPreSerialization Pool");
        for(int i=0;i<Pool.Length;i++){
            syncedChanges[i]=changes[i];
            changes[i]=false;
        }
    }
    int index=0;
    public override GameObject TryToSpawn(){
        //Debug.Log("TrytoSpawn: " );
        if(!Networking.IsOwner(Networking.LocalPlayer,gameObject))return null;
        var next=0;
        var target=0;
        while(next<Pool.Length){
            target=shuffles[(index+next)%Pool.Length];
            if(actives[target]){
                next++;
            }else{
                break;
            }
        }
        //Debug.Log("TrytoSpawn: "+next);
        if(next==Pool.Length)return null;
        //target=(index+next)%Pool.Length;
        var obj=Pool[target];
        int spawn=0;
        for(int i=0;i<Pool.Length;i++){
            spawn+=actives[i]?1:0;
        }
        if(spawn<maxSpawn){
            obj.SetActive(true);
        }
        actives[target]=true;
        changes[target]=true;
        RequestSerialization();
        index=shuffles[(target+1)%Pool.Length];
        return obj.gameObject;
    }
    public override  void Return(GameObject obj){
        //if(!Networking.IsOwner(gameObject))return;
        if(obj==null)return;
        for(int i=0;i<Pool.Length;i++){
            if(Pool[i].gameObject==obj){
                Pool[i]._SetActive(false);
                return;
            }
        }
        Debug.Log("Return failed");
    }


    public override  void Shuffle(){
        if(!Networking.IsOwner(Networking.LocalPlayer,gameObject))return;
        index=0;
        for(int i=0;i<Pool.Length;i++){
            var target = (index+i)%Pool.Length;
            var r=Random.Range(0,Pool.Length);
            var tmp=shuffles[target];
            shuffles[target]=shuffles[r];
            shuffles[r]=tmp;
        }
    }

    public  override void Clear(){
        if(!Networking.IsOwner(Networking.LocalPlayer,gameObject))return;
        for(int i=0;i<Pool.Length;i++){
            Pool[i].SetActive(false);
            actives[i]=false;
            changes[i]=true;
        }
        RequestSerialization();
        index=0;
    }


}
