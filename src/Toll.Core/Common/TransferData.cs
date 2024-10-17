using System.ComponentModel.DataAnnotations;

namespace Toll.Core.Common;

public class TransferData
{
    /// <summary>
    /// 数据属于接收的数据还是发送数据
    /// </summary>
    public DataDirection TrDirection { get; set; }
    
    /// <summary>
    /// 传输的数据
    /// </summary>
    public byte[]? Data { get; set; }
}