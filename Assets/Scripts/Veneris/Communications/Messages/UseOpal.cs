/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Veneris.Communications
{

using global::System;
using global::FlatBuffers;

public struct UseOpal : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static UseOpal GetRootAsUseOpal(ByteBuffer _bb) { return GetRootAsUseOpal(_bb, new UseOpal()); }
  public static UseOpal GetRootAsUseOpal(ByteBuffer _bb, UseOpal obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public UseOpal __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float Frequency { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }
  public uint AzimuthDelta { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public uint ElevationDelta { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public uint MaxReflections { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public bool UseDecimalDegrees { get { int o = __p.__offset(12); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }

  public static Offset<UseOpal> CreateUseOpal(FlatBufferBuilder builder,
      float frequency = 0.0f,
      uint azimuthDelta = 0,
      uint elevationDelta = 0,
      uint maxReflections = 0,
      bool useDecimalDegrees = false) {
    builder.StartObject(5);
    UseOpal.AddMaxReflections(builder, maxReflections);
    UseOpal.AddElevationDelta(builder, elevationDelta);
    UseOpal.AddAzimuthDelta(builder, azimuthDelta);
    UseOpal.AddFrequency(builder, frequency);
    UseOpal.AddUseDecimalDegrees(builder, useDecimalDegrees);
    return UseOpal.EndUseOpal(builder);
  }

  public static void StartUseOpal(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddFrequency(FlatBufferBuilder builder, float frequency) { builder.AddFloat(0, frequency, 0.0f); }
  public static void AddAzimuthDelta(FlatBufferBuilder builder, uint azimuthDelta) { builder.AddUint(1, azimuthDelta, 0); }
  public static void AddElevationDelta(FlatBufferBuilder builder, uint elevationDelta) { builder.AddUint(2, elevationDelta, 0); }
  public static void AddMaxReflections(FlatBufferBuilder builder, uint maxReflections) { builder.AddUint(3, maxReflections, 0); }
  public static void AddUseDecimalDegrees(FlatBufferBuilder builder, bool useDecimalDegrees) { builder.AddBool(4, useDecimalDegrees, false); }
  public static Offset<UseOpal> EndUseOpal(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<UseOpal>(o);
  }
  public static void FinishUseOpalBuffer(FlatBufferBuilder builder, Offset<UseOpal> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedUseOpalBuffer(FlatBufferBuilder builder, Offset<UseOpal> offset) { builder.FinishSizePrefixed(offset.Value); }
};


}
