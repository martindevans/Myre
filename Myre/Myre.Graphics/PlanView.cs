using System.Collections.Generic;

namespace Myre.Graphics
{
    public abstract class PlanView
        : View
    {
        private readonly Dictionary<Renderer, RenderPlan> _plans = new Dictionary<Renderer, RenderPlan>();

        protected abstract RenderPlan CreatePlan(Renderer renderer);

        private RenderPlan _previousPlan;

        public override void Begin(Renderer renderer)
        {
            base.Begin(renderer);

            //Get or create a plan for this renderer
            RenderPlan plan;
            if (!_plans.TryGetValue(renderer, out plan))
            {
                plan = CreatePlan(renderer);
                _plans[renderer] = plan;
            }

            //Cache plan which is already applied
            _previousPlan = renderer.Plan;

            //Apply new plan
            plan.Apply();
        }

        public override void End(Renderer renderer)
        {
            base.End(renderer);

            //Restore previous plan
            if (_previousPlan != null)
                _previousPlan.Apply();
            _previousPlan = null;
        }
    }
}
