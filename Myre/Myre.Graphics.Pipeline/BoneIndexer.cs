using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Myre.Graphics.Pipeline
{
    /// <summary>
    /// Assists in creating palette indices for mesh bones.
    /// </summary>
    public sealed class BoneIndexer
    {
        private int _currentIndex = 0;
        private readonly Dictionary<string, int> _dict = new Dictionary<string, int>();
        private readonly List<string> _skinnedBoneNames = new List<string>();

        /// <summary>
        /// The names of the skinned bones that have indices attached to this indexer.
        /// </summary>
        public ReadOnlyCollection<string> SkinnedBoneNames
        {
            get { return _skinnedBoneNames.AsReadOnly(); }
        }

        /// <summary>
        /// True if an index has been created for the given bone.
        /// </summary>
        /// <param name="boneName">The name of the bone.</param>
        /// <returns>True if an index has been created for the given bone.</returns>
        public bool ContainsBone(string boneName)
        {
            return _dict.ContainsKey(boneName);
        }

        /// <summary>
        /// Creates an index for a bone if one doesn't exist, and returns the palette
        /// index for the given bone.
        /// </summary>
        /// <param name="boneName">The name of the bone.</param>
        /// <returns>The matrix palette index of the bone.</returns>
        public byte GetBoneIndex(string boneName)
        {
            if (!_dict.ContainsKey(boneName))
            {
                _dict.Add(boneName, _currentIndex);
                _skinnedBoneNames.Add(boneName);
                _currentIndex++;
                return (byte)(_currentIndex - 1);
            }
            else
                return (byte)_dict[boneName];
        }

        public bool TryGetValue(string key, out int boneIndex)
        {
            return _dict.TryGetValue(key, out boneIndex);
        }
    }
}
