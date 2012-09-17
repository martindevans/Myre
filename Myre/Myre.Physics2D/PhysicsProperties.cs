using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Physics2D
{
    public static class PhysicsProperties
    {
        public const string POSITION = "position";
        public const string ROTATION = "rotation";

        public const string MASS = "mass";
        public const string INVERSE_MASS = "inverse_mass";

        public const string INERTIA_TENSOR = "inertia_tensor";

        public const string LINEAR_VELOCITY = "linear_velocity";
        public const string ANGULAR_VELOCITY = "angular_velocity";

        public const string TIME_MULTIPLIER = "time_multiplier";

        public const string LINEAR_ACCELERATION = "linear_acceleration";
        public const string ANGULAR_ACCELERATION = "angular_acceleration";

        public const string LINEAR_VELOCITY_BIAS = "linear_velocity_bias";
        public const string ANGULAR_VELOCITY_BIAS = "angular_velocity_bias";

        public const string SLEEPING = "sleeping";
    }
}
