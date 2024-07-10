
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LightningChain : UdonSharpBehaviour
{
    IObjectPool pool;
    [SerializeField]LightningChainMaker maker;
    //[SerializeField]AudioSource audioSource;
    public virtual void Activate(IObjectPool pool,Vector3 start,Vector3 end){
        this.pool=pool;
        timer=0f;
        isOwner=true;
        Init(start,end);
        //audioSource.Play();
    }
    protected void Init(Vector3 start,Vector3 end){
        maker.StartObject.transform.position = start;
        maker.EndObject.transform.position = end;
        //maker.Trigger();
    }
    protected bool isOwner=false;
    float timer=-1f;
    [SerializeField]float thresholdTime=0.5f;
    void FixedUpdate()
    {
        if(timer<0f || !isOwner)return;
        timer   += Time.deltaTime;
        if(timer>thresholdTime){
            timer=-1f;
            isOwner=false;
            //Debug.Assert(pool!=null);
            //audioSource.Stop();
            if(pool!=null)pool.Return(this.gameObject);
        }
    }
}
