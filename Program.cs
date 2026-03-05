// See https://aka.ms/new-console-template for more information
using System.Transactions;


using (var transaction = new TransactionScope())
{
    var path = "C:\\temporary";
    Directory.CreateDirectory(path);

    TransactionFile.WriteAllText($"{path}\\file1.txt", "Hello, World 1!");
    TransactionFile.WriteAllText($"{path}\\file2.txt", "Hello, World 2!");
    
    transaction.Complete();
}

