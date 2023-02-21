
namespace Lithium.Serialization
{
    using System;
    using System.IO;

    internal partial class DataSchema
    {
        partial class ReferenceSchema : ObjectSchema
        {
            private class Collection : TypeSchema
            {
                public override object Deserialize(BinaryReader reader)
                {
                    throw new NotImplementedException();
                }

                public override void Serialize(BinaryWriter writer, object typeInstance)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
