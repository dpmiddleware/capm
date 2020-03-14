


# Context-aware Preservation Manager Error Handling POC

> A proof-of-concept implementation of the error handling functionality for the Context-aware Preservation Manager (CaPM)

![CaPM GIF - showing error handling flow](/docs/imgs/ingest-with-error-handling.gif)

## Table of Contents

- [Getting Started - Running Locally](#getting-started---running-locally)
    - [Single Process In-Memory Storage](#single-process-in-memory-storage)
    - [Persistent Storage](#persistent-storage)
    - [Distributed Processing](#distributed-processing)
- [Supported Providers](#supported-providers)
    - [Staging Store](#staging-store)
    - [Messaging](#messaging)
    - [Digital Preservation System - AIP Store](#digital-preservation-system---aip-store)
- [License](#license)

---

## Getting Started - Running Locally

There are a few different ways to run the application locally. Either in a single-process fashion, where all Components (e.g. Archiver, CaPM and Collector) are run in the same process, or in a distributed fashion where there is one process for each component.

---

### Single Process In-Memory Storage

Running the application using an in-memory storage is a convenient and quick way to get the application up and running with just three steps.

> Clone the repository

> Run the WebRunner project without any parameters

```
cd ./src/WebRunner
dotnet run
```

> Open your browser and navigate to `https://localhost:5001/`

---

### Persistent Storage

There are a few different storage providers implemented (read more about providers in the section [Supported Providers](#supported_providers)), but there is only one that is persistent and that is implemented for both Staging Store, Messaging and AIP Store and that is the Azure Storage provider.

The Azure Storage provider can be easily run locally if you have the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) installed. 

To run the application using the Azure Storage Emulator, use the following steps:

> Clone the repository

> Start the Azure Storage Emulator

> Run the WebRunner project with the `--use-storage-emulator` flag

```
cd ./src/WebRunner
dotnet run -- --use-storage-emulator
```

> Open your browser and navigate to `https://localhost:5001/`

If you prefer to use an Azure hosted Storage account, you can do so by replacing the `--use-storage-emulator` flag with instead setting the connection string similar to the following:

```
cd ./src/WebRunner
dotnet run -- --AzureBlobStorageConnectionString <connection string here>
```

---

### Distributed Processing

If you use a different messaging provider than the in-memory messaging provider, for example the Azure Storage Queues messaging provider, the application can also be run in a distributed fashion. 

> Clone the repository

> Start the Azure Storage Emulator

> Run the WebRunner project with the `--use-storage-emulator` flag and the `--use-external-component-runners` flag

```
cd ./src/WebRunner
dotnet run -- --use-storage-emulator --use-external-component-runners
```

> Start a new terminal window for each of the three implemented components and run the following commands, one in each window.

```
cd ./src/ComponentRunners/ArchiverComponentRunner
dotnet run
```

```
cd ./src/ComponentRunners/CollectorComponentRunner
dotnet run
```

```
cd ./src/ComponentRunners/RandomErrorComponentRunner
dotnet run
```

> Open your browser and navigate to `https://localhost:5001/`

When triggering an ingest process you should now be able to see each process handle the work of that respective component.

---

## Supported Providers

There are a few different providers implemented in this POC. The following lists which implementations exist today:

### Staging Store

- In-memory  

  The in-memory staging store provider only stores data in memory. This data is lost if the process is restarted. This provider is useful for development and quick-starts.

- Filesystem  

  The filesystem staging store provider creates a folder structure on disk to store staging data.

- Azure Storage Blobs  

  The Azure storage blobs staging store provider creates a blob container for each ingest process and a blob for each event. 
  
  Retrieving a list of all ingest processes which have been started, along with all their events, is very inefficient in the current implementation, but it is good enough for this POC.

### Messaging 

- In-memory  

  Sends all messages between components in-memory within the process.
  
  This messaging provider only supports running all components inside the same process and does not support running the components in a distributed fashion.

- Azure Storage Queues  

  When this messaging provider is configured, all messages are sent using Azure storage queues.

  The Azure storage queues messaging provider provides the capability for running the components in a distributed fashion.

### Digital Preservation System - AIP Store

- In-memory  

  All AIPs are only stored in memory and will be lost when the process is restarted.

- Azure Storage Blobs  

  All AIPs are stored as Azure storage blobs.

---

## License

- [MIT license](LICENSE.md)