
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WeaponStatusMenuViewController : UdonSharpBehaviour
{
    [SerializeField]TextMeshProUGUI firePower;
    [SerializeField]TextMeshProUGUI lightningPower;
    [SerializeField]TextMeshProUGUI coldPower;
    [SerializeField]TextMeshProUGUI blackPower;
    [SerializeField]TextMeshProUGUI whitePower;

    [SerializeField]TextMeshProUGUI restLevelUp;
    [SerializeField]TextMeshProUGUI waveText;


    [SerializeField]Player player;
    [SerializeField]BattleStart bs;

    void Start()
    {
    }

    public void LevelUpExplosion(){
        player.TryToImproveAbilityFromUI(JewelElement.Fire);
    }

    public void LevelUpBasic(){
        player.TryToImproveAbilityFromUI(JewelElement.Black);
    }

    public void LevelUpLightningChain(){
        player.TryToImproveAbilityFromUI(JewelElement.Lightning);
    }

    public void LevelUpWhite(){
        player.TryToImproveAbilityFromUI(JewelElement.White);
    }

    public void LevelUpCold(){
        player.TryToImproveAbilityFromUI(JewelElement.Cold);
    }

    void Update()
    {
        firePower.text = powerToString(player.FirePower);
        lightningPower.text = powerToString(player.LightningPower);
        coldPower.text = powerToString(player.ColdPower);
        blackPower.text = powerToString(player.BlackPower);
        whitePower.text = powerToString(player.WhitePower);
        
        waveText.text = "Wave "+ bs.Wave;
        restLevelUp.text = "+ "+player.RestLevelUp.ToString();
    }

    string powerToString(int power){
        return "レベル"+power.ToString();
    }

}
