using MemoryPack;
using CraftNet.Services;


namespace Demo
{
    
    [MemoryPackable]
    public partial class Login2G_GetTokenReq : IRequest, IMessageMeta
    {
        public static ushort Opcode => 1;
        ushort IMessageBase.GetOpcode() => Opcode;
        /// <summary>
        /// 账号Id
        /// </summary>
        [MemoryPackOrder(1)] public int AccountId { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        [MemoryPackOrder(2)] public string Token { get; set; }
            
    }
    
    [MemoryPackable]
    public partial class Login2G_GetTokenResp : IResponse, IMessageMeta
    {
        public static ushort Opcode => 2;
        ushort IMessageBase.GetOpcode() => Opcode;
        [MemoryPackOrder(1)] public int Err { get; set; }
            
    }

}