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
        public SpriteRenderer selectedSprite;
        public SpriteRenderer stackSprite;
        public SpriteRenderer stackNumberSprite;
        //frozen
        //etc


        private bool isSelectable;
        public bool IsSelectable
        {
            get { return isSelectable; }

            set
            {
                isSelectable = value;
                UpdatePresentation();
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }

            set
            {
                isSelected = value;
                UpdatePresentation();
            }
        }

        
		public void Awake()
        {
            isSelectable = true;
            isSelected = false;
		}

        [SerializeField]
        private TileType tileType;
        public TileType TileType
        {
            get { return tileType; }

            set
            {
                tileType = value;

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
                //do tile logic better here
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

        public void AddToStack(int add)
        {
            StackValue += add;
        }


        private void UpdatePresentation()
        {
            //logic to change cells appearance based on states
            if(stackValue > 1)
            {
                stackSprite.gameObject.SetActive(true);
                //set stack number
            }
            else
            {
                stackSprite.gameObject.SetActive(false);
            }

            tileSprite.color = isSelectable ?  Color.white : Color.Lerp(Color.white, Color.black, 0.8f);

            selectedSprite.enabled = isSelected ? true : false;
        }


    }

}