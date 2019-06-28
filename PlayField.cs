using System;
using System.Collections.Generic;

namespace LazTris
{
	public class PlayField
	{
		public struct ColorPoint
		{
			public ColorPoint(Point p, ConsoleColor color)
			{
				point = p;
				Color = color;
			}
			public Point point;
			public ConsoleColor Color;
		}

		public PlayField(int width, int height)
		{
			//Initialize a PlayField of a specific size
			Width = width;
			Height = height;
			//Initialize current and next part with random ones
			current = Part.Manager.GetRandomPart();
			next = Part.Manager.GetRandomPart();
			//Center the current part
			current.Position = new Point(Width/2, 0);
		}

		public int Width {get; set;}
		public int Height {get; set;}

		private List<ColorPoint> gridcontent = new List<ColorPoint>();
		public List<ColorPoint> GridContent {get {return gridcontent;}}
		private Part current, next;
		public Part Current {get {return current;}}
		public Part Next {get {return next;}}

		//Event for loosing
		public delegate void LostEventHandler(Object o, EventArgs e);
		public event LostEventHandler LostEvent;

		//Event for completing lines
		public delegate void CompletedLinesEventHandler(Object o, int amount);
		public event CompletedLinesEventHandler CompletedLinesEvent;

		public void Tick()
		{
			//This function updates the game. It is called regularly

			//Fall if you can!
			if (CanFall())
			{
				current.Fall();
			}
			else
			{
				//Meh. It can't.
				//Maybe the player lost the game?
				if (Lost() && LostEvent != null)
				{
					//Yep. Fire Lost-event
					LostEvent(this, new EventArgs());
				}
				else 
				{
					//Nope. Let's add the part's coordinates and color to the tetris-grid.
					foreach (Point p in current.Points)
					{
						gridcontent.Add(new ColorPoint(p, current.Color));
					}

					//Now, check for completed lines.
					int linescompleted = 0;
					//To do so, lines have to be checked from bottom to top
					for (int i = Height - 1; i >= 0;)
					{
						//while loop to check lines which were moved down
						while (true)
						{
							//Checker-variables to check if a line is completed
							bool[] checker = new bool[Width];
							int check = 0;
							for (int j = 0; j < gridcontent.Count; j++)
							{
								//Get all points of a line
								if (gridcontent[j].point.Y == i)
								{
									if (!checker[gridcontent[j].point.X])
									{
										checker[gridcontent[j].point.X] = true;
										//The checker-array helps us to count every point only once:
										check ++;
									}
								}
							}
							if (check == Width)
							{
								//Great! The line is completed!
								//Increase number of lines completed
								linescompleted ++;
								//Now, delete all the points in the line by iterating backwards
								for (int j = gridcontent.Count - 1; j >= 0; j--)
								{
									if (gridcontent[j].point.Y == i)
									{
										gridcontent.RemoveAt(j);
									}
								}
								//Move all points whose Y-position is over the completed line down by one
								for (int j = 0; j < gridcontent.Count; j++)
								{
									if (gridcontent[j].point.Y < i)
									{
										gridcontent[j] = new ColorPoint(new Point(gridcontent[j].point.X, gridcontent[j].point.Y + 1), gridcontent[j].Color);
									}
								}
							}
							else
							{
								//There is nothing to complete in this line. Better check the other ones:
								i--;
								break;
							}
						}
					}
					if (linescompleted > 0 && CompletedLinesEvent != null)
					{
						//Fire CompletedLines event
						CompletedLinesEvent(this, linescompleted);
					}
					//Generate a new part and center the current one
					current = next;
					current.Position = new Point(Width/2, 0);
					next = Part.Manager.GetRandomPart();
				}
			}
		}

		public bool Lost()
		{
			//Function to check wether the game is lost or not.
			//Basically it checks wether the falling part's Y-position is zero and wether it's coordinates are also part of the filled tetris-grid.
			foreach(Point p in current.Points)
			{
				if (current.Position.Y <= 0 && gridcontent.ConvertAll<Point>(
					delegate (ColorPoint cp)
					{
						return cp.point;
					}
				).Contains(p))
				{
					return true;
				}
			}	
			return false;
		}

		public void Move(int amount)
		{
			//Moves the tetris part, if it can
			if (CanMove(amount))
			{
				current.Move(amount);
			}
		}

		public void Rotate(Part.RotationDirection direction)
		{
			//Rotates the tetris part, if it can
			if (CanRotate(direction))
			{
				current.Rotate(direction);
			}
		}

		public bool CanFall()
		{
			//Check if the part can fall.
			//Basically checks wether it's coordinates moved down by one are also part of the filled tetris-grid.
			Point[] preview = current.FallPreview();
			foreach (Point p in preview)
			{
				if (p.Y >= Height || gridcontent.ConvertAll<Point>(
					delegate (ColorPoint cp)
					{
						return cp.point;
					}
				).Contains(p))
				{
					return false;
				}
			}
			return true;
		}

		private bool CanMove(int amount)
		{
			//Check if the tetris part can be moved left or right by "amount".
			//Works just as all the other "Can..." methods.
			Point[] preview = current.MovePreview(amount);
			foreach (Point p in preview)
			{
				if (p.X < 0 || p.X >= Width || gridcontent.ConvertAll<Point>(
					delegate (ColorPoint cp)
					{
						return cp.point;
					}
				).Contains(p))
				{
					return false;
				}
			}
			return true;
		}

		private bool CanRotate(Part.RotationDirection direction)
		{
			//Check if the tetris part can be rotated in a given direction
			Point[] preview = current.RotatePreview(direction);
			foreach (Point p in preview)
			{
				if (p.X < 0 || p.X >= Width || p.Y >= Height || gridcontent.ConvertAll<Point>(
					delegate (ColorPoint cp)
					{
						return cp.point;
					}
				).Contains(p))
				{
					return false;
				}
			}
			return true;
		}
	}
}

