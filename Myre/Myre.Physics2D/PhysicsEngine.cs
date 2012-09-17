using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Services;

namespace Myre.Physics2D
{
    public interface IForceProvider
    {
        void Update(float elapsedTime);
    }

    public interface IForceApplier
    {
        void CalculateAccelerations();
    }

    public interface IIntegrator
    {
        void UpdateVelocity(float elapsedTime);
        void UpdatePosition(float elapsedTime);
    }

    public interface IActivityManager
    {
        void UpdateActivityStatus(float elapsedTime, float linearVelocityThreshold, float angularVelocityThreshold);
        void FreezeSleepingObjects();
    }

    public interface ICollisionResolver
    {
        void Update(float elapsedTime, float allowedPenetration, float biasFactor, int iterations);
    }

    public class PhysicsEngine
        : Service
    {
        private ReadOnlyCollection<IForceProvider> _forceProviders;
        private ReadOnlyCollection<IIntegrator> _integrators;
        private ReadOnlyCollection<IActivityManager> _activityManagers;
        private ReadOnlyCollection<ICollisionResolver> _collisionResolvers;
        private ReadOnlyCollection<IForceApplier> _forceAppliers;

        private Box<float> _allowedPenetration;
        private Box<float> _biasFactor;
        private Box<int> _iterations;
        private Box<float> _linearVelocitySleepThreshold;
        private Box<float> _angularVelocitySleepThreshold;

        public float AllowedPenetration
        {
            get { return _allowedPenetration.Value; }
            set { _allowedPenetration.Value = value; }
        }

        public float BiasFactor
        {
            get { return _biasFactor.Value; }
            set { _biasFactor.Value = value; }
        }

        public int Iterations 
        {
            get { return _iterations.Value; }
            set { _iterations.Value = value; }
        }

        public float LinearVelocitySleepThreshold
        {
            get { return _linearVelocitySleepThreshold.Value; }
            set { _linearVelocitySleepThreshold.Value = value; }
        }

        public float AngularVelocitySleepThreshold
        {
            get { return _angularVelocitySleepThreshold.Value; }
            set { _angularVelocitySleepThreshold.Value = value; }
        }

        public override void Initialise(Scene scene)
        {
            base.Initialise(scene);

            _forceProviders = scene.FindManagers<IForceProvider>();
            _integrators = scene.FindManagers<IIntegrator>();
            _activityManagers = scene.FindManagers<IActivityManager>();
            _collisionResolvers = scene.FindManagers<ICollisionResolver>();
            _forceAppliers = scene.FindManagers<IForceApplier>();

            _allowedPenetration = new Box<float>();
            _biasFactor = new Box<float>();
            _iterations = new Box<int>();
            _linearVelocitySleepThreshold = new Box<float>();
            _angularVelocitySleepThreshold = new Box<float>();

            AllowedPenetration = 1;
            BiasFactor = 0.2f;
            Iterations = 10;
            LinearVelocitySleepThreshold = 0.5f;
            AngularVelocitySleepThreshold = MathHelper.TwoPi * 0.025f;
        }

        public void BindSettings(Box<float> allowedPenetration, Box<float> biasFactor, Box<int> iterations, Box<float> linearThreshold, Box<float> angularThreshold)
        {
            Assert.ArgumentNotNull("allowedPenetration", allowedPenetration);
            Assert.ArgumentNotNull("biasFactor", biasFactor);
            Assert.ArgumentNotNull("iterations", iterations);
            Assert.ArgumentNotNull("linearThreshold", linearThreshold);
            Assert.ArgumentNotNull("angularThreshold", angularThreshold);

            _allowedPenetration = allowedPenetration;
            _biasFactor = biasFactor;
            _iterations = iterations;
            _linearVelocitySleepThreshold = linearThreshold;
            _angularVelocitySleepThreshold = angularThreshold;
        }

        public override void Update(float elapsedTime)
        {
            for (int i = 0; i < _forceProviders.Count; i++)
                _forceProviders[i].Update(elapsedTime);

            for (int i = 0; i < _forceAppliers.Count; i++)
                _forceAppliers[i].CalculateAccelerations();

            for (int i = 0; i < _integrators.Count; i++)
                _integrators[i].UpdateVelocity(elapsedTime);

            for (int i = 0; i < _activityManagers.Count; i++)
                _activityManagers[i].UpdateActivityStatus(elapsedTime, LinearVelocitySleepThreshold, AngularVelocitySleepThreshold);

            for (int i = 0; i < _collisionResolvers.Count; i++)
                _collisionResolvers[i].Update(elapsedTime, AllowedPenetration, BiasFactor, Iterations);

            for (int i = 0; i < _activityManagers.Count; i++)
                _activityManagers[i].FreezeSleepingObjects();

            for (int i = 0; i < _integrators.Count; i++)
                _integrators[i].UpdatePosition(elapsedTime);
            
            base.Update(elapsedTime);
        }
    }
}
