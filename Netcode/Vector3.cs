using System;

namespace NetCode
{
	public class Vector3
	{
		public static Vector3 Zero()
		{
			return new Vector3 (0f, 0f, 0f);
		}

		public float x;
		public float y;
		public float z;

		public Vector3(float a, float b, float c){
			x = a;
			y = b;
			z = c;
		}
	}
}

