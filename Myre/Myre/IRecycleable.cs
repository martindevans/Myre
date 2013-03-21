
namespace Myre
{
    /// <summary>
    /// An object which can have its instances recycled, or reset.
    /// </summary>
    public interface IRecycleable
    {
        /// <summary>
        /// Prepares this instance for re-use.
        /// </summary>
        void Recycle();
    }
}
