//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using Myre.Entities;
//using Myre.Graphics.Lighting;
//using Myre.Graphics.Translucency.Particles;
//using Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity;
//using Myre.Graphics.Translucency.Particles.Initialisers.Colour;
//using Myre.Graphics.Translucency.Particles.Initialisers.Lifetime;
//using Myre.Graphics.Translucency.Particles.Initialisers.Position;
//using Myre.Graphics.Translucency.Particles.Initialisers.Size;
//using Myre.Graphics.Translucency.Particles.Initialisers.Velocity;
//using Ninject;

//namespace GraphicsTests
//{
//    static class Fire
//    {
//        public static Entity Create(IKernel kernel, ContentManager content, Vector3 position)
//        {
//            var particleEntityDesc = kernel.Get<EntityDescription>();
//            particleEntityDesc.AddProperty<Vector3>("position");
//            particleEntityDesc.AddProperty<Vector3>("colour");
//            particleEntityDesc.AddProperty<float>("range");
//            particleEntityDesc.AddBehaviour<ParticleEmitter>();
//            particleEntityDesc.AddBehaviour<PointLight>();
            
//            var particleEntity = particleEntityDesc.Create();

//            particleEntity.GetProperty<Vector3>("position").Value = position;
//            particleEntity.GetProperty<Vector3>("colour").Value = Vector3.Normalize(new Vector3(5, 2, 2)) * 2;
//            particleEntity.GetProperty<float>("range").Value = 70f;

//            var emitter = particleEntity.GetBehaviour<ParticleEmitter>();

//            emitter.AddInitialiser(new Ellipsoid(new Vector3(2, 1, 2), 0));
//            emitter.AddInitialiser(new RandomVelocity(new Vector3(-0.5f, 0, -0.5f)));
//            emitter.AddInitialiser(new RandomAngularVelocity(-MathHelper.PiOver4, MathHelper.PiOver4));
//            emitter.AddInitialiser(new RandomSize(1, 10));
//            emitter.AddInitialiser(new RandomStartColour(Color.Red, Color.White));
//            emitter.AddInitialiser(new RandomEndColour(Color.White, Color.Blue));
//            emitter.AddInitialiser(new RandomLifetime(0.5f, 1f));

//            emitter.BlendState = BlendState.Additive;
//            emitter.Type = ParticleType.Soft;
//            emitter.Enabled = true;
//            emitter.EndLinearVelocity = 0.75f;
//            emitter.EndScale = 0.25f;
//            emitter.Gravity = new Vector3(0, 5, 0);
//            emitter.Lifetime = 10f;
//            emitter.Texture = content.Load<Texture2D>("fire");
//            emitter.EmitPerSecond = 100;
//            emitter.Capacity = (int)(emitter.Lifetime * emitter.EmitPerSecond + 1);
//            emitter.VelocityBleedThrough = 0;

//            return particleEntity;
//        }
//    }
//}
