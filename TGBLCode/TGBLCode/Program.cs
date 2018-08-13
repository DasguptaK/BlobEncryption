using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidiSoft.Pgp;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.WindowsAzure.Storage.Table; //Namespace for Table Storage
using Microsoft.Azure;
using System.IO;
using System.Data.SqlClient;

namespace TGBLCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // EncryptAllRawFiles();
            RawFileMovetoEncrypt();
            datacheck();
            Console.ReadLine();


        }

        static void EncryptAllRawFiles()
        {


            // specify should the output be ASCII or binary

            // should additional integrity information be added
            // set to false for compatibility with older versions of PGP such as 6.5.8.




        }

        static void RawFileMovetoEncrypt()
        {
            PGPLib pgp = new PGPLib();
            bool asciiArmor = false;
            bool withIntegrityCheck = false;
            string sourcePath = @"C:\DATA FILES\Raw Data Files\";
            string targetPath = @"C:\DATA FILES\Encrypted Data Files\";
            string fileName = "";
            int fileSize;
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);
            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {

                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(s);
                    pgp.EncryptFile(sourcePath + fileName,
                            @"C:\DATA FILES\publickey.asc",
                            targetPath + fileName,
                            asciiArmor,
                            withIntegrityCheck);
                    // destFile = System.IO.Path.Combine(targetPath, fileName);
                    // System.IO.File.Copy(s, destFile, true);
                }
            }


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //encryptedtgblfiles
            CloudBlobContainer container = blobClient.GetContainerReference("encryptedtgblfiles");
            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    fileName = System.IO.Path.GetFileName(s);
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                    fileSize = fileName.Length;
                    // Create or overwrite the "myblob" blob with contents from a local file.
                    using (var fileStream = System.IO.File.OpenRead(targetPath + fileName))
                    {
                        blockBlob.UploadFromStream(fileStream);
                    }

                    // To move a file or folder to a new location:
                    //  logger(fileName, "Uploaded",fileSize);

                    // To move an entire directory. To programmatically modify or combine
                    // path strings, use the System.IO.Path class.
                    //System.IO.Directory.Move(@"C:\Users\Public\public\test\", @"C:\Users\Public\private");
                }

            }
        }



        static void datacheck()
        {
            string sourcePath = @"C:\DATA FILES\Raw Data Files\";
            string[] allfiles = new string[10];
            int i = 0;
            string fileName = "";
            //code to check if files were updated sccessfully.
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "tgbldev.database.windows.net";
                builder.UserID = "dbadmin";
                builder.Password = "Password@1234567";
                builder.InitialCatalog = "tgbldevdata";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");
                    Console.WriteLine("FileName");
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT dcp.Filename as FNAME   ");
                    sb.Append("FROM [dbo].[DataCheckPoint] dcp");                    
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allfiles[i] = reader.GetString(0);
                                i = i + 1;
                                                         
                            }
                        }
                    }
                }



                if (System.IO.Directory.Exists(sourcePath))
                {
                    string path = @"C:\DATA FILES\Raw Data Files\";
                    string[] files = new string[100];

                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] all_files = directory.GetFiles("*.txt");
                    int filecount = 0;
                    foreach (FileInfo f in all_files)
                    {
                        string filenameWithoutPath = Path.GetFileName(f.FullName);
                        files[filecount] = filenameWithoutPath.GetUntilOrEmpty();
                        for (int t = 0; t < i; t++)
                        {
                            if (files[filecount] == allfiles[t])
                            {

                                System.IO.File.Delete(sourcePath + fileName);
                            }
                        }
                        filecount++;
                    }

                   

                }
                    
                
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

        }


        static void logger(string filename, string operation,int filesize)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("logEncryptandUpload");

            // Create a new customer entity.
            LogEntity loggerdata = new LogEntity(filename, operation);
            loggerdata.filesize = Convert.ToString(filesize)+" Bytes";
            loggerdata.Operation = operation;

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(loggerdata);

            // Execute the insert operation.
            table.Execute(insertOperation);

        }

    }

   
    public class LogEntity : TableEntity
    {
        public LogEntity(string filename, string uploadeddatetime)
        {
            this.PartitionKey = filename;
            this.RowKey = uploadeddatetime;
        }

        public LogEntity() { }

        public string filesize { get; set; }

        public string Operation { get; set; }
    }

    static class Helper
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = "_")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }
    }

}
