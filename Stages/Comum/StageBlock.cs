using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Stages.Comum
{
    public class StageBlock<TItem> : IPropagatorBlock<TItem, TItem[]>, IReceivableSourceBlock<TItem[]>
    {
        private const int WindowSizeInternal = 10;
        private readonly int m_windowSize;
        private readonly ITargetBlock<TItem> m_target;
        private readonly IReceivableSourceBlock<TItem[]> m_source;

        public StageBlock()
        {
            //Internal Queue
            var queue = new Queue<TItem>();

            // The source part of the propagator holds arrays of size windowSize
            // and propagates data out to any connected targets.
            var source = new BufferBlock<TItem[]>();

            // The target part receives data and adds them to the queue.
            var target = new ActionBlock<TItem>(item =>
            {
                // Add the item to the queue.
                queue.Enqueue(item);
                // Remove the oldest item when the queue size exceeds the window size.
                if (queue.Count > WindowSizeInternal)
                queue.Dequeue();
                // Post the data in the queue to the source block when the queue size
                // equals the window size.
                if (queue.Count == WindowSizeInternal)
                source.Post(queue.ToArray());
            });

            // When the target is set to the completed state, propagate out any
            // remaining data and set the source to the completed state.
            target.Completion.ContinueWith(delegate
            {
                if (queue.Count > 0 && queue.Count < WindowSizeInternal)
                source.Post(queue.ToArray());
                source.Complete();
            });

            m_windowSize = WindowSizeInternal;
            m_target = target;
            m_source = source;
        }
        public int WindowSize { get { return m_windowSize; } }

        public Task Completion { get { return m_source.Completion; } }

        
        #region IReceivableSourceBlock<TOutput> members
        public bool TryReceive(Predicate<TItem[]> filter, out TItem[] item) 
            => m_source.TryReceive(filter, out item);
        
        public bool TryReceiveAll(out IList<TItem[]> items)
            => m_source.TryReceiveAll(out items);
        
        #endregion

        #region IDataflowBlock members
        public void Complete()
            => m_target.Complete();

        public void Fault(Exception exception) 
            => m_target.Fault(exception);
        
        #endregion

        #region ISourceBlock<TOutput> members
        public IDisposable LinkTo(ITargetBlock<TItem[]> target, DataflowLinkOptions linkOptions)
            => m_source.LinkTo(target, linkOptions);

        TItem[] ISourceBlock<TItem[]>.ConsumeMessage(DataflowMessageHeader messageHeader, 
            ITargetBlock<TItem[]> target,
            out bool messageConsumed)
                => m_source.ConsumeMessage(messageHeader, target, out messageConsumed);

        void ISourceBlock<TItem[]>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TItem[]> target)
                => m_source.ReleaseReservation(messageHeader, target);

        bool ISourceBlock<TItem[]>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TItem[]> target)   
            => m_source.ReserveMessage(messageHeader, target);
   

        #endregion

        #region ITargetBlock<TInput> members
        DataflowMessageStatus ITargetBlock<TItem>.OfferMessage(DataflowMessageHeader messageHeader, 
            TItem messageValue, 
            ISourceBlock<TItem> source, 
            bool consumeToAccept)
                => m_target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        #endregion
    }
}