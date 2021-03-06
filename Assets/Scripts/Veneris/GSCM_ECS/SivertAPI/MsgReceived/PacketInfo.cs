// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace SivertAPI.MsgReceived
{

using global::System;
using global::FlatBuffers;

public struct PacketInfo : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public PacketInfo __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public uint SenderID { get { return __p.bb.GetUint(__p.bb_pos + 0); } }
  public uint ReceiverID { get { return __p.bb.GetUint(__p.bb_pos + 4); } }
  public long TimeReceived { get { return __p.bb.GetLong(__p.bb_pos + 8); } }
  public bool Sent { get { return 0!=__p.bb.Get(__p.bb_pos + 16); } }

  public static Offset<PacketInfo> CreatePacketInfo(FlatBufferBuilder builder, uint SenderID, uint ReceiverID, long TimeReceived, bool Sent) {
    builder.Prep(8, 24);
    builder.Pad(7);
    builder.PutBool(Sent);
    builder.PutLong(TimeReceived);
    builder.PutUint(ReceiverID);
    builder.PutUint(SenderID);
    return new Offset<PacketInfo>(builder.Offset);
  }
};


}
