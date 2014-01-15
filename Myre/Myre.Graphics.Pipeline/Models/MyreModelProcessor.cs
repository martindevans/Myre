using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Pipeline.Animations;
using Myre.Graphics.Pipeline.Materials;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Myre.Graphics.Pipeline.Models
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
        private Dictionary<string, int> _boneIndices;

        /// <summary>
        /// Converts incoming graphics data into our custom model format.
        /// </summary>
        public override MyreModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            _context = context;

            _directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            List<Vector3>[] verticesPerBone = null;

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);
            if (skeleton != null)
            {
                _bones = MeshHelper.FlattenSkeleton(skeleton);
                _boneIndices = _bones.Select((a, i) => new {a, i}).ToDictionary(a => a.a.Name, a => a.i);

                //Create a list of positions for each bone
                verticesPerBone = new List<Vector3>[MeshHelper.FlattenSkeleton(skeleton).Count];
                for (int i = 0; i < verticesPerBone.Length; i++)
                    verticesPerBone[i] = new List<Vector3>();
            }

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            _outputModel = new MyreModelContent();

            //Process meshes
            List<MeshContent> meshes = new List<MeshContent>();
            FindMeshes(input, meshes);
            foreach (var mesh in meshes)
                ProcessMesh(mesh, context, verticesPerBone);

            //Extract skeleton data from the input
            ProcessSkinningData(input, skeleton, context, verticesPerBone);

            return _outputModel;
        }

        #region animation processing
        private void ProcessSkinningData(NodeContent node, BoneContent skeleton, ContentProcessorContext context, IEnumerable<List<Vector3>> verticesPerBone)
        {
            if (skeleton == null)
            {
                _outputModel.SkinningData = null;
                return;
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in _bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(_bones.IndexOf(bone.Parent as BoneContent));
            }

            _outputModel.SkinningData = new SkinningDataContent(
                bindPose,
                inverseBindPose,
                skeletonHierarchy,
                _bones.Select(b => b.Name).ToList(),
                verticesPerBone.Select((a, i) => CalculateBoundingBox(a, _bones[i])).ToList()
            );
        }

        #region ABB fitting
        private static BoundingBox CalculateBoundingBox(List<Vector3> points, BoneContent bone)
        {
            if (points.Count == 0)
                return new BoundingBox();

            Matrix m = Matrix.Invert(bone.AbsoluteTransform);

            //We could sample a load of rotations *around* the bone axis here, and establish a rotated bounding box to find a slightly smaller volume

            return BoundingBox.CreateFromPoints(points.Select(p => Vector3.Transform(p, m)));
        }
        #endregion

        #region OBB fitting
        //        //OBB fitting Code adapted from here:
//        //http://jamesgregson.blogspot.co.uk/2011/03/latex-test.html

//        private static RotatedBoundingBox CalculateBoundingBox(List<Vector3> points)
//        {
//            var bounds = BuildBoundsFromConvexHull(points);

//            //int failures = 0;

//            //foreach (var vector3 in points)
//            //{
//            //    var trans = Vector3.Transform(vector3, bounds.Rotation);

//            //    var dmax = bounds.Bounds.Max - trans;
//            //    if (dmax.X < -0.0001f || dmax.Y < -0.0001f || dmax.Z < -0.0001f)
//            //    {
//            //        failures++;
//            //    }
//            //    var dmin = bounds.Bounds.Min - trans;
//            //    if (dmin.X > 0.0001f || dmin.Y > 0.0001f || dmin.Z > 0.0001f)
//            //    {
//            //        failures++;
//            //    }
//            //}

//            //if (failures > 0)
//            //    throw new NotImplementedException();

//            return bounds;
//        }

//        private static RotatedBoundingBox BuildBoundsFromConvexHull(IEnumerable<Vector3> vertices)
//        {
//            if (vertices.Any())
//            {
//                var hull = QHull(vertices).ToArray();
//                if (hull.Any())
//                    return BuildBoundsFromTriangles(hull, vertices);
//            }

//            return new RotatedBoundingBox
//            {
//                Bounds = new BoundingBox(Vector3.Zero, Vector3.Zero),
//                Rotation = Quaternion.Identity
//            };
//        }

//        private static RotatedBoundingBox BuildBoundsFromTriangles(IList<Triangle> triangles, IEnumerable<Vector3> vertices)
//        {
//            Vector3 avg = Vector3.Zero;
//            float cxx = 0, cxy = 0, cxz = 0, cyy = 0, cyz = 0, czz = 0;
//            float totalArea = 0;

//            // loop over the triangles this time to find the
//            // mean location
//            for (int i = 0; i < triangles.Count; i += 3)
//            {
//                var p = triangles[i].A;
//                var q = triangles[i].B;
//                var r = triangles[i].C;
//                var mui = (p + q + r) / 3f;

//                float triArea = Vector3.Cross((q - p), (r - p)).Length() / 2;
//                avg += mui * triArea;
//                totalArea += triArea;

//                // these bits set the c terms to Am*E[xx], Am*E[xy], Am*E[xz]....
//                cxx += (9.0f * mui.X * mui.X + p.X * p.X + q.X * q.X + r.X * r.X) * (triArea / 12);
//                cxy += (9.0f * mui.X * mui.Y + p.X * p.Y + q.X * q.Y + r.X * r.Y) * (triArea / 12);
//                cxz += (9.0f * mui.X * mui.Z + p.X * p.Z + q.X * q.Z + r.X * r.Z) * (triArea / 12);
//                cyy += (9.0f * mui.Y * mui.Y + p.Y * p.Y + q.Y * q.Y + r.Y * r.Y) * (triArea / 12);
//                cyz += (9.0f * mui.Y * mui.Z + p.Y * p.Z + q.Y * q.Z + r.Y * r.Z) * (triArea / 12);
//                czz += (9.0f * mui.Z * mui.Z + p.Z * p.Z + q.Z * q.Z + r.Z * r.Z) * (triArea / 12.0f);
//            }

//            // divide out the Am fraction from the average position and 
//            // covariance terms
//            avg /= totalArea;
//            cxx /= totalArea;
//            cxy /= totalArea;
//            cxz /= totalArea;
//            cyy /= totalArea;
//            cyz /= totalArea;
//            czz /= totalArea;

//            // now subtract off the E[x]*E[x], E[x]*E[y], ... terms
//            cxx -= avg.X * avg.X;
//            cxy -= avg.X * avg.Y;
//            cxz -= avg.X * avg.Z;
//            cyy -= avg.Y * avg.Y;
//            cyz -= avg.Y * avg.Z;
//            czz -= avg.Z * avg.Z;

//            var covariance = DenseMatrix.OfArray(new float[,]
//            {
//                {cxx, cxy, cxz},
//                {cxy, cyy, cyz},
//                {cxz, cyz, czz}
//            });

//            return BuildBoundsFromCovarianceMatrix(covariance, vertices);
//        }

//        private static RotatedBoundingBox BuildBoundsFromCovarianceMatrix(DenseMatrix covarianceMatrix, IEnumerable<Vector3> vertices)
//        {
//            var evd = covarianceMatrix.Evd();
//            var eigen = evd.EigenVectors();

//            Func<Vector<float>, Vector3> toVector3 = v =>
//            {
//                if (v.Count != 3)
//                    throw new ArgumentException("v");

//                return new Vector3(v[0], v[1], v[2]);
//            };

//            var r = Vector3.Normalize(toVector3(eigen.Column(0)));
//            var u = Vector3.Normalize(toVector3(eigen.Column(1)));
//            var f = Vector3.Normalize(toVector3(eigen.Column(2)));

//            Vector3 min = new Vector3(float.MaxValue);
//            Vector3 max = new Vector3(float.MinValue);
//            foreach (var vertex in vertices)
//            {
//                var pPrime = new Vector3(Vector3.Dot(r, vertex), Vector3.Dot(u, vertex), Vector3.Dot(f, vertex));
//                min.X = Math.Min(min.X, pPrime.X);
//                min.Y = Math.Min(min.Y, pPrime.Y);
//                min.Z = Math.Min(min.Z, pPrime.Z);
//                max.X = Math.Max(max.X, pPrime.X);
//                max.Y = Math.Max(max.Y, pPrime.Y);
//                max.Z = Math.Max(max.Z, pPrime.Z);
//            }

//            Matrix rotation = Matrix.Identity;
//            rotation.Right = r;
//            rotation.Forward = f;
//            rotation.Up = u;

//            var result = new RotatedBoundingBox
//            {
//                Bounds = new BoundingBox(min, max),
//                Rotation = Quaternion.CreateFromRotationMatrix(rotation)
//            };

//            return result;
//        }

//        private static RotatedBoundingBox BuildFromVertices(IEnumerable<Vector3> vertices)
//        {
//            var count = vertices.Count();
//            if (count == 0)
//            {
//                return new RotatedBoundingBox
//                {
//                    Bounds = new BoundingBox(Vector3.Zero, Vector3.Zero),
//                    Rotation = Quaternion.Identity
//                };
//            }

//            var avg = vertices.Select(a => a / count).Aggregate((a, b) => a + b);

//            float cxx = 0, cxy = 0, cxz = 0, cyy = 0, cyz = 0, czz = 0;
//            foreach (var point in vertices)
//            {
//                cxx += point.X * point.X - avg.X * avg.X;
//                cxy += point.X * point.Y - avg.X * avg.Y;
//                cxz += point.X * point.Z - avg.X * avg.Z;
//                cyy += point.Y * point.Y - avg.Y * avg.Y;
//                cyz += point.Y * point.Z - avg.Y * avg.Z;
//                czz += point.Z * point.Z - avg.Z * avg.Z;
//            }

//            var covariance = DenseMatrix.OfArray(new float[,]
//            {
//                {cxx, cxy, cxz},
//                {cxy, cyy, cyz},
//                {cxz, cyz, czz}
//            });

//            return BuildBoundsFromCovarianceMatrix(covariance, vertices);
//        }

//        private struct Triangle
//        {
//            public Vector3 A;
//            public Vector3 B;
//            public Vector3 C;

//            public Triangle(Vector3 a, Vector3 b, Vector3 c)
//            {
//                A = a;
//                B = b;
//                C = c;
//            }
//        }

//        private static IEnumerable<Triangle> QHull(IEnumerable<Vector3> vertices)
//        {
//            StringBuilder shapeBuilder = new StringBuilder();
//            shapeBuilder.AppendLine("3");                                                         //Dimensions
//            shapeBuilder.AppendLine(vertices.Count().ToString(CultureInfo.InvariantCulture));     //vertex count
//            foreach (var vector3 in vertices)
//                shapeBuilder.AppendLine(string.Format("{0} {1} {2}", vector3.X, vector3.Y, vector3.Z));   //Vertices
//            var shape = shapeBuilder.ToString();

//            var tmp = Path.GetTempFileName();
//            using (var qhull = File.OpenWrite(tmp))
//            {
//                using (var qhullRes = Assembly.GetExecutingAssembly().GetManifestResourceStream("Myre.Graphics.Pipeline.qhull.exe"))
//                    qhullRes.CopyTo(qhull);
//            }

//            string result;
//            try
//            {
//                Process p = new Process();
//                p.StartInfo = new ProcessStartInfo
//                {
//                    FileName = tmp,
//                    UseShellExecute = false,
//                    Arguments = "Ft",
//                    CreateNoWindow = true,
//                    RedirectStandardInput = true,
//                    RedirectStandardOutput = true,
//                };

//                p.Start();
//                p.StandardInput.Write(shape);
//                p.StandardInput.Close();

//                result = p.StandardOutput.ReadToEnd();

//                p.WaitForExit();
//            }
//            finally
//            {
//                try
//                {
//                    File.Delete(tmp);
//                }
//// ReSharper disable EmptyGeneralCatchClause
//                catch
//// ReSharper restore EmptyGeneralCatchClause
//                {
//                    //Well, we tried
//                }
//            }

//            if (string.IsNullOrWhiteSpace(result))
//                return new Triangle[0];

//            var lines = result.Split('\n').ToArray();

//            if (int.Parse(lines[0]) != 3)
//                throw new InvalidOperationException("Incorrect number of dimensions");

//            if (lines[1].Split(' ').Length != 3)
//                throw new InvalidOperationException("Incorrect number of counts");

//            var vertexCount = int.Parse(lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
//            var outputVertices = lines.Skip(2).Take(vertexCount).Select(line =>
//            {
//                var floats = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();
//                return new Vector3(floats[0], floats[1], floats[2]);
//            }).ToArray();

//            int trianglesCount = int.Parse(lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
//            var outputTriangles = lines.Skip(2).Skip(vertexCount).Take(trianglesCount).Select(line =>
//            {
//                var ints = line.Trim().Split(' ').Select(int.Parse).ToArray();
//                if (ints[0] != 3)
//                    throw new InvalidOperationException("Incorrect number of indices");
//                return new Triangle(outputVertices[ints[1]], outputVertices[ints[2]], outputVertices[ints[3]]);
//            }).ToArray();

//            return outputTriangles;
//        }
        #endregion
        #endregion

        #region geometry processing
        private void ProcessMesh(MeshContent mesh, ContentProcessorContext context, List<Vector3>[] verticesPerBone)
        {
            MeshHelper.MergeDuplicateVertices(mesh);
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

            // Process all the geometry in the mesh.
            foreach (GeometryContent geometry in mesh.Geometry)
                ProcessGeometry(geometry, _outputModel, context, verticesPerBone);
        }

        /// <summary>
        /// Converts a single piece of input geometry into our custom format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry, MyreModelContent model, ContentProcessorContext context, List<Vector3>[] verticesPerBone)
        {
            //save which vertices are assigned to which bone
            if (geometry.Vertices.Channels.Contains(VertexChannelNames.Weights(0)) && verticesPerBone != null)
            {
                var weights = geometry.Vertices.Channels.Get<BoneWeightCollection>(VertexChannelNames.Weights(0));
                for (int i = 0; i < weights.Count; i++)
                {
                    var maxBone = weights[i].Aggregate((a, b) => a.Weight > b.Weight ? a : b).BoneName;
                    verticesPerBone[_boneIndices[maxBone]].Add(geometry.Vertices.Positions[i]);
                }
            }

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
            
            // Add the new piece of geometry to our output model.
            model.AddMesh(new MyreMeshContent
            {
                Name = geometry.Parent.Name,
                BoundingSphere = BoundingSphere.CreateFromPoints(geometry.Vertices.Positions),
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
            var materialData = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(ShadowEffectName), Technique = animated ? "AnimatedViewLength" : "ViewLength" };

            var shadowMaterial = _context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("shadows_viewlength", shadowMaterial);

            materialData = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(ShadowEffectName), Technique = animated ? "AnimatedViewZ" : "ViewZ" };

            shadowMaterial = _context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("shadows_viewz", shadowMaterial);
        }

        private void CreateGBufferMaterial(MaterialContent material, MeshContent mesh, bool animated)
        {
            var diffuseTexture = CanonicalizeTexturePath(FindDiffuseTexture(mesh, material));
            var normalTexture = CanonicalizeTexturePath(FindNormalTexture(mesh, material));
            var specularTexture = CanonicalizeTexturePath(FindSpecularTexture(mesh, material));

            if (diffuseTexture == null)
                return;

            var materialData = new MyreMaterialDefinition { EffectName = Path.GetFileNameWithoutExtension(_gbufferEffectName), Technique = _gbufferTechnique ?? (animated ? "Animated" : "Default") };

            materialData.Textures.Add("DiffuseMap", diffuseTexture);
            materialData.Textures.Add("NormalMap", normalTexture);
            materialData.Textures.Add("SpecularMap", specularTexture);

            var gbufferMaterial = _context.Convert<MyreMaterialDefinition, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            _processedMaterials[material].Add("gbuffer", gbufferMaterial);
        }
        #endregion

        #region find texture resources
        private string CanonicalizeTexturePath(string texturePath)
        {
            if (texturePath == null)
                return null;

            if (!Path.IsPathRooted(texturePath))
                return texturePath;

            Uri from = new Uri(_directory);
            Uri to = new Uri(new ExternalReference<Texture2DContent>(texturePath).Filename);

            Uri relative = from.MakeRelativeUri(to);
            string path = Uri.UnescapeDataString(relative.ToString()).Replace('/', Path.DirectorySeparatorChar);

            var filename = Path.GetFileNameWithoutExtension(path);
            var dirname = Path.GetDirectoryName(path);

            return Path.Combine(dirname, filename);
        }

        private string FindDiffuseTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(DiffuseTexture))
            {
                var texture = FindTexture(mesh, material, "texture", "diffuse", "diff", "d", "c");

                if (texture != null)
                    return texture;

                if (AllowNullDiffuseTexture)
                    return "null_specular";

                return null;
            }
            return DiffuseTexture;
        }

        private string FindNormalTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(NormalTexture))
                return FindTexture(mesh, material, "normalmap", "normal", "norm", "n", "bumpmap", "bump", "b") ?? "null_normal";
// ReSharper disable AssignNullToNotNullAttribute
            return Path.Combine(_directory, Path.GetFileNameWithoutExtension(NormalTexture));
// ReSharper restore AssignNullToNotNullAttribute
        }

        private string FindSpecularTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(SpecularTexture))
                return FindTexture(mesh, material, "specularmap", "specular", "spec", "s") ?? "null_specular";
// ReSharper disable AssignNullToNotNullAttribute
            return Path.Combine(_directory, Path.GetFileNameWithoutExtension(SpecularTexture));
// ReSharper restore AssignNullToNotNullAttribute
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

        /// <summary>
        /// Determine if a node is a skinned node, meaning it has bone weights associated with it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsSkinned(NodeContent node)
        {
            // It has to be a MeshContent node
            MeshContent mesh = node as MeshContent;
            if (mesh == null)
                return false;

            // In the geometry we have to find a vertex channel that
            // has a bone weight collection
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                {
                    if (vchannel is VertexChannel<BoneWeightCollection>)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        private static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // This is important: Don't bake in the transforms except
                // for geometry that is part of a skinned mesh
                if (!IsSkinned(child))
                    continue;

                FlattenAllTransforms(child);
            }
        }

        /// <summary>
        /// Recursively flatten all transforms from this node down
        /// </summary>
        /// <param name="node"></param>
        private static void FlattenAllTransforms(NodeContent node)
        {
            // Bake the local transform into the actual geometry.
            MeshHelper.TransformScene(node, node.Transform);

            // Having baked it, we can now set the local
            // coordinate system back to identity.
            node.Transform = Matrix.Identity;

            foreach (NodeContent child in node.Children)
                FlattenAllTransforms(child);
        }
        #endregion
    }
}