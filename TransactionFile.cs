using ResourceManagerSample;

public class TransactionFile
{
    public static void WriteAllText(string path, string? contents)
    {        
        System.IO.File.WriteAllText(path, contents);

        var rm = FileTransactionManager.GetResourceManager();
        rm.AddFile(path);
    }
}


