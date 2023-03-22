using System.Collections.Generic;

namespace Lithium
{
    internal class IdentifierPool
    {
        private int NextIdentifier = 0;
        private readonly Queue<int> AvailableIdentifiers = new Queue<int>();

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
