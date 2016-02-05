using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Gamelogic;
using Gamelogic.Grids;

namespace DuckDuckBoom.GunRunner.Game
{
	public class MyMatchGrid : GridBehaviour<RectPoint>
	{
		public int fuel = 0;
		public Text fuelUI;
		public int guns = 0;
		public Text gunsUI;
		public int money = 0;
		public Text moneyUI;
		public int buyCost = 50;
		public Text buyCostUI;
		public int bluePower = 10; //army2, red vs. blue, army1 vs army2
		public Text bluePowerUI;
		public int redPower = 10; //army1, red vs. blue, army1 vs army2
		public Text redPowerUI;
		public Slider powerSlider;

		public float animationTimePerCell = .1f;
		public bool useAcceleration = false;

		private bool isDrag = false;
		private RectGrid<MyMatchCell> myMatchGrid;

		private MyMatchCell lastCellSelected;
		private RectPoint lastCellPoint;
		private HashSet<RectPoint> selectedSet = new HashSet<RectPoint>();
		private HashSet<TileType> selectedTileTypes = new HashSet<TileType>();

		private List<TileType> tileTypeDist = new List<TileType>(); //weighted random distributed list


		public SoundsSet soundSet;
		private AudioSource myAudio;

		public Text debugText;

		void Start()
		{
			myAudio = GetComponent<AudioSource>();

			BuildWeightedTileTypeList();
			UpdateGuiElements();
		}

		public override void InitGrid()
		{
			myMatchGrid = (RectGrid<MyMatchCell>)Grid.CastValues<MyMatchCell, RectPoint>();
			myMatchGrid.Apply(InitCell);
		}


		// call after turn ends, before spawning new things 
		private void BuildWeightedTileTypeList()
		{
			//build a bag of random
			tileTypeDist.Clear();
			float powerBalance = (float)redPower / (float)bluePower;
			if (powerBalance < 1.0f)
			{
				powerBalance = 1f / powerBalance;
			}
			int armyWin = (int)Mathf.Lerp(25, 30, powerBalance - 1.0f);
			int armyLose = (int)Mathf.Lerp(25, 15, powerBalance - 1.0f);
			
			int weightArmyBlue = armyWin;
			int weightArmyRed = armyLose;
			if (redPower > bluePower)
			{
				weightArmyBlue = armyLose;
				weightArmyRed = armyWin;
			}
			int weightMoney = (int)Mathf.Lerp(20, 10, powerBalance - 1.0f);
			int weightGuns = (int)Mathf.Lerp(20, 10, powerBalance - 1.0f);
			int weightFuel = (int)Mathf.Lerp(5, 5, powerBalance - 1.0f);
			//int weightObstacles = 5;

			
			tileTypeDist.Clear();
			for (int i = 0; i < weightArmyRed; i++) tileTypeDist.Add(TileType.Army1);
			for (int i = 0; i < weightArmyBlue; i++) tileTypeDist.Add(TileType.Army2);
			for (int i = 0; i < weightMoney; i++) tileTypeDist.Add(TileType.Money);
			for (int i = 0; i < weightGuns; i++) tileTypeDist.Add(TileType.Gun);
			for (int i = 0; i < weightFuel; i++) tileTypeDist.Add(TileType.Fuel);
			//for (int i = 0; i < 20; i++) tileTypeDist.Add(TileType.Obstacle);

			

			//
			// impact of balance on important numbers
			buyCost = (int)Mathf.LerpUnclamped(50, 100, powerBalance - 1.0f);


			Debug.Log("Weight dist, p-balance " + powerBalance + " -> [ " + weightArmyRed + " " + weightArmyBlue + " " + weightMoney + " " + weightGuns + " " + weightFuel + " ] $" + buyCost);
		}

		
		private void InitCell(MyMatchCell cell)
		{
			//cell.TileType = TileTypeExtensions.SelectRandom();
			cell.TileType = tileTypeDist[Random.Range(0, tileTypeDist.Count)];
			cell.IsMoving = false;
		}

		void Update()
		{
			if (isDrag && myMatchGrid.Contains(MousePosition))
			{
				var mouseOverCell = myMatchGrid[MousePosition];
				//use distnace between mouse and current cell to help with diagonal selections
				float distMouseToOverCell = GridBuilderUtils.ScreenToWorld(mouseOverCell.gameObject, Input.mousePosition).magnitude;

				if (distMouseToOverCell < 80)
				{
					if (mouseOverCell.IsSelectable && !mouseOverCell.IsSelected)
					{
						var neighbors = myMatchGrid.GetNeighbors(lastCellPoint).ToPointList();
						if (lastCellSelected == null || neighbors.Contains(MousePosition))
						{
							mouseOverCell.IsSelectable = false;
							mouseOverCell.IsSelected = true;
							myAudio.PlayOneShot(soundSet.swipe.RandomItem(), 1.0f);

							lastCellSelected = mouseOverCell;
							lastCellPoint = MousePosition;

							selectedSet.Add(lastCellPoint);
							selectedTileTypes.Add(lastCellSelected.TileType);

							//what are the valid selection types
							DetermineValidTileTypesForMatching();
						}
					}
				}
			}

			if (Input.GetMouseButtonUp(0)) DoneDrag();
		}

		void UpdateGuiElements()
		{
			fuelUI.text = fuel.ToString();
			gunsUI.text = guns.ToString();
			moneyUI.text = money.ToString("$0");
			buyCostUI.text = buyCost.ToString("$0");
			bluePowerUI.text = bluePower.ToString();
			redPowerUI.text = redPower.ToString();
			powerSlider.value = 100 * bluePower / (redPower + bluePower);
		}


		void DetermineValidTileTypesForMatching()
		{
			//HashSet<TileType> validTileTypesForSelection = new HashSet<TileType>();
			//validTileTypesForSelection.Add(TileType.Money);
			//validTileTypesForSelection.Add(TileType.Gun);
			//validTileTypesForSelection.Add(TileType.Fuel);
			//validTileTypesForSelection.Add(TileType.Army1);
			//validTileTypesForSelection.Add(TileType.Army2);
			//validTileTypesForSelection.Add(TileType.Obstacle);

			//int stackedLimit = -1; //for valid cell check later, -1 is no limit, like when stacking

			//dictionary for first tile to combine with this
			Dictionary<TileType, int> tileTypeMatch = new Dictionary<TileType, int>();
			tileTypeMatch.Add(TileType.Money, -1);
			tileTypeMatch.Add(TileType.Gun, -1);
			tileTypeMatch.Add(TileType.Fuel, -1);
			tileTypeMatch.Add(TileType.Army1, -1);
			tileTypeMatch.Add(TileType.Army2, -1);
			//-1 is no limit, 0 is nothing can combine and > 0 is can combine if under this number

			if(selectedSet.Count == 0)
			{
				//everything is ok to select
			}
			else if(selectedSet.Count == 1)
			{
				int firstStack = myMatchGrid[selectedSet.ElementAt(0)].StackValue;
				//only one selected tile, can match with everything that's ok
				switch (selectedTileTypes.ElementAt(0))
				{
					case TileType.Money:
						tileTypeMatch[TileType.Money] = -1; //no limit, stack
						tileTypeMatch[TileType.Gun] = money / buyCost / firstStack;
						tileTypeMatch[TileType.Fuel] = money / buyCost / firstStack;
						tileTypeMatch[TileType.Army1] = 0; //can't combine
						tileTypeMatch[TileType.Army2] = 0; //can't combine
						break;
					case TileType.Gun:
						tileTypeMatch[TileType.Money] = money / buyCost / firstStack;
						tileTypeMatch[TileType.Gun] = -1; //no limit, stack
						tileTypeMatch[TileType.Fuel] = 0; //can't combine
						tileTypeMatch[TileType.Army1] = guns / firstStack;
						tileTypeMatch[TileType.Army2] = guns / firstStack;
						break;
					case TileType.Fuel:
						tileTypeMatch[TileType.Money] = money / buyCost / firstStack;
						tileTypeMatch[TileType.Gun] = 0; //can't combine
						tileTypeMatch[TileType.Fuel] = -1; //no limit, stack
						tileTypeMatch[TileType.Army1] = 0; //can't combine
						tileTypeMatch[TileType.Army2] = 0; //can't combine
						break;
					case TileType.Army1:
						tileTypeMatch[TileType.Money] = 0; //can't combine
						tileTypeMatch[TileType.Gun] = guns / firstStack; 
						tileTypeMatch[TileType.Fuel] = 0;
						tileTypeMatch[TileType.Army1] = -1; //no limit, stack
						tileTypeMatch[TileType.Army2] = -1; //no limit, fight
						break;
					case TileType.Army2:
						tileTypeMatch[TileType.Money] = 0; //can't combine
						tileTypeMatch[TileType.Gun] = guns / firstStack;
						tileTypeMatch[TileType.Fuel] = 0;
						tileTypeMatch[TileType.Army1] = -1; //no limit, fight
						tileTypeMatch[TileType.Army2] = -1; //no limit, stack
						break;
					default:
						throw new ArgumentOutOfRangeException("tileType");
				}

				//Debug.Log("FirstStack is " + firstStack + "   stacked limit is " + stackedLimit);
			}
			else if (selectedSet.Count >= 2)
			{
				if(selectedTileTypes.Count == 1)
				{
					//can only select the current type, set all to 0 can't combine, then the one we are to this type
					tileTypeMatch[TileType.Money] = 0;
					tileTypeMatch[TileType.Gun] = 0;
					tileTypeMatch[TileType.Fuel] = 0;
					tileTypeMatch[TileType.Army1] = 0;
					tileTypeMatch[TileType.Army2] = 0;

					tileTypeMatch[selectedTileTypes.ElementAt(0)] = -1;
				}
				if (selectedTileTypes.Count >= 2)
				{
					//can only merge two tiles of different types, so move is over
					tileTypeMatch[TileType.Money] = 0;
					tileTypeMatch[TileType.Gun] = 0;
					tileTypeMatch[TileType.Fuel] = 0;
					tileTypeMatch[TileType.Army1] = 0;
					tileTypeMatch[TileType.Army2] = 0;
				}
			}
			
			//go through the grid and make it obvious what can be selected based on types and inventory
			foreach (var point in myMatchGrid)
			{
				//turn it off first, then back on if conditions are met
				myMatchGrid[point].IsSelectable = false;
				//ignore if already selected
				if (!myMatchGrid[point].IsSelected)
				{
					if(tileTypeMatch[myMatchGrid[point].TileType] < 0)
					{
						myMatchGrid[point].IsSelectable = true;
					}
					else if(tileTypeMatch[myMatchGrid[point].TileType] >= myMatchGrid[point].StackValue)
					{
						myMatchGrid[point].IsSelectable = true;
					}
					//foreach (TileType tt in validTileTypesForSelection)
					//{
					//	if (myMatchGrid[point].TileType == tt)
					//	{
					//		//if (stackedLimit >= 0)
					//		//{
					//		//	//check the limit
					//		//	if(myMatchGrid[point].StackValue <= stackedLimit)
					//		//	{
					//		//		myMatchGrid[point].IsSelectable = true;
					//		//	}
					//		//}
					//		//else
					//		//{
					//			myMatchGrid[point].IsSelectable = true;
					//		//}
					//	}
					//}
				}
			}
		}



		// clear match tiles & sort tile grid when dragged
		void DoneDrag()
		{
			isDrag = false;

			//unset all
			foreach (var point in myMatchGrid)
			{
				myMatchGrid[point].IsSelectable = true;
				myMatchGrid[point].IsSelected = false;
			}

			if (lastCellSelected != null && selectedSet.Count > 1)
			{

				//to catch long, multi-type chain selections, such as g-g-g-m-m-g-m-g-g-m
				// this combines each type into their last locations and stores them in remainingCells
				List<MyMatchCell> remainingCells = new List<MyMatchCell>();
				foreach (TileType tt in selectedTileTypes)
				{
					int countStack = 0;
					RectPoint lastpoint = lastCellPoint;
					foreach (RectPoint rp in selectedSet)
					{
						if (myMatchGrid[rp].TileType == tt)
						{
							countStack += myMatchGrid[rp].StackValue;
							myMatchGrid[rp].StackValue = 0;
							lastpoint = rp;
						}
					}
					myMatchGrid[lastpoint].StackValue = countStack;
					remainingCells.Add(myMatchGrid[lastpoint]);
				}


				// order by tile type, so simpler if() logic
				var result = from element in remainingCells
								 orderby element.TileType
								 select element;

				//determine if combines should occur
				if (remainingCells.Count > 1)
				{
					MyMatchCell cell1 = result.ElementAt(0);
					MyMatchCell cell2 = result.ElementAt(1);
					CombineTiles(cell1, cell2, lastCellSelected);
				}
				else
				{
					//do combine sounds
					switch (lastCellSelected.TileType)
					{
						case TileType.Money:
							myAudio.PlayOneShot(soundSet.stackCoins.RandomItem(), 1.0f);
							break;
						case TileType.Gun:
							myAudio.PlayOneShot(soundSet.stackGuns.RandomItem(), 1.0f);
							break;
						case TileType.Fuel:
							myAudio.PlayOneShot(soundSet.stackFuel.RandomItem(), 1.0f);
							break;
						case TileType.Army1:
							myAudio.PlayOneShot(soundSet.stackArmy.RandomItem(), 1.0f);
							break;
						case TileType.Army2:
							myAudio.PlayOneShot(soundSet.stackArmy.RandomItem(), 1.0f);
							break;
						default:
							throw new ArgumentOutOfRangeException("tileType");
					}
				}


				// Destroy <= Zero Value Cells
				foreach (var point in myMatchGrid)
				{
					var MyMatchCell = myMatchGrid[point];
					if (MyMatchCell != null && MyMatchCell.StackValue < 1)
					{
						Destroy(MyMatchCell.gameObject);
						myMatchGrid[point] = null;
					}
				}

				IGrid<int, RectPoint> emptyCellsBelowCount = CountEmptyCellsBelowEachCell();
				StartMovingCells(emptyCellsBelowCount);

				// call after turn ends, before spawning new things 
				BuildWeightedTileTypeList();

				int[] emptyCellsBelowTopCount = CountEmptyCellsBelowTop();
				MakeNewCellsAndStartMovingThem(emptyCellsBelowTopCount);

				PowerStruggle();
			}

			selectedSet.Clear();
			selectedTileTypes.Clear();
			lastCellSelected = null;
			UpdateGuiElements();
		}

		/// <summary>
		/// Starts the drag process, which is done in Update.
		/// </summary>
		/// <param name="clickedPoint"></param>
		public void OnClick(RectPoint clickedPoint)
		{
			if (myMatchGrid.Values.Any(c => c == null || c.IsMoving)) //If any cell is moving, ignore input
			{
				return;
			}

			isDrag = true;
		}

		/// <summary>
		/// Hamdles the comnining conditions of multiple cell types.
		/// </summary>
		/// <param name="cell1"></param>
		/// <param name="cell2"></param>
		/// <param name="lastCell"></param>
		void CombineTiles(MyMatchCell cell1, MyMatchCell cell2, MyMatchCell lastCell)
		{
			//Debug.Log("  Combining " + cell1.TileType + " and " + cell2.TileType);
			int moneyStart = money;
			if(cell1.TileType == TileType.Money)
			{
				if(cell2.TileType == TileType.Gun)
				{
					//buy guns, both go away
					int qty = cell1.StackValue * cell2.StackValue;
					money -= buyCost * qty;
					guns += qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;

					myAudio.PlayOneShot(soundSet.combineBuyGuns.RandomItem(), 1.0f);
					debugText.text = "Money with Gun: \n   buyCost        $" + buyCost +
																"\n   qty             " + qty +
																"\n   cost           $" + (buyCost * qty) +
																"\n   money start    $" + moneyStart +
																"\n   money now      $" + money;
				}
				else if (cell2.TileType == TileType.Fuel)
				{
					//buy fuel, both go away
					int qty = cell1.StackValue * cell2.StackValue;
					money -= buyCost * qty;
					fuel += qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;

					myAudio.PlayOneShot(soundSet.combineBuyFuel.RandomItem(), 1.0f);
					debugText.text = "Money with Fuel:\n   buyCost        $" + buyCost +
																"\n   qty             " + qty +
																"\n   cost           $" + (buyCost * qty) +
																"\n   money start    $" + moneyStart +
																"\n   money now      $" + money;
				}
			}
			else if (cell1.TileType == TileType.Gun)
			{
				if (cell2.TileType == TileType.Army1)
				{
					//sell guns into, set both to none, then change last
					int qty = cell1.StackValue * cell2.StackValue;
					int armySize = qty; //cell2.StackValue;
					guns -= qty;
					money = money + buyCost * qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
					lastCell.TileType = TileType.Army1;
					lastCell.StackValue = armySize;

					myAudio.PlayOneShot(soundSet.combineSell.RandomItem(), 1.0f);
					debugText.text = "Gun with Army:  \n   buyCost        $" + buyCost +
																"\n   qty             " + qty +
																"\n   sell value     $" + (buyCost * qty) +
																"\n   money start    $" + moneyStart +
																"\n   money now      $" + money;
				}
				else if (cell2.TileType == TileType.Army2)
				{
					//sell guns into, set both to none, then change last
					int qty = cell1.StackValue * cell2.StackValue;
					int armySize = qty; // cell2.StackValue;
					guns -= qty;
					money = money + buyCost * qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
					lastCell.TileType = TileType.Army2;
					lastCell.StackValue = armySize;

					myAudio.PlayOneShot(soundSet.combineSell.RandomItem(), 1.0f);
					debugText.text = "Gun with Army:  \n   buyCost        $" + buyCost +
																"\n   qty             " + qty +
																"\n   sell value     $" + (buyCost * qty) +
																"\n   money start    $" + moneyStart +
																"\n   money now      $" + money;
				}
			}
			else if (cell1.TileType == TileType.Army1)
			{
				if (cell2.TileType == TileType.Army2)
				{
					//armies fight, could be one wins or trade
					int redArmy = cell1.StackValue;
					int blueArmy = cell2.StackValue;
					if (redArmy > blueArmy)
					{
						//red wins, blue is overwhelmed losing its troops and red converts troops into power
						bluePower = bluePower - blueArmy;
						redPower = redPower + redArmy - blueArmy;

						myAudio.PlayOneShot(soundSet.combineFight.RandomItem(), 1.0f);
						debugText.text = "Red beats Blue:  \n   redArmy        " + redArmy +
																	 "\n   blueArmy       " + blueArmy +
																	 "\n   blue loss      " + blueArmy +
																	 "\n   red gain       " + (redArmy - blueArmy);
					}
					else if (cell1.StackValue < cell2.StackValue)
					{
						//blue wins, red is overwhelmed losing its troops
						bluePower = bluePower + blueArmy - redArmy;
						redPower = redPower - redArmy;

						myAudio.PlayOneShot(soundSet.combineFight.RandomItem(), 1.0f);
						debugText.text = "Blue beats Red:  \n   redArmy        " + redArmy +
																	 "\n   blueArmy       " + blueArmy +
																	 "\n   blue gain      " + (blueArmy - redArmy) +
																	 "\n   red loss       " + redArmy;
					}
					else
					{
						//its a tie
						bluePower = bluePower - blueArmy;
						redPower = redPower - redArmy;

						myAudio.PlayOneShot(soundSet.combineFight.RandomItem(), 1.0f);
						debugText.text = "Army Even Fight: \n   redArmy        " + redArmy +
																	 "\n   blueArmy       " + blueArmy +
																	 "\n   blue loss      " + blueArmy +
																	 "\n   red loss       " + redArmy;
					}
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
			}
		}


		private void PowerStruggle()
		{

		}

		//original
		private bool CheckMatch(RectPoint p, RectPoint q)
		{
			if (myMatchGrid[p] == null) return false;
			if (myMatchGrid[q] == null) return false;

			return myMatchGrid[p].TileType == myMatchGrid[q].TileType;
		}


		//Start moving cells that have empty cells below them
		private void StartMovingCells(IGrid<int, RectPoint> emptyCellsBelowCount)
		{
			foreach (var point in emptyCellsBelowCount.WhereCell(c => c > 0).Where(point => myMatchGrid[point] != null))
			{
				StartCoroutine(MoveCell(point, myMatchGrid[point], emptyCellsBelowCount[point]));
			}
		}

		private void MakeNewCellsAndStartMovingThem(int[] emptyCellsBelowTopCount)
		{
			for (int columnIndex = 0; columnIndex < myMatchGrid.Width; columnIndex++)
			{
				for (int i = 0; i < emptyCellsBelowTopCount[columnIndex]; i++)
				{
					var point = new RectPoint(columnIndex, myMatchGrid.Height + i);
					var newCell = MakeNewCell(point);

					StartCoroutine(MoveCell(point, newCell, emptyCellsBelowTopCount[columnIndex]));
				}
			}
		}

		private int[] CountEmptyCellsBelowTop()
		{
			var topRowEmptyCellsBelowCount = new int[myMatchGrid.Width];

			for (int columnIndex = 0; columnIndex < myMatchGrid.Width; columnIndex++)
			{
				var point = new RectPoint(columnIndex, myMatchGrid.Height);
				var pointBelow = point + RectPoint.South;
				int count = 0;

				while (myMatchGrid.Contains(pointBelow))
				{
					if (myMatchGrid[pointBelow] == null)
					{
						count++;
					}

					pointBelow += RectPoint.South;
				}

				topRowEmptyCellsBelowCount[columnIndex] = count;
			}
			return topRowEmptyCellsBelowCount;
		}


		private IGrid<int, RectPoint> CountEmptyCellsBelowEachCell()
		{
			var emptyCellsBelowCount = myMatchGrid.CloneStructure<int>();

			foreach (var point in myMatchGrid)
			{
				var pointBelow = point + RectPoint.South;
				int count = 0;

				while (myMatchGrid.Contains(pointBelow))
				{
					if (myMatchGrid[pointBelow] == null)
					{
						count++;
					}

					pointBelow += RectPoint.South;
				}

				emptyCellsBelowCount[point] = count;
			}
			return emptyCellsBelowCount;
		}

		private MyMatchCell MakeNewCell(RectPoint point)
		{
			var newCell = Instantiate(GridBuilder.CellPrefab).GetComponent<MyMatchCell>();

			newCell.transform.parent = transform;
			newCell.transform.localScale = Vector3.one;
			newCell.transform.localPosition = Map[point];

			InitCell(newCell);

			newCell.name = "-"; //set the name to empty until the cell has been put in the grid

			return newCell;
		}

		private IEnumerator MoveCell(RectPoint start, MyMatchCell cell, int numberOfCellsToMove)
		{
			return useAcceleration
				? MoveCellWithAcceleration(start, cell, numberOfCellsToMove)
				: MoveCellWithoutAcceleration(start, cell, numberOfCellsToMove);
		}

		private IEnumerator MoveCellWithoutAcceleration(RectPoint start, MyMatchCell cell, int numberOfCellsToMove)
		{
			cell.IsMoving = true;

			float totalTime = animationTimePerCell * numberOfCellsToMove;
			float time = 0;
			float t = 0;

			RectPoint destination = start + RectPoint.South * numberOfCellsToMove;

			while (t < 1)
			{
				time += Time.deltaTime;
				t = time / totalTime;

				var newPosition = Vector3.Lerp(Map[start], Map[destination], t);
				cell.transform.localPosition = newPosition;

				yield return null;
			}

			cell.transform.localPosition = Map[destination];
			myMatchGrid[destination] = cell;
			cell.name = destination.ToString(); //This allows us to see what is going on if we render gizmos

			cell.IsMoving = false;
		}

		private IEnumerator MoveCellWithAcceleration(RectPoint start, MyMatchCell cell, int numberOfCellsToMove)
		{
			cell.IsMoving = true;


			float displacement = 0;
			float t = 0;
			float speed = 0;
			const float acceleration = 10000;

			RectPoint destination = start + RectPoint.South * numberOfCellsToMove;
			float totalDisplacement = (Map[destination] - Map[start]).magnitude;

			while (t < 1)
			{
				speed += Time.deltaTime * acceleration;
				displacement += Time.deltaTime * speed;
				t = displacement / totalDisplacement;

				var newPosition = Map[start] + Vector3.down * displacement;
				cell.transform.localPosition = newPosition;

				yield return null;
			}

			cell.transform.localPosition = Map[destination];
			myMatchGrid[destination] = cell;
			cell.name = destination.ToString(); //This allows us to see what
			//is going on if we render gizmos

			cell.IsMoving = false;
		}
	}
}