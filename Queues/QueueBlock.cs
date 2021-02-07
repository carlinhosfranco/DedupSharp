using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Queues
{
    public class QueueBlock
    {
        public static void QueueDequeue<T>(int windowSize)
        {
            // The size of the window.
            int m_windowSize;
            // The target part of the block.
            ITargetBlock<T> m_target;
            // The source part of the block.
            IReceivableSourceBlock<T[]> m_source;

        }
        
    }
}