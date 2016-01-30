using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Gamelogic;
using Gamelogic.Grids;

//http://gamelogic.co.za/grids/documentation-contents/quick-start-tutorial/making-your-own-cells/

namespace DuckDuckBoom.GunRunner.Game
{
    //We use this enum to be able to make comparisons for matches.
    public enum TileType
    {
        Money,
        Gun,
        Fuel,
        Army1,
        Army2
    }

    public static class TileTypeExtensions
    {
        public static TileType SelectRandom()
        {
            int tileIndex = Random.Range(0, 5);

            return (TileType)tileIndex;
        }
    }

    public class MyMatchCell : SpriteCell
    {
        public MyIconTileSet tileSet;

        public SpriteRenderer tileSprite;
        public SpriteRenderer stackSprite;
        public SpriteRenderer stackNumberSprite;
        //frozen
        //etc

        [SerializeField]
        private TileType tileType;
        public TileType TileType
        {
            get { return tileType; }

            set
            {
                tileType = value;
                //FrameIndex = (int)value; //do logic better here
                UpdatePresentation(); //Update presentation to display new state.
            }
        }

        //We use this to block input while the cell is moving
        public bool IsMoving { get; set; }

        //Keep states private
        private bool isFrozen;
        private bool isDiseased;
        private int stackValue;

        //Provide public properties to change the state
        public bool IsFrozen
        {
            get { return isFrozen; }
            set
            {
                isFrozen = value;
                UpdatePresentation(); //Update presentation to display new state.
            }
        }

        //Provide public properties to change the state
        public bool IsDiseased
        {
            get { return isDiseased; }
            set
            {
                isDiseased = value;
                UpdatePresentation(); //Update presentation to display new state.
            }
        }


        public int StackValue
        {
            get { return stackValue; }
            set
            {
                stackValue = value;
                UpdatePresentation();
            }
        }


        private void UpdatePresentation()
        {
            //logic to change cells appearance based on states

            switch (tileType)
            {
                case TileType.Money:
                    tileSprite.sprite = tileSet.moneySprites.RandomItem();
                    break;
                case TileType.Gun:
                    tileSprite.sprite = tileSet.gunSprites.RandomItem();
                    break;
                case TileType.Fuel:
                    tileSprite.sprite = tileSet.fuelSprites.RandomItem();
                    break;
                case TileType.Army1:
                    tileSprite.sprite = tileSet.army1Sprites.RandomItem();
                    break;
                case TileType.Army2:
                    tileSprite.sprite = tileSet.army2Sprites.RandomItem();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tileType");
            }

            if(stackValue > 1)
            {
                stackSprite.gameObject.SetActive(true);
                //set stack number
            }
            else
            {
                stackSprite.gameObject.SetActive(false);
            }
        }

    }

}