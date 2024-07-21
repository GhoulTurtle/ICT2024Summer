using UnityEngine;

[CreateAssetMenu(menuName = "World Switch Data", fileName = "NewWorldSwitchData")]
public class WorldSwitchDataSO : ScriptableObject{
	[Header("World Type A Material Variables")]
	public Material WorldTypeABackgroundMat;
	public Material WorldTypeAFadeMat;
	public Material WorldTypeAStatueMat;
	public Material WorldTypeASkyBoxMat;
	public Material WorldTypeAMat;

	[Header("World Type B Material Variables")]
	public Material WorldTypeBBackgroundMat;
    public Material WorldTypeBFadeMat;
    public Material WorldTypeBStatueMat;
	public Material WorldTypeBSkyBoxMat;
	public Material WorldTypeBMat;
}