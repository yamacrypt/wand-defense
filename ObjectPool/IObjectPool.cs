
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonObjectPool{
        
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class IObjectPool : UdonSharpBehaviour
    {
        public ObjectPoolItem[] Pool; 
        public virtual GameObject TryToSpawn(){
            Debug.LogError("Need to override TryToSpawn");
            return null;
        }

        public virtual void Return(GameObject obj){
            Debug.LogError("Need to override Return");
        }
        public void Return(ObjectPoolItem obj){
            Return(obj.gameObject);
        }

        public virtual void Shuffle(){
            Debug.LogError("Need to override Shuffle");
        }

        public virtual void Clear(){
            Debug.LogError("Need to override Clear");
        }

    }
}