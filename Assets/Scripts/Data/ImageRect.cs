using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
	[Serializable]
	public struct ImageRect
	{
		public Position From;
		public Position To;

		public ImageRect(Position from, Position to)
		{
			From = from;
			To = to;
		}

		public ImageRect(int width, int height)
		{
			From = new Position();
			To = new Position(From.X + width - 1, From.Y + height - 1);
		}

		public ImageRect(Position from, int width, int height)
		{
			From = from;
			To = new Position(From.X + width - 1, From.Y + height - 1);
		}

		public int X => Mathf.Min(From.X, To.X);

		public int Y => Mathf.Min(From.Y, To.Y);

		public int Width => Mathf.Abs(To.X - From.X) + 1;

		public int Height => Mathf.Abs(To.Y - From.Y) + 1;

		public Position Min => new Position(X, Y);

		public Position Max => new Position(X + Width - 1, Y + Height - 1);

		public Position Center => new Position(X + Width / 2, Y + Height / 2);

        public int Direction => To.X > From.X ? To.Y > From.Y ? 2 : 3 : To.Y > From.Y ? 1 : 0;

        public string ToJson()
		{
			return JsonUtility.ToJson(this);
		}

		public bool Contains(Position position)
		{
			return Contains(position.X, position.Y);
		}

		public bool Contains(int x, int y)
		{
			return x >= X && x <= X + Width - 1 && y >= Y && y <= Y + Height - 1;
		}

		public bool Intersects(ImageRect other)
		{
			return Contains(other.Min)
			       || Contains(other.Min + new Position(0, other.Height))
			       || Contains(other.Min + new Position(other.Width, other.Height))
			       || Contains(other.Min + new Position(other.Width, 0));
		}

	    public bool Equals(ImageRect other)
	    {
	        return From == other.From && To == other.To;
	    }

	    public bool IsEmpty()
	    {
	        return From.X == 0 && To.X == 0 && From.Y == 0 && To.Y == 0;
	    }
    }
}