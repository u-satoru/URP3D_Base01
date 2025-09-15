using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Strategy.Resources
{
    /// <summary>
    /// Represents the cost of an action in terms of resources
    /// Used for building construction, unit training, technology research, etc.
    /// </summary>
    [Serializable]
    public class ResourceCost
    {
        [Header("Resource Cost")]
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int amount;

        public ResourceType ResourceType => resourceType;
        public int Amount => amount;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceCost()
        {
            resourceType = ResourceType.Wood;
            amount = 0;
        }

        /// <summary>
        /// Constructor with resource type and amount
        /// </summary>
        public ResourceCost(ResourceType type, int cost)
        {
            resourceType = type;
            amount = Mathf.Max(0, cost);
        }

        /// <summary>
        /// Check if this cost is valid (amount > 0)
        /// </summary>
        public bool IsValid => amount > 0;

        /// <summary>
        /// Create a copy of this resource cost
        /// </summary>
        public ResourceCost Copy()
        {
            return new ResourceCost(resourceType, amount);
        }

        /// <summary>
        /// Convert to dictionary entry
        /// </summary>
        public KeyValuePair<ResourceType, int> ToDictionaryEntry()
        {
            return new KeyValuePair<ResourceType, int>(resourceType, amount);
        }

        /// <summary>
        /// Create resource cost from dictionary entry
        /// </summary>
        public static ResourceCost FromDictionaryEntry(KeyValuePair<ResourceType, int> entry)
        {
            return new ResourceCost(entry.Key, entry.Value);
        }

        /// <summary>
        /// Convert array of resource costs to dictionary
        /// </summary>
        public static Dictionary<ResourceType, int> ToDictionary(ResourceCost[] costs)
        {
            if (costs == null || costs.Length == 0)
                return new Dictionary<ResourceType, int>();

            return costs
                .Where(cost => cost != null && cost.IsValid)
                .ToDictionary(cost => cost.ResourceType, cost => cost.Amount);
        }

        /// <summary>
        /// Convert dictionary to array of resource costs
        /// </summary>
        public static ResourceCost[] FromDictionary(Dictionary<ResourceType, int> costs)
        {
            if (costs == null || costs.Count == 0)
                return new ResourceCost[0];

            return costs
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => new ResourceCost(kvp.Key, kvp.Value))
                .ToArray();
        }

        /// <summary>
        /// Multiply this cost by a factor
        /// </summary>
        public ResourceCost Multiply(float multiplier)
        {
            return new ResourceCost(resourceType, Mathf.RoundToInt(amount * multiplier));
        }

        /// <summary>
        /// Add two resource costs together (same type only)
        /// </summary>
        public static ResourceCost operator +(ResourceCost a, ResourceCost b)
        {
            if (a == null) return b?.Copy();
            if (b == null) return a.Copy();
            
            if (a.ResourceType != b.ResourceType)
            {
                Debug.LogWarning($"Cannot add different resource types: {a.ResourceType} + {b.ResourceType}");
                return a.Copy();
            }
            
            return new ResourceCost(a.ResourceType, a.Amount + b.Amount);
        }

        /// <summary>
        /// Get display string for UI
        /// </summary>
        public string GetDisplayString()
        {
            return $"{amount} {resourceType}";
        }

        /// <summary>
        /// Get short display string for UI (abbreviated)
        /// </summary>
        public string GetShortDisplayString()
        {
            string suffix = resourceType switch
            {
                ResourceType.Wood => "W",
                ResourceType.Stone => "S",
                ResourceType.Food => "F",
                ResourceType.Gold => "G",
                ResourceType.Metal => "M",
                ResourceType.Population => "P",
                ResourceType.Energy => "E",
                _ => resourceType.ToString()[0].ToString()
            };
            
            return $"{amount}{suffix}";
        }

        public override string ToString()
        {
            return GetDisplayString();
        }

        public override bool Equals(object obj)
        {
            if (obj is ResourceCost other)
            {
                return resourceType == other.resourceType && amount == other.amount;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(resourceType, amount);
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only validation
        /// </summary>
        public void OnValidate()
        {
            amount = Mathf.Max(0, amount);
        }
        #endif
    }

    /// <summary>
    /// Helper class for working with multiple resource costs
    /// </summary>
    [Serializable]
    public class ResourceCostCollection
    {
        [SerializeField] private ResourceCost[] costs = new ResourceCost[0];

        public ResourceCost[] Costs => costs;
        public int Count => costs?.Length ?? 0;
        public bool IsEmpty => Count == 0 || costs.All(c => c == null || !c.IsValid);

        public ResourceCostCollection()
        {
            costs = new ResourceCost[0];
        }

        public ResourceCostCollection(params ResourceCost[] resourceCosts)
        {
            costs = resourceCosts?.Where(c => c != null && c.IsValid).ToArray() ?? new ResourceCost[0];
        }

        /// <summary>
        /// Convert to dictionary for resource manager operations
        /// </summary>
        public Dictionary<ResourceType, int> ToDictionary()
        {
            return ResourceCost.ToDictionary(costs);
        }

        /// <summary>
        /// Get total cost for a specific resource type
        /// </summary>
        public int GetCost(ResourceType resourceType)
        {
            return costs?.Where(c => c != null && c.ResourceType == resourceType).Sum(c => c.Amount) ?? 0;
        }

        /// <summary>
        /// Check if collection contains cost for specific resource type
        /// </summary>
        public bool HasCost(ResourceType resourceType)
        {
            return GetCost(resourceType) > 0;
        }

        /// <summary>
        /// Multiply all costs by a factor
        /// </summary>
        public ResourceCostCollection Multiply(float multiplier)
        {
            return new ResourceCostCollection(costs?.Select(c => c?.Multiply(multiplier)).Where(c => c != null).ToArray());
        }

        /// <summary>
        /// Get display string for all costs
        /// </summary>
        public string GetDisplayString(string separator = ", ")
        {
            if (IsEmpty) return "Free";
            
            return string.Join(separator, costs.Where(c => c != null && c.IsValid).Select(c => c.GetDisplayString()));
        }

        /// <summary>
        /// Get short display string for all costs
        /// </summary>
        public string GetShortDisplayString(string separator = " ")
        {
            if (IsEmpty) return "Free";
            
            return string.Join(separator, costs.Where(c => c != null && c.IsValid).Select(c => c.GetShortDisplayString()));
        }
    }
}