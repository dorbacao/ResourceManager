// See https://aka.ms/new-console-template for more information
using System.Transactions;

using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
{
    var path = "C:\\temporary";
    Directory.CreateDirectory(path);

    await TransactionFile.WriteAllTextAsync($"{path}\\file1.txt", "Hello, World 1!");
    await TransactionFile.WriteAllTextAsync($"{path}\\file2.txt", "Hello, World 2!");

    using var email = new EmailApiClient();
    await email.SubmitEmailAsync("teste@teste.com", "Email de Teste transacional", "Aqui teremos o corpo do email");

    transaction.Complete();
}
