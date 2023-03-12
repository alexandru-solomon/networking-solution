using System.Collections.Generic;

namespace Lithium.Transport.Net
{
    internal class IdentifierPool
    {
        int NextIdentifier = 0;
        Queue<int> AvailableIdentifiers = new Queue<int>();

        public int Rent()
        {
            if(AvailableIdentifiers.Count == 0) return NextIdentifier++;

            return AvailableIdentifiers.Dequeue();
        }
        public void Return(int identifier) 
        {
            AvailableIdentifiers.Enqueue(identifier);
        }
    }
}
