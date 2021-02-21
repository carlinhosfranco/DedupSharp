using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Stages.Comum
{
    public static class SlidingWindow
    {
        public static IPropagatorBlock<T, T[]> CreateSlidingWindow<T>(int windowSize)
        {
            var queue = new Queue<T>();
            var source = new BufferBlock<T[]>();

            var target = new ActionBlock<T>(item =>
            {
                queue.Enqueue(item);

                if (queue.Count > windowSize)
                    queue.Dequeue();

                if (queue.Count == windowSize)
                    source.Post(queue.ToArray());
            });

            target.Completion.ContinueWith(delegate
            {
                if (queue.Count > 0 && queue.Count < windowSize)
                    source.Post(queue.ToArray());
                source.Complete();
            });

            return DataflowBlock.Encapsulate(target, source);
        }        
    }
}