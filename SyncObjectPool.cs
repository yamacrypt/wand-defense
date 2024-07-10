﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncObjectPool : IObjectPool
{
    void Start()
    {
        for(int i=0;i<Pool.Length;i++){
            Pool[i].SetActive(false);
        }
        int maxSpawnCount = actives.Length; 
        if(Pool.Length>maxSpawnCount){
            Debug.LogError("UdonObjectPool Setting Error: Pool Length  must be less than"+maxSpawnCount);
        }
        if(actives.Length!=syncedChanges.Length){
            Debug.LogError("UdonObjectPool Setting Error: actives.Length!=syncedChanges.Length");
        }
    }
    [UdonSynced]bool[] actives=new bool[30]; // You can change this value to fit your needs.
    [UdonSynced]bool[] syncedChanges=new bool[30]; // You can change this value to fit your needs.
    bool[] _changes=null;

    bool[] changes{
        get{
            if(_changes==null){
                _changes=new bool[Pool.Length];
            }
            return _changes;
        }
        set{
            _changes=value;
        }
    }

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
        for(int i=0;i<Pool.Length;i++){
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
    public override  GameObject TryToSpawn(){
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
        var res =obj.SetActive(true);
        if(res){
            actives[target]=true;
            changes[target]=true;
            RequestSerialization();
            index=shuffles[(target+1)%Pool.Length];
        }
        return obj.gameObject;
    }
    public override  void Return(GameObject obj){
        //if(!Networking.IsOwner(gameObject))return;
        if(obj==null)return;
        for(int i=0;i<Pool.Length;i++){
            if(Pool[i].gameObject==obj){
                if(Networking.IsOwner(Networking.LocalPlayer,gameObject)){
                    var res =Pool[i].SetActive(false);
                    if(res){
                        actives[i]=false;
                        changes[i]=true;
                        RequestSerialization();
                    }
                } else {
                    Pool[i]._SetActive(false);
                }
                return;
            }
        }
        Debug.Log("Return failed");
    }

  
    public  override void Shuffle(){
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
