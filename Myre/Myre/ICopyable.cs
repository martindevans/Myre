
namespace Myre
{
    /// <summary>
    /// An object which can create copies of itself.
    /// </summary>
    /// <remarks>This is a replacement for the ICloneable interface, which does not exist in silverlight.</remarks>
    public interface ICopyable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object Copy();
    }
}