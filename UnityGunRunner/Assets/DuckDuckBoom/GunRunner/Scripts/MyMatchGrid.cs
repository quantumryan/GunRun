using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gamelogic.Grids;
using UnityEngine.UI;

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
		public int costOfGuns = 50;
		public Text costOfGunsUI;
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
		private HashSet<RectPoint> selectedSet;

		private HashSet<TileType> selectedTileTypes;

		private List<TileType> tileTypeDist = new List<TileType>(); //weighted random distributed list
		void Start()
		{
			BuildWeightedTileTypeList();
			selectedSet = new HashSet<RectPoint>();
			selectedTileTypes = new HashSet<TileType>();
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
			float powerBalance = (float)redPower / (float)bluePower;
			if (powerBalance < 1.0f)
			{
				powerBalance = 1f / powerBalance;
				//flip army1 and army2
				
			}
			else
			{

			}

			int weightArmy1 = (int)Mathf.Lerp(25, 30, powerBalance - 1.0f);
			int weightArmy2 = (int)Mathf.Lerp(25, 15, powerBalance - 1.0f);
			int weightMoney = (int)Mathf.Lerp(20, 10, powerBalance - 1.0f);
			int weightGuns = (int)Mathf.Lerp(20, 10, powerBalance - 1.0f);
			int weightFuel = (int)Mathf.Lerp(5, 5, powerBalance - 1.0f);
			//int weightObstacles = 5;

			Debug.Log("Weight distribution, power balance is " + powerBalance + " which gives: " + weightArmy1 + " " + weightArmy2 + " " + weightMoney + " " + weightGuns + " " + weightFuel);

			tileTypeDist.Clear();
			for (int i = 0; i < weightArmy1; i++) tileTypeDist.Add(TileType.Army1);
			for (int i = 0; i < weightArmy2; i++) tileTypeDist.Add(TileType.Army2);
			for (int i = 0; i < weightMoney; i++) tileTypeDist.Add(TileType.Money);
			for (int i = 0; i < weightGuns; i++) tileTypeDist.Add(TileType.Gun);
			for (int i = 0; i < weightFuel; i++) tileTypeDist.Add(TileType.Fuel);
			//for (int i = 0; i < 20; i++) tileTypeDist.Add(TileType.Obstacle);
		}

		
		private void InitCell(MyMatchCell cell)
		{
			cell.TileType = TileTypeExtensions.SelectRandom();
			//cell.Color = cellColors[colorIndex];
			cell.IsMoving = false;
		}

		void Update()
		{
			if (isDrag && myMatchGrid.Contains(MousePosition))
			{
				var mouseOverCell = myMatchGrid[MousePosition];
				if (mouseOverCell.IsSelectable && !mouseOverCell.IsSelected)
				{
					var neighbors = myMatchGrid.GetNeighbors(lastCellPoint).ToPointList();
					if (lastCellSelected == null || neighbors.Contains(MousePosition))
					{
						mouseOverCell.IsSelectable = false;
						mouseOverCell.IsSelected = true;
						lastCellSelected = mouseOverCell;
						lastCellPoint = MousePosition;

						selectedSet.Add(lastCellPoint);
						selectedTileTypes.Add(lastCellSelected.TileType);

						//what are the valid selection types
						DetermineValidTileTypesForMatching();
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
			costOfGunsUI.text = costOfGuns.ToString();
			bluePowerUI.text = bluePower.ToString();
			redPowerUI.text = redPower.ToString();
			powerSlider.value = 100 * bluePower / (redPower + bluePower);
		}


		void DetermineValidTileTypesForMatching()
		{
			HashSet<TileType> validTileTypesForSelection = new HashSet<TileType>();

			if (selectedTileTypes.Count > 1)
			{
				validTileTypesForSelection = selectedTileTypes;
			}
			else
			{
				validTileTypesForSelection.Add(TileType.Money);
				validTileTypesForSelection.Add(TileType.Gun);
				validTileTypesForSelection.Add(TileType.Fuel);
				validTileTypesForSelection.Add(TileType.Army1);
				validTileTypesForSelection.Add(TileType.Army2);
				switch (selectedTileTypes.ElementAt(0))
				{
					case TileType.Money:
						validTileTypesForSelection.Remove(TileType.Army1);
						validTileTypesForSelection.Remove(TileType.Army2);
						break;
					case TileType.Gun:
						validTileTypesForSelection.Remove(TileType.Fuel);
						break;
					case TileType.Fuel:
						validTileTypesForSelection.Remove(TileType.Gun);
						validTileTypesForSelection.Remove(TileType.Army1);
						validTileTypesForSelection.Remove(TileType.Army2);
						break;
					case TileType.Army1:
						validTileTypesForSelection.Remove(TileType.Money);
						validTileTypesForSelection.Remove(TileType.Fuel);
						break;
					case TileType.Army2:
						validTileTypesForSelection.Remove(TileType.Money);
						validTileTypesForSelection.Remove(TileType.Fuel);
						break;
					default:
						throw new ArgumentOutOfRangeException("tileType");
				}
			}

			foreach (var point in myMatchGrid)
			{
				myMatchGrid[point].IsSelectable = false;
				foreach (TileType tt in validTileTypesForSelection)
				{
					if (myMatchGrid[point].TileType == tt && !myMatchGrid[point].IsSelected)
					{
						myMatchGrid[point].IsSelectable = true;
					}
				}
			}
		}



		// clear match tiles & sort tile grid when dragged
		void DoneDrag()
		{
			isDrag = false;
			
			if (lastCellSelected != null)
			{
				//unset all
				foreach (var point in myMatchGrid)
				{
					myMatchGrid[point].IsSelectable = true;
					myMatchGrid[point].IsSelected = false;
				}

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


				// order by tile type
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

				selectedSet.Clear();
				selectedTileTypes.Clear();
				lastCellSelected = null;
				UpdateGuiElements();
			}
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
			if(cell1.TileType == TileType.Money)
			{
				if(cell2.TileType == TileType.Gun)
				{
					//buy guns, both go away
					int qty = cell1.StackValue*cell2.StackValue;
					money -= costOfGuns * qty;
					guns += qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
				else if (cell2.TileType == TileType.Fuel)
				{
					//buy fuel, both go away
					int qty = cell1.StackValue * cell2.StackValue;
					money -= costOfGuns * qty;
					fuel += qty;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
			}
			else if (cell1.TileType == TileType.Gun)
			{
				if (cell2.TileType == TileType.Army1)
				{
					//guns turn into army, set both to none, then change last
					int qty = cell1.StackValue + cell2.StackValue;
					lastCell.TileType = TileType.Army1;
					lastCell.StackValue = qty;
					guns -= cell1.StackValue;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
				else if (cell2.TileType == TileType.Army2)
				{
					//guns turn into army
					int qty = cell1.StackValue + cell2.StackValue;
					lastCell.TileType = TileType.Army2;
					lastCell.StackValue = qty;
					guns -= cell1.StackValue;
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
			}
			else if (cell1.TileType == TileType.Army1)
			{
				if (cell2.TileType == TileType.Army2)
				{
					//armies fight, could be one wins or trade
					int delta = Mathf.Abs(cell1.StackValue - cell2.StackValue);
					if (cell1.StackValue > cell2.StackValue)
					{
						//red wins
						bluePower -= cell1.StackValue;
						redPower += delta;
					}
					else
					{
						//blue wins
						bluePower += delta;
						redPower -= cell2.StackValue;
					}
					cell1.StackValue = 0;
					cell2.StackValue = 0;
				}
			}
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