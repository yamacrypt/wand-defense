
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CrystalCore : UdonSharpBehaviour
{
    [SerializeField]BattleStart bs;
    [SerializeField]CrystalCoreView view;
    
    void Start()
    {
        view.Notify(syncedLife);
    }


    public void Reset(){
        syncedLife=100;
        RequestSerialization();
    }

    [UdonSynced, FieldChangeCallback(nameof(syncedLife))] private int _syncedLife =100;

    //
    public int syncedLife{
        get => _syncedLife;
        set
        {
            if(value>0)_syncedLife = value;
            else _syncedLife=0;
            if(isGameOver){
                bs.GameOver();
            }
            view.Notify(syncedLife);
        }
    }

    public void TakeDamage(int damage){
        if(Networking.IsOwner(Networking.LocalPlayer, gameObject) && !isGameOver){
            syncedLife-=damage;
            RequestSerialization();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"PlayDamageSound");
        }
    }

    public void PlayDamageSound(){
        if(audioSource!=null){
            audioSource.Stop();
            audioSource.Play();
        }
    }

    [SerializeField]AudioSource audioSource;

    public bool isGameOver=>syncedLife<=0;
    string GunTarget="GunTarget";
    private void OnTriggerEnter(Collider col)
    {
        if(col.name!=GunTarget || isGameOver)return;
        ////Debug.Log("crystal ore enter");
        if(!Networking.IsOwner(Networking.LocalPlayer, gameObject))return;
        var obj = col.transform.parent.gameObject;
        if (obj==null || !obj.activeSelf || !this.gameObject.activeSelf) return;
        var enemy = obj.GetComponent<Enemy>();
        if(enemy==null || !enemy.isAlive)return;
        ////Debug.Log("EnemyEnter: " + col.gameObject.name);
        enemy.AttackCore();
    }

    


}
