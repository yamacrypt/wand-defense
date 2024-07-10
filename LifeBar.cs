
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LifeBar : UdonSharpBehaviour
{
    [SerializeField]SpriteRenderer sr;
    void Start()
    {
        
    }

    float percentage=1;

    public float Percentage{
        get{return percentage;}
        set{
            percentage=Mathf.Clamp01(value);
            if(percentage==1){
                this.gameObject.SetActive(false);
            } else {
                this.gameObject.SetActive(true);
            }
            sr.size=new Vector2(percentage,1);
        }
    }
}
