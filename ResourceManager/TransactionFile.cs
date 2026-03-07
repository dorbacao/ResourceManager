using ResourceManagerSample;

public class TransactionFile
{
    public static void WriteAllText(string path, string? contents)
    {        
        File.WriteAllText(path, contents);

        var rm = FileTransactionManager.GetResourceManager();
        rm.AddFile(path);
    }

    public static async Task WriteAllTextAsync(string path, string? contents)
    {
       await File.WriteAllTextAsync(path, contents);

        var rm = FileTransactionManager.GetResourceManager();
        rm.AddFile(path);
    }
}


