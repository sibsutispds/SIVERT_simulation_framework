// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace SivertAPI.PosUpdNoMultiGSCM
{

using global::System;
using global::FlatBuffers;

public struct EEBL : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static EEBL GetRootAsEEBL(ByteBuffer _bb) { return GetRootAsEEBL(_bb, new EEBL()); }
  public static EEBL GetRootAsEEBL(ByteBuffer _bb, EEBL obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public EEBL __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public bool Triggered { get { int o = __p.__offset(4); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }
  public int VehID { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<EEBL> CreateEEBL(FlatBufferBuilder builder,
      bool triggered = false,
      int VehID = 0) {
    builder.StartObject(2);
    EEBL.AddVehID(builder, VehID);
    EEBL.AddTriggered(builder, triggered);
    return EEBL.EndEEBL(builder);
  }

  public static void StartEEBL(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddTriggered(FlatBufferBuilder builder, bool triggered) { builder.AddBool(0, triggered, false); }
  public static void AddVehID(FlatBufferBuilder builder, int VehID) { builder.AddInt(1, VehID, 0); }
  public static Offset<EEBL> EndEEBL(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<EEBL>(o);
  }
};


}
