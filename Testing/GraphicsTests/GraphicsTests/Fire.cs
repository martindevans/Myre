using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Graphics.Translucency.Particles;
using Ninject;
using Myre.Entities;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Particles;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Lighting;

namespace GraphicsTests
{
    static class Fire
    {
        public static Entity Create(IKernel kernel, ContentManager content, Vector3 position)
        {
            var particleEntityDesc = kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty<Vector3>("position");
            particleEntityDesc.AddProperty<Vector3>("colour");
            particleEntityDesc.AddProperty<float>("range");
            particleEntityDesc.AddBehaviour<EllipsoidParticleEmitter>();
            particleEntityDesc.AddBehaviour<PointLight>();
            
            var particleEntity = particleEntityDesc.Create();

            particleEntity.GetProperty<Vector3>("position").Value = position;
            particleEntity.GetProperty<Vector3>("colour").Value = Vector3.Normalize(new Vector3(5, 2, 2)) * 2;
            particleEntity.GetProperty<float>("range").Value = 70f;

            var emitter = particleEntity.GetBehaviour<EllipsoidParticleEmitter>();

            emitter.BlendState = BlendState.Additive;
            emitter.Type = ParticleType.Soft;
            emitter.Enabled = true;
            emitter.EndLinearVelocity = 0.75f;
            emitter.EndScale = 0.25f;
            emitter.Gravity = Vector3.Zero;
            emitter.Lifetime = 4f;
            emitter.Texture = content.Load<Texture2D>("fire");
            emitter.EmitPerSecond = 100;
            emitter.Capacity = (int)(emitter.Lifetime * emitter.EmitPerSecond + 1);
            emitter.HorizontalVelocityVariance = 1;
            emitter.LifetimeVariance = 0f;
            emitter.MaxAngularVelocity = MathHelper.Pi / 4;
            emitter.MaxEndColour = Color.Blue;
            emitter.MaxStartColour = Color.White;
            emitter.MaxStartSize = 10;
            emitter.MinAngularVelocity = -MathHelper.Pi / 4;
            emitter.MinEndColour = Color.White;
            emitter.MinStartColour = Color.Red;
            emitter.MinStartSize = 1;
            emitter.Transform = Matrix.Identity;
            emitter.Velocity = new Vector3(0, 5, 0);
            emitter.VelocityBleedThrough = 0;
            emitter.VerticalVelocityVariance = 0;
            emitter.Ellipsoid = new Vector3(4, 1, 4);
            emitter.MinEmitDistance = 0;

            return particleEntity;
        }
    }
}
