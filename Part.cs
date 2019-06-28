using System;
using System.Collections.Generic;

namespace LazTris
{
	public class Part
	{
		public struct Initializer
		{
			//This struct makes defining new parts easier
			public Initializer(char[,] pointDefinition, RotationType rotation, ConsoleColor color, int probability)
			{
				PointDefinition = pointDefinition;
				Rotation = rotation;
				Color = color;
				Probability = probability;
			}
			public char[,] PointDefinition;
			public RotationType Rotation;
			public ConsoleColor Color;
			public int Probability;
		}

		public static class Manager
		{
			//The Manager class stores the types of parts and gives other parts of this program access to them
			private static readonly Initializer[] partTypes = {
				new Initializer() {
					PointDefinition = new char[,] {
						{'#', '#'}, 
					    {'#', '#'},
					},
					Rotation = RotationType.None,
					Color = ConsoleColor.Yellow,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{' ', ' ', ' ', ' '}, 
						{'#', '#', '#', '#'}, 
					},
					Rotation = RotationType.TwoState,
					Color = ConsoleColor.Cyan,
					Probability = 5,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{' ', ' ', ' '},
						{'#', '#', '#'}, 
						{' ', '#', ' '},
					},
					Rotation = RotationType.Full,
					Color = ConsoleColor.Magenta,
					Probability = 4,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{' ', ' ', ' '},
						{'#', '#', '#'}, 
						{'#', ' ', ' '},
					},
					Rotation = RotationType.Full,
					Color = ConsoleColor.DarkYellow,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{' ', ' ', ' '},
						{'#', '#', '#'}, 
						{' ', ' ', '#'},
					},
					Rotation = RotationType.Full,
					Color = ConsoleColor.DarkBlue,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{'#', '#', ' '}, 
						{' ', '#', '#'},
					},
					Rotation = RotationType.TwoState,
					Color = ConsoleColor.Red,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{' ', '#', '#'}, 
						{'#', '#', ' '},
					},
					Rotation = RotationType.TwoState,
					Color = ConsoleColor.Green,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{'#'}, 
					},
					Rotation = RotationType.None,
					Color = ConsoleColor.DarkGreen,
					Probability = 3,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{'#', '#', '#'}, 
						{'#', ' ', '#'}, 
						{'#', ' ', '#'},
					},
					Rotation = RotationType.Full,
					Color = ConsoleColor.DarkCyan,
					Probability = 2,
				},
				new Initializer() {
					PointDefinition = new char[,] {
						{'#', '#', '#'}, 
						{'#', ' ', '#'}, 
						{'#', '#', '#'},
					},
					Rotation = RotationType.None,
					Color = ConsoleColor.DarkRed,
					Probability = 1,
				},
			};

			public static int GetPartCount()
			{
				return partTypes.Length;
			}

			public static Part GetPart(int id)
			{
				if (id < 0)
				{
					id = 0;
				}
				else if (id > partTypes.Length - 1)
				{
					id = partTypes.Length - 1;
				}
				return new Part(partTypes[id].PointDefinition, partTypes[id].Rotation, partTypes[id].Color);
			}

			private static Random random = new Random();

			public static Part GetRandomPart()
			{
				//This function takes care of "Probability" variable in the part's initializers
				int maxrandom = 0;
				foreach (Initializer init in partTypes)
				{
					maxrandom += init.Probability;
				}
				int randomvalue = random.Next(maxrandom);
				int previous = 0;
				for (int i = 0; i < partTypes.Length; i++)
				{
					if (randomvalue >= previous && randomvalue < (previous += partTypes[i].Probability))
					{
						return GetPart(i);
					}
				}
				return null;
			}
		}

		private Part(char[,] pointDefinition, RotationType rotation, ConsoleColor color)
		{
			PointDefinition = pointDefinition;
			Color = color;
			Rotation = rotation;
		}

		public char[,] PointDefinition { 
			set {
				//Converts a nice PointDefinition in form of a 2D-char-array into an array of points
				List<Point> RawPoints = new List<Point>();
				for (int i = 0; i < value.GetLength(0); i++)
				{
					for (int j = 0; j < value.GetLength(1); j++)
					{
						if (value[i, j] != ' ')
						{
							RawPoints.Add(new Point(j, i));
						}
					}
				}
				//It also calculates the Y-offset to align the part to the top of the field
				int k = 0;
				foreach (Point p in RawPoints)
				{
					if (k == 0)
					{
						yoffset = p.Y;
					}
					else if (p.Y < yoffset)
					{
						yoffset = p.Y;
					}
					k++;
				}
				yoffset *= -1;
				points = new Point[RawPoints.Count];
				RawPoints.CopyTo(points);
				//And it determines the dimensions of the part
				width = value.GetLength(1);
				height = value.GetLength(0);
			}
		}

		private Point[] points;
		public Point[] Points {
			get {
				return AddPosition(points, Width);
			}
			private set {
				points = value;
			}
		}

		public Point Position;

		private int yoffset = 0;

		public ConsoleColor Color {get; set;}

		private int width;
		public int Width {get {return width;} private set {width = value;}}
		private int height;
		public int Height {get {return height + yoffset;} private set {height = value - yoffset;}}

		public RotationType Rotation {get; set;}
		private bool toggle = false;

		private Point[] AddPosition(Point[] positionless, int partwidth)
		{
			//Applies the position to an easily rotatable, positionless point-array of a tetris-part
			Point[] positioned = new Point[positionless.Length];
			for (int i = 0; i < positioned.Length; i++)
			{
				positioned[i] = new Point(positionless[i].X - partwidth/2 + Position.X, positionless[i].Y + Position.Y + yoffset);
			}
			return positioned;
		}

		public enum RotationDirection {
			Right,
			Left,
		}

		public enum RotationType {
			None,
			TwoState,
			Full,
		}

		public void Rotate(RotationDirection dir)
		{
			//Rotation
			if (Rotation == RotationType.None)
			{
				return;
			}
			else if (Rotation == RotationType.TwoState)
			{
				if (toggle)
				{
					dir = RotationDirection.Left;
				}
				else
				{
					dir = RotationDirection.Right;
				}
				toggle = !toggle;
			}
			//Swap width and height
			int temp = Width;
			width = height;
			height = temp;

			for (int i = 0; i < points.Length; i++)
			{
				//Swap X and Y (mirror vertically)
				points[i].SwapXY();
				//Mirror horizontally or vertically based on direction
				if (dir == RotationDirection.Right)
				{
					points[i].X = width - 1 - points[i].X;
				}
				else
				{
					points[i].Y = height - 1 - points[i].Y;
				}
			}
		}

		public Point[] RotatePreview(RotationDirection dir)
		{
			//Same thing, but generates a preview to check for collisions
			if (Rotation == RotationType.None)
			{
				return Points;
			}
			else if (Rotation == RotationType.TwoState)
			{
				if (toggle)
				{
					dir = RotationDirection.Left;
				}
				else
				{
					dir = RotationDirection.Right;
				}
			}
			int widthpreview = height;
			int heightpreview = width;

			Point[] pointspreview = new Point[points.Length];
			points.CopyTo(pointspreview, 0);

			for (int i = 0; i < pointspreview.Length; i++)
			{
				pointspreview[i].SwapXY();
				if (dir == RotationDirection.Right)
				{
					pointspreview[i].X = widthpreview - 1 - pointspreview[i].X;
				}
				else
				{
					pointspreview[i].Y = heightpreview - 1 - pointspreview[i].Y;
				}
			}
			return AddPosition(pointspreview, widthpreview);
		}

		public void Move(int amount)
		{
			Position.X += amount;
		}

		public Point[] MovePreview(int amount)
		{
			Point[] pointspreview = new Point[points.Length];
			points.CopyTo(pointspreview, 0);

			for (int i = 0; i < pointspreview.Length; i++)
			{
				pointspreview[i].X += amount;
			}
			return AddPosition(pointspreview, Width);
		}

		public void Fall()
		{
			Position.Y ++;
		}

		public Point[] FallPreview()
		{
			Point[] pointspreview = new Point[points.Length];
			points.CopyTo(pointspreview, 0);

			for (int i = 0; i < pointspreview.Length; i++)
			{
				pointspreview[i].Y ++;
			}
			return AddPosition(pointspreview, Width);
		}
	}
}

