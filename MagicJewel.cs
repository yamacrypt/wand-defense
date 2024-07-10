
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public enum JewelElement{
    Fire,
    Lightning,
    Cold,
    Black,
    White,
    None
}
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MagicJewel : UdonSharpBehaviour
{
    [SerializeField]MagicJewelSyncedPool pool;
    [SerializeField]JewelElement element;
    [SerializeField]Player player;
    [SerializeField]ParticleSystem ps;
    void Start()
    {
        
    }
    VRCPickup _pickup;
    VRCPickup pickup{
        get{
            if(_pickup==null){
                _pickup=GetComponent<VRCPickup>();
            }
            return _pickup;
        }
    }

    [UdonSynced]Vector3 _syncedPosition;

    public override void OnDeserialization() { 
        //Debug.Log("OnDeserialization Jewel "+ _syncedPosition);
        if(rg) {
            rg.MovePosition(_syncedPosition);
        } else{
            this.transform.position=_syncedPosition;
        }
    }

    public void setPosition(Vector3 position){
        rg.MovePosition(position);
        _syncedPosition=position;
        //Debug.Log("setPosition "+ position);
        RequestSerialization();
    }   

    bool _isUsed=false;

    bool isUsed{
        get{
            return _isUsed;
        }
        set{
            _isUsed=value;
            //pickup.pickupable=!value;
        }
    }
    public void DropSelf(){
        pickup.Drop();
    }
    public override void OnPickupUseDown(){
       if(isUsed)return;
       bool result = player.TryToImproveAbility(element);
       if(result){
        isUsed=true;
        SendCustomEventDelayedFrames("DropSelf",5);
        Debug.Assert(pool!=null);
        pool.Return(gameObject);
        // TODO: テロップ表示
        //ReturnToPool();
       }
    }

    public override void OnPickup(){
        ps.Stop();
    }

    public override void OnDrop(){
        isUsed=true;
        pool.Return(gameObject);
    }

    Rigidbody _rg;
    Rigidbody rg{
        get{
            if(_rg==null){
                _rg=GetComponent<Rigidbody>();
            }
            return _rg;
        }
    }

    public void _OnEnable()
    {
        // [WARN]rotation調整
        //this.transform.rotation = Quaternion.Euler(-90, 0, 0);
        isUsed=false;
        OnDeserialization();
        //Debug.Log("jewel OnEnable");
        /*mesh.enabled=true;
        ps.Play();
        RequestSerialization();*/
    }

    public void _OnDisable()
    {
        isUsed=true;
        //Debug.Log("jewel OnDisable");
        //ps.Stop();
    }

    
}
