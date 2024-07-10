
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SearchNearEnemy : UdonSharpBehaviour
{
    [SerializeField]Enemy me;
    //Enemy[] enemy_valArray;
    void Start()
    {
        //enemy_valArray= new Enemy[1];
    }
    string NearEnemy="NearEnemy";
    void OnTriggerEnter(Collider col)
    {
        if(col.name!=NearEnemy)return;
        if(me.NearestEnemy_valArray[0]!=null)return;
        ////Debug.Log("search Enemy!");
        var enemy = col.transform.parent.GetComponent<Enemy>();
        if(enemy==null || enemy == me || enemy.IsInPool)return;
        //enemy_valArray[0]=enemy;
        me.NearestEnemy_valArray[0]=enemy;//enemy_valArray;//.SetNearestEnemy(enemy_valArray);
        if(enemy.NearestEnemy_valArray!=null && enemy.NearestEnemy_valArray.Length>0 && enemy.NearestEnemy_valArray[0]==null)enemy.NearestEnemy_valArray[0]=me;
    }
}
