namespace Myre.Entities.Services
{
    /// <summary>
    /// Manages wallclock time passed and counts the number of frames passed
    /// </summary>
    public class TimeService
        : Service
    {
        /// <summary>
        /// The number of frames which have passed since the scene was constucted or the time was last reset
        /// </summary>
        public uint Tick
        {
            get;
            private set;
        }

        public TimeService()
        {
            UpdateOrder = int.MaxValue;
        }

        public override void Update(float elapsedTime)
        {
            //Either we overflow, or the game crashes. Either way it doesn't matter because with a 16ms frame time this occurs after 2.1 YEARS of gameplay!
            unchecked { Tick++; }

            base.Update(elapsedTime);
        }

        public void Reset()
        {
            Tick = 0;
        }
    }
}
