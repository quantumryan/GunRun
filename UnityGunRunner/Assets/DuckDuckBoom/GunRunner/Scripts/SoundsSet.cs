using UnityEngine;

namespace DuckDuckBoom.GunRunner.Game
{
	public class SoundsSet : ScriptableObject
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("DuckDuckBoom/Create Sounds Set")]
		public static void CreateTileSet()
		{
			var tileSet = CreateInstance<SoundsSet>();
			UnityEditor.AssetDatabase.CreateAsset(tileSet, "Assets/SoundsSet.asset");
			UnityEditor.AssetDatabase.SaveAssets();
		}
#endif

		public AudioClip[] swipe;
		public AudioClip[] stackCoins;
		public AudioClip[] stackArmy;
		public AudioClip[] stackGuns;
		public AudioClip[] stackFuel;
		public AudioClip[] combineSell;
		public AudioClip[] combineBuyGuns;
		public AudioClip[] combineBuyFuel;
		public AudioClip[] combineFight;
	}
}