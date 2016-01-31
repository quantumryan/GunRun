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
		public int guns = 0;
		public int money = 0;
		public int costOfGuns = 50;

		public float animationTimePerCell = .1f;
		public bool useAcceleration = false;

		private bool isDrag = false;
		private RectGrid<MyMatchCell> myMatchGrid;

		private MyMatchCell lastCellSelected;
		private RectPoint lastCellPoint;
		private HashSet<RectPoint> selectedSet;

		private HashSet<TileType> selectedTileTypes;


		public Text debugTextUI;

		void Start()
		{
			selectedSet = new HashSet<RectPoint>();
			selectedTileTypes = new HashSet<TileType>();
		}

		public override void InitGrid()
		{
			myMatchGrid = (RectGrid<MyMatchCell>)Grid.CastValues<MyMatchCell, RectPoint>();
			myMatchGrid.Apply(InitCell);
		}

		private void InitCell(MyMatchCell cell)
		{
			//int colorIndex = Random.Range(0, cellColors.Length);
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

						//DetermineValidMatchTypes();
					}
				}
			}

			if (Input.GetMouseButtonUp(0)) DoneDrag();
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



			debugTextUI.text += "DetermineValidTileTypesForMatching\n\n";
			debugTextUI.text += "    selectedTileTypes\n";
			foreach (TileType tt in selectedTileTypes)
			{
				debugTextUI.text += tt.ToString() + "\n";
			}

			debugTextUI.text += "    validTileTypesForSelection TileTypes\n";
			if (validTileTypesForSelection.Count < 1)
			{
				debugTextUI.text += "None";
			}
			else
			{
				foreach (TileType tt in validTileTypesForSelection)
				{
					debugTextUI.text += tt.ToString() + "\n";
				}
			}
		}


		//void DetermineValidMatchTypes()
		//{
		//	foreach (var point in myMatchGrid)
		//	{
		//		var MyMatchCell = myMatchGrid[point];
		//		//if (!CheckMatch(MousePosition, point))
		//		if (CheckSelectableTleType(point))
		//		{
		//			MyMatchCell.IsSelectable = false;
		//		}
		//		else
		//		{
		//			MyMatchCell.IsSelectable = true;
		//		}
		//	}
		//}

		// clear match tiles & sort tile grid when dragged
		void DoneDrag()
		{
			isDrag = false;

			debugTextUI.text = "";

			if (lastCellSelected != null)
			{
				//unset all
				foreach (var point in myMatchGrid)
				{
					myMatchGrid[point].IsSelectable = true;
					myMatchGrid[point].IsSelected = false;
				}

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
					if(lastpoint != null) myMatchGrid[lastpoint].StackValue = countStack;
					//lastCellSelected.StackValue = countStack;
				}
				//selectedSet.Remove(lastCellPoint);

				//DestroyMatchedCells(selectedSet);
				// Destroy Zero Value Cells
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

				int[] emptyCellsBelowTopCount = CountEmptyCellsBelowTop();
				MakeNewCellsAndStartMovingThem(emptyCellsBelowTopCount);

				selectedSet.Clear();
				selectedTileTypes.Clear();
				lastCellSelected = null;
			}
		}


		public void OnClick(RectPoint clickedPoint)
		{
			if (myMatchGrid.Values.Any(c => c == null || c.IsMoving)) //If any cell is moving, ignore input
			{
				return;
			}

			isDrag = true;
		}

		//original
		private bool CheckMatch(RectPoint p, RectPoint q)
		{
			if (myMatchGrid[p] == null) return false;
			if (myMatchGrid[q] == null) return false;

			return myMatchGrid[p].TileType == myMatchGrid[q].TileType;
		}


		//private bool CheckSelectableTleType(RectPoint p)
		//{
		//	if (myMatchGrid[p] == null) return false;
			
		//	foreach(TileType tt in validTileTypesForSelection)
		//	{
		//		if (myMatchGrid[p].TileType == tt)
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}



		//private void DestroyMatchedCells(IEnumerable<RectPoint> connectedSet)
		//{
		//	foreach (var rectPoint in connectedSet)
		//	{
		//		var MyMatchCell = myMatchGrid[rectPoint];

		//		if (MyMatchCell != null)
		//		{
		//			Destroy(MyMatchCell.gameObject);
		//		}

		//		myMatchGrid[rectPoint] = null;
		//	}
		//}

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