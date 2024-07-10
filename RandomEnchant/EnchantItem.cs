
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum ItemGrade{
    Normal,
    Magic,
    Rare,
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EnchantItem : UdonSharpBehaviour
{
    [SerializeField]EnchantItemView view;
    [SerializeField]Player player;
    [SerializeField]int[] enchant={0,0,0};
    [SerializeField]SpriteRenderer sr;
    public int[] Enchants => enchant;
    // 1 : element + 1  11~15
    // 2 : element +2 and -1 112 ~ 154
    //3 chain +1
    //4 exp range +0.3
    //5 pene +1
    //6 bullet +1
    //7 homing +1
    //8 attackspeed +15%
    ItemGrade grade;
    public ItemGrade Grade=>grade;
    void Start()
    {
        localPlayer=Networking.LocalPlayer;
        if(view!=null)view.Show(enchant);
    }

    VRCPlayerApi localPlayer;

    public override void Interact()
    {
        if(player.EquippedItem != this){
            player.EquipItem(this);
            if(view!=null)view._OnDisable();
            if(sr!=null)sr.enabled=false;
        }
    }

    public void UnEquip(){
        if(view!=null)view._OnEnable();
        if(sr!=null)sr.enabled=true;
    }
    public void Init(ItemGrade grade){
        this.grade=grade;
        enchant=new int[3];
        switch(grade){
            case ItemGrade.Normal:
                for(int i=0;i<1;i++)
                    enchant[i]=calcEnchant();
                sr.color = new Color32(30,255,0,100);
                break;
            case ItemGrade.Magic:
                for(int i=0;i<2;i++)
                    enchant[i]=calcEnchant();
                sr.color = new Color32(0,112,221,100);
                break;
            case ItemGrade.Rare:
                for(int i=0;i<3;i++)
                    enchant[i]=calcEnchant();
                sr.color = new Color32(255,128,0,100);
                break;
        }
        if(view!=null)view.Show(enchant);
    }

    int calcEnchant(){
        int s = Random.Range(1,9);
        if(s==1){
            s = 10 + Random.Range(1,6);
        } else if(s==2){
            int strong = Random.Range(1,6);
            int weak = Random.Range(1,6);
            if(strong==weak){
                if(strong==1)
                    weak=2;
                else
                    weak-=1;
            }
            s=100+strong*10+weak;
        }
        return s;
    }


    int CalcElement(int target){
        int res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]>100){
                var a = enchant[i]-100;
                var strong = a/10;
                var weak = a%10;
                if(strong==target)
                    res+=2;
                else if(weak==target)
                    res-=1;
            } else if(enchant[i]>10){
                int p =enchant[i]-10;
                if(p==target){
                    res+=1;
                }
            }
        }
        return res;
    }

    public int CalcFirePower(){
        return CalcElement(1);
    }

    public int CalcLightningPower(){
        return CalcElement(2);
    }

    public int CalcColdPower(){
        return CalcElement(3);
    }

    public int CalcBlackPower(){
        return CalcElement(4);
    }

    public int CalcWhitePower(){
        return CalcElement(5);
    }

    
    public int CalcChain(){
        int res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==3)
                res+=1;
        }
        return res;
    }
    public float CalcExpRange(){
        float res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==4)
                res+=0.3f;
        }
        return res;
    }

    public int CalcPene(){
         int res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==5)
                res+=1;
        }
        return res;
    }

    public int CalcBullet(){
         int res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==6)
                res+=1;
        }
        return res;
    }


    public int CalcHoming(){
         int res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==7)
                res+=1;
        }
        return res;
    }

    public float CalcAttackSpeed(){
        float res=0;
        for(int i=0;i<3;i++){
            if(enchant[i]==8)
                res+=0.15f;
        }
        return res;
    }
}
