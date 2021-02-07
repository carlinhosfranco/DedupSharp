using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Queues
{
    public class StageProcess<T> : IPropagatorBlock<T, T[]>, IReceivableSourceBlock<T[]>
    {
        private readonly int WS = 10;
        private readonly ITargetBlock<T> _target;
        private readonly IReceivableSourceBlock<T[]> _source;
        
        public StageProcess()
        {
            var queue = new Queue<T>();
            var source = new BufferBlock<T[]>();

            var target = new ActionBlock<T>(item =>
            {
                queue.Enqueue(item);
                if (queue.Count > WS)
                    queue.Dequeue();

                if (queue.Count == WS)
                    source.Post(queue.ToArray());
            });

            target.Completion.ContinueWith(delegate
            {
                if (queue.Count > 0 && queue.Count < WS)
                    source.Post(queue.ToArray());

                source.Complete();
            });

            _target = target;
            _source = source;
        }
        #region IReceivableSourceBlock Interface

        public bool TryReceive(Predicate<T[]> filter, out T[] item)
        {
            return _source.TryReceive(filter, out item);
        }

        public bool TryReceiveAll(out IList<T[]> items)
        {
            return _source.TryReceiveAll(out items);
        }

        #endregion

        #region  ISourceBlock<T> Interface
        public IDisposable LinkTo(ITargetBlock<T[]> target, DataflowLinkOptions linkOptions)
        {
            return _source.LinkTo(target, linkOptions);
        }
        // public IDisposable LinkTo(TransformBlock<T,T[]> target, DataflowLinkOptions linkOptions)
        // {
        //     return _source.LinkTo(target, linkOptions);
        // }

        bool ISourceBlock<T[]>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
        {
            return _source.ReserveMessage(messageHeader, target);
        }

        T[] ISourceBlock<T[]>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target, out bool messageConsumed)
        {
            return _source.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        void ISourceBlock<T[]>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
        {
            _source.ReleaseReservation(messageHeader, target);
        }

        #endregion

        #region  ITargetBlock<T> Intergafe

        DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            return _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region IDataflowBlock members

        public Task Completion { get { return _source.Completion; } }

        public void Complete()
        {
            _target.Complete();
        }

        public void Fault(Exception error)
        {
            _target.Fault(error);
        }

        #endregion
    }
}