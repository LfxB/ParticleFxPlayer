﻿using System;
using System.Runtime.InteropServices;

// https://gist.github.com/CamxxCore/38591de5b89088883478f09e82f0c924

namespace PTFXStructures
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PtfxAssetStore
    {
        [FieldOffset(0x10)]
        public int MaxItems; //0x10-0x14
        [FieldOffset(0x18)]
        [MarshalAs(UnmanagedType.LPStr)]
        public string StoreName;
        [FieldOffset(0x38)]
        public IntPtr Items; //0x38
        [FieldOffset(0x40)]
        public IntPtr BitMap; //0x40
        [FieldOffset(0x48)]
        public int ItemCount; //0x48-0x4C
        [FieldOffset(0x4C)]
        public int ItemSize;
        [FieldOffset(0x60)]
        [MarshalAs(UnmanagedType.LPStr)]
        public string AssetType; //0x60-0x68
        [FieldOffset(0x82)]
        public short NextAvailableSlot; //0x82 (hashMap + 0x12)
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PtxColour
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }; //sizeof=0x8

    [StructLayout(LayoutKind.Sequential)]
    public struct PtxVarVector
    {
        public PtxColour Min; //0x0-0x8
        public PtxColour Max; //0x8-0x10
    }; //sizeof=0x10

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PtxKeyframeVars
    {
        public PtxVarVector* Vectors;
        public short UnkCount;
        public short MaxVars;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PtxKeyframeData
    {
        public PtxKeyframeVars* Vars;
        public short UnkCount;
        public short MaxVars;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct PtxVarVectorKfp
    {
        [FieldOffset(0x0)]
        public IntPtr SzArg1; //0x0-0x28
        [FieldOffset(0x4)]
        public IntPtr SzArg2;
        [FieldOffset(0x8)]
        public IntPtr SzArg3;
        [FieldOffset(0xC)]
        public IntPtr SzArg4;
        [FieldOffset(0x10)]
        public IntPtr SzArg5;
        [FieldOffset(0x40)]
        public PtxVarVector data;
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PtxKeyframeProp
    {
        [FieldOffset(0x18)]
        public PtxKeyframeData* F1;
        [FieldOffset(0x20)]
        public PtxKeyframeData* F2;
        [FieldOffset(0x28)]
        public PtxKeyframeData* F3;
        [FieldOffset(0x30)]
        public PtxKeyframeData* F4;
        [FieldOffset(0x70)]
        public PgCollection Current; // PtxVarVector* collection
        [FieldOffset(0x88)]
        public PtxVarVectorKfp Defaults;
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Ptxu_Colour
    {
        [FieldOffset(0x8)]
        public uint HashName; // inherited from ptxBehaviour
        [FieldOffset(0x10)]
        public PtxKeyframeProp** KeyframeProps; //0x10-0x18 // inherited from ptxBehaviour
        [FieldOffset(0x18)]
        public short NumFrames; //0x18-0x1A // inherited from ptxBehaviour
        [FieldOffset(0x1A)]
        public short MaxFrames; //0x1A-0x1C assumed // inherited from ptxBehaviour
        [FieldOffset(0xA0)]
        public PgCollection RGBA_Min; //0xA0-0xA8 PtxVarVector* collection
        [FieldOffset(0x130)]
        public PgCollection RGBA_Max; //0x130-0x138 PtxVarVector* collection
        [FieldOffset(0x1C0)]
        public PgCollection Emissive_Intensity; //0x130-0x138 PtxVarVector* collection
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PtxEffectRule
    {
        [FieldOffset(0x20)]
        [MarshalAs(UnmanagedType.LPStr)]
        public string EffectName; //0x20-0x28
        [FieldOffset(0x38)]
        public PtxEventEmitter** Emitters;
        [FieldOffset(0x40)]
        public short EmittersCount;
        [FieldOffset(0x42)]
        public short MaxEmitters;
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PtxParticleRule
    {
        [FieldOffset(0x20)]
        public IntPtr Spawner; //0x20 ptxEffectSpawner
        [FieldOffset(0x90)]
        public IntPtr Spawner1; //0x90-0x100  ptxEffectSpawner
        [FieldOffset(0x128)]
        public Ptxu_Colour** Behaviours; //0x128-0x130
        [FieldOffset(0x130)]
        public short BehavioursCount; //0x130-0x132
        [FieldOffset(0x132)]
        public short MaxBehaviours; // 0x132-0x134 assumed
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PtxEventEmitter
    {
        [FieldOffset(0x8)]
        public int Index; //0x8-0xC
        [FieldOffset(0x18)]
        public IntPtr EvolutionParams; //0x18-0x20
        [FieldOffset(0x30)]
        public IntPtr SzEmitterName; //0x30-0x38
        [FieldOffset(0x38)]
        public IntPtr SzParticleName; //0x38-0x40
        [FieldOffset(0x40)]
        public IntPtr EmitterRule; //0x40-0x48
        [FieldOffset(0x48)]
        public PtxParticleRule* ParticleRule; //0x48-0x50
        [FieldOffset(0x50)]
        public float MoveSpeedScale; //0x50-0x54
        [FieldOffset(0x54)]
        public float MoveSpeedScaleModifier; //0x54-0x58
        [FieldOffset(0x58)]
        public float ParticleScale; //0x58-0x5C
        [FieldOffset(0x5C)]
        public float ParticleScaleModifier; //0x5C-0x60
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct PgCollection
    {
        [FieldOffset(0x0)]
        public IntPtr Items; //0x30-0x38
        [FieldOffset(0x08)]
        public short Count;//0x38-0x3A
        [FieldOffset(0xC)]
        public short Size;//0x38-0x3A
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct PgDictionary
    {
        [FieldOffset(0x30)]
        public IntPtr Items; //0x30-0x38
        [FieldOffset(0x38)]
        public short ItemsCount;//0x38-0x3A
    };

    /*[StructLayout(LayoutKind.Sequential)]
    internal struct FwPool
    {
        public long Items; //0x0-0x8
        public IntPtr BitMap; //0x8-0x10
        public int Count; //0x10-0x14
        public int ItemSize; //0x14-0x18
        public int UnkIndex; //0x18-0x1C
        public int NextFreeSlot; //0x1C-0x20
        public uint Flags; //0x20-0x24
        public int Unk; //0x24-0x28

        public long GetMask(int index)
        {
            long num = Marshal.ReadByte(BitMap + index) & 0x80;
            return ~((num | -num) >> 0x3F);
        }

        public IntPtr GetAddress(int index)
        {
            return new IntPtr(GetMask(index) & (Items + ItemSize * index));
        }

        public bool IsFull()
        {
            return Count - (Flags & 0x3FFFFFFF) <= 256;
        }
    }*/
}
