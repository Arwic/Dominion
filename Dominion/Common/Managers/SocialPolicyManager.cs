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

        public SocialPolicyDataPack() { }
    }

    [Serializable]
    public class SocialPolicyManager
    {
        private Dictionary<string, SocialPolicy> data = new Dictionary<string, SocialPolicy>();

        private HashSet<string> policyTrees = new HashSet<string>();

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
                policyTrees.Add(p.TreeID);
                data.Add(p.ID, p);
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
            if (data == null)
                data = new Dictionary<string, SocialPolicy>();
            else
                data.Clear();

            if (policyTrees == null)
                policyTrees = new HashSet<string>();
            else
                policyTrees.Clear();

            // fill it with technologies from all the data packs
            foreach (SocialPolicyDataPack dp in dataPacks)
            {
                foreach (SocialPolicy p in dp.SocialPolicies)
                {
                    p.Name = SocialPolicy.FormatName(p.ID);
                    policyTrees.Add(p.TreeID);
                    data.Add(p.ID, p);
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
            foreach (KeyValuePair<string, SocialPolicy> kvp in data)
            {
                allPolicies.Add(kvp.Value);
            }
            return allPolicies;
        }

        /// <summary>
        /// Gets a collection of all the social policy trees in the social policy manager
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetAllSocialPolicyTrees()
        {
            return policyTrees;
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
            if (!data.TryGetValue(id, out policy))
                return null;
            return policy;
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
    }
}
