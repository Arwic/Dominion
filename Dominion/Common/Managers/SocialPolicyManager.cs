// Dominion - Copyright (C) Timothy Ings
// SocialPolicyManager.cs
// This file defines classes that define the social policy manager

using ArwicEngine.Core;
using ArwicEngine.TypeConverters;
using Dominion.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Xml.Serialization;

namespace Dominion.Common.Managers
{
    [Serializable]
    public class SocialPolicyInstance
    {
        private Dictionary<string, SocialPolicy> data = new Dictionary<string, SocialPolicy>();

        public SocialPolicyInstance(SocialPolicyManager manager)
        {
            List<SocialPolicy> policies = (List<SocialPolicy>)manager.GetAllSocialPolicies();
            foreach (SocialPolicy policy in policies)
            {
                policy.Unlocked = false;
                data.Add(policy.ID, policy);
            }
        }

        /// <summary>
        /// Gets a collection of all the social policies in the social policy instance
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
        /// Gets a collection of all the social policies in the social policy instance that belong to the given tree
        /// </summary>
        /// <returns></returns>
        public ICollection<SocialPolicy> GetAllSocialPoliciesInTree(string treeID)
        {
            List<SocialPolicy> allPolicies = new List<SocialPolicy>();
            foreach (KeyValuePair<string, SocialPolicy> kvp in data)
            {
                if (kvp.Value.TreeID == treeID)
                {
                    allPolicies.Add(kvp.Value);
                }
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

        [Editor(typeof(SocialPolicy.Editor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("SocialPolicies"), XmlArrayItem(typeof(SocialPolicy), ElementName = "SocialPolicy")]
        public List<SocialPolicy> SocialPolicies { get; set; } = new List<SocialPolicy>();

        [Editor(typeof(SocialPolicyTree.Editor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ListConverter))]
        [XmlArray("SocialPolicyTrees"), XmlArrayItem(typeof(SocialPolicyTree), ElementName = "SocialPolicyTrees")]
        public List<SocialPolicyTree> SocialPolicyTrees { get; set; } = new List<SocialPolicyTree>();

        public SocialPolicyDataPack() { }
    }

    [Serializable]
    public class SocialPolicyManager
    {
        private Dictionary<string, SocialPolicy> policyData = new Dictionary<string, SocialPolicy>();
        private Dictionary<string, SocialPolicyTree> policyTreeData = new Dictionary<string, SocialPolicyTree>();

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

            foreach (SocialPolicy p in pack.SocialPolicies)
            {
                p.Name = SocialPolicy.FormatName(p.ID);
                policyData.Add(p.ID, p);
            }

            foreach (SocialPolicyTree t in pack.SocialPolicyTrees)
            {
                t.Name = SocialPolicyTree.FormatName(t.ID);
                policyTreeData.Add(t.ID, t);
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

            // fill it with technologies from all the data packs
            foreach (SocialPolicyDataPack dp in dataPacks)
            {
                foreach (SocialPolicy p in dp.SocialPolicies)
                {
                    p.Name = SocialPolicy.FormatName(p.ID);
                    policyData.Add(p.ID, p);
                }

                foreach (SocialPolicyTree t in dp.SocialPolicyTrees)
                {
                    t.Name = SocialPolicyTree.FormatName(t.ID);
                    policyTreeData.Add(t.ID, t);
                }
            }
        }

        /// <summary>
        /// Returns a new social policy instance
        /// </summary>
        /// <returns></returns>
        public SocialPolicyInstance GetNewInstance()
        {
            SocialPolicyInstance inst = new SocialPolicyInstance(this);
            return inst;
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
        /// Gets a collection of all the social policy trees in the social policy manager
        /// </summary>
        /// <returns></returns>
        public ICollection<SocialPolicyTree> GetAllSocialPolicyTrees()
        {
            List<SocialPolicyTree> allPolicyTrees = new List<SocialPolicyTree>();
            foreach (KeyValuePair<string, SocialPolicyTree> kvp in policyTreeData)
            {
                allPolicyTrees.Add(kvp.Value);
            }
            return allPolicyTrees;
        }

        /// <summary>
        /// Gets a collection of all the social policies in the social policy manager that belong
        /// </summary>
        /// <returns></returns>
        public ICollection<SocialPolicy> GetAllSocialPoliciesInTree(string treeID)
        {
            List<SocialPolicy> policies = new List<SocialPolicy>();

            foreach (SocialPolicy policy in GetAllSocialPolicies())
            {
                if (policy.TreeID == treeID)
                {
                    policies.Add(policy);
                }
            }
            return policies;
        }

        /// <summary>
        /// Gets the social policy with the given name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SocialPolicy GetSocialPolicy(string id)
        {
            SocialPolicy policy;
            if (!policyData.TryGetValue(id, out policy))
                return null;
            return policy;
        }

        /// <summary>
        /// Gets the social policy tree with the given name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SocialPolicyTree GetSocialPolicyTree(string id)
        {
            SocialPolicyTree policyTree;
            if (!policyTreeData.TryGetValue(id, out policyTree))
                return null;
            return policyTree;
        }

        /// <summary>
        /// Determines if a social policy with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SocialPolicyExists(string name)
        {
            return GetSocialPolicy(name) != null;
        }

        /// <summary>
        /// Determines if a social policy with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SocialPolicyTreeExists(string name)
        {
            return GetSocialPolicyTree(name) != null;
        }
    }
}
