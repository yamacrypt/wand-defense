
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MagicJewelTest : UdonSharpBehaviour
{
    [SerializeField]JewelElement element;
    [SerializeField]Player player;
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

    BoxCollider _col;
    BoxCollider col{
        get{
            if(_col==null){
                _col=GetComponent<BoxCollider>();
            }
            return _col;
        }
    }

    MeshRenderer _mesh;
    MeshRenderer mesh{
        get{
            if(_mesh==null){
                _mesh=GetComponent<MeshRenderer>();
            }
            return _mesh;
        }
    }

    bool _isUsed=false;

    bool isUsed{
        get{
            return _isUsed;
        }
        set{
            _isUsed=value;
            pickup.pickupable=!value;
            col.enabled=!value;
        }
    }
    
    public void DropSelf(){
        pickup.Drop();
    }
    public override void OnPickupUseDown(){
       if(isUsed)return;
       bool result = player.ImproveAbilityTest(element);
       if(result){
        isUsed=true;
        SendCustomEventDelayedFrames("DropSelf",5);
        mesh.enabled=false;
        // TODO: テロップ表示
        //ReturnToPool();
       }
    }

    void OnEnable()
    {
        // [WARN]rotation調整
        this.transform.rotation = Quaternion.Euler(-90, 0, 0);
        isUsed=false;
        mesh.enabled=true;
    }

}
