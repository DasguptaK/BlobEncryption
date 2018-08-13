using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidiSoft.Pgp;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Azure;
using System.IO;

namespace TGBLDownloadDecrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            DownloadandDecrypt();
        }

        static void DownloadandDecrypt()
        {

            PGPLib pgp = new PGPLib();
            bool asciiArmor = false;
            bool withIntegrityCheck = false;
            string downloadedblobpath = @"C:\DATA FILES\Downloaded Files\";
            string decryptedblobpath = @"C:\DATA FILES\Decrypted Files\";
            string fileName = "";
            string privatekeypassword = "password@tgbl";
            string downloadedfile = System.IO.Path.Combine(downloadedblobpath, fileName);
            string decryptedfile = System.IO.Path.Combine(decryptedblobpath, fileName);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("encryptedtgblfiles");

            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;

                    // Retrieve reference to a blob named "photo1.jpg".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob.Name);

                    // Save blob contents to a file.
                    using (var fileStream = System.IO.File.OpenWrite(downloadedblobpath+blob.Name))
                    {
                        
                            blockBlob.DownloadToStream(fileStream);
                      
                    }
                }
            }

            if (System.IO.Directory.Exists(downloadedblobpath))
            {
                string[] files = System.IO.Directory.GetFiles(downloadedblobpath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {

                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(s);
                    
                    pgp.DecryptFile(downloadedblobpath + fileName,
                            @"C:\DATA FILES\privatekey.asc",
                            privatekeypassword,
                            decryptedblobpath + fileName);
                    // destFile = System.IO.Path.Combine(targetPath, fileName);
                    // System.IO.File.Copy(s, destFile, true);
                }
            }
        }
    }
}
