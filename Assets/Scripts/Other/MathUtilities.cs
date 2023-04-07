using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static System.Math;

public static class MathUtilities
{
    /// Returns (a modulo b)
    public static int Modulo(int a, int b)
    {
        // this method exists because C# % operator is a "remainder" and doesn't work as I expected for negative integers
        // https://stackoverflow.com/questions/2691025/mathematical-modulus-in-c-sharp
        return (Abs(a * b) + a) % b;
    }
}
