using System.IO;

namespace Lithium.Serialization
{
    internal abstract class TypeSchema
    {
        public abstract void Serialize(BinaryWriter writer, object typeInstance);
        public abstract object Deserialize(BinaryReader reader);
    }
}