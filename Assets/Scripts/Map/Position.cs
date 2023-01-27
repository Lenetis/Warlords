using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Position
{
    public int x;
    public int y;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector3(Position position)
    {
        return new Vector3(position.x, position.y);
    }

    public static implicit operator Vector2(Position position)
    {
        return new Vector2(position.x, position.y);
    }

    // I really dislike that I had to write like 50 lines of code
    // just to be able to check if position1 != position2   >:(

    // override object.Equals
    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        Position other = (Position)obj;
        return x == other.x && y == other.y;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        int tmp = y + ((x + 1) / 2);
        return x + tmp*tmp;  // from: https://stackoverflow.com/a/22826582
    }

    public static bool operator ==(Position a, Position b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        // Return true if the fields match:
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Position a, Position b)
    {
        return !(a == b);
    }

    public static Position operator +(Position a, Position b)
    {
        return new Position(a.x + b.x, a.y + b.y);
    }

    public static Position operator -(Position a, Position b)
    {
        return new Position(a.x - b.x, a.y - b.y);
    }

    public override string ToString()
    {
        return $"Position({x}, {y})";
    }
}
