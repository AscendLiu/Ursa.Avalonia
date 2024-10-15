using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Toll.Core.Common;


[Serializable]
[StructLayout(LayoutKind.Sequential)]
[TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
public readonly struct BitRange :
    IEquatable<BitRange>
{
    private readonly int _StartBit;
    public int StartBit => _StartBit;

    private readonly int _EndBit;
    public int EndBit => _EndBit;

    public int Length => _EndBit - _StartBit + 1;

    public const int MaxRangeValue = int.MaxValue;
    public const int MinRangeValue = 0;

    public BitRange(int startBit, int endBit)
    {
        if (startBit < MinRangeValue)
        {
            throw new ArgumentOutOfRangeException(nameof(StartBit), "StartBit cannot be less than 0");
        }

        if (endBit < startBit)
        {
            throw new ArgumentOutOfRangeException(nameof(EndBit), "EndBit must be greater than or equal to endBit");
        }

        _StartBit = startBit;
        _EndBit = endBit;
    }

    public static bool operator ==(BitRange left, BitRange right) =>
        left.StartBit == right.StartBit && left.EndBit == right.EndBit;

    public static bool operator !=(BitRange left, BitRange right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (null == obj || GetType() != obj.GetType())
        {
            return false;
        }

        return obj is BitRange range && range == this;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + StartBit.GetHashCode();
            hash = hash * 23 + EndBit.GetHashCode();
            return hash;
        }
    }

    public override string? ToString() => $"({StartBit},{EndBit})";

    public bool Equals(BitRange other)
    {
        return StartBit == other.StartBit && EndBit == other.EndBit;
    }


}