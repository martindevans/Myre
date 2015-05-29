using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Myre.Graphics.Pipeline.Animations.BVH
{
    public class BvhParser
    {
        private readonly IEnumerable<string> _enumerable; 
        private IEnumerator<string> _enumerator;

        public BvhParser(IEnumerable<string> lines)
        {
            _enumerable = lines;
        }

        private readonly List<ChannelReader> _channelReaders = new List<ChannelReader>(); 

        public NodeContent Parse(string animationName)
        {
            _channelReaders.Clear();
            _enumerator = _enumerable.GetEnumerator();

            int channels;
            var root = ParseHierarchy(out channels);
            root.Animations.Add(animationName, ParseMotion(channels, animationName));

            return root;
        }

        private AnimationContent ParseMotion(int channels, string animationName)
        {
            Match("MOTION");

            var frames = int.Parse(Match(@"Frames: (?<count>[0-9]+)").Groups["count"].Value);
            var frameTime = float.Parse(Match(@"Frame Time: (?<value>\d*\.?\d*)").Groups["value"].Value);

            //After this point, every line is just a big list of numbers
            for (int i = 0; i < frames; i++)
            {
                _enumerator.MoveNext();
                float[] values = _enumerator.Current.Split(new []  { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();

                int offset = 0;
                foreach (var reader in _channelReaders)
                    offset += reader.Read(TimeSpan.FromSeconds(frameTime * i), values, offset);

                if (offset != values.Length)
                    throw new InvalidContentException(string.Format("Incorrect number of channels, expected {0} found {1}", offset, values.Length));
            }

            var motion = new AnimationContent {
                Duration = TimeSpan.FromSeconds(frameTime * frames),
                Name = animationName
            };
            foreach (var channelReader in _channelReaders)
                motion.Channels.Add(channelReader.Name, channelReader.Channel);

            return motion;
        }

        private NodeContent ParseHierarchy(out int channels)
        {
            Match("HIERARCHY");

            var root = ParseHierarchyNode();

            channels = MeshHelper.FlattenSkeleton(root).Count;

            return root;
        }

        private BoneContent ParseHierarchyNode()
        {
            var decl = Match(@"((?<type>ROOT|JOINT)\s(?<name>.+)$)|(?<type>End Site)", false);
            if (!decl.Success)
                return null;

            var type = decl.Groups["type"].Value;
            if (type == "End Site")
            {
                return ParseEndSite();
            }
            else
                return ParseJoint(decl.Groups["name"].Value);
        }

        private BoneContent ParseJoint(string name)
        {
            var b = new BoneContent { Name = name };

            Match("{$");

            float x, y, z;
            ParseOffset(out x, out y, out z);
            b.Transform = Matrix.CreateTranslation(x, y, z);

            var channels = Match(@"CHANNELS\s(?<count>\d+)\s(?<channelNames>.*)$");
            var count = int.Parse(channels.Groups["count"].Value);
            var channelNames = channels.Groups["channelNames"].Value.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            _channelReaders.Add(new ChannelReader(b, channelNames));

            while (true)
            {
                var child = ParseHierarchyNode();
                if (child == null)
                    break;
                b.Children.Add(child);
            }

            Match("}$");

            return b;
        }

        private BoneContent ParseEndSite()
        {
            Match("{$");

            float x, y, z;
            ParseOffset(out x, out y, out z);

            Match("}$");

            return null;
            //var b = new BoneContent { Name = Guid.NewGuid().ToString() };
            //b.Transform = Matrix.CreateTranslation(x, y, z);
            //return b;
        }

        private void ParseOffset(out float x, out float y, out float z)
        {
            var offsets = Match(@"OFFSET\s(?<x>-?\d*\.?\d*)\s(?<y>-?\d*\.?\d*)\s(?<z>-?\d*\.?\d*)");
            x = float.Parse(offsets.Groups["x"].Value);
            y = float.Parse(offsets.Groups["y"].Value);
            z = float.Parse(offsets.Groups["z"].Value);
        }

        private Match Match(string hierarchy, bool throwException = true)
        {
            return Match(new Regex(hierarchy), throwException);
        }

        private bool _lastMatchWasSuccessful = true;
        private Match Match(Regex match, bool throwException = true)
        {
            if (_lastMatchWasSuccessful)
                _enumerator.MoveNext();

            var m = match.Match(_enumerator.Current);
            _lastMatchWasSuccessful = m.Success;

            if (!_lastMatchWasSuccessful && throwException)
                throw new InvalidContentException(string.Format("Failed to parse BVH, expected {0}, got {1}", match, _enumerator.Current));

            return m;
        }

        private class ChannelReader
        {
            private readonly BoneContent _bone;
            public string Name
            {
                get
                {
                    return _bone.Name;
                }
            }

            private readonly List<string> _channels;
            private readonly float[] _values;

            public AnimationChannel Channel { get; private set; }

            public ChannelReader(BoneContent bone, string[] channels)
            {
                _bone = bone;
                _channels = channels.ToList();
                _values = new float[channels.Length];

                Channel = new AnimationChannel();
            }

            public int Read(TimeSpan timestamp, IList<float> values, int offset)
            {
                //Copy the values
                for (int i = 0; i < _channels.Count; i++)
                    _values[i] = values[i + offset];

                //Turn these values into a transform
                Matrix transform = ReadTransform(_values);

                Channel.Add(new AnimationKeyframe(timestamp, transform));

                return _values.Length;
            }

            private Matrix ReadTransform(IList<float> values)
            {
                var rot = Quaternion.Identity;
                var loc = Vector3.Zero;
                for (int i = 0; i < _channels.Count; i++)
                {
                    switch (_channels[i].ToLowerInvariant())
                    {
                        case "xrotation":
                            rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.ToRadians(values[i]));
                            break;
                        case "yrotation":
                            rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(values[i]));
                            break;
                        case "zrotation":
                            rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(values[i]));
                            break;
                        case "xposition":
                            loc.X += values[i];
                            break;
                        case "yposition":
                            loc.Y += values[i];
                            break;
                        case "zposition":
                            loc.Z += values[i];
                            break;
                    }
                }

                var parent = Matrix.Identity;//_bone.Parent == null ? Matrix.Identity : _bone.Parent.Transform;
                var translate = loc.Length() < float.Epsilon ? parent : Matrix.CreateTranslation(loc);

                return Matrix.CreateFromQuaternion(Quaternion.Normalize(rot)) * translate;
            }
        }
    }
}
