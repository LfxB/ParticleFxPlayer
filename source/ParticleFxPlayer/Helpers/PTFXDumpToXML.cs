using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using XMLSerializationHelper;
using FxXmlLayout;
using GTA;
using PtfxMemoryAccess = Memory.PtfxMemoryAccess;

namespace ParticleFxPlayer
{
    public static class PTFXDumpToXML
    {
        private static void CamxxCore_Dump(ref List<FxAsset> fxAssets)
        {
            List<string> assetList = Directory.GetFiles(@"scripts\Particle Asset Dump").ToList();

            foreach (string txtFilePath in assetList)
            {
                string assetName = Path.GetFileNameWithoutExtension(txtFilePath);

                FxAsset asset = new FxAsset(assetName, new List<FxName>());

                List<string> lines = File.ReadAllLines(txtFilePath).ToList();

                foreach (string line in lines)
                {
                    if (line.Contains("\""))
                    {
                        var ptfxName = line.Split('\"', '\"')[1];

                        FxName fx = new FxName(ptfxName, new List<FxEvolution>());

                        if (line.Contains("->"))
                        {
                            var evolutionArgs = line.Substring(line.LastIndexOf("-> ") + 3);

                            if (evolutionArgs.Contains(","))
                            {
                                IList<string> evoArgList = evolutionArgs.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var arg in evoArgList)
                                {
                                    fx.EvolutionList.Add(new FxEvolution(arg, 1.0f));
                                }
                            }
                            else
                            {
                                fx.EvolutionList.Add(new FxEvolution(evolutionArgs, 1.0f));
                            }
                        }

                        asset.FxNames.Add(fx);
                    }
                }

                fxAssets.Add(asset);
            }
        }

        private static void Alexguirre_Dump(ref List<FxAsset> fxAssets)
        {
            using (StreamReader file = File.OpenText(@"scripts\Particle Asset Dump\PTFX Dump - AlexGuirre\Particles Effects Dump.txt"))
            {
                string currentAsset = "";

                FxAsset fxAsset = new FxAsset();

                string readLine = file.ReadLine();

                while (!file.EndOfStream)
                {
                    if (readLine.Contains('#') || string.IsNullOrWhiteSpace(readLine))
                    {
                        readLine = file.ReadLine();
                        continue;
                    }

                    // if read line is an asset name
                    if (readLine.Contains('['))
                    {
                        currentAsset = readLine.Replace("[", "").Replace("]", "").Replace(" ", "");

                        fxAsset = fxAssets.FirstOrDefault(x => x.AssetName.Equals(currentAsset));

                        // if the fx asset doesn't exist
                        if (fxAsset == default(FxAsset))
                        {
                            fxAsset = new FxAsset(currentAsset, new List<FxName>());
                            fxAssets.Add(fxAsset);
                        }
                    }
                    // if read line is an fx name
                    else
                    {
                        // if the fx doesn't exist
                        if (!fxAsset.FxNames.Exists(x => x.PTFXName.Equals(readLine.Replace(" ", ""))))
                        {
                            fxAsset.FxNames.Add(new FxName(readLine.Replace(" ", ""), new List<FxEvolution>()));
                        }
                    }

                    readLine = file.ReadLine();
                }
            }
        }

        /// <summary>
        /// Attempts to dump all ptfx assets from Alexguirre's list directly from memory.
        /// Unfortunately, always causes an AccessViolationException after the first asset.
        /// Only conclusion is there must be some delay between each asset dump to avoid this exception..
        /// </summary>
        /// <param name="fxAssets"></param>
        private static void DumpFullPtfxInfo(ref List<FxAsset> fxAssets)
        {
            FxAsset fxAsset = new FxAsset();

            List<string> assets = new List<string>();

            using (StreamReader file = File.OpenText(@"scripts\Particle Asset Dump\PTFX Dump - AlexGuirre\Particles Effects Dump.txt"))
            {
                string readLine = file.ReadLine();

                while (!file.EndOfStream)
                {
                    if (readLine.Contains('#') || string.IsNullOrWhiteSpace(readLine))
                    {
                        readLine = file.ReadLine();
                        continue;
                    }

                    // if read line is an asset name
                    if (readLine.Contains('['))
                    {
                        assets.Add(readLine.Replace("[", "").Replace("]", "").Replace(" ", ""));
                    }

                    readLine = file.ReadLine();
                }
            }

            foreach (var asset in assets)
            {
                fxAsset = fxAssets.FirstOrDefault(x => x.AssetName.Equals(asset));

                // if the fx asset doesn't exist
                if (fxAsset == default(FxAsset))
                {
                    fxAsset = new FxAsset(asset, new List<FxName>());

                    PtfxMemoryAccess.PtfxAssetToXML(asset, fxAsset);

                    fxAssets.Add(fxAsset);
                }
            }
        }

        public static void DumpToCamxxCoreStyleTxt(string asset)
        {
            PtfxMemoryAccess.DumpPtfxAsset(asset);
        }

        public static void ConvertToXML()
        {
            List<FxAsset> FxAssets = new List<FxAsset>();

            CamxxCore_Dump(ref FxAssets);

            Alexguirre_Dump(ref FxAssets);

            //DumpFullPtfxInfo(ref FxAssets);

            FxAssets.Sort((fa1, fa2) => string.Compare(fa1.AssetName, fa2.AssetName, true));

            XMLHelper.SaveObjectToXML(FxAssets, @"scripts\ParticleFxList.xml");
        }
    }
}
