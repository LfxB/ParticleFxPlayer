using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleFxHelper
{
    public class PTFXNonLooped
    {
        public PTFXNonLooped()
        {

        }
    }

    public static class PTFXHelper
    {
        public static void RemovePTFX(int handle)
        {
            Function.Call(Hash.REMOVE_PARTICLE_FX, handle, true);
        }

        public static void RemovePTFXInRange(Vector3 position, float range)
        {
            Function.Call(Hash.REMOVE_PARTICLE_FX_IN_RANGE, position.X, position.Y, position.Z, range);
        }

        public static bool HasNamedPTFXAssetLoaded(string asset)
        {
            return Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, asset);
        }

        public static void RequestNamedPTFXAsset(string asset)
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, asset);
        }

        private static void SetPTFXAssetNextCall(string asset)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
        }

        public static bool DoesPTFXLoopedExist(int handle)
        {
            return Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, handle);
        }

        public static int StartPTFXAtCoordinate(string asset, string fxname, Vector3 position, Vector3 rot, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, fxname, position.X, position.Y, position.Z, rot.X, rot.Y, rot.Z, fxScale, false, false, false, false);
        }

        public static int StartPTFXOnEntity(string asset, string fxname, Entity entity, Vector3 posOffset, Vector3 rot, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, fxname, entity, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z, fxScale, false, false, false);
        }

        public static int StartPTFXOnPedBone(string asset, string fxname, Ped ped, Vector3 posOffset, Vector3 rot, int boneindex, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, fxname, ped, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z, boneindex, fxScale, false, false, false);
        }

        public static int SpawnPTFXAtCoordinate(string asset, string fxname, Vector3 position, Vector3 rot, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, fxname, position.X, position.Y, position.Z, rot.X, rot.Y, rot.Z, fxScale, false, false, false, false);
        }

        public static int SpawnPTFXOnEntity(string asset, string fxname, Entity entity, Vector3 posOffset, Vector3 rot, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY, fxname, entity, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z, fxScale, false, false, false);
        }

        public static int SpawnPTFXOnPedBone(string asset, string fxname, Ped ped, Vector3 posOffset, Vector3 rot, int boneindex, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_PED_BONE, fxname, ped, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z, boneindex, fxScale, false, false, false);
        }

        public static int SpawnPTFXOnEntityBone(string asset, string fxname, Entity entity, Vector3 posOffset, Vector3 rot, int boneindex, float fxScale)
        {
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, asset);
            return Function.Call<int>(Hash._START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE, fxname, entity, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z, boneindex, fxScale, false, false, false);
        }

        public static void SetOffset(int handle, Vector3 posOffset, Vector3 rot)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_OFFSETS, handle, posOffset.X, posOffset.Y, posOffset.Z, rot.X, rot.Y, rot.Z);
        }

        public static void SetEvolution(int handle, string evolutionName, float amount, bool Id = false)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, handle, evolutionName, amount, Id);
        }

        public static void SetColour(int handle, float red, float green, float blue)
        {
            if (handle > -1)
            {
                Function.Call(Hash.SET_PARTICLE_FX_LOOPED_COLOUR, handle, red / 255.0f, green / 255.0f, blue / 255.0f);
            }
            else
            {
                Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, red / 255.0f, green / 255.0f, blue / 255.0f);
            }
        }

        public static void SetAlpha(int handle, float alpha = 1.0f)
        {
            if (handle > -1)
            {
                Function.Call(Hash.SET_PARTICLE_FX_LOOPED_ALPHA, handle, alpha);
            }
            else
            {
                Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_ALPHA, alpha);
            }
        }

        public static void SetScale(int handle, float scale)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_SCALE, handle, scale);
        }

        public static void SetRange(int handle, float range)
        {
            Function.Call((Hash)0xDCB194B85EF7B541, handle, range); //_SET_PARTICLE_FX_LOOPED_RANGE
        }
    }
}
