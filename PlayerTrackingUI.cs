
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerTrackingUI : UdonSharpBehaviour
{
    Vector3 playerPosition;
    Quaternion playerRotation;
    VRCPlayerApi player;
    void Start()
    {
        player = Networking.LocalPlayer;
        if (player != null){
            var transform = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            this.transform.position = transform.position;
            this.transform.rotation = transform.rotation;
        } 
        //startPosition=this.transform.position;
        //startRotation=this.transform.rotation;
    }

    void Update(){
        if (player != null){
            var transform = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            var pos = transform.position;
            var rot = transform.rotation;
            playerPosition =pos;
            playerRotation = rot;
        }
    }

    private void LateUpdate()
    {
        transform.position = playerPosition;//Vector3.Lerp(transform.position, playerPosition, followMoveSpeed);

        //上と同じく回転をLerp関数で滑らかに補完
        //lockHorizonがtrueの場合はxzの角度を0にすることで水平に固定
        transform.rotation = playerRotation;//Quaternion.Lerp(transform.rotation, rot, followRotateSpeed);
        
    }
}
