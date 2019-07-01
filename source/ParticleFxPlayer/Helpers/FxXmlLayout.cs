using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxXmlLayout
{
    public class FxSection
    {
        public string SectionName { get; set; }
        public List<FxAsset> FxAssets { get; set; }

        public FxSection()
        {

        }

        public FxSection(string sectionName, List<FxAsset> assetList)
        {
            SectionName = sectionName;
            FxAssets = assetList;
        }
    }

    public class FxAsset
    {
        public string AssetName { get; set; }
        public List<FxName> FxNames { get; set; }

        public FxAsset()
        {

        }

        public FxAsset(string asset, List<FxName> fxNameList)
        {
            AssetName = asset;
            FxNames = fxNameList;
        }
    }

    public class FxName
    {
        public string PTFXName { get; set; }
        public List<FxEvolution> EvolutionList { get; set; }
        
        public FxName()
        {

        }

        public FxName(string name, List<FxEvolution> evolutionList)
        {
            PTFXName = name;
            EvolutionList = evolutionList;
        }
    }

    public class FxEvolution
    {
        public string EvolutionName { get; set; }
        public float Amount { get; set; }

        public FxEvolution()
        {

        }

        public FxEvolution(string evoName, float amount)
        {
            EvolutionName = evoName;
            Amount = amount;
        }
    }

    public class DetailedFx
    {
        public string AssetName { get; set; }
        public FxName FxName { get; set; }

        public DetailedFx()
        {
        }

        public DetailedFx(string asset, FxName fxname)
        {
            AssetName = asset;
            FxName = fxname;
        }
    }
}
