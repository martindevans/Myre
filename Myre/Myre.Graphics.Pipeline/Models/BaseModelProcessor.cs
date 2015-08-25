using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Myre.Graphics.Pipeline.Animations;
using Myre.Graphics.Pipeline.Materials;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Myre.Graphics.Pipeline.Models
{
    public abstract class BaseModelProcessor<TInput, TOutput>
        : ContentProcessor<TInput, TOutput>
    {
        #region config properties
        private string _diffuseTexture;
        [DisplayName("Diffuse Texture"), DefaultValue(null)]
        public string DiffuseTexture
        {
            get { return _diffuseTexture; }
            set { _diffuseTexture = value; }
        }

        private string _specularTexture;
        [DisplayName("Specular Texture"), DefaultValue(null)]
        public string SpecularTexture
        {
            get { return _specularTexture; }
            set { _specularTexture = value; }
        }

        private string _normalTexture;
        [DisplayName("Normal Texture"), DefaultValue(null)]
        public string NormalTexture
        {
            get { return _normalTexture; }
            set { _normalTexture = value; }
        }

        private string _gbufferEffectName;
        [DisplayName("GBuffer Effect"), DefaultValue(null)]
        public string GBufferEffectName
        {
            get { return _gbufferEffectName; }
            set { _gbufferEffectName = value; }
        }

        private string _translucentEffectName;
        [DisplayName("Translucent Effect"), DefaultValue(null)]
        public string TranslucentEffectName
        {
            get { return _translucentEffectName; }
            set { _translucentEffectName = value; }
        }

        private string _translucentEffectTechnique = "translucent";
        [DisplayName("Translucent Effect Technique"), DefaultValue("translucent")]
        public string TranslucentEffectTechnique
        {
            get { return _translucentEffectTechnique; }
            set { _translucentEffectTechnique = value; }
        }

        private string _shadowEffectTechnique;
        [DisplayName("GBuffer Shadow Effect"), DefaultValue(null)]
        public string ShadowEffectName
        {
            get { return _shadowEffectTechnique; }
            set { _shadowEffectTechnique = value; }
        }

        private string _gbufferTechnique;
        [DisplayName("GBuffer Technique"), DefaultValue(null)]
        public string GBufferTechnique
        {
            get { return _gbufferTechnique; }
            set { _gbufferTechnique = value; }
        }

        #endregion

        protected ContentProcessorContext Context { get; private set; }

        protected abstract string Directory { get; }

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        readonly Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>> _processedMaterials = new Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>>();

        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            Context = context;

            return Process(input);
        }

        protected abstract TOutput Process(TInput input);

        #region textures
        private string CanonicalizeTexturePath(string texturePath)
        {
            if (texturePath == null)
                return null;

            if (!Path.IsPathRooted(texturePath))
                return texturePath;

            Uri from = new Uri(Directory);
            Uri to = new Uri(new ExternalReference<Texture2DContent>(texturePath).Filename);

            Uri relative = from.MakeRelativeUri(to);
            string path = Uri.UnescapeDataString(relative.ToString()).Replace('/', Path.DirectorySeparatorChar);

            var filename = Path.GetFileNameWithoutExtension(path);
            var dirname = Path.GetDirectoryName(path);

            return Path.Combine(dirname, filename);
        }

        private static string MakeRelative(string fromPath, string toPath)
        {
            Uri from = new Uri(fromPath);
            Uri to = new Uri(toPath);

            Uri relative = from.MakeRelativeUri(to);
            string path = Uri.UnescapeDataString(relative.ToString()).Replace('/', Path.DirectorySeparatorChar);

            var filename = Path.GetFileNameWithoutExtension(path);
            var dirname = Path.GetDirectoryName(path);

            return Path.Combine(dirname, filename);
        }

        /// <summary>
        /// Find an appropriate diffuse texture to use
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        protected string FindDiffuseTexture(MeshContent mesh, MaterialContent material)
        {
            if (!string.IsNullOrEmpty(DiffuseTexture))
                return CanonicalizeTexturePath(DiffuseTexture);

            return FindAndBuildTexture(mesh, material, true, "texture", "diffuse", "diff", "d", "c") ?? CanonicalizeTexturePath("null_diffuse");
        }

        /// <summary>
        /// Find an appropriate normal texture to use
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        protected string FindNormalTexture(ContentItem mesh, MaterialContent material)
        {
            if (!string.IsNullOrEmpty(NormalTexture))
                return CanonicalizeTexturePath(NormalTexture);

            return FindAndBuildTexture(mesh, material, false, "normalmap", "normal", "norm", "n", "bumpmap", "bump", "b") ?? CanonicalizeTexturePath("null_normal");
        }

        /// <summary>
        /// Find an appropriate specular texture to use
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        protected string FindSpecularTexture(ContentItem mesh, MaterialContent material)
        {
            if (!string.IsNullOrEmpty(SpecularTexture))
                return CanonicalizeTexturePath(SpecularTexture);

            return FindAndBuildTexture(mesh, material, true, "specularmap", "specular", "spec", "s") ?? CanonicalizeTexturePath("null_specular");
        }

        private string FindAndBuildTexture(ContentItem mesh, MaterialContent material, bool dxt, params string[] possibleKeys)
        {
            //Find a path to the unbuilt content
            var paths = FindTexturePath(mesh, material, Directory, possibleKeys)
                .Where(a => !string.IsNullOrEmpty(a) && File.Exists(a))
                .ToArray();

            var path = paths.FirstOrDefault();
            if (string.IsNullOrEmpty(path))
                return null;

            //Build the content
            var contentItem = Context.BuildAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(path), "TextureProcessor", new OpaqueDataDictionary
            {
                { "GenerateMipmaps", true },
                { "TextureFormat", dxt ? TextureProcessorOutputFormat.DxtCompressed : TextureProcessorOutputFormat.Color },

            }, null, null);

            //Return the path (relative to the output root) to this item
            return CanonicalizeTexturePath(MakeRelative(Context.OutputDirectory, contentItem.Filename));
        }

        private static IEnumerable<string> FindTexturePath(ContentItem mesh, MaterialContent material, string directory, string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                if (material != null)
                {
                    // search in existing material textures
                    foreach (var item in material.Textures)
                    {
                        if (item.Key.ToLowerInvariant() == key)
                            yield return item.Value.Filename;
                    }

                    // search in material opaque data
                    foreach (var item in material.OpaqueData)
                    {
                        if (item.Key.ToLowerInvariant() == key && item.Value is string)
                        {
                            var file = item.Value as string;
                            if (!Path.IsPathRooted(file))
                                file = Path.Combine(directory, file);

                            yield return file;
                        }
                    }
                }

                if (mesh != null)
                {
                    // search in mesh opaque data
                    foreach (var item in mesh.OpaqueData)
                    {
                        if (item.Key.ToLowerInvariant() == key && item.Value is string)
                        {
                            var file = item.Value as string;
                            if (!Path.IsPathRooted(file))
                                file = Path.Combine(directory, file);

                            yield return file;
                        }
                    }
                }
            }

            // try and find the file in the meshs' directory
            yield return possibleKeys
                .SelectMany(key => System.IO.Directory.EnumerateFiles(directory, (mesh == null ? key : mesh.Name + "_" + key) + ".*", SearchOption.AllDirectories))
                .FirstOrDefault();
        }
        #endregion

        #region material processing
        /// <summary>
        /// Creates default materials suitable for rendering in the myre deferred renderer.
        /// The current material is searched for diffuse, normal and specular textures.
        /// </summary>
        protected Dictionary<string, MyreMaterialContent> ProcessMaterial(MaterialContent material, MeshContent mesh)
        {
            //If material is null then create material without caching
            if (material == null)
                return CreateMaterials(null, mesh);

            // Have we already processed this material? If not, process it now
            if (!_processedMaterials.ContainsKey(material))
                _processedMaterials[material] = CreateMaterials(material, mesh);   

            return _processedMaterials[material];
        }

        private Dictionary<string, MyreMaterialContent> CreateMaterials(MaterialContent material, MeshContent mesh)
        {
            Dictionary<string, MyreMaterialContent> output = new Dictionary<string, MyreMaterialContent>();

            bool animatedMaterials = MeshHelper.FindSkeleton(mesh) != null;
            CreateGBufferMaterial(material, mesh, animatedMaterials, output);
            CreateShadowMaterial(material, animatedMaterials, output);
            CreateTransparentMaterial(material, mesh, animatedMaterials, output);

            return output;
        }

        private void CreateTransparentMaterial(MaterialContent material, MeshContent mesh, bool animated, IDictionary<string, MyreMaterialContent> output)
        {
            if (TranslucentEffectName == null)
                return;

            var materialData = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(TranslucentEffectName), Technique = TranslucentEffectTechnique ?? (animated ? "AnimatedTranslucent" : "Translucent") };
            var diffuseTexture = FindDiffuseTexture(mesh, material);
            if (diffuseTexture != null)
                materialData.Textures.Add("DiffuseMap", diffuseTexture);

            output.Add("translucent", Context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialData, "MyreMaterialProcessor"));
        }

        private void CreateShadowMaterial(MaterialContent material, bool animated, IDictionary<string, MyreMaterialContent> output)
        {
            if (ShadowEffectName == null)
                return;

            var materialDataLength = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(ShadowEffectName), Technique = animated ? "AnimatedViewLength" : "ViewLength" };
            var shadowMaterialLength = Context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialDataLength, "MyreMaterialProcessor");
            output.Add("shadows_viewlength", shadowMaterialLength);

            var materialDataZ = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(ShadowEffectName), Technique = animated ? "AnimatedViewZ" : "ViewZ" };
            var shadowMaterialZ = Context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialDataZ, "MyreMaterialProcessor");
            output.Add("shadows_viewz", shadowMaterialZ);
        }

        private void CreateGBufferMaterial(MaterialContent material, MeshContent mesh, bool animated, IDictionary<string, MyreMaterialContent> output)
        {
            if (GBufferEffectName == null)
                return;

            var diffuseTexture = FindDiffuseTexture(mesh, material);
            var normalTexture = FindNormalTexture(mesh, material);
            var specularTexture = FindSpecularTexture(mesh, material);

            if (diffuseTexture == null)
                return;

            var materialData = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(GBufferEffectName), Technique = GBufferTechnique ?? (animated ? "Animated" : "Default") };

            materialData.Textures.Add("DiffuseMap", diffuseTexture);
            materialData.Textures.Add("NormalMap", normalTexture);
            materialData.Textures.Add("SpecularMap", specularTexture);

            var gbufferMaterial = Context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            output.Add("gbuffer", gbufferMaterial);
        }
        #endregion

        #region vertex channel processing
        private readonly Dictionary<string, Action<GeometryContent, VertexChannel>> _vertexChannelProcessors = new Dictionary<string, Action<GeometryContent, VertexChannel>>();

        protected void AddChannelProcessor<T>(string name, Action<GeometryContent, VertexChannel<T>> processor)
        {
            _vertexChannelProcessors[name] = (a, b) => processor(a, (VertexChannel<T>)b);
        }

        protected void ProcessChannels(GeometryContent geometry)
        {
            var channels = geometry.Vertices.Channels.ToArray();
            foreach (var channel in channels)
            {
                Action<GeometryContent, VertexChannel> act;
                if (_vertexChannelProcessors.TryGetValue(channel.Name, out act))
                    act(geometry, channel);
            }
        }
        #endregion

        #region animation processing
        protected static SkinningDataContent ProcessSkinningData(BoneContent skeleton, IList<BoneContent> bones, IEnumerable<List<Vector3>> verticesPerBone)
        {
            if (skeleton == null)
                return null;

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);

                Matrix inverted = Matrix.Invert(bone.AbsoluteTransform);
                inverseBindPose.Add(inverted);

                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            return new SkinningDataContent(
                bindPose,
                inverseBindPose,
                skeletonHierarchy,
                bones.Select(b => b.Name).ToList(),
                verticesPerBone.Select((a, i) => CalculateBoundingBox(a, bones[i])).ToList()
            );
        }

        #region ABB fitting
        private static Microsoft.Xna.Framework.BoundingBox CalculateBoundingBox(ICollection<Vector3> points, BoneContent bone)
        {
            if (points.Count == 0)
                return new Microsoft.Xna.Framework.BoundingBox();

            Matrix m = Matrix.Invert(bone.AbsoluteTransform);

            //We could sample a load of rotations *around* the bone axis here, and establish a rotated bounding box to find a slightly smaller volume

            return Microsoft.Xna.Framework.BoundingBox.CreateFromPoints(points.Select(p => Vector3.Transform(p, m)));
        }
        #endregion
        #endregion

        #region geometry processing
        protected IEnumerable<MyreMeshContent> ProcessMesh(MeshContent mesh, List<Vector3>[] verticesPerBoneIndex = null, ReadOnlyDictionary<string, int> boneIndices = null)
        {
            MeshHelper.MergeDuplicateVertices(mesh);
            MeshHelper.OptimizeForCache(mesh);

            // create texture coordinates of 0 if none are present
            var texCoord0 = VertexChannelNames.TextureCoordinate(0);
            foreach (var item in mesh.Geometry)
            {
                if (!item.Vertices.Channels.Contains(texCoord0))
                    item.Vertices.Channels.Add<Microsoft.Xna.Framework.Vector2>(texCoord0, null);
            }

            // calculate tangent frames for normal mapping
            var hasTangents = ModelHelpers.GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Tangent(0));
            var hasBinormals = ModelHelpers.GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Binormal(0));
            if (!hasTangents || !hasBinormals)
            {
                var tangentName = hasTangents ? null : VertexChannelNames.Tangent(0);
                var binormalName = hasBinormals ? null : VertexChannelNames.Binormal(0);
                MeshHelper.CalculateTangentFrames(mesh, VertexChannelNames.TextureCoordinate(0), tangentName, binormalName);
            }

            // Process all the geometry in the mesh and add it to the model
            return mesh.Geometry.Select(geometry => ProcessGeometry(geometry, verticesPerBoneIndex, boneIndices)).ToArray();
        }

        protected MyreMeshContent ProcessGeometry(GeometryContent geometry, List<Vector3>[] verticesPerBoneIndex = null, ReadOnlyDictionary<string, int> boneIndices = null)
        {
            //save which vertices are assigned to which bone
            if (geometry.Vertices.Channels.Contains(VertexChannelNames.Weights(0)) && verticesPerBoneIndex != null && boneIndices != null)
            {
                //Weights for this geometry, each index represents a single vertex (each vertex has multiple weights)
                var weights = geometry.Vertices.Channels.Get<BoneWeightCollection>(VertexChannelNames.Weights(0));

                for (int i = 0; i < weights.Count; i++)
                {
                    foreach (var weight in weights[i])
                    {
                        verticesPerBoneIndex[boneIndices[weight.BoneName]].Add(geometry.Vertices.Positions[i]);
                    }
                }
            }

            // Apply channel processing as necessary
            ProcessChannels(geometry);

            // Convert the input material.
            var materials = ProcessMaterial(geometry.Material, geometry.Parent);

            // return a myre mesh object
            return ModelHelpers.CreateMyreMesh(geometry, materials);
        }
        #endregion
    }
}
