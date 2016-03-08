// Dominion - Copyright (C) Timothy Ings
// SocialPolicyManager.cs
// This file defines classes that define the social policy manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class SocialPolicyCollectionInstance
    {
        private Dictionary<string, SocialPolicy> data = new Dictionary<string, SocialPolicy>();

        public SocialPolicyCollectionInstance(SocialPolicyManager manager)
        {
            List<SocialPolicy> policies = (List<SocialPolicy>)manager.GetAllSocialPolicies();
            foreach (SocialPolicy policy in policies)
            {
                policy.Unlocked = false;
                data.Add(policy.ID, policy);
            }
        }

        /// <summary>
        /// Gets a collection of all the social policies in the social policy manager
        /// </summary>
        /// <returns></returns>
        public ICollection<SocialPolicy> GetAllSocialPolicies()
        {
            List<SocialPolicy> allPolicies = new List<SocialPolicy>();
            foreach (KeyValuePair<string, SocialPolicy> kvp in data)
            {
                allPolicies.Add(kvp.Value);
            }
            return allPolicies;
        }

        /// <summary>
        /// Gets the building with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SocialPolicy GetSocialPolicy(string name)
        {
            SocialPolicy policy;
            bool res = data.TryGetValue(name, out policy);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"SocialPolicy '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return policy;
        }

        /// <summary>
        /// Determines if a social policy with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SocialPolicyExists(string name)
        {
            return data.ContainsKey(name);
        }
    }

    [Serializable]
    public class SocialPolicyDataPack
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "New Social Policy Data Pack";

        [TypeConverter(typeof(ListConverter))]
        [XmlArray("SocialPolicies"), XmlArrayItem(typeof(SocialPolicy), ElementName = "SocialPolicy")]
        public List<SocialPolicy> SocialPolicies { get; set; } = new List<SocialPolicy>();

        [TypeConverter(typeof(ListConverter))]
        [XmlArray("SocialPolicyTrees"), XmlArrayItem(typeof(SocialPolicyTree), ElementName = "SocialPolicyTree")]
        public List<SocialPolicyTree> SocialPolicyTrees { get; set; } = new List<SocialPolicyTree>();

        public SocialPolicyDataPack() { }
    }

    [Serializable]
    public class SocialPolicyManager
    {
        public int SocialPolicyCount => policyData.Count;

        private Dictionary<string, SocialPolicyTree> treeData = new Dictionary<string, SocialPolicyTree>();
        private Dictionary<string, SocialPolicy> policyData = new Dictionary<string, SocialPolicy>();

        private List<SocialPolicyDataPack> dataPacks = new List<SocialPolicyDataPack>();

        public SocialPolicyManager() { }
        
        /// <summary>
        /// Adds a data pack to the manager
        /// </summary>
        /// <param name="stream"></param>
        public void AddDataPack(Stream stream)
        {
            if (stream == null)
            {
                ConsoleManager.Instance.WriteLine("Missing social policy data", MsgType.ServerWarning);
                return;
            }
            // load data pack
            SocialPolicyDataPack pack = SerializationHelper.XmlDeserialize<SocialPolicyDataPack>(stream);
            dataPacks.Add(pack);

            foreach (SocialPolicyTree p in pack.SocialPolicyTrees)
            {
                p.Name = SocialPolicyTree.FormatName(p.ID);
                treeData.Add(p.ID, p);
            }

            foreach (SocialPolicy p in pack.SocialPolicies)
            {
                p.Name = SocialPolicy.FormatName(p.ID);
                policyData.Add(p.ID, p);
            }
        }

        /// <summary>
        /// Removes all data packs with the given name from the manager
        /// </summary>
        /// <param name="name"></param>
        public void RemoveDataPack(string name)
        {
            dataPacks.RemoveAll(p => p.Name == name);
            ConstructData();
        }

        // clears data dictionary and recreates it from the list of data packs
        private void ConstructData()
        {
            // clear the data dictionary
            if (policyData == null)
                policyData = new Dictionary<string, SocialPolicy>();
            else
                policyData.Clear();
            if (treeData == null)
                treeData = new Dictionary<string, SocialPolicyTree>();
            else
                treeData.Clear();

            // fill it with technologies from all the data packs
            foreach (SocialPolicyDataPack dp in dataPacks)
            {
                foreach (SocialPolicyTree p in dp.SocialPolicyTrees)
                {
                    p.Name = SocialPolicyTree.FormatName(p.ID);
                    treeData.Add(p.ID, p);
                }

                foreach (SocialPolicy p in dp.SocialPolicies)
                {
                    p.Name = SocialPolicy.FormatName(p.ID);
                    policyData.Add(p.ID, p);
                }
            }
        }

        /// <summary>
        /// Returns a new social policy instance
        /// </summary>
        /// <returns></returns>
        public SocialPolicyCollectionInstance GetNewInstance()
        {
            SocialPolicyCollectionInstance spCollection = new SocialPolicyCollectionInstance(this);
            return spCollection;
        }

        /// <summary>
        /// Gets a collection of all the social policies in the social policy manager
        /// </summary>
        /// <returns></returns>
        public ICollection<SocialPolicy> GetAllSocialPolicies()
        {
            List<SocialPolicy> allPolicies = new List<SocialPolicy>();
            foreach (KeyValuePair<string, SocialPolicy> kvp in policyData)
            {
                allPolicies.Add(kvp.Value);
            }
            return allPolicies;
        }

        /// <summary>
        /// Gets the social policy with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SocialPolicy GetSocialPolicy(string name)
        {
            SocialPolicy sp;
            bool res = policyData.TryGetValue(name, out sp);
            if (!res)
            {
                ConsoleManager.Instance.WriteLine($"SocialPolicy '{name}' does not exist", MsgType.Failed);
                return null;
            }
            return sp;
        }

        /// <summary>
        /// Determines if a social policy with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SocialPolicyExists(string name)
        {
            return policyData.ContainsKey(name);
        }
    }
}
