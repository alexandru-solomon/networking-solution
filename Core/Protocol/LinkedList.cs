namespace Lithium.Protocol
{
    internal class LinkedList<T>
    {
        public Node<T>? First;
        public Node<T>? Last;

        public void AddFirst(T value)
        {
            Node<T> newNode = new Node<T>(value);

            if(First == null) // empty list
            {
                First = newNode;
                Last = newNode;
                return;
            }   

            newNode.Next = First;
            First.Previous = newNode;
            First = newNode;
        }
        public void AddLast(T value)
        {   
            Node<T> newNode = new Node<T>(value);
            if(Last == null)
            {
                First = newNode;
                Last = newNode;
                return;
            }

            newNode.Previous = Last;
            Last.Next = newNode;
            Last = newNode;

        }
        public void Remove(Node<T> node)
        {
            if(node == First && node == Last) // only node in list
            {
                First = null;
                Last = null;
                return;
            }
            if(node == First)
            {
                First = node.Next;
                First!.Previous = null;
                return;
            }
            if(node == Last)
            {
                Last = node.Previous;
                Last!.Next = null;
            }

            node.Previous!.Next = node.Next;
            node.Next!.Previous = node.Previous;
        }
    }
    internal class Node<T>
    {
        public Node(T value)
        {
            Value = value;
        }

        public Node<T>? Next;
        public Node<T>? Previous;
        public T Value { get; }
    }
}

