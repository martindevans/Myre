//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Myre.Graphics
//{
//    public abstract class PlanView
//        : View
//    {
//        protected override void Initialised()
//        {
//            base.Initialised();

//            CreatePlan(new RenderPlan(Owner.Scene.Kernel, Owner.Scene.GetService<Renderer>()));
//        }

//        protected abstract void CreatePlan();

//        public override void Begin(Renderer renderer)
//        {
//            base.Begin(renderer);
//        }

//        public override void End(Renderer renderer)
//        {
//            base.End(renderer);
//        }
//    }
//}
