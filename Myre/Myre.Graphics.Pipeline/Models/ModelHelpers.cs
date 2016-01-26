using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Collections.Generic;
using System.Linq;
using Myre.Graphics.Pipeline.Materials;

namespace Myre.Graphics.Pipeline.Models
{
    static class ModelHelpers
    {
        public static MyreMeshContent CreateMyreMesh(GeometryContent geometry, Dictionary<string, MyreMaterialContent> materials)
        {
            return new MyreMeshContent
            {
                Name = geometry.Parent.Name ?? "",
                BoundingSphere = geometry.Vertices.Positions.Count == 0 ? new BoundingSphere(Vector3.Zero, 0) : BoundingSphere.CreateFromPoints(geometry.Vertices.Positions),
                Materials = materials,
                IndexBuffer = geometry.Indices,
                VertexBuffer = geometry.Vertices.CreateVertexBuffer(),
                VertexCount = geometry.Vertices.VertexCount,
                TriangleCount = geometry.Indices.Count / 3,
                ParentBoneIndex = null
            };
        }

        public static void FindMeshes(NodeContent root, ICollection<MeshContent> meshes)
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

        public static bool GeometryContainsChannel(IEnumerable<GeometryContent> geometry, string channel)
        {
            return geometry.Any(item => item.Vertices.Channels.Contains(channel));
        }

        /// <summary>
        /// Determine if a node is a skinned node, meaning it has bone weights associated with it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsSkinned(NodeContent node)
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
        public static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                //------------------------------------------------
                // TODO: Support static meshes parented to a bone
                // -----------------------------------------------
                // What's this all about?
                // If Myre supported meshes which were not skinned, but still had a bone parent this would be important
                // But it doesn't, so we skip this.

                //// This is important: Don't bake in the transforms except
                //// for geometry that is part of a skinned mesh
                //if (!IsSkinned(child))
                //    continue;

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
    }
}
