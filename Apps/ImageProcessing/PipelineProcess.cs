using DedupSharp.Apps.ImageProcessing.Utilities;
using DedupSharp.Helpers.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DedupSharp.Apps.ImageProcessing
{
    public class PipelineProcess
    {
        const int QueueBoundedCapacity = 4;
        const int LoadBalancingDegreeOfConcurrency = 2;
        const int MaxNumberOfImages = 500;
        const double GaussianNoiseAmount = 50.0;

        #region Image Pipeline Top Level Loop

        /// <summary>
        /// Runs the image pipeline example. The program goes through the jpg images located in the SourceDir
        /// directory and performs a series of steps: it resizes each image and adds a black border and then applies
        /// a Gaussian noise filter operation to give the image a grainy effect. Finally, the program invokes
        /// a user-provided delegate to the image (for example, to display the image on the user interface).
        ///
        /// Images are processed in sequential order. That is, the display delegate will be invoked in exactly the same
        /// order as the images appear in the file system.
        /// </summary>
        /// <param name="displayFn">A delegate that is invoked for each image at the end of the pipeline, for example, to
        /// display the image in the user interface.</param>
        /// <param name="token">A token that can signal an external cancellation request.</param>
        /// <param name="algorithmChoice">The method of calculation. 0=sequential, 1=pipeline, 2=load balanced pipeline</param>
        /// <param name="errorFn">A delegate that will be invoked if this method or any of its parallel subtasks observe an exception during their execution.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void ImagePipelineMainLoop(Action<Image> displayFn, CancellationToken token, int algorithmChoice, Action<Exception> errorFn)
        {
            try
            {
                string sourceDir = Directory.GetCurrentDirectory();

                // Ensure that frames are presented in sequence before invoking the user-provided display function.
                int imagesSoFar = 0;
                Action<Image> safeDisplayFn = info =>
                    {
                        if (info.SequenceNumber != imagesSoFar)
                            throw new InvalidOperationException("Images processed out of order. Saw " + info.SequenceNumber.ToString() + " , expected " + imagesSoFar);

                        displayFn(info);
                        imagesSoFar += 1;
                    };

                // Create a cancellation handle for inter-task signaling of exceptions. This cancellation
                // handle is also triggered by the incoming token that indicates user-requested
                // cancellation.
                using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    IEnumerable<string> fileNames = BasicUtilities.GetImageFilenames(sourceDir, MaxNumberOfImages);
                    switch (algorithmChoice)
                    {
                        case 0:
                            RunSequential(fileNames, sourceDir, safeDisplayFn, cts);
                            break;
                        case 1:
                            RunPipelined(fileNames, sourceDir, QueueBoundedCapacity, safeDisplayFn, cts);
                            break;
                        case 2:
                            Console.WriteLine("Método não implementado");
                            //RunLoadBalancedPipeline(fileNames, sourceDir, QueueBoundedCapacity, safeDisplayFn, cts, LoadBalancingDegreeOfConcurrency);
                            break;
                        default:
							throw new InvalidOperationException("Invalid algorithm choice.");
                    }
                }
            }
            catch (AggregateException ae)
            {
                errorFn((ae.InnerExceptions.Count == 1) ? ae.InnerExceptions[0] : ae);
            }
            catch (Exception e)
            {
                errorFn(e);
            }
        }

        #endregion

        #region Variations (Sequential and Pipelined)

        /// <summary>
        /// Run the image processing pipeline.
        /// </summary>
        /// <param name="fileNames">List of image file names in source directory</param>
        /// <param name="sourceDir">Name of directory of source images</param>
        /// <param name="displayFn">Display action</param>
        /// <param name="cts">Cancellation token</param>
        static void RunSequential(IEnumerable<string> fileNames, string sourceDir, Action<Image> displayFn, CancellationTokenSource cts)
        {
            int count = 0;
            int clockOffset = Environment.TickCount;
            int duration = 0;
            var token = cts.Token;
            Image info = null;
            try
            {
                foreach (var fileName in fileNames)
                {
                    if (token.IsCancellationRequested)
                        break;

                    info = ImageOperations.LoadImage(fileName, sourceDir, count, clockOffset);
                    ImageOperations.ScaleImage(info);
                    ImageOperations.FilterImage(info);
                    int displayStart = Environment.TickCount;
                    ImageOperations.DisplayImage(info, count + 1, displayFn, duration);
                    duration = Environment.TickCount - displayStart;

                    count += 1;
                    info = null;
                }
            }
            finally
            {
                if (info != null) info.Dispose();
            }
        }

        /// <summary>
        /// Run the image processing pipeline.
        /// </summary>
        /// <param name="fileNames">List of image file names in source directory</param>
        /// <param name="sourceDir">Name of directory of source images</param>
        /// <param name="queueLength">Length of image queue</param>
        /// <param name="displayFn">Display action</param>
        /// <param name="cts">Cancellation token</param>
        public static void RunPipelined(IEnumerable<string> fileNames, string sourceDir, int queueLength, Action<Image> displayFn, CancellationTokenSource cts)
        {
            // Data pipes
            var originalImages = new BlockingCollection<Image>(queueLength);
            var thumbnailImages = new BlockingCollection<Image>(queueLength);
            var filteredImages = new BlockingCollection<Image>(queueLength);
            try
            {
                var f = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
                Action<Image> updateStatisticsFn = info =>
                {
                    info.QueueCount1 = originalImages.Count();
                    info.QueueCount2 = thumbnailImages.Count();
                    info.QueueCount3 = filteredImages.Count();
                };

                // Start pipelined tasks
                var loadTask = f.StartNew(() => ImageProcessingStages.LoadPipelinedImages(fileNames, sourceDir, originalImages, cts));

                var scaleTask = f.StartNew(() => ImageProcessingStages.ScalePipelinedImages(originalImages, thumbnailImages, cts));

                var filterTask = f.StartNew(() => ImageProcessingStages.FilterPipelinedImages(thumbnailImages, filteredImages, cts));

                var displayTask = f.StartNew(() => ImageProcessingStages.DisplayPipelinedImages(filteredImages.GetConsumingEnumerable(), displayFn, updateStatisticsFn, cts));

                Task.WaitAll(loadTask, scaleTask, filterTask, displayTask);
            }
            finally
            {
                // in case of exception or cancellation, there might be bitmaps
                // that need to be disposed.
                ImageProcessingStages.DisposeImagesInQueue(originalImages);
                ImageProcessingStages.DisposeImagesInQueue(thumbnailImages);
                ImageProcessingStages.DisposeImagesInQueue(filteredImages);
            }
        }

        // /// <summary>
        // /// Run a variation of the pipeline that uses a user-specified number of tasks for the filter stage.
        // /// </summary>
        // /// <param name="fileNames">List of image file names in source directory</param>
        // /// <param name="sourceDir">Name of directory of source images</param>
        // /// <param name="queueLength">Length of image queue</param>
        // /// <param name="displayFn">Display action</param>
        // /// <param name="cts">Cancellation token</param>
        // /// <param name="filterTaskCount">Number of filter tasks</param>
        // static void RunLoadBalancedPipeline(IEnumerable<string> fileNames, string sourceDir, int queueLength, Action<Image> displayFn,
        //     CancellationTokenSource cts, int filterTaskCount)
        // {
        //     // Create data pipes
        //     var originalImages = new BlockingCollection<Image>(queueLength);
        //     var thumbnailImages = new BlockingCollection<Image>(queueLength);
        //     var filteredImageMultiplexer = new BlockingMultiplexer<Image>(info => info.SequenceNumber, 0, queueLength);
        //     var filteredImagesCollections = (BlockingCollection<Image>[])Array.CreateInstance(
        //                            typeof(BlockingCollection<Image>), filterTaskCount);

        //     try
        //     {
        //         // Start pipelined tasks
        //         Action<Image> updateStatisticsFn = info =>
        //         {
        //             info.QueueCount1 = originalImages.Count();
        //             info.QueueCount2 = thumbnailImages.Count();
        //             info.QueueCount3 = filteredImageMultiplexer.Count;
        //         };
        //         const TaskCreationOptions options = TaskCreationOptions.LongRunning;
        //         var f = new TaskFactory(CancellationToken.None, options, TaskContinuationOptions.None, TaskScheduler.Default);
        //         Task[] tasks = (Task[])Array.CreateInstance(typeof(Task), filterTaskCount + 3);
        //         int taskId = 0;

        //         tasks[taskId++] = f.StartNew(() =>
        //               LoadPipelinedImages(fileNames, sourceDir, originalImages, cts));

        //         tasks[taskId++] = f.StartNew(() =>
        //               ScalePipelinedImages(originalImages, thumbnailImages, cts));

        //         for (int i = 0; i < filterTaskCount; i++)
        //         {
        //             var tmp = i;
        //             filteredImagesCollections[tmp] = filteredImageMultiplexer.GetProducerQueue();
        //             tasks[taskId++] = f.StartNew(() => FilterPipelinedImages(thumbnailImages, filteredImagesCollections[tmp], cts));
        //         }

        //         tasks[taskId++] = f.StartNew(() =>
        //               DisplayPipelinedImages(filteredImageMultiplexer.GetConsumingEnumerable(), displayFn,
        //                                      updateStatisticsFn, cts));

        //         Task.WaitAll(tasks);
        //     }
        //     finally
        //     {
        //         // there might be cleanup in the case of cancellation or an exception.
        //         DisposeImagesInQueue(originalImages);
        //         DisposeImagesInQueue(thumbnailImages);
        //         foreach (var filteredImages in filteredImagesCollections)
        //             DisposeImagesInQueue(filteredImages);
        //         foreach (var info in filteredImageMultiplexer.GetCleanupEnumerable())
        //             info.Dispose();
        //     }
        // }

        #endregion

    }
}