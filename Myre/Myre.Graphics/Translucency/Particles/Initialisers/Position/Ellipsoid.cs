using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Position
{
    public class Ellipsoid
        :BaseParticleInitialiser
    {
        public Vector3 Shape { get; set; }
        public float MinEmitDistance { get; set; }

        public Ellipsoid(Vector3 ellipsoidShape, float minEmitDistance)
        {
            Shape = ellipsoidShape;
            MinEmitDistance = minEmitDistance;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            particle.Position += RandomPositionOffset(random);
        }

        private Vector3 RandomPositionOffset(Random random)
        {
            Vector3 min;
            Vector3 max;

            do
            {
                Vector3 rand = random.RandomNormalVector();
                max = rand * Shape;
                min = Vector3.Normalize(max) * MinEmitDistance;
            } while (MinEmitDistance > max.Length());

            return Vector3.Lerp(min, max, (float)random.NextDouble());
        }

        public override object Clone()
        {
            return new Ellipsoid(Shape, MinEmitDistance);
        }
    }

    public class EllipsoidReader : ContentTypeReader<Ellipsoid>
    {
        protected override Ellipsoid Read(ContentReader input, Ellipsoid existingInstance)
        {
            return new Ellipsoid(input.ReadVector3(), input.ReadSingle());
        }
    }
}
