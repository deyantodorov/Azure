using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Common;

using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace AzureBatchServices
{
    public class Program
    {
        private const string PoolId = "DotNetQuickstartPool";
        private const string JobId = "DotNetQuickstartJob";
        private const int PoolNodeCount = 2;
        private const string PoolVMSize = "STANDARD_A1_v2";

        internal static async Task Main()
        {
            var userSecrets = new UserSecrets();

            string batchAccountName = userSecrets.Configuration[MySecrets.BatchAccountName];
            string batchAccountKey = userSecrets.Configuration[MySecrets.BatchAccountKey];
            string batchAccountUrl = userSecrets.Configuration[MySecrets.BatchAccountUrl];

            string storageAccountName = userSecrets.Configuration[MySecrets.StorageAccountName];
            string storageAccountKey = userSecrets.Configuration[MySecrets.StorageAccountKey];

            if (string.IsNullOrEmpty(batchAccountName) ||
                string.IsNullOrEmpty(batchAccountKey) ||
                string.IsNullOrEmpty(batchAccountUrl) ||
                string.IsNullOrEmpty(storageAccountName) ||
                string.IsNullOrEmpty(storageAccountKey))
            {
                throw new InvalidOperationException("One or more account credential strings have not been populated in Common project");
            }

            try
            {
                Console.WriteLine($"Sample start: {DateTime.UtcNow}");

                var timer = new Stopwatch();
                timer.Start();

                var blobClient = CreateCloudBlobClient(storageAccountName, storageAccountKey);

                var inputContainerName = "input";

                var container = blobClient.GetContainerReference(inputContainerName);

                await container.CreateIfNotExistsAsync();

                var filePaths = new List<string>()
                {
                    "taskdata0.txt",
                    "taskdata1.txt",
                    "taskdata2.txt"
                };

                var inputFiles = new List<ResourceFile>();

                foreach (var filePath in filePaths)
                {
                    inputFiles.Add(await UploadFileToContainerAsync(blobClient, inputContainerName, filePath));
                }

                var credentials = new BatchSharedKeyCredentials(batchAccountUrl, batchAccountName, batchAccountKey);

                using (var batchClient = BatchClient.Open(credentials))
                {
                    Console.WriteLine($"Creating pool {PoolId}");

                    var imageReference = CreateImageReference();

                    var vmConfiguration = CreateVirtualMachineConfiguration(imageReference);

                    CreateBatchPool(batchClient, vmConfiguration);

                    Console.WriteLine($"Creating job {JobId}");

                    try
                    {
                        var job = batchClient.JobOperations.CreateJob();

                        job.Id = JobId;
                        job.PoolInformation = new PoolInformation { PoolId = PoolId };

                        await job.CommitAsync();
                    }
                    catch (BatchException be)
                    {
                        // Accept the specific error code JobExists as that is expected if the job already exists
                        if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                        {
                            Console.WriteLine("The job {0} already existed when we tried to create it", JobId);
                        }
                        else
                        {
                            throw; // Any other exception is unexpected
                        }
                    }

                    Console.WriteLine($"Adding {inputFiles.Count} tasks to job {JobId}...");

                    var tasks = new List<CloudTask>();

                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        var taskId = $"Task {i}";
                        var inputFileName = inputFiles[i].FilePath;
                        var taskCommandLine = $"cmd /c type {inputFileName}";

                        var task = new CloudTask(taskId, taskCommandLine);

                        task.ResourceFiles = new List<ResourceFile>() { inputFiles[i] };

                        tasks.Add(task);
                    }

                    await batchClient.JobOperations.AddTaskAsync(JobId, tasks);

                    // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.
                    var timeout = TimeSpan.FromMinutes(30);
                    Console.WriteLine($"Monitoring all tasks for 'Completed' state, timeout {timeout}...");

                    var addedTasks = batchClient.JobOperations.ListTasks(JobId);

                    batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                    Console.WriteLine("All tasks reached state completed.");

                    Console.WriteLine("Printing task output...");

                    var completedTasks = batchClient.JobOperations.ListTasks(JobId);

                    foreach (var completedTask in completedTasks)
                    {
                        var nodeId = $"{completedTask.ComputeNodeInformation.ComputeNodeId}";

                        Console.WriteLine("Task: {0}", completedTask.Id);
                        Console.WriteLine("Node: {0}", nodeId);
                        Console.WriteLine("Standard out:");
                        Console.WriteLine(completedTask.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                    }

                    timer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Sample end: {0}", DateTime.Now);
                    Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                    container.DeleteIfExistsAsync().Wait();
                    Console.WriteLine("Container [{0}] deleted.", inputContainerName);

                    // Clean up Batch resources (if the user so chooses)
                    Console.Write("Delete job? [yes] no: ");
                    var response = Console.ReadLine()?.ToLower();

                    if (response != "n" && response != "no")
                    {
                        batchClient.JobOperations.DeleteJob(JobId);
                    }

                    Console.Write("Delete pool? [yes] no: ");
                    response = Console.ReadLine()?.ToLower();
                    
                    if (response != "n" && response != "no")
                    {
                        batchClient.PoolOperations.DeletePool(PoolId);
                    }
                }
            }
            finally
            {
                Console.WriteLine("Complete, hit ENTER to exit...");
            }
        }

        /// <summary>
        /// Creates a blob client
        /// </summary>
        /// <param name="storageAccountName">The name of the Storage Account</param>
        /// <param name="storageAccountKey">The key of the Storage Account</param>
        private static CloudBlobClient CreateCloudBlobClient(string storageAccountName, string storageAccountKey)
        {
            var storageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}";

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient;
        }

        /// <summary>
        /// Uploads the specified file to the specified Blob container.
        /// </summary>
        /// <param name="blobClient">A <see cref="CloudBlobClient"/>.</param>
        /// <param name="containerName">The name of the blob storage container to which the file should be uploaded.</param>
        /// <param name="filePath">The full path to the file to upload to Storage.</param>
        /// <returns>A ResourceFile instance representing the file within blob storage.</returns>
        private static async Task<ResourceFile> UploadFileToContainerAsync(CloudBlobClient blobClient, string containerName, string filePath)
        {
            Console.WriteLine($"Uploading file {filePath} to container {containerName}...");

            var blobName = Path.GetFileName(filePath);

            filePath = Path.Combine(Environment.CurrentDirectory, filePath);

            var container = blobClient.GetContainerReference(containerName);
            var blobData = container.GetBlockBlobReference(blobName);

            await blobData.UploadFromFileAsync(filePath);

            var sasConstraints = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(2),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
            var blobSasUri = $"{blobData.Uri}{sasBlobToken}";

            return ResourceFile.FromUrl(blobSasUri, filePath);
        }

        private static ImageReference CreateImageReference()
        {
            return new ImageReference(publisher: "MicrosoftWindowsServer", offer: "WindowsServer", sku: "2016-datacenter-smalldisk", version: "latest");
        }

        private static VirtualMachineConfiguration CreateVirtualMachineConfiguration(ImageReference imageReference)
        {
            return new VirtualMachineConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.windows amd64");
        }

        private static void CreateBatchPool(BatchClient batchClient, VirtualMachineConfiguration vmConfiguration)
        {
            try
            {
                var pool = batchClient.PoolOperations.CreatePool(poolId: PoolId, targetDedicatedComputeNodes: PoolNodeCount, virtualMachineSize: PoolVMSize, virtualMachineConfiguration: vmConfiguration);

                pool.Commit();
            }
            catch (BatchException be)
            {
                // Accept the specific error code PoolExists as that is expected if the pool already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine("The pool {0} already existed when we tried to create it", PoolId);
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }
        }
    }
}
