﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorageExp.Models;
using AzureBlobStorageExp.Models.Interfaces;
using AzureBlobStorageExp.Options;
using AzureBlobStorageExp.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureBlobStorageExp.Services.Implementations
{
    public class AzureFileService : IFileService
    {
        private BlobServiceClient blobServiceClient;

        private readonly string azureStorageAccountConnectionString;
        private readonly string blobContainerName;
        private readonly string blobEndpoint;
        private readonly string storageAccountName;

        private readonly AzureOptions azureOptions;

        public AzureFileService(IOptionsSnapshot<AzureOptions> snapshot)
        {
            azureOptions = snapshot.Value;
            azureStorageAccountConnectionString = azureOptions.StorageAccount.ConnectionString;

            blobContainerName = azureOptions.StorageAccount.BlobContainerName;
            blobEndpoint = azureOptions.StorageAccount.BlobEndpoint;
            blobServiceClient = new BlobServiceClient(azureStorageAccountConnectionString);
            storageAccountName = azureOptions.StorageAccount.StorageAccountName;
        }

        public async Task<IEnumerable<AzureFile>> ListFiles()
        {
            var files = new List<AzureFile>();

            var blobContainerClient = await GetBlobContainerClient();
            var blobs = blobContainerClient.GetBlobsAsync();
            await foreach (var blob in blobs)
            {
                var file = new AzureFile(blob.Name);
                file.Uri = new Uri($"https://{storageAccountName}.{blobEndpoint}/{blobContainerName}/{blob.Name}");
                files.Add(file);
            }

            return files;
        }

        private async Task<BlobContainerClient> GetBlobContainerClient()
        {
            BlobContainerClient blobContainerClient = null;

            var blobContainers = blobServiceClient.GetBlobContainersAsync();
            await foreach (var blobContainer in blobContainers)
            {
                if (blobContainer.Name.Equals(blobContainerName, StringComparison.InvariantCultureIgnoreCase))
                {
                    blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainer.Name);
                }
            }

            if (blobContainerClient == null)
            {
                blobContainerClient = blobServiceClient.CreateBlobContainer(blobContainerName);
            }

            blobContainerClient.SetAccessPolicy(PublicAccessType.Blob);

            return blobContainerClient;
        }
    }
}
