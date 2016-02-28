// Dominion - Copyright (C) Timothy Ings
// Building.cs
// This file defines classes that define a building

using ArwicEngine.TypeConverters;
using Dominion.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Dominion.Common.Data
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class Building
    {
        /// <summary>
        /// The name of the building
        /// </summary>
        [Description("The name of the building")]
        [DisplayName("Name"), Browsable(true), Category("General")]
        [XmlElement("Name")]
        public string Name { get; set; } = "BUILDING_NULL";

        /// <summary>
        /// The name of the building in a display ready format
        /// </summary>
        [XmlIgnore]
        public string DisplayName { get; set; } = "Null";

        /// <summary>
        /// The building's description
        /// </summary>
        [Description("The building's description")]
        [DisplayName("Description"), Browsable(true), Category("General")]
        [XmlElement("Description")]
        public string Description { get; set; } = "Default description";

        /// <summary>
        /// Tags the building has
        /// </summary>
        [Description("Tags the building has")]
        [DisplayName("Tags"), Browsable(true), Category("General")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("Tags"), XmlArrayItem(typeof(string), ElementName = "Tag")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// The maintenence cost of the building per turn
        /// </summary>
        [Description("The maintenence cost of the building per turn")]
        [DisplayName("Gold Upkeep"), Browsable(true), Category("General")]
        [XmlElement("GoldUpkeep")]
        public int GoldMaintenance { get; set; } = 0;

        /// <summary>
        /// If the city requires access to water
        /// </summary>
        [Description("If the city requires access to water")]
        [DisplayName("Water"), Browsable(true), Category("Geography")]
        [XmlElement("Water")]
        public bool Water { get; set; } = false;

        /// <summary>
        /// If the city requires access to a river
        /// </summary>
        [Description("If the city requires access to a river")]
        [DisplayName("River"), Browsable(true), Category("Geography")]
        [XmlElement("River")]
        public bool River { get; set; } = false;

        /// <summary>
        /// If the city requires access to fresh water
        /// </summary>
        [Description("If the city requires access to fresh water")]
        [DisplayName("Fresh Water"), Browsable(true), Category("Geography")]
        [XmlElement("FreshWater")]
        public bool FreshWater { get; set; } = false;

        /// <summary>
        /// If the city requires access to a mountain
        /// </summary>
        [Description("If the city requires access to a mountain")]
        [DisplayName("Mountain"), Browsable(true), Category("Geography")]
        [XmlElement("Mountain")]
        public bool Mountain { get; set; } = false;

        /// <summary>
        /// If the city must be built on a hill
        /// </summary>
        [Description("City must be built on a hill")]
        [DisplayName("Hill"), Browsable(true), Category("Geography")]
        [XmlElement("Hill")]
        public bool Hill { get; set; } = false;

        /// <summary>
        /// If the city cannot be built on a hill
        /// </summary>
        [Description("City cannot be built on a hill")]
        [DisplayName("Flat"), Browsable(true), Category("Geography")]
        [XmlElement("Flat")]
        public bool Flat { get; set; } = false;

        ///// <summary>
        ///// If the building is religious
        ///// </summary>
        //[Description("If the building is religious")]
        //[DisplayName("IsReligious"), Browsable(true), Category("General")]
        //[XmlElement("IsReligious")]
        //public bool IsReligious { get; set; } = false;

        ///// <summary>
        ///// If the city is a wonder
        ///// </summary>
        //[Description("If the building is a wonder")]
        //[DisplayName("IsWonder"), Browsable(true), Category("General")]
        //[XmlElement("IsWonder")]
        //public bool IsWonder { get; set; } = false;

        /// <summary>
        /// If the building hinders enemy unit movment in the city's borders
        /// </summary>
        [Description("If the building hinders enemy unit movment in the city's borders")]
        [DisplayName("BorderObstacle"), Browsable(true), Category("City")]
        [XmlElement("BorderObstacle")]
        public bool BorderObstacle { get; set; } = false;

        /// <summary>
        /// If the building can only be constructed in a capital city
        /// </summary>
        [Description("If the building can only be constructed in a capital city")]
        [DisplayName("Capital"), Browsable(true), Category("General")]
        [XmlElement("Capital")]
        public bool Capital { get; set; } = false;

        /// <summary>
        /// If constructing the building causes a golden age
        /// </summary>
        [Description("If constructing the building causes a golden age")]
        [DisplayName("GoldenAge"), Browsable(true), Category("General")]
        [XmlElement("GoldenAge")]
        public bool GoldenAge { get; set; } = false;

        /// <summary>
        /// If the building is immune to nukes
        /// </summary>
        [Description("If the building is immune to nukes")]
        [DisplayName("NukeImmune"), Browsable(true), Category("General")]
        [XmlElement("NukeImmune")]
        public bool NukeImmune { get; set; } = false;

        /// <summary>
        /// If the city can establish water trade routes
        /// </summary>
        [Description("If the city can establish water trade routes")]
        [DisplayName("AllowsWaterRoutes"), Browsable(true), Category("Trade")]
        [XmlElement("AllowsWaterRoutes")]
        public bool AllowsWaterRoutes { get; set; } = false;

        /// <summary>
        /// If the city produces extra luxuries
        /// </summary>
        [Description("If the city produces extra luxuries")]
        [DisplayName("ExtraLuxuries"), Browsable(true), Category("Happiness")]
        [XmlElement("ExtraLuxuries")]
        public bool ExtraLuxuries { get; set; } = false;

        /// <summary>
        /// Votes earned by building
        /// </summary>
        [Description("Votes earned by building")]
        [DisplayName("DiplomaticVotes"), Browsable(true), Category("Diplomacy")]
        [XmlElement("DiplomaticVotes")]
        public int DiplomaticVotes { get; set; } = 0;

        /// <summary>
        /// Production cost of the building
        /// </summary>
        [Description("Production cost of the building")]
        [DisplayName("Cost"), Browsable(true), Category("General")]
        [XmlElement("Cost")]
        public int Cost { get; set; } = 0;

        /// <summary>
        /// Modifier to effectivness of golden age
        /// </summary>
        [Description("Modifier to effectivness of golden age")]
        [DisplayName("GoldenAgeModifier"), Browsable(true), Category("General")]
        [XmlElement("GoldenAgeModifier")]
        public float GoldenAgeModifier { get; set; } = 1f;

        /// <summary>
        /// Modifier to gold cost to upgrade units
        /// </summary>
        [Description("Modifier to gold cost to upgrade units")]
        [DisplayName("UnitUpgradeCostMod"), Browsable(true), Category("Unit")]
        [XmlElement("UnitUpgradeCostMod")]
        public float UnitUpgradeCostMod { get; set; } = 1f;

        /// <summary>
        /// Experience granted to all military units produced by the city
        /// </summary>
        [Description("Experience granted to all military units produced by the city")]
        [DisplayName("Experience"), Browsable(true), Category("Unit")]
        [XmlElement("Experience")]
        public int Experience { get; set; } = 0;

        /// <summary>
        /// Global experience granted to all military units
        /// </summary>
        [Description("Global experience granted to all military units")]
        [DisplayName("GlobalExperience"), Browsable(true), Category("Unit")]
        [XmlElement("GlobalExperience")]
        public int GlobalExperience { get; set; } = 0;

        /// <summary>
        /// If the city cannot be built on a hill
        /// </summary>
        [Description("Percentage of excess food kept after population grows")]
        [DisplayName("FoodKept"), Browsable(true), Category("City")]
        [XmlElement("FoodKept")]
        public float FoodKept { get; set; } = 1f;

        /// <summary>
        /// Local modifier to nuke effectivness
        /// </summary>
        [Description("Local modifier to nuke effectivness")]
        [DisplayName("NukeModifier"), Browsable(true), Category("City")]
        [XmlElement("NukeModifier")]
        public float NukeModifier { get; set; } = 1f;

        /// <summary>
        /// Change to city heal rate
        /// </summary>
        [Description("Change to city heal rate")]
        [DisplayName("HealRateChange"), Browsable(true), Category("City")]
        [XmlElement("HealRateChange")]
        public int HealRateChange { get; set; } = 0;

        /// <summary>
        /// If the city cannot be built on a hill
        /// </summary>
        [Description("City cannot be built on a hill")]
        [DisplayName("Happiness"), Browsable(true), Category("Happiness")]
        [XmlElement("Happiness")]
        public int Happiness { get; set; } = 0;

        /// <summary>
        /// Modifier to unhappiness produced by number of cities
        /// </summary>
        [Description("Local unhappiness modifier")]
        [DisplayName("UnhappinessModifier"), Browsable(true), Category("Happiness")]
        [XmlElement("UnhappinessModifier")]
        public float UnhappinessModifier { get; set; } = 1f;

        /// <summary>
        /// Happiness produced per city
        /// </summary>
        [Description("Global happiness produced per city")]
        [DisplayName("HappinessPerCity"), Browsable(true), Category("Happiness")]
        [XmlElement("HappinessPerCity")]
        public int HappinessPerCity { get; set; } = 0;

        /// <summary>
        /// Modifier to unhappiness produced by number of cities
        /// </summary>
        [Description("Modifier to unhappiness produced by number of cities")]
        [DisplayName("CityCountUnhappinessMod"), Browsable(true), Category("Happiness")]
        [XmlElement("CityCountUnhappinessMod")]
        public float CityCountUnhappinessMod { get; set; } = 1f;

        /// <summary>
        /// Prevents unhappiness from city occupation
        /// </summary>
        [Description("Prevents unhappiness from city occupation")]
        [DisplayName("NoOccupiedUnhappiness"), Browsable(true), Category("Happiness")]
        [XmlElement("NoOccupiedUnhappiness")]
        public bool NoOccupiedUnhappiness { get; set; } = false;

        /// <summary>
        /// Modifier to worker tile improvment construction speed
        /// </summary>
        [Description("Modifier to worker tile improvment construction speed")]
        [DisplayName("WorkerSpeedModifier"), Browsable(true), Category("Unit")]
        [XmlElement("WorkerSpeedModifier")]
        public float WorkerSpeedModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to military production
        /// </summary>
        [Description("Local modifier to military production")]
        [DisplayName("MilitaryProductionModifier"), Browsable(true), Category("Production Modifier")]
        [XmlElement("MilitaryProductionModifier")]
        public float MilitaryProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to spaceship production
        /// </summary>
        [Description("Local modifier to spaceship production")]
        [DisplayName("SpaceProductionModifier"), Browsable(true), Category("Production Modifier")]
        [XmlElement("SpaceProductionModifier")]
        public float SpaceProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to building production
        /// </summary>
        [Description("Local modifier to building production")]
        [DisplayName("BuildingProductionModifier"), Browsable(true), Category("Production Modifier")]
        [XmlElement("BuildingProductionModifier")]
        public float BuildingProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to wonder production
        /// </summary>
        [Description("Local modifier to wonder production")]
        [DisplayName("WonderProductionModifier"), Browsable(true), Category("Production Modifier")]
        [XmlElement("WonderProductionModifier")]
        public float WonderProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to the effectivness of trade routes
        /// </summary>
        [Description("Local modifier to the effectivness of trade routes")]
        [DisplayName("TradeRouteModifier"), Browsable(true), Category("Trade")]
        [XmlElement("TradeRouteModifier")]
        public float TradeRouteModifier { get; set; } = 1f;

        /// <summary>
        /// Global modifier to the effectivness of trade routes
        /// </summary>
        [Description("Global modifier to the effectivness of trade routes")]
        [DisplayName("GlobalTradeRouteModifier"), Browsable(true), Category("Trade")]
        [XmlElement("GlobalTradeRouteModifier")]
        public float GlobalTradeRouteModifier { get; set; } = 1f;

        /// <summary>
        /// Modifier to plundered gold recieved if the city is captured
        /// </summary>
        [Description("Modifier to plundered gold recieved if the city is captured")]
        [DisplayName("CapturePlunderModifier"), Browsable(true), Category("General")]
        [XmlElement("CapturePlunderModifier")]
        public float CapturePlunderModifier { get; set; } = 1f;

        /// <summary>
        /// Modifier to the cost of future social policies
        /// </summary>
        [Description("Modifier to the cost of future social policies")]
        [DisplayName("PolicyCostModifier"), Browsable(true), Category("General")]
        [XmlElement("PolicyCostModifier")]
        public float PolicyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to city culture border expansion
        /// </summary>
        [Description("Local modifier to city culture border expansion")]
        [DisplayName("TileCultureCostModifier"), Browsable(true), Category("City")]
        [XmlElement("TileCultureCostModifier")]
        public float TileCultureCostModifier { get; set; } = 1f;

        /// <summary>
        /// Global modifier to city culture border expansion
        /// </summary>
        [Description("Global modifier to city culture border expansion")]
        [DisplayName("GlobalTileCultureCostModifier"), Browsable(true), Category("City")]
        [XmlElement("GlobalTileCultureCostModifier")]
        public float GlobalTileCultureCostModifier { get; set; } = 1f;

        /// <summary>
        /// Local modifier to city tile purchasing
        /// </summary>
        [Description("Local modifier to city tile purchasing")]
        [DisplayName("TileBuyCostModifier"), Browsable(true), Category("City")]
        [XmlElement("TileBuyCostModifier")]
        public float TileBuyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Global modifier to city tile purchasing
        /// </summary>
        [Description("Global modifier to city tile purchasing")]
        [DisplayName("GlobalTileBuyCostModifier"), Browsable(true), Category("City")]
        [XmlElement("GlobalTileBuyCostModifier")]
        public float GlobalTileBuyCostModifier { get; set; } = 1f;

        /// <summary>
        /// Number of free technologies earned
        /// </summary>
        [Description("Number of free technologies earned")]
        [DisplayName("FreeTechs"), Browsable(true), Category("Free Items")]
        [XmlElement("FreeTechs")]
        public int FreeTechs { get; set; } = 0;

        /// <summary>
        /// Number of free social policies earned
        /// </summary>
        [Description("Number of free social policies earned")]
        [DisplayName("FreePolicies"), Browsable(true), Category("Free Items")]
        [XmlElement("FreePolicies")]
        public int FreePolicies { get; set; } = 0;

        /// <summary>
        /// Number of free great people produced
        /// </summary>
        [Description("Number of free great people produced")]
        [DisplayName("FreeGreatPeople"), Browsable(true), Category("Free Items")]
        [XmlElement("FreeGreatPeople")]
        public int FreeGreatPeople { get; set; } = 0;

        /// <summary>
        /// Defense contributed to city
        /// </summary>
        [Description("Defense contributed to city")]
        [DisplayName("Defense"), Browsable(true), Category("City")]
        [XmlElement("Defense")]
        public int Defense { get; set; } = 0;

        /// <summary>
        /// Local modifer to city defense
        /// </summary>
        [Description("Local modifer to city defense")]
        [DisplayName("DefenseModifier"), Browsable(true), Category("City")]
        [XmlElement("DefenseModifier")]
        public float DefenseModifier { get; set; } = 1f;

        /// <summary>
        /// Global modifer to city defense
        /// </summary>
        [Description("Global modifer to city defense")]
        [DisplayName("GlobalDefenseModifier"), Browsable(true), Category("City")]
        [XmlElement("GlobalDefenseModifier")]
        public float GlobalDefenseModifier { get; set; } = 1f;

        /// <summary>
        /// The number of victory points earned
        /// </summary>
        [Description("Victory points contributed to empire")]
        [DisplayName("VictoryPoints"), Browsable(true), Category("General")]
        [XmlElement("VictoryPoints")]
        public int VictoryPoints { get; set; } = 0;

        /// <summary>
        /// City must have access to this terrain
        /// </summary>
        [Description("City must have access to this terrain")]
        [DisplayName("NearbyTerrainRequired"), Browsable(true), Category("Geography")]
        [XmlElement("NearbyTerrainRequired")]
        public TileTerrainBase NearbyTerrainRequired { get; set; } = TileTerrainBase.Null;

        /// <summary>
        /// City cannot be on this terrain
        /// </summary>
        [Description("City cannot be on this terrain")]
        [DisplayName("ProhibitedCityTerrain"), Browsable(true), Category("Geography")]
        [XmlElement("ProhibitedCityTerrain")]
        public TileTerrainBase ProhibitedCityTerrain { get; set; } = TileTerrainBase.Null;

        /// <summary>
        /// The key to used to get the icon from the icon atlas
        /// </summary>
        [Description("The key to used to get the icon from the icon atlas")]
        [DisplayName("Icon Key"), Browsable(true), Category("Graphics")]
        [XmlElement("IconKey")]
        public string IconKey { get; set; } = "BUILDING_NULL";

        /// <summary>
        /// The icon atlas to source the building's icon from
        /// </summary>
        [Description("The icon atlas to source the building's icon from")]
        [DisplayName("Icon Atlas"), Browsable(true), Category("Graphics")]
        [XmlElement("IconAtlas")]
        public string IconAtlas { get; set; } = "Core:XML/AtlasDefinitions/BuildingAtlasDefinition";

        /// <summary>
        /// A list of buildings that need to be built in the same city before this building becomes available
        /// </summary>
        [Description("A list of buildings that need to be built in the same city before this building becomes available")]
        [DisplayName("Building Prereq"), Browsable(true), Category("Prerequisites")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("BuildingPrereqs"), XmlArrayItem(typeof(string), ElementName = "BuildingPrereq")]
        public List<string> BuildingPrereq { get; set; } = new List<string>();

        /// <summary>
        /// A list of technologies that need to be unlocked before this building becomes available
        /// </summary>
        [Description("A list of technologies that need to be unlocked before this building becomes available")]
        [DisplayName("Tech Prereq"), Browsable(true), Category("Prerequisites")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("TechPrereqs"), XmlArrayItem(typeof(string), ElementName = "TechPrereq")]
        public List<string> TechPrereq { get; set; } = new List<string>();

        /// <summary>
        /// Food produced by this building
        /// </summary>
        [Description("Food produced by this building")]
        [DisplayName("IncomeFood"), Browsable(true), Category("Income")]
        [XmlElement("IncomeFood")]
        public int IncomeFood { get; set; } = 0;

        /// <summary>
        /// Production produced by this building
        /// </summary>
        [Description("Production produced by this building")]
        [DisplayName("IncomeProduction"), Browsable(true), Category("Income")]
        [XmlElement("IncomeProduction")]
        public int IncomeProduction { get; set; } = 0;

        /// <summary>
        /// Gold produced by this building
        /// </summary>
        [Description("Gold produced by this building")]
        [DisplayName("IncomeGold"), Browsable(true), Category("Income")]
        [XmlElement("IncomeGold")]
        public int IncomeGold { get; set; } = 0;

        /// <summary>
        /// Science produced by this building
        /// </summary>
        [Description("Science produced by this building")]
        [DisplayName("IncomeScience"), Browsable(true), Category("Income")]
        [XmlElement("IncomeScience")]
        public int IncomeScience { get; set; } = 0;

        /// <summary>
        /// Culture produced by this building
        /// </summary>
        [Description("Food produced by this building")]
        [DisplayName("IncomeCulture"), Browsable(true), Category("Income")]
        [XmlElement("IncomeCulture")]
        public int IncomeCulture { get; set; } = 0;

        /// <summary>
        /// Faith produced by this building
        /// </summary>
        [Description("Faith produced by this building")]
        [DisplayName("IncomeFaith"), Browsable(true), Category("Income")]
        [XmlElement("IncomeFaith")]
        public int IncomeFaith { get; set; } = 0;

        /// <summary>
        /// Tourism produced by this building
        /// </summary>
        [Description("Tourism produced by this building")]
        [DisplayName("IncomeTourism"), Browsable(true), Category("Income")]
        [XmlElement("IncomeTourism")]
        public int IncomeTourism { get; set; } = 0;

        /// <summary>
        /// Local food income modifier
        /// </summary>
        [Description("Local food income modifier")]
        [DisplayName("IncomeFoodModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeFoodModifier")]
        public float IncomeFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Local production income modifier
        /// </summary>
        [Description("Local production income modifier")]
        [DisplayName("IncomeProductionModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeProductionModifier")]
        public float IncomeProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local gold income modifier
        /// </summary>
        [Description("Local gold income modifier")]
        [DisplayName("IncomeGoldModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeGoldModifier")]
        public float IncomeGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Local science income modifier
        /// </summary>
        [Description("Local science income modifier")]
        [DisplayName("IncomeScienceModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeScienceModifier")]
        public float IncomeScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Local culture income modifier
        /// </summary>
        [Description("Local culture income modifier")]
        [DisplayName("IncomeCultureModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeCultureModifier")]
        public float IncomeCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Local faith income modifier
        /// </summary>
        [Description("Local faith income modifier")]
        [DisplayName("IncomeFaithModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeFaithModifier")]
        public float IncomeFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Local tourism income modifier
        /// </summary>
        [Description("Local tourism income modifier")]
        [DisplayName("IncomeTourismModifier"), Browsable(true), Category("Income Modifier")]
        [XmlElement("IncomeTourismModifier")]
        public float IncomeTourismModifier { get; set; } = 1f;

        /// <summary>
        /// Local food yield modifier
        /// </summary>
        [Description("Local food yield modifier")]
        [DisplayName("YieldFoodModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldFoodModifier")]
        public float YieldFoodModifier { get; set; } = 1f;

        /// <summary>
        /// Local production yield modifier
        /// </summary>
        [Description("Local production yield modifier")]
        [DisplayName("YieldProductionModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldProductionModifier")]
        public float YieldProductionModifier { get; set; } = 1f;

        /// <summary>
        /// Local gold yield modifier
        /// </summary>
        [Description("Local gold yield modifier")]
        [DisplayName("YieldGoldModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldGoldModifier")]
        public float YieldGoldModifier { get; set; } = 1f;

        /// <summary>
        /// Local science yield modifier
        /// </summary>
        [Description("Local science yield modifier")]
        [DisplayName("YieldScienceModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldScienceModifier")]
        public float YieldScienceModifier { get; set; } = 1f;

        /// <summary>
        /// Local culture yield modifier
        /// </summary>
        [Description("Local culture yield modifier")]
        [DisplayName("YieldCultureModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldCultureModifier")]
        public float YieldCultureModifier { get; set; } = 1f;

        /// <summary>
        /// Local faith yield modifier
        /// </summary>
        [Description("Local faith yield modifier")]
        [DisplayName("YieldFaithModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldFaithModifier")]
        public float YieldFaithModifier { get; set; } = 1f;

        /// <summary>
        /// Local tourism yield modifier
        /// </summary>
        [Description("Local tourism yield modifier")]
        [DisplayName("YieldTourismModifier"), Browsable(true), Category("Yield Modifier")]
        [XmlElement("YieldTourismModifier")]
        public float YieldTourismModifier { get; set; } = 1f;

        public Building() { }

        /// <summary>
        /// Converts a building name to a presentable form
        /// I.e. converts "BUILDING_MY_NAME" to "My Name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
        {
            string prefix = "BUILDING_";

            // check if the string is valid
            if (!name.Contains(prefix))
                return name;

            // strip "BUILDING_"
            // "BUILDING_MY_NAME" -> "MY_NAME"
            name = name.Remove(0, prefix.Length);

            // replace all "_"
            // "MY_NAME" -> "MY NAME"
            name = name.Replace('_', ' ');

            // convert to lower case
            // "MY NAME" -> "my name"
            name = name.ToLowerInvariant();

            // convert to title case
            // "my name" -> "My Name"
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name);

            return name;
        }
    }
}
