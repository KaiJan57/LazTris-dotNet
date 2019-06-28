namespace LazTris
{
    public struct Point
	{
		public Point (int x, int y)
		{
			X = x;
			Y = y;
		}

		public void SwapXY()
		{
			int temp = X;
			X = Y;
			Y = temp;
		}
		public int X, Y;
	}
}

