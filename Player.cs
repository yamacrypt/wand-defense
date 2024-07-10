
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Player : UdonSharpBehaviour
{
    int firePower=0;
    int blackPower=0; // 弾丸数増加と射程強化
    int whitePower=0; // 弾丸ダメージと発射レート増加
    int coldPower=0; // 敵速度低下と貫通
    int lightingPower=0;

     int fakeFirePower=0;
    int fakeBlackPower=0; // 弾丸数増加と射程強化
    int fakeWhitePower=0; // 弾丸ダメージと発射レート増加
    int fakeColdPower=0; // 敵速度低下と貫通
    int fakeLightingPower=0;

    [SerializeField]EnchantItem _equippedItem; // default none enchanted item
    EnchantItem equippedItem;

    public EnchantItem EquippedItem{
        get{
            if(equippedItem==null){
                return  _equippedItem;
            }
            return equippedItem;
        }
    }
    
    int calcPower(JewelElement e){
        if(this.yourGun != null && this.yourGun.mainJewelElement == e){
            return 2;
        } 
        return 0;
    }

    public int BlackPower => Math.Max(0,blackPower + calcPower(JewelElement.Black) +fakeBlackPower + EquippedItem.CalcBlackPower());
    public int FirePower => Math.Max(0,firePower + calcPower(JewelElement.Fire) +fakeFirePower + EquippedItem.CalcFirePower());
    public int ColdPower => Math.Max(0,coldPower + calcPower(JewelElement.Cold) +fakeColdPower + EquippedItem.CalcColdPower()); 
    public int LightningPower => Math.Max(0,lightingPower + calcPower(JewelElement.Lightning) +fakeLightingPower + EquippedItem.CalcLightningPower());

    public int WhitePower => Math.Max(0,whitePower + calcPower(JewelElement.White) +fakeWhitePower + EquippedItem.CalcWhitePower());


    public EnemyAura TargetAura=>yourGun!=null ? jewelToAura(yourGun.mainJewelElement) : EnemyAura.None;

    EnemyAura jewelToAura(JewelElement element){
        switch(element){
        case JewelElement.White:
            return EnemyAura.White;
        case JewelElement.Black:
            return EnemyAura.Black;
        case JewelElement.Fire:
            return EnemyAura.Red;
        case JewelElement.Cold:
            return EnemyAura.Blue;
        case JewelElement.Lightning:
            return EnemyAura.Yellow;
        default:
            return EnemyAura.None;
        }
    }
    GunController yourGun;
    bool isEquip=false;

    [SerializeField]GameSetting setting;

    public void Equip(GunController gun){
        yourGun=gun;
        isEquip=true;
        /*var index = GetPlayerIndex();
        if(index!=-1){
            Debug.Log("Equip: " + index);
            gun.SetBulletSyncPool(bulletPools[index]);
            damageSyncByOnes[index].TransferOwnership(Networking.LocalPlayer);
        } else {
            Debug.LogWarning("Equip: index is -1");
            UnEquip();
        }*/
        //gun.SetBulletSyncPool(bulletPools[index]);
        //damageSyncByOnes[index].TransferOwnership(Networking.LocalPlayer);
    }

    /*int GetPlayerIndex(){
        var player=Networking.LocalPlayer;
        VRCPlayerApi[] players=new VRCPlayerApi[8];
        VRCPlayerApi.GetPlayers(players);
        for(var i=0;i<players.Length;i++){
            if(players[i]==player)return i;
        }
        return -1;
    }*/

    void Start()
    {
        //ResetKillCountInterval();
        localPlayer=Networking.LocalPlayer;
        equippedItem=_equippedItem;
    }

    VRCPlayerApi localPlayer;

    public void ResetKillCountInterval(){
        killCount=0;
        SendCustomEventDelayedSeconds("ResetKillCountInterval",10);
    }

    public void UnEquip(){
        isEquip=false;
    }

    public void UnTap(){
        yourGun=null;
        isEquip=false;
    }

    public bool IsYourGun(GunController gun){
        //if(!isEquip)return false;
        return yourGun==gun;
    }

    public bool IsEquipGun(GunController gun){
        return isEquip && yourGun==gun;
    }

    public bool IsEquip=>isEquip;

    public bool HasYourGun(){
        return yourGun!=null;
    }

    [SerializeField]LevelUpEffect levelUpEffect;

    public void ResetFake(){
        fakeFirePower=0;
        fakeBlackPower=0;
        fakeWhitePower=0;
        fakeColdPower=0;
        fakeLightingPower=0;
    }

    public void Reset(){
        equippedItem=_equippedItem;
        firePower=0;
        blackPower=0;
        whitePower=0;
        coldPower=0;
        lightingPower=0;
        fakeFirePower=0;
        fakeBlackPower=0;
        fakeWhitePower=0;
        fakeColdPower=0;
        fakeLightingPower=0;
        currentLevel=0;
        killCount=0;
        if(Networking.IsOwner(Networking.LocalPlayer, this.gameObject)){
            ResetLevelSync();
        }
    }

    int killCount=0;

    public int KillCount10=>Math.Max(0,killCount);

    public void AddKillCount(){
        killCount++;
        SendCustomEventDelayedSeconds(nameof(SubKillCount),10);
    }

    public void SubKillCount(){
        if(killCount>0)killCount--;
    }

    void ResetLevelSync(){
        syncedLevel=0;
        syncedExp=0;
        RequestSerialization();
    }

    public void DebugReset(int level){
        syncedLevel=level;
        RequestSerialization();
    }

    [SerializeField]int maxLevel=10;
    [SerializeField]int maxPlayerLevel=20;

    [SerializeField]LevelBonusPopup levelBonusPopup;
    bool TryToAddPower(JewelElement type,int power){
        if(yourGun==null || yourGun.mainJewelElement== JewelElement.None)return false;
        if(getPower(type)+power > maxLevel)return false;
        switch(type){
            case JewelElement.Black:
                    blackPower+=power;
                    if(BlackPower==5 || BlackPower==10){
                        levelBonusPopup.ShowTemp(JewelElement.Black);
                    }
                    break;
                case JewelElement.White:
                    whitePower+=power;
                    if(WhitePower==5 || WhitePower==10){
                        levelBonusPopup.ShowTemp(JewelElement.White);
                    }
                    break;
                case JewelElement.Fire:
                    firePower+=power;
                    if(FirePower==5 || FirePower==10){
                        levelBonusPopup.ShowTemp(JewelElement.Fire);
                    }
                    break;
                case JewelElement.Lightning:
                    lightingPower+=power;
                    if(LightningPower==5 || LightningPower==10){
                        levelBonusPopup.ShowTemp(JewelElement.Lightning);
                    }
                    break;
                case JewelElement.Cold:
                    coldPower+=power;
                    if(ColdPower==5 || ColdPower==10){
                        levelBonusPopup.ShowTemp(JewelElement.Cold);
                    }
                    break;

        }
        return true;
    }

    int getPower(JewelElement type){
        switch(type){
            case JewelElement.Black:
                return BlackPower;
            case JewelElement.White:
                return WhitePower;
            case JewelElement.Fire:
                return FirePower;
            case JewelElement.Lightning:
                return LightningPower;
            case JewelElement.Cold:
                return ColdPower;
        }
        //Debug.LogError("getPower error");
        return 0;
    }

    [UdonSynced, FieldChangeCallback(nameof(syncedLevel))] private int _syncedLevel =0;
    [UdonSynced, FieldChangeCallback(nameof(syncedExp))] private int _syncedExp = 0;
    //  
    public int syncedLevel{
        get => _syncedLevel;
        set
        {
            if(value > _syncedLevel){
                levelUpEffect.ShowTemp(value,maxPlayerLevel);
            }
            _syncedLevel = value;
        }
    }

    public int syncedExp{
        get => _syncedExp;
        set
        {
            _syncedExp = value;
        }
    }
    
    int currentLevel=0;

    public int RestLevelUp=>syncedLevel-currentLevel;
    public bool TryToImproveAbility(JewelElement type){
        /*if(currentLevel==0){
            var res = TryToAddPower(type,3);
            if(res)currentLevel+=1;
            return res;
        } else if(currentLevel<syncedLevel){
            var res = TryToAddPower(type,1);
            if(res)currentLevel+=1;
            return res;
        }*/
        if(currentLevel<syncedLevel){
            var res = TryToAddPower(type,1);
            if(res)currentLevel+=1;
            return res;
        }
        return false;
    }

    public bool TryToImproveAbilityFromUI(JewelElement type){
        if(currentLevel<=syncedLevel-5){
            var res = TryToAddPower(type,1);
            if(res)currentLevel+=1;
            return res;
        }
        return false;
    }


    public bool ImproveAbilityTest(JewelElement type){
        if(currentLevel>=1)return false;
        fakeFirePower=0;
        fakeBlackPower=0;
        fakeWhitePower=0;
        fakeColdPower=0;
        fakeLightingPower=0;
        switch(type){
            case JewelElement.Black:
                fakeBlackPower=1;
                break;
            case JewelElement.White:
                fakeWhitePower=1;
                break;
            case JewelElement.Fire:
                fakeFirePower=1;
                break;
            case JewelElement.Lightning:
                fakeLightingPower=1;
                break;
            case JewelElement.Cold:
                fakeColdPower+=1;
                break;
        }
        return true;
    }


    public void EquipItem(EnchantItem item){
        if(equippedItem!=null){
            equippedItem.UnEquip();
        }
        equippedItem=item;
    }
    


    public int requiredExp=>(int)(((syncedLevel+1)*(syncedLevel+1)/4.5f+(syncedLevel+1)/1.5f+3)*setting.PartyAmountMultiplier);
    public void GetExperience(int exp){
        syncedExp+=exp;
        if(syncedExp>=requiredExp && maxPlayerLevel>syncedLevel){
            syncedExp-=requiredExp;
            syncedLevel+=1;
        }
        RequestSerialization();
    }

    [SerializeField]Vector3 revisionPos=new Vector3(0.25f,-0.2f,-0.1f);
    [SerializeField]Vector3 itemRevisionPos=new Vector3(-0.25f,-0.2f,-0.1f);

    void Update()
    {
        if(!isEquip && HasYourGun()/* && localPlayer!=null && localPlayer.GetVelocity().sqrMagnitude>0*/){
            var player = Networking.LocalPlayer;
            if (player != null){
                var right = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                yourGun.transform.position = right.position + revisionPos;//new Vector3(0.25f,-0.1f,0.1f);//new Vector3(0.35f,-0.2f,-1f); 
                var rot=right.rotation.eulerAngles;
                yourGun.transform.rotation = Quaternion.Euler(0,rot.y,rot.z) *  Quaternion.Euler(90,0,0);
                var left = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                equippedItem.transform.position  = left.position + itemRevisionPos;//new Vector3(-0.25f,-0.1f,0.1f);//new Vector3(-0.35f,-0.2f,-1f);

            }
        }
    }
}
