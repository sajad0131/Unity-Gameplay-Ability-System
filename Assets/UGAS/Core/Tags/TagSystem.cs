using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGAS
{
    public class TagSystem : MonoBehaviour
    {
        private readonly HashSet<GameplayTag> activeTags = new HashSet<GameplayTag>();

        public void AddTag(GameplayTag tag)
        {
            activeTags.Add(tag);
        }

        public void AddTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                activeTags.Add(tag);
            }
        }

        public void RemoveTag(GameplayTag tag)
        {
            activeTags.Remove(tag);
        }

        public void RemoveTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                activeTags.Remove(tag);
            }
        }

        public void RemoveAllTags()
        {
            activeTags.Clear();
        }

        public bool HasTag(GameplayTag tag)
        {
            return activeTags.Contains(tag);
        }

        public bool HasTagExact(GameplayTag tag)
        {
            return activeTags.Contains(tag);
        }

        public bool HasTagHierarchical(GameplayTag tag)
        {
            if (activeTags.Contains(tag))
                return true;

            string tagPath = tag.name;
            foreach (var activeTag in activeTags)
            {
                if (activeTag.name.StartsWith(tagPath + "."))
                    return true;
            }
            return false;
        }

        public bool HasAllTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (!HasTagHierarchical(tag))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasAnyTag(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (HasTagHierarchical(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasAnyTagExact(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (activeTags.Contains(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public GameplayTag[] GetActiveTags()
        {
            return activeTags.ToArray();
        }
    }
}