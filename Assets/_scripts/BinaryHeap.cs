using System;
using System.Collections.Generic;

public class BinaryHeap<T> where T : class
{
    private class Node
    {
        public T Item;
        public float Priority;
        public int Index; 
    }

    private List<Node> heap = new();
    private Dictionary<T , Node> itemToNode = new(); 

    public int Count => heap.Count;

    public void Enqueue(T item , float priority)
    {
        if (itemToNode.ContainsKey(item))
        {
            // If already in, update
            UpdatePriority(item , priority); 
            return;
        }

        Node node = new() { Item = item , Priority = priority , Index = heap.Count };
        heap.Add(node);
        itemToNode[item] = node;
        HeapifyUp(node.Index);
    }

    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("Heap is empty");

        Node minNode = heap[0];
        Node lastNode = heap[Count - 1];
        heap[0] = lastNode;
        lastNode.Index = 0;

        heap.RemoveAt(Count - 1);
        itemToNode.Remove(minNode.Item);

        if (Count > 0)
            HeapifyDown(0);

        return minNode.Item;
    }

    public bool Contains(T item) => itemToNode.ContainsKey(item);

    public void UpdatePriority(T item , float newPriority)
    {
        if (!itemToNode.TryGetValue(item , out Node node)) return;

        float oldPriority = node.Priority;
        node.Priority = newPriority;

        if (newPriority < oldPriority)
            HeapifyUp(node.Index);
        else
            HeapifyDown(node.Index);
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (heap[index].Priority >= heap[parentIndex].Priority) break;

            Swap(index , parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int size = Count;
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left < size && heap[left].Priority < heap[smallest].Priority)
                smallest = left;
            if (right < size && heap[right].Priority < heap[smallest].Priority)
                smallest = right;

            if (smallest == index) break;

            Swap(index , smallest);
            index = smallest;
        }
    }

    private void Swap(int i , int j)
    {
        Node temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;

        heap[i].Index = i;
        heap[j].Index = j;
    }
}