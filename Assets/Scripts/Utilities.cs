using System;

static public class Utilities
{
	// adaptation of the Arduino map function
	// takes the value x that's in range (fromMin, fromMax) and maps it to (toMin, toMax)
	// max values are inclusive
	static public float Map(float x, float fromMin, float fromMax, float toMin, float toMax)
	{
		return (x - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
	}
}