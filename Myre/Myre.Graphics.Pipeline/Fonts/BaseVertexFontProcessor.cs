using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Myre.Extensions;
using Myre.Graphics.Pipeline.Materials;
using Myre.Graphics.Pipeline.Models;
using Poly2Tri;
using Poly2Tri.Triangulation.Delaunay;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

namespace Myre.Graphics.Pipeline.Fonts
{
    public abstract class BaseVertexFontProcessor<TInput>
        : BaseModelProcessor<TInput, VertexFontContent>
    {
        private const string DEFAULT_CHARACTERS = " ,.;:!\"£$%^&*()";

        private string _characters = DEFAULT_CHARACTERS;
        [DisplayName("Characters"), DefaultValue(DEFAULT_CHARACTERS)]
        public string Characters
        {
            get { return _characters; }
            set { _characters = value; }
        }

        private bool _includeLowercaseAlphabet = true;
        [DisplayName("Include Lowercase Alphabet"), DefaultValue(true)]
        public bool IncludeLowercaseAlphabet
        {
            get { return _includeLowercaseAlphabet; }
            set { _includeLowercaseAlphabet = value; }
        }

        private bool _includeUppercaseAlphabet = true;
        [DisplayName("Include Uppercase Alphabet"), DefaultValue(true)]
        public bool IncludeUppercaseAlphabet
        {
            get { return _includeUppercaseAlphabet; }
            set { _includeUppercaseAlphabet = value; }
        }

        private bool _includeNumbers = true;
        [DisplayName("Include Numbers"), DefaultValue(true)]
        public bool IncludeNumbers
        {
            get { return _includeNumbers; }
            set { _includeNumbers = value; }
        }

        private float _fontSize = 128;
        [DisplayName("Font Size"), DefaultValue(128f)]
        public float FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }

        private char _defaultChar = ' ';
        [DisplayName("Default Character"), DefaultValue(' ')]
        public char DefaultCharacter
        {
            get { return _defaultChar; }
            set { _defaultChar = value; }
        }

        private float _angularPrecision = 5;
        [DisplayName("Angular Precision"), DefaultValue(5f)]
        public float AngularPrecision
        {
            get { return _angularPrecision; }
            set { _angularPrecision = value; }
        }

        private float _areaPrecision = 0.01f;
        [DisplayName("Area Precision"), DefaultValue(0.01f)]
        public float AreaPrecision
        {
            get { return _areaPrecision; }
            set { _areaPrecision = value; }
        }

        #region model properties
        [DisplayName("Diffuse Texture")]
        public string DiffuseTexture { get; set; }

        [DisplayName("Specular Texture")]
        public string SpecularTexture { get; set; }

        [DisplayName("Normal Texture")]
        public string NormalTexture { get; set; }

        [DisplayName("Allow null diffuse textures"), DefaultValue(false)]
        public bool AllowNullDiffuseTexture { get; set; }

        private string _gbufferEffectName = null;
        [DisplayName("GBuffer Effect")]
        [DefaultValue(null)]
        public string GBufferEffectName
        {
            get { return _gbufferEffectName; }
            set { _gbufferEffectName = value; }
        }

        private string _translucentEffectName = null;
        [DisplayName("Translucent Effect")]
        [DefaultValue(null)]
        public string TranslucentEffectName
        {
            get { return _translucentEffectName; }
            set { _translucentEffectName = value; }
        }

        private string _translucentEffectTechnique = "translucent";
        [DisplayName("Translucent Effect Technique")]
        [DefaultValue("translucent")]
        public string TranslucentEffectTechnique
        {
            get { return _translucentEffectTechnique; }
            set { _translucentEffectTechnique = value; }
        }

        private string _shadowEffectName = null;
        [DisplayName("GBuffer Shadow Effect")]
        [DefaultValue(null)]
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
        #endregion

        protected override VertexFontContent Process(TInput input)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (FontSize == 0)
                FontSize = 128f;

            try
            {
                using (var fc = Font(input))
                    return Process(fc, IncludedCharacters().ToArray());
            }
            finally
            {
                Processed();
            }
        }

        private VertexFontContent Process(FontFamily input, IEnumerable<char> characters)
        {
            Dictionary<char, VertexCharacterContent> chars = new Dictionary<char, VertexCharacterContent>();

            using (Font f = new Font(input, FontSize))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                foreach (var character in characters)
                    chars.Add(character, CreateVertexCharacter(g, f, character));
            }

            return new VertexFontContent(chars, DefaultCharacter);
        }

        private VertexCharacterContent CreateVertexCharacter(System.Drawing.Graphics g, Font f, char c)
        {
            //Create raw mesh (position data)
            int[] indices;
            Vector3[] vertices;
            GetGlyphMesh(f, c, out vertices, out indices);

            //Create XNA mesh (generate normal, binormal and tangent channels)
            var meshContent = CreateGeometry(indices, vertices);

            //Apply mesh processing (this will find appropriate materials to apply to the font)
            ProcessMesh(meshContent);

            //Return a vertex character
            return new VertexCharacterContent(
                c,
                GetKerningTable(g, f, c, IncludedCharacters()),
                ModelHelpers.CreateMyreMesh(meshContent.Geometry[0], new Dictionary<string, MyreMaterialContent>()),
                GetSize(g, f, c.ToString(CultureInfo.InvariantCulture)).X
            );
        }

        private static MeshContent CreateGeometry(int[] indices, Vector3[] vertices)
        {
            //Copy positions into mesh
            MeshContent mc = new MeshContent();
            foreach (var vertex in vertices)
                mc.Positions.Add(vertex);

            //Create a single geometry batch
            var geometry = new GeometryContent();
            geometry.Vertices.AddRange(indices.Distinct());
            geometry.Indices.AddRange(indices);
            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), typeof(Vector2), null);
            geometry.Vertices.Channels.Add(VertexChannelNames.Normal(0), typeof(Vector3), null);
            mc.Geometry.Add(geometry);

            //Calculate normals, binormals and tangets
            MeshHelper.CalculateNormals(mc, true);
            if (vertices.Length == 0)
            {
                geometry.Vertices.Channels.Add(VertexChannelNames.Binormal(0), typeof(Vector3), null);
                geometry.Vertices.Channels.Add(VertexChannelNames.Tangent(0), typeof(Vector3), null);
            }
            else
                MeshHelper.CalculateTangentFrames(mc, VertexChannelNames.TextureCoordinate(0), VertexChannelNames.Tangent(0), VertexChannelNames.Binormal(0));

            //Optimize the geometry
            MeshHelper.MergeDuplicateVertices(mc);
            MeshHelper.OptimizeForCache(mc);

            return mc;
        }

        #region glyph meshing
        private static bool GetGlyphPoints(Font font, char character, out PointF[] points, out byte[] pointTypes)
        {
            //Write the character into a GraphicsPath, this renders out the character as a series of polygons
            //We can then extract the polygons for ourselves
            using (var path = new GraphicsPath { FillMode = FillMode.Winding })
            {
                path.AddString(character.ToString(CultureInfo.InvariantCulture), font.FontFamily, (int)FontStyle.Regular, font.Size, new PointF(0f, 0f), StringFormat.GenericTypographic);
                path.Flatten();

                if (path.PointCount == 0)
                {
                    points = null;
                    pointTypes = null;
                    return false;
                }

                points = path.PathPoints;
                pointTypes = path.PathTypes;
                return true;
            }
        }

        private static IEnumerable<Polygon> GetGlyphPolygons(PointF[] pts, byte[] ptsType)
        {
            //PathTypes indicates the *type* of each point https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.graphicspath.pathtypes(v=vs.110).aspx
            //basically we're looking for ranges from 0 to 0x80, which a load of 1s in the middle
            var polygons = new List<Polygon>();
            List<PolygonPoint> points = null;
            var start = -1;

            for (var i = 0; i < pts.Length; i++)
            {
                var pointType = ptsType[i] & 0x07;

                if (pointType == 0)
                {
                    points = new List<PolygonPoint> { new PolygonPoint(pts[i].X, pts[i].Y) };
                    start = i;
                    continue;
                }
                if (pointType != 1) throw new Exception("Unsupported point type");

                if ((ptsType[i] & 0x80) != 0)
                {
                    //- Last point in the polygon
                    if (pts[i] != pts[start])
                        points.Add(new PolygonPoint(pts[i].X, pts[i].Y));

                    polygons.Add(new Polygon(points));
                    points = null;
                }
                else
                {
                    points.Add(new PolygonPoint(pts[i].X, pts[i].Y));
                }
            }

            return polygons;
        }

        private static void ClassifyPolygons(IEnumerable<Polygon> allPolygons, ICollection<Polygon> polys, ICollection<Polygon> holes)
        {
            //Classify polygons based on winding order
            foreach (var polygon in allPolygons)
            {
                if (polygon.CalculateWindingOrder() == Point2DList.WindingOrderType.AntiClockwise)
                    polys.Add(polygon);
                else
                    holes.Add(polygon);
            }
        }

        private static void GenerateVerticalFaces(Polygon polygon, List<Vector3> vertices, List<int> indices)
        {
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                var a = polygon.Points[i];
                var b = polygon.Points[(i + 1) % polygon.Points.Count];

                var aTop = new Vector3(a.Xf, 0.5f, a.Yf);
                var bTop = new Vector3(b.Xf, 0.5f, b.Yf);
                var aBot = new Vector3(a.Xf, -0.5f, a.Yf);
                var bBot = new Vector3(b.Xf, -0.5f, b.Yf);

                CreateTriangle(bTop, aTop, aBot, vertices, indices);
                CreateTriangle(bTop, bBot, aBot, vertices, indices);
            }
        }

        private void GetGlyphMesh(Font font, char character, out Vector3[] vertices, out int[] indices)
        {
            // Get points which make up the character
            PointF[] pts;
            byte[] ptsType;
            if (!GetGlyphPoints(font, character, out pts, out ptsType))
            {
                vertices = new Vector3[0];
                indices = new int[0];
                return;
            }

            //Extract polygons from points
            var polygons = GetGlyphPolygons(pts, ptsType).Select(Simplify);

            //Sorts holes from polygons
            var polys = new List<Polygon>();
            var holes = new List<Polygon>();
            ClassifyPolygons(polygons, polys, holes);

            //Punch holes into polygons
            foreach (var poly in polys)
                foreach (var hole in holes)
                    poly.AddHole(hole);

            //Triangulate the set of polygons
            P2T.Triangulate(polys);

            List<Vector3> outputVertices = new List<Vector3>();
            List<int> outputIndices = new List<int>();

            //generate triangle mesh (upper face)
            polys.SelectMany(a => a.Triangles).ForEach(t => CreateTriangle(t, 0.5f, false, outputVertices, outputIndices));

            //generate triangle mesh (lower face, all triangles reversed in order)
            polys.SelectMany(a => a.Triangles).ForEach(t => CreateTriangle(t, -0.5f, true, outputVertices, outputIndices));

            //generate connecting triangles
            foreach (var polygon in polygons)
                GenerateVerticalFaces(polygon, outputVertices, outputIndices);

            vertices = outputVertices.ToArray();
            indices = outputIndices.ToArray();
        }

        private Polygon Simplify(Polygon polygon)
        {
            var points = polygon.Points.Cast<PolygonPoint>().ToList();

            for (int i = 0; i < points.Count && points.Count > 3; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                var c = points[(i + 2) % points.Count];

                var av = new Vector2(a.Xf, a.Yf);
                var bv = new Vector2(b.Xf, b.Yf);
                var cv = new Vector2(c.Xf, c.Yf);

                //calculate angle at b (acos( (a . b) / (|a||b|) ) ) - http://stackoverflow.com/a/1354158/108234
                var ab = bv - av;
                var cb = bv - cv;
                var angle = Math.Acos(Vector2.Dot(ab, cb) / (ab.Length() * cb.Length()));

                //eliminate point if angle is small enough
                if (angle < MathHelper.ToRadians(AngularPrecision))
                {
                    points.RemoveAt((i + 1) % points.Count);
                    i = -1;
                    continue;
                }

                //eliminate triangle with small enough area
                var triangleArea = Math.Abs(ab.Cross(cb) / 2);
                if (triangleArea < FontSize * AreaPrecision)
                {
                    points.RemoveAt((i + 1) % points.Count);
                    i = -1;
                    continue;
                }
            }

            return new Polygon(points);
        }

        private static void CreateTriangle(DelaunayTriangle triangle, float yOffset, bool flip, List<Vector3> vertices, List<int> indices)
        {
            //indices of points (reversed or not?)
            var x = flip ? 0 : 2;
            var z = flip ? 2 : 0;

            //Generate vertices
            var a = new Vector3(triangle.Points[x].Xf, yOffset, triangle.Points[x].Yf);
            var b = new Vector3(triangle.Points.Item1.Xf, yOffset, triangle.Points.Item1.Yf);
            var c = new Vector3(triangle.Points[z].Xf, yOffset, triangle.Points[z].Yf);

            CreateTriangle(a, b, c, vertices, indices);
        }

        private static void CreateTriangle(Vector3 a, Vector3 b, Vector3 c, List<Vector3> vertices, List<int> indices)
        {
            //Find or create indices
            var ai = Index(vertices, a);
            var bi = Index(vertices, b);
            var ci = Index(vertices, c);

            //Create triangle in index buffer
            indices.Add(ai);
            indices.Add(bi);
            indices.Add(ci);
        }

        private static int Index(ICollection<Vector3> vertices, Vector3 vertex)
        {
            vertices.Add(vertex);
            return vertices.Count - 1;
        }
        #endregion

        #region size measurement
        private static Dictionary<char, float> GetKerningTable(System.Drawing.Graphics g, Font f, char character, IEnumerable<char> otherCharacters)
        {
            Dictionary<char, float> kerning = new Dictionary<char, float>();

            foreach (var other in otherCharacters)
            {
                //Measure size of character 1
                var a = GetSize(g, f, character.ToString(CultureInfo.InvariantCulture));

                //measure size of character 2
                var b = GetSize(g, f, other.ToString(CultureInfo.InvariantCulture));

                //Measure size of string (character1)(character2)
                var kernedSize = GetSize(g, f, string.Format("{0}{1}", character, other));

                //The difference in lengths is the kerning between these two characters
                var kern = (a.X + b.X) - kernedSize.X;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (kern != 0)
                    kerning[other] = kern;
            }

            return kerning;
        }

        private static Vector2 GetSize(System.Drawing.Graphics g, Font font, string measureString)
        {
            var stringFormat = new StringFormat
            {
                FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap
            };

            //Measure string gets us a rough idea of the size
            SizeF size = g.MeasureString(measureString, font, new PointF(0, 0), stringFormat);
            RectangleF layoutRect = new RectangleF(0.0f, 0.0f, size.Width, size.Height);

            //This is a more accurate measure of size
            stringFormat.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, measureString.Length) });
            var stringRegions = g.MeasureCharacterRanges(
                measureString,
                font,
                layoutRect,
                stringFormat);
            var s = stringRegions[0].GetBounds(g);

            return new Vector2(s.Width, s.Height);
        }
        #endregion

        protected abstract void Processed();

        protected abstract FontFamily Font(TInput input);

        private IEnumerable<char> IncludedCharacters()
        {
            if (Characters != null)
                foreach (var character in Characters)
                    yield return character;

            if (IncludeLowercaseAlphabet)
                foreach (var character in "abcdefghijklmnopqrstuvwxyz")
                    yield return character;

            if (IncludeUppercaseAlphabet)
                foreach (var character in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                    yield return character;

            if (IncludeNumbers)
                foreach (var character in "1234567890")
                    yield return character;
        }
    }
}
