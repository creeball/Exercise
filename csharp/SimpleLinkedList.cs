using System.Collections;
using System.Runtime.InteropServices;

public unsafe struct SimpleLinkedList<T> : IEnumerable<T>, IDisposable
    where T : unmanaged
{
    public readonly struct Node(T value, Node* next = null)
    {
        public T Value { get; } = value;
        public Node* Next { get; } = next;
    }

    public Node* Head { get; private set; }
    public int Count { get; private set; }
    public SimpleLinkedList() { }
    public SimpleLinkedList(IEnumerable<T> values)
    {
        foreach (var value in values) Push(value);
    }
    public void Push(T value)
    {
        var node = (Node*)NativeMemory.Alloc((nuint)sizeof(Node));
        *node = new Node(value, Head);
        Head = node;
        Count++;
    }

    public T Pop()
    {
        if (Head == null) throw new InvalidOperationException();
        var node = Head;
        Head = node->Next;
        Count--;
        var value = node->Value;
        NativeMemory.Free(node);
        return value;
    }

    public T Peek() => Head != null ? Head->Value : throw new InvalidOperationException();

    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            index = Count - 1 - index;
            var node = Head;
            for (var i = 0; i < index; i++) node = node->Next;
            return node->Value;
        }
    }

    public void Dispose()
    {
        var node = Head;
        while (node != null)
        {
            var next = node->Next;
            NativeMemory.Free(node);
            node = next;
        }

        Head = null;
        Count = 0;
    }

    public Enumerator GetEnumerator() => new(Head);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator(Node* head) : IEnumerator<T>
    {
        private readonly Node* _head = head;
        private Node* _next = head;
        private Node* _current = null;

        public readonly T Current => _current != null
            ? _current->Value
            : throw new InvalidOperationException();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_next == null)
            {
                _current = null;
                return false;
            }

            _current = _next;
            _next = _next->Next;
            return true;
        }

        public void Reset()
        {
            _next = _head;
            _current = null;
        }

        public readonly void Dispose() { }
    }
}