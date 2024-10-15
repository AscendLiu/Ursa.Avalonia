using System.Runtime.CompilerServices;

namespace Toll.Core.Utils;

public static class ThrowHelper
{
    /// <summary>
    /// 如果val类对象是null，则抛出一个ArgumentNullException异常，否则返回val
    /// </summary>
    /// <param name="value">参数</param>
    /// <param name="paramName">参数名字</param>
    /// <typeparam name="T">参数的类型</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">参数为null的异常</exception>
    public static T ThrowIfNull<T>( T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : class
    {
        if (value is null) 
            throw new ArgumentNullException(paramName);
        return value;
    }


    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : struct
    {
        if(value is < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "value is negative");
    }

    public static void ThrowIfArgumentOutOfRange <T>(T value,string? message = null, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        throw new ArgumentOutOfRangeException(paramName, value, message);
    }

    public static void ThrowArgumentException(string? message = null,string? paramName = null)
    {
        throw new ArgumentException(message ?? "ArgumentException", paramName);
    }

    public static void ThrowInvalidOperationException_EnumCurrent(int index)
    {
        throw new InvalidOperationException(index < 0 ? "InvalidOperation_EnumNotStarted" : "InvalidOperation_EnumEnded");
    }
    
    public static void ThrowInvalidOperationException(string? message)
    {
        throw new InvalidOperationException(message);
    }
    
}