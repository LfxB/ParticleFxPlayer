using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using PTFXStructures;
using FxXmlLayout;
using Pattern = Memory.MemoryAccess.Pattern;

// https://gist.github.com/CamxxCore/3c81920f4ab30e5659310bd580268aae

namespace Memory
{
    public static unsafe class PtfxMemoryAccess
    {
        private static bool bInitialized = false;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate IntPtr FwGetAssetIndexFn(IntPtr assetStore, out int index, StringBuilder name);

        public delegate IntPtr GetPooledPtfxAddressFn(int handle);

        public delegate ulong PtfxAddressFuncDelegate(int handle);

        private static IntPtr PtfxAssetStorePtr;

        private static FwGetAssetIndexFn FwGetAssetIndex;

        private static readonly uint PtfxColourHash = (uint)Game.GenerateHash("ptxu_Colour");

        private static Dictionary<string, IntPtr> ptfxRulePtrList = new Dictionary<string, IntPtr>();

        private static PtfxAddressFuncDelegate PtfxAddressFunc;

        static PtfxMemoryAccess()
        {
            
        }

        public static void Init()
        {
            #region SetupPTFXAssetStore

            var pattern = new Pattern("\x0F\xBF\x04\x9F\xB9", "xxxxx");

            var result = pattern.Get(0x19);

            if (result != IntPtr.Zero)
            {
                var rip = result.ToInt64() + 7;
                var value = Marshal.ReadInt32(IntPtr.Add(result, 3));
                PtfxAssetStorePtr = new IntPtr(rip + value);
            }

            #endregion

            #region SetupfwGetAssetIndex

            pattern = new Pattern("\x41\x8B\xDE\x4C\x63\x00", "xxxxx?");

            result = pattern.Get();

            if (result != IntPtr.Zero)
            {
                var rip = result.ToInt64();
                var value = Marshal.ReadInt32(result - 4);
                FwGetAssetIndex = Marshal.GetDelegateForFunctionPointer<FwGetAssetIndexFn>(new IntPtr(rip + value));
            }

            #endregion

            #region SetupPTFXAddressFunc

            pattern = new Pattern("\x74\x21\x48\x8B\x48\x20\x48\x85\xC9\x74\x18\x48\x8B\xD6\xE8", "xxxxxxxxxxxxxxx");

            result = pattern.Get();

            if (result != IntPtr.Zero)
            {
                byte* address;

                address = (byte*)result - 10;
                PtfxAddressFunc = Marshal.GetDelegateForFunctionPointer<PtfxAddressFuncDelegate>(new IntPtr(*(int*)(address) + address + 4));
            }

            #endregion

            bInitialized = true;
        }
        
        public static IntPtr GetPtfxAddress(int handle)
        {
            return new IntPtr((long)PtfxAddressFunc(handle));
        }

        private static PgDictionary* GetPtfxRuleDictionary(string ptxAssetName)
        {
            if (bInitialized == false)
                return null;

            var assetStore = Marshal.PtrToStructure<PtfxAssetStore>(PtfxAssetStorePtr);

            FwGetAssetIndex(PtfxAssetStorePtr, out var index, new StringBuilder(ptxAssetName));
            UI.ShowSubtitle(ptxAssetName + " PtfxAssetStorePtr: " + PtfxAssetStorePtr, 10000);
            var ptxFxListPtr = Marshal.ReadIntPtr(assetStore.Items + assetStore.ItemSize * index);

            return (PgDictionary*)Marshal.ReadIntPtr(ptxFxListPtr + 0x48);
        }

        public static bool FindPtxEffectRule(PgDictionary* ptxRulesDict, string fxName, out IntPtr result)
        {
            if (bInitialized == false)
            {
                result = IntPtr.Zero;
                return false;
            }

            for (var i = 0; i < ptxRulesDict->ItemsCount; i++)
            {
                var itAddress = Marshal.ReadIntPtr(ptxRulesDict->Items + i * 8);

                if (itAddress == IntPtr.Zero) continue;

                var szName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itAddress + 0x20));

                if (szName != fxName) continue;

                result = itAddress;

                return true;
            }

            result = IntPtr.Zero;

            return false;
        }

        /// <summary>
        /// Get emitter by its name for the given asset rule
        /// </summary>
        /// <param name="ptxAssetRulePtr">Pointer to the PtfxAssetRule instance</param>
        /// <param name="emitterName">Name of the child emitter object</param>
        /// <returns></returns>
        private static PtxEventEmitter* GetPtfxEventEmitterByName(IntPtr ptxAssetRulePtr, string emitterName)
        {
            if (bInitialized == false)
                return null;

            PtxEventEmitter* foundEmitter = null;

            var ptxRule = Marshal.PtrToStructure<PtxEffectRule>(ptxAssetRulePtr);

            for (var i = 0; i < ptxRule.EmittersCount; i++)
            {
                var emitter = ptxRule.Emitters[i];

                var szName = Marshal.PtrToStringAnsi(emitter->SzEmitterName);

                if (szName == emitterName)
                {
                    foundEmitter = emitter;

                    break;
                }
            }

            return foundEmitter;
        }

        /// <summary>
        /// Lightweight function for when we know the emitters index
        /// </summary>
        /// <param name="ptxAssetRulePtr"></param>
        /// <param name="emitterIndex"></param>
        /// <returns></returns>
        private static PtxEventEmitter* GetPtfxEventEmitterByIndex(IntPtr ptxAssetRulePtr, int emitterIndex)
        {
            return (*(PtxEventEmitter***)IntPtr.Add(ptxAssetRulePtr, 0x38))[emitterIndex];
        }

        public static void SetPtfxLOD(string baseAsset, string particleName)
        {
            string key = baseAsset + ':' + particleName;

            if (!ptfxRulePtrList.TryGetValue(key, out var result) &&
                !FindPtxEffectRule(GetPtfxRuleDictionary(baseAsset), particleName, out result))
            {
                return;
            }

            ptfxRulePtrList[key] = result;
        }

        public static void SetPtfxColor(string baseAsset, string particleName, int emitterIndex, Color newColor)
        {
            if (bInitialized == false)
                return;

            string key = baseAsset + ':' + particleName;

            if (!ptfxRulePtrList.TryGetValue(key, out var result) &&
                !FindPtxEffectRule(GetPtfxRuleDictionary(baseAsset), particleName, out result))
            {
                return;
            }

            ptfxRulePtrList[key] = result;

            PtxEventEmitter* emitter = GetPtfxEventEmitterByIndex(result, emitterIndex);

            //Debug.Assert(emitter != null);

            SetEmitterColour(emitter, newColor);
        }

        private static void SetEmitterColour(PtxEventEmitter* emitter, Color colour)
        {
            SetEmitterColour(emitter, colour.R, colour.G, colour.B, colour.A);
        }

        private static void SetEmitterColour(PtxEventEmitter* emitter, byte red, byte green, byte blue, byte alpha)
        {
            var r = 1.0f / 255 * red;
            var g = 1.0f / 255 * green;
            var b = 1.0f / 255 * blue;
            var a = 1.0f / 255 * alpha;

            for (var i = 0; i < emitter->ParticleRule->BehavioursCount; i++)
            {
                Ptxu_Colour* behaviour = emitter->ParticleRule->Behaviours[i];

                if (behaviour->HashName != PtfxColourHash) continue;

                for (var x = 0; x < behaviour->NumFrames; x++)
                {
                    PtxKeyframeProp* keyframe = behaviour->KeyframeProps[x];

                    if (keyframe->Current.Items == IntPtr.Zero) continue;

                    var items = (PtxVarVector*)keyframe->Current.Items;

                    for (var y = 0; y < keyframe->Current.Count; y++)
                    {
                        if (items == null) continue;

                        items[y].Min.R = r;
                        items[y].Min.G = g;
                        items[y].Min.B = b;
                        items[y].Min.A = a;

                        items[y].Max.R = r;
                        items[y].Max.G = g;
                        items[y].Max.B = b;
                        items[y].Max.A = a;
                    }
                }

                break;
            }
        }

        /*private static void DumpPtxEffectRule(PgDictionary* ptxRulesDict)
        {
            using (var writer = XmlWriter.Create("scripts\\ptfx_dump.xml",
                new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("ptxEffectRules");

                for (var i = 0; i < ptxRulesDict->ItemsCount; i++)
                {
                    var itAddress = Marshal.ReadIntPtr(ptxRulesDict->Items + i * 8);

                    if (itAddress == IntPtr.Zero) continue;

                    var szName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itAddress + 0x20));

                    if (string.IsNullOrEmpty(szName)) continue;

                    var emitters = Marshal.ReadIntPtr(itAddress + 0x38);

                    var emitterCount = Marshal.ReadInt16(itAddress + 0x40);

                    writer.WriteStartElement("ptxEffectRule");

                    writer.WriteAttributeString("name", szName);

                    writer.WriteStartElement("ptxEventEmitters");

                    for (int e = 0; e < emitterCount; e++)
                    {
                        writer.WriteStartElement("item");

                        var itEmitter = Marshal.ReadIntPtr(emitters + 0x8 * e);

                        string emitterName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itEmitter + 0x30));

                        //string secondaryName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itEmitter + 0x38));

                        writer.WriteAttributeString("name", emitterName);

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    var parameters = Marshal.ReadIntPtr(itAddress + 0x48);

                    if (parameters != IntPtr.Zero)
                    {
                        writer.WriteStartElement("ptxEvolutionArgs");

                        var parametersList = Marshal.ReadIntPtr(parameters);

                        var numParams = Marshal.ReadInt16(parameters + 0x8);

                        for (int p = 0; p < numParams; p++)
                        {
                            var itParam = parametersList + p * 0x18;

                            writer.WriteStartElement("item");

                            writer.WriteAttributeString("name", Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itParam)));

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }*/

        private static void DumpPtxEffectRule(PgDictionary* ptxRulesDict, string dict)
        {
            string path = @"scripts\Particle Asset Dump\" + dict + ".txt";

            File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {

                for (var i = 0; i < ptxRulesDict->ItemsCount; i++)
                {
                    var itAddress = Marshal.ReadIntPtr(ptxRulesDict->Items + i * 8);

                    if (itAddress == IntPtr.Zero) continue;

                    var szName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itAddress + 0x20));

                    if (string.IsNullOrEmpty(szName)) continue;

                    sw.Write("\"" + szName + "\"");
                    
                    var parameters = Marshal.ReadIntPtr(itAddress + 0x48);

                    if (parameters != IntPtr.Zero)
                    {
                        string argsString = "-> ";

                        var parametersList = Marshal.ReadIntPtr(parameters);

                        var numParams = Marshal.ReadInt16(parameters + 0x8);

                        for (int p = 0; p < numParams; p++)
                        {
                            var itParam = parametersList + p * 0x18;

                            argsString += Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itParam));

                            if (p < numParams - 1)
                                argsString += ", ";
                        }

                        sw.Write(argsString);
                    }

                    sw.WriteLine();
                }
            }
        }

        private static void PtxEffectRuleToXML(PgDictionary* ptxRulesDict, FxAsset fxAsset)
        {
            for (var i = 0; i < ptxRulesDict->ItemsCount; i++)
            {
                var itAddress = Marshal.ReadIntPtr(ptxRulesDict->Items + i * 8);

                if (itAddress == IntPtr.Zero) continue;

                var ptfxName = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itAddress + 0x20));

                if (string.IsNullOrEmpty(ptfxName)) continue;

                FxName fx = new FxName(ptfxName, new List<FxEvolution>());

                var parameters = Marshal.ReadIntPtr(itAddress + 0x48);

                if (parameters != IntPtr.Zero)
                {
                    var parametersList = Marshal.ReadIntPtr(parameters);

                    var numParams = Marshal.ReadInt16(parameters + 0x8);

                    for (int p = 0; p < numParams; p++)
                    {
                        var itParam = parametersList + p * 0x18;

                        string evolutionParam = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(itParam));
                        fx.EvolutionList.Add(new FxEvolution(evolutionParam, 1.0f));

                    }
                }

                fxAsset.FxNames.Add(fx);
            }
        }

        public static void DumpPtfxAsset(string dict = "core")
        {
            var ptxRulesDict = GetPtfxRuleDictionary(dict);

            if (ptxRulesDict != null)
            {
                DumpPtxEffectRule(ptxRulesDict, dict);
            }
        }

        public static void PtfxAssetToXML(string dict, FxAsset fxAsset)
        {
            var ptxRulesDict = GetPtfxRuleDictionary(dict);

            if (ptxRulesDict != null)
            {
                PtxEffectRuleToXML(ptxRulesDict, fxAsset);
            }
        }
    }
}