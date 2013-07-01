using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Pipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    /// </summary>
    [ContentProcessor(DisplayName = "Myre Model Processor")]
    public class MyreModelProcessor : ContentProcessor<NodeContent, MyreModelContent>
    {
        ContentProcessorContext _context;
        MyreModelContent _outputModel;
        string _directory;

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        readonly Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>> _processedMaterials =
                            new Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>>();

        [DisplayName("Diffuse Texture")]
        public string DiffuseTexture { get; set; }

        [DisplayName("Specular Texture")]
        public string SpecularTexture { get; set; }

        [DisplayName("Normal Texture")]
        public string NormalTexture { get; set; }

        private bool _allowNullDiffuseTexture = true;
        [DisplayName("Allow null diffuse textures")]
        [DefaultValue(true)]
        public bool AllowNullDiffuseTexture
        {
            get { return _allowNullDiffuseTexture; }
            set { _allowNullDiffuseTexture = value; }
        }

        private string _gbufferEffectName = "DefaultGBuffer.fx";
        [DisplayName("GBuffer Effect")]
        [DefaultValue("DefaultGBuffer.fx")]
        public string GBufferEffectName
        {
            get { return _gbufferEffectName; }
            set { _gbufferEffectName = value; }
        }

        private string _shadowEffectName = "DefaultShadows.fx";
        [DisplayName("GBuffer Shadow Effect")]
        [DefaultValue("DefaultShadows.fx")]
        public string ShadowEffectName
        {
            get { return _shadowEffectName; }
            set { _shadowEffectName = value; }
        }

        private string _gbufferTechnique = null;
        [DisplayName("GBuffer Technique")]
        [DefaultValue(null)]
        public string GBufferTechnique
        {
            get { return _gbufferTechnique; }
            set { _gbufferTechnique = value; }
        }

        private IList<BoneContent> _bones;
        /// <summary>
        /// Converts incoming graphics data into our custom model format.
        /// </summary>
        public override MyreModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            _context = context;

            _directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            _outputModel = new MyreModelContent();

            //Extract skeleton data from the input
            ProcessSkinningData(input, skeleton, context);

            //Process meshes
            List<MeshContent> meshes = new List<MeshContent>();
            FindMeshes(input, meshes);
            foreach (var mesh in meshes)
                ProcessMesh(mesh, context);

            return _outputModel;
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }

        #region animation processing
        private void ProcessSkinningData(NodeContent node, BoneContent skeleton, ContentProcessorContext context)
        {
            if (skeleton == null)
            {
                _outputModel.SkinningData = null;
                return;
            }

            _bones = MeshHelper.FlattenSkeleton(skeleton);

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in _bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(_bones.IndexOf(bone.Parent as BoneContent));
            }

            _outputModel.SkinningData = new MyreSkinningDataContent(
                bindPose,
                inverseBindPose,
                skeletonHierarchy
            );
        }
        #endregion

        #region geometry processing
        private void ProcessMesh(MeshContent mesh, ContentProcessorContext context)
        {
            MeshHelper.OptimizeForCache(mesh);

            // create texture coordinates of 0 if none are present
            var texCoord0 = VertexChannelNames.TextureCoordinate(0);
            foreach (var item in mesh.Geometry)
            {
                if (!item.Vertices.Channels.Contains(texCoord0))
                    item.Vertices.Channels.Add<Vector2>(texCoord0, null);
            }

            // calculate tangent frames for normal mapping
            var hasTangents = GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Tangent(0));
            var hasBinormals = GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Binormal(0));
            if (!hasTangents || !hasBinormals)
            {
                var tangentName = hasTangents ? null : VertexChannelNames.Tangent(0);
                var binormalName = hasBinormals ? null : VertexChannelNames.Binormal(0);
                MeshHelper.CalculateTangentFrames(mesh, VertexChannelNames.TextureCoordinate(0), tangentName, binormalName);
            }

            //var outputMesh = new MyreMeshContent();
            //outputMesh.Parent = mesh.Parent;
            //outputMesh.BoundingSphere = BoundingSphere.CreateFromPoints(mesh.Positions);

            // Process all the geometry in the mesh.
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                ProcessGeometry(geometry, _outputModel, context);
            }

            //outputModel.AddMesh(outputMesh);
        }

        /// <summary>
        /// Converts a single piece of input geometry into our custom format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry, MyreModelContent model, ContentProcessorContext context)
        {
            var channels = geometry.Vertices.Channels.ToArray();
            foreach (var channel in channels)
                ProcessChannel(geometry, channel, context);

            int triangleCount = geometry.Indices.Count / 3;
            int vertexCount = geometry.Vertices.VertexCount;

            // Flatten the flexible input vertex channel data into
            // a simple GPU style vertex buffer byte array.
            var vertexBufferContent = geometry.Vertices.CreateVertexBuffer();

            // Convert the input material.
            var materials = ProcessMaterial(geometry.Material, geometry.Parent);

            var boundingSphere = BoundingSphere.CreateFromPoints(geometry.Vertices.Positions);
            
            // Add the new piece of geometry to our output model.
            model.AddMesh(new MyreMeshContent
            {
                //Parent = geometry.Parent,
                Name = geometry.Parent.Name,
                BoundingSphere = boundingSphere,
                Materials = materials,
                IndexBuffer = geometry.Indices,
                VertexBuffer = vertexBufferContent,
                VertexCount = vertexCount,
                TriangleCount = triangleCount,
            });
        }
        #endregion

        #region vertex channel processing
        private void ProcessChannel(GeometryContent geometry, VertexChannel channel, ContentProcessorContext context)
        {
            if (channel.Name == VertexChannelNames.Weights())
                ProcessWeightsChannel(geometry, (VertexChannel<BoneWeightCollection>)channel, context);
        }

        /// <summary>
        /// Replace this vertex channel (BoneWeightCollection) with weight and index channels
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="channel"></param>
        /// <param name="context"></param>
        private void ProcessWeightsChannel(GeometryContent geometry, VertexChannel<BoneWeightCollection> channel, ContentProcessorContext context)
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

                bwc.NormalizeWeights(4);
                int count = bwc.Count;

                // Add the appropriate bone indices based on the bone names in the
                // BoneWeightCollection
                Vector4 bi = new Vector4(
                    count > 0 ? FindBoneIndex(bwc[0].BoneName, _bones) : 0,
                    count > 1 ? FindBoneIndex(bwc[1].BoneName, _bones) : 0,
                    count > 2 ? FindBoneIndex(bwc[2].BoneName, _bones) : 0,
                    count > 3 ? FindBoneIndex(bwc[3].BoneName, _bones) : 0
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
                context.Logger.LogWarning("", geometry.Identity, "BonesWeightCollections with zero weights found in geometry.");
        }
        #endregion

        #region material processing
        /// <summary>
        /// Creates default materials suitable for rendering in the myre deferred renderer.
        /// The current material is searched for diffuse, normal and specular textures.
        /// </summary>
        Dictionary<string, MyreMaterialContent> ProcessMaterial(MaterialContent material, MeshContent mesh)
        {
            //material = context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor");
            if (material == null)
                material = new MaterialContent();

            // Have we already processed this material?
            if (!_processedMaterials.ContainsKey(material))
            {
                // If not, process it now.
                _processedMaterials[material] = new Dictionary<string, MyreMaterialContent>();

                bool animatedMaterials = MeshHelper.FindSkeleton(mesh) != null;
                CreateGBufferMaterial(material, mesh, animatedMaterials);
                CreateShadowMaterial(material, animatedMaterials);
            }

            return _processedMaterials[material];
        }

        private void CreateShadowMaterial(MaterialContent material, bool animated)
        {
            var materialData = new MyreMaterialData { EffectName = Path.GetFullPath(ShadowEffectName), Technique = animated ? "AnimatedViewLength" : "ViewLength" };

            var shadowMaterial = _context.Convert<MyreMaterialData, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("shadows_viewlength", shadowMaterial);

            materialData = new MyreMaterialData { EffectName = Path.GetFullPath(ShadowEffectName), Technique = animated ? "AnimatedViewZ" : "ViewZ" };

            shadowMaterial = _context.Convert<MyreMaterialData, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("shadows_viewz", shadowMaterial);
        }

        private void CreateGBufferMaterial(MaterialContent material, MeshContent mesh, bool animated)
        {
            var diffuseTexture = FindDiffuseTexture(mesh, material);
            var normalTexture = FindNormalTexture(mesh, material);
            var specularTexture = FindSpecularTexture(mesh, material);

            if (diffuseTexture == null)
                return;

            var materialData = new MyreMaterialData { EffectName = Path.GetFullPath(GBufferEffectName), Technique = GBufferTechnique ?? (animated ? "Animated" : "Default") };
            materialData.Textures.Add("DiffuseMap", diffuseTexture);
            materialData.Textures.Add("NormalMap", normalTexture);
            materialData.Textures.Add("SpecularMap", specularTexture);

            var gbufferMaterial = _context.Convert<MyreMaterialData, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("gbuffer", gbufferMaterial);
        }
        #endregion

        #region find texture resources
        private string FindDiffuseTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(DiffuseTexture))
            {
                var texture = FindTexture(mesh, material, "texture", "diffuse", "diff", "d", "c");

                if (texture != null)
                    return texture;

                if (AllowNullDiffuseTexture)
                    return "null_specular.tga";

                return null;
            }
            return Path.Combine(_directory, DiffuseTexture);
        }

        private string FindNormalTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(NormalTexture))
                return FindTexture(mesh, material, "normalmap", "normal", "norm", "n", "bumpmap", "bump", "b") ?? "null_normal.tga";
            return Path.Combine(_directory, NormalTexture);
        }

        private string FindSpecularTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(SpecularTexture))
                return FindTexture(mesh, material, "specularmap", "specular", "spec", "s") ?? "null_specular.tga";
            return Path.Combine(_directory, SpecularTexture);
        }

        private string FindTexture(MeshContent mesh, MaterialContent material, params string[] possibleKeys)
        {
            var path = FindTexturePath(mesh, material, possibleKeys);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            return path;
        }

        private string FindTexturePath(MeshContent mesh, MaterialContent material, string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                // search in existing material textures
                foreach (var item in material.Textures)
                {
                    if (item.Key.ToLowerInvariant() == key)
                        return item.Value.Filename;
                }

                // search in material opaque data
                foreach (var item in material.OpaqueData)
                {
                    if (item.Key.ToLowerInvariant() == key && item.Value is string)
                    {
                        var file = item.Value as string;
                        if (!Path.IsPathRooted(file))
                            file = Path.Combine(_directory, file);

                        return file;
                    }
                }

                // search in mesh opaque data
                foreach (var item in mesh.OpaqueData)
                {
                    if (item.Key.ToLowerInvariant() == key && item.Value is string)
                    {
                        var file = item.Value as string;
                        if (!Path.IsPathRooted(file))
                            file = Path.Combine(_directory, file);

                        return file;
                    }
                }
            }

            // try and find the file in the meshs' directory
            foreach (var key in possibleKeys)
            {
                foreach (var file in Directory.EnumerateFiles(_directory, mesh.Name + "_" + key + ".*", SearchOption.AllDirectories))
                    return file;
            }

            // cant find anything
            return null;
        }
        #endregion

        #region static helpers
        private static void FindMeshes(NodeContent root, ICollection<MeshContent> meshes)
        {
            var content = root as MeshContent;
            if (content != null)
            {
                MeshContent mesh = content;
                mesh.OpaqueData.Add("MeshIndex", meshes.Count);
                meshes.Add(mesh);
            }
            foreach (NodeContent child in root.Children)
                FindMeshes(child, meshes);
        }

        private static bool GeometryContainsChannel(IEnumerable<GeometryContent> geometry, string channel)
        {
            return geometry.Any(item => item.Vertices.Channels.Contains(channel));
        }

        public static int FindBoneIndex(string name, IList<BoneContent> bones)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                if (bone.Name == name)
                    return i;
            }
            
            throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the skeleton.", name));
        }
        #endregion
    }
}