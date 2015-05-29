using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Myre.Graphics.Pipeline.Animations.BVH;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assertt = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Myre.Tests.Myre.Graphics.Pipeline.Animations.BVH
{
    [TestClass]
    public class BvhParserTest
    {
        private string[] _bvh;
        private NodeContent _parsed;

        [TestInitializeAttribute]
        public void Initialize()
        {
            List<string> lines = new List<string>();
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var r = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Myre.Tests.Myre.Graphics.Pipeline.Animations.BVH.gangnam_style.bvh")))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    lines.Add(line);
            }

            _bvh = lines.ToArray();

            _parsed = new BvhParser(_bvh).Parse("gangnam_style");
        }

        [TestMethod]
        public void ParsedResultIsNotNull()
        {
            Assertt.IsNotNull(_parsed);
        }

        [TestMethod]
        public void ParsedResultContainsSkeleton()
        {
            var skeleton = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(_parsed));
            Assertt.IsNotNull(skeleton);

            var expected = new[] {
                "mixamorig:Hips", "mixamorig:Spine", "mixamorig:Spine1", "mixamorig:Spine2", "mixamorig:Neck", "mixamorig:Head", "mixamorig:RightEye", "mixamorig:LeftEye", "mixamorig:HeadTop_End", "mixamorig:LeftShoulder", "mixamorig:LeftArm", "mixamorig:LeftForeArm", "mixamorig:LeftHand", "mixamorig:LeftHandThumb1", "mixamorig:LeftHandThumb2", "mixamorig:LeftHandThumb3", "mixamorig:LeftHandIndex1", "mixamorig:LeftHandIndex2", "mixamorig:LeftHandIndex3", "mixamorig:LeftHandRing1", "mixamorig:LeftHandRing2", "mixamorig:LeftHandRing3", "mixamorig:RightShoulder", "mixamorig:RightArm", "mixamorig:RightForeArm", "mixamorig:RightHand", "mixamorig:RightHandRing1", "mixamorig:RightHandRing2", "mixamorig:RightHandRing3", "mixamorig:RightHandIndex1", "mixamorig:RightHandIndex2", "mixamorig:RightHandIndex3", "mixamorig:RightHandThumb1", "mixamorig:RightHandThumb2", "mixamorig:RightHandThumb3", "mixamorig:LeftUpLeg", "mixamorig:LeftLeg", "mixamorig:LeftFoot", "mixamorig:LeftToeBase", "mixamorig:RightUpLeg", "mixamorig:RightLeg", "mixamorig:RightFoot", "mixamorig:RightToeBase"
            };

            foreach (string name in expected)
                Assertt.IsTrue(skeleton.Any(a => a.Name == name));
        }

        [TestMethod]
        public void ParsedResultContainsCorrectNumberOfChannels()
        {
            var skeleton = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(_parsed));

            Assertt.AreEqual(skeleton.Count, _parsed.Animations.Single().Value.Channels.Count);
        }

        [TestMethod]
        public void ParsedResultContainsCorrectNumberOfKeyframes()
        {
            var channels = _parsed.Animations.Single().Value.Channels;

            foreach (var channel in channels)
                Assertt.AreEqual(372, channel.Value.Count);
        }
    }
}
