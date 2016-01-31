using UnityEngine;

namespace DuckDuckBoom.GunRunner.Game
{
	public class MyIconTileSet : ScriptableObject
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("DuckDuckBoom/Create Icon Tile Set")]
		public static void CreateTileSet()
		{
			var tileSet = CreateInstance<MyIconTileSet>();
			UnityEditor.AssetDatabase.CreateAsset(tileSet, "Assets/MyIconTiles.asset");
			UnityEditor.AssetDatabase.SaveAssets();
		}
#endif

		public Sprite[] moneySprites;
		public Sprite[] gunSprites;
		public Sprite[] fuelSprites;
		public Sprite[] army1Sprites;
		public Sprite[] army2Sprites;
		public Sprite[] obstacles;

		//public Color[] colors = Gamelogic.Grids.Examples.ExampleUtils.Colors;
		public Sprite[] numberSprites;
	}
}