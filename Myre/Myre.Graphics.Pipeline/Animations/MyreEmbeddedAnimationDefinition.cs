namespace Myre.Graphics.Pipeline.Animations
{
    public class MyreEmbeddedAnimationDefinition
    {
// ReSharper disable UnassignedField.Global
        public string AnimationSourceFile;
        public string SourceTakeName;
        public float StartTime;
        public float EndTime;
        public double FramesPerSecond;
        public string RootBone;
        public bool FixLooping;
        public bool LinearKeyframeReduction;

        /* <Events>
              <Item>
                  <Time>1.0</Time>
                  <Type>TestEvent</Type>
                  <Data><![CDATA[
                    <Foo>1</Foo>
                    <Bar>2</Bar>
                  ]]></Data>
              </Item>
          </Events> 
         * ^ Marekup like this can generate the comment out event invocation array
         */
        //public TimelineEventInvocation[] Events;

// ReSharper restore UnassignedField.Global
    }

    //public class TimelineEventInvocation
    //{
    //    public float Time;

    //    public string Type;

    //    public string Data;
    //}
}
