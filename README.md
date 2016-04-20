Clone of the repo at https://cloudcopy.codeplex.com/ with some additional fixes.

Download, upload, delete and copy blobs from Azure Storage with this command line tool. You can use it to create cmd/bat scripts that requires to manage files in Azure Blob storage.

# Features
- Download containers, directories or blobs
- Upload files to containers, directories or specific blob
- Copy content between containers
- Delete containers, directories or blobs
- List the account, container or directory content
- Creates destination container if not exist when uploading/copying
- Creates destination folders if not exist when downloading
- Allows wildchars (* ?) to filter the set of files to download, upload or copy
- Parallel upload/download/copy
- Retry policy
- Omits download or upload of files that already exists (checks MD5 hash)
- Cancellation allowed (ESC key)

# Examples

Upload text files to storage emulator</br>
```CloudCopy.exe "c:\temp\*.txt" "http://127.0.0.1:10000/devstoreaccount1/mycontainer"``` 

Upload text files to a blob storage container</br>
```CloudCopy.exe "c:\temp\*.txt" "https://myaccount.blob.core.windows.net/mycontainer" "DefaultEndpointsProtocol=https;AccountName=user;AccountKey=key"``` 

Download text files from a blob storage container</br>
```CloudCopy.exe "c:\temp" "https://myaccount.blob.core.windows.net/mycontainer/*.txt" "DefaultEndpointsProtocol=https;AccountName=user;AccountKey=key"``` 

Copy text files from a blob storage container to another container</br>
```CloudCopy.exe "https://myaccount.blob.core.windows.net/mycontainer/*.txt" "https://myaccount.blob.core.windows.net/mycontainer2" "DefaultEndpointsProtocol=https;AccountName=user;AccountKey=key"``` 

List container content</br>
```CloudCopy.exe "https://myaccount.blob.core.windows.net/mycontainer" "DefaultEndpointsProtocol=https;AccountName=user;AccountKey=key" -L``` 

Remove content</br>
```CloudCopy.exe "https://myaccount.blob.core.windows.net/mycontainer/*.txt" "DefaultEndpointsProtocol=https;AccountName=user;AccountKey=key" -R```

# Build

Install the required packages in Visual Studio in the "Tools/Nuget Package Manager/Package Manager Console" menu:

https://www.nuget.org/packages/UnofficialAzure.StorageClient/: ```Install-Package UnofficialAzure.StorageClient```
