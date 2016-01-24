using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Myre.Graphics.Pipeline.Models
{
    [ContentProcessor(DisplayName = "Myre Model Processor")]
    public class MyreModelProcessor : BaseModelProcessor<NodeContent, MyreModelContent>
    {
        //Rotation XYZ based on http://blog.diabolicalgame.co.uk/2011/07/exporting-animated-models-from-blender.html
        private float _degreesX;
        [DisplayName("Rotate X"), DefaultValue(null)]
        public float DegreesX
        {
            get { return _degreesX; }
            set { _degreesX = value; }
        }

        private float _degreesY;
        [DisplayName("Rotate Y"), DefaultValue(null)]
        public float DegreesY
        {
            get { return _degreesY; }
            set { _degreesY = value; }
        }

        private float _degreesZ;
        [DisplayName("Rotate Z"), DefaultValue(null)]
        public float DegreesZ
        {
            get { return _degreesZ; }
            set { _degreesZ = value; }
        }

        private IList<BoneContent> _bones;
        private Dictionary<string, int> _boneIndices;
        private List<Vector3>[] _verticesPerBone = null;

        private string _directory;
        protected override string Directory
        {
            get { return _directory; }
        }

        protected override MyreModelContent Process(NodeContent input)
        {
            _directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            // http://blog.diabolicalgame.co.uk/2011/07/exporting-animated-models-from-blender.html
            // Before anything rotate the entire model and animations
            RotateAll(input, DegreesX, DegreesY, DegreesZ);

            AddChannelProcessor<BoneWeightCollection>(VertexChannelNames.Weights(), ProcessWeightsChannel);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);
            if (skeleton != null)
            {
                _bones = MeshHelper.FlattenSkeleton(skeleton);
                _boneIndices = _bones.Select((a, i) => new {a, i}).ToDictionary(a => a.a.Name, a => a.i);

                //Create a list of positions for each bone
                _verticesPerBone = new List<Vector3>[MeshHelper.FlattenSkeleton(skeleton).Count];
                for (int i = 0; i < _verticesPerBone.Length; i++)
                    _verticesPerBone[i] = new List<Vector3>();
            }

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            ModelHelpers.FlattenTransforms(input, skeleton);

            var outputModel = new MyreModelContent();

            //Process meshes
            List<MeshContent> meshes = new List<MeshContent>();
            ModelHelpers.FindMeshes(input, meshes);

            var bi = _boneIndices == null ? null : new Dictionary<string, int>(_boneIndices);
            foreach (var mesh in meshes)
                foreach (var geom in ProcessMesh(mesh, _verticesPerBone, bi))
                    outputModel.AddMesh(geom);

            //Extract skeleton data from the input
            outputModel.SkinningData = ProcessSkinningData(skeleton, _bones, _verticesPerBone);

            return outputModel;
        }

        // Rotate all the content before anything else
        // see http://forums.xna.com/forums/p/60188/370817.aspx#370817
        // http://forums.create.msdn.com/forums/p/60188/370817.aspx
        // http://forums.create.msdn.com/forums/p/64690/395491.aspx#395491
        /*
         * Shawn Hargreaves
         * If you look at the skinned model processor, you will see that it pulls out animation data from the model into its own keyframe data structures,
         * then chains to the base ModelProcessor which converts the model itself from NodeContent into ModelContent format.
         * This base ModelProcessor call applies whatever rotation has been specified via these processor parameters,
         * but this happens AFTER the keyframe data was extracted, so the keyframe values are not rotated.
         * There are several ways you could fix this:
         * Manually apply the necessary rotation to each keyframe matrix
         * Or, instead of using ModelProcessor to apply the rotation, do this yourself at the very start of your Process method (before you call ModelProcessor
         * and before any of the keyframe extraction). The easiest way to do that is to call MeshHelper.TransformScene. 
         * */
        // This only works if the animation keyframes are also rotated
        // As my animations are separate the source model would need to be rotated first.
        public static void RotateAll(NodeContent node, float degX, float degY, float degZ)
        {
            var rotate = Matrix.Identity *
                Matrix.CreateRotationX(MathHelper.ToRadians(degX)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(degY)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(degZ));

            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.content.pipeline.graphics.meshhelper.transformscene.aspx
            MeshHelper.TransformScene(node, rotate);
        }


        /// <summary>
        /// Replace this vertex channel (BoneWeightCollection) with weight and index channels
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="channel"></param>
        private void ProcessWeightsChannel(GeometryContent geometry, VertexChannel<BoneWeightCollection> channel)
        {
            bool boneCollectionsWithZeroWeights = false;

            // and indices as packed 4byte vectors.
            Vector4[] weightsToAdd = new Vector4[channel.Count];
            Vector4[] indicesToAdd = new Vector4[channel.Count];

            // Go through the BoneWeightCollections and create a new
            // weightsToAdd and indicesToAdd array for each BoneWeightCollection.
            for (int i = 0; i < channel.Count; i++)
            {
                BoneWeightCollection bwc = channel[i];

                if (bwc.Count == 0)
                {
                    boneCollectionsWithZeroWeights = true;
                    continue;
                }

                int count = bwc.Count;
                bwc.NormalizeWeights(4);

                // Add the appropriate bone indices based on the bone names in the
                // BoneWeightCollection
                Vector4 bi = new Vector4(
                    count > 0 ? _boneIndices[bwc[0].BoneName] : 0,
                    count > 1 ? _boneIndices[bwc[1].BoneName] : 0,
                    count > 2 ? _boneIndices[bwc[2].BoneName] : 0,
                    count > 3 ? _boneIndices[bwc[3].BoneName] : 0
                );
                indicesToAdd[i] = bi;

                Vector4 bw = new Vector4
                {
                    X = count > 0 ? bwc[0].Weight : 0,
                    Y = count > 1 ? bwc[1].Weight : 0,
                    Z = count > 2 ? bwc[2].Weight : 0,
                    W = count > 3 ? bwc[3].Weight : 0
                };
                weightsToAdd[i] = bw;
            }

            // Remove the old BoneWeightCollection channel
            geometry.Vertices.Channels.Remove(channel);
            // Add the new channels
            geometry.Vertices.Channels.Add<Vector4>(VertexElementUsage.BlendIndices.ToString(), indicesToAdd);
            geometry.Vertices.Channels.Add<Vector4>(VertexElementUsage.BlendWeight.ToString(), weightsToAdd);

            if (boneCollectionsWithZeroWeights)
                Context.Logger.LogWarning("", geometry.Identity, "BonesWeightCollections with zero weights found in geometry.");
        }
    }
}