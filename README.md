# Custom Resource Managers

Um exemplo simples e funcional de como implementar **Resource Managers** customizados para participar em **transações distribuídas** (`System.Transactions`) aplicadas tanto a operações de escrita em ficheiros quanto a solicitações de envio de emails. O objetivo é tornar operações externas **transacionais**, garantindo que ficheiros criados e emails registados sejam automaticamente revertidos em caso de rollback.

## 🎯 Objetivo

Este projeto demonstra:

- **Resource managers customizados:** implementação de `IEnlistmentNotification`.
- **Integração com transações:** associação automática das operações à transação ativa.
- **Rollback de ficheiros:** remoção de ficheiros criados se a transação falhar.
- **Rollback de emails:** cancelamento de solicitações de envio registadas numa API.
- **Uso de `TransactionScope`:** coordenação de I/O e chamadas externas de forma consistente.

## 🧱 Estrutura do Projeto

### `FileTransactionManager`

- Gere instâncias de `TransactionFileResourceManager` associadas à transação atual.
- Usa `ConditionalWeakTable<Transaction, TransactionFileResourceManager>` para garantir um resource manager por transação.
- Inscreve automaticamente o resource manager via `EnlistVolatile`.
- Remove o resource manager quando a transação termina.

### `TransactionFileResourceManager`

- Implementa `IEnlistmentNotification` para participar no ciclo de vida da transação.
- Mantém uma `ConcurrentQueue<string>` com os caminhos dos ficheiros criados.
- Comportamento:
  - `Prepare` → validação simples.  
  - `Commit` → mantém os ficheiros.  
  - `Rollback` → apaga todos os ficheiros registados.

### `EmailTransactionManager`

- Regista solicitações de envio de email numa API com estado `Pending`.
- Garante que cada transação possui um único `EmailResourceManager`.
- Inscreve automaticamente o resource manager na transação ativa.

### `EmailResourceManager`

- Implementa `IEnlistmentNotification` para controlar o ciclo de vida do email.
- Comportamento:
  - `Prepare` → validação simples.  
  - `Commit` → confirma o envio na API (`Pending` → `Sent`).  
  - `Rollback` → cancela a solicitação (`Pending` → `Canceled`).  

### `Program.cs`

Demonstra o uso de `TransactionScope` para criar ficheiros e solicitar envio de emails de forma transacional.

## ⚙️ Como Funciona

1. `TransactionFile.WriteAllTextAsync` cria o ficheiro e regista-o no resource manager.  
2. `EmailApiClient.SubmitEmailAsync` cria uma solicitação de email com estado `Pending`.  
3. Os resource managers são inscritos automaticamente na transação ativa.  
4. Quando a transação termina:  
   - **Commit** → ficheiros permanecem e emails são enviados.  
   - **Rollback** → ficheiros são apagados e emails são cancelados.

Assim, operações de I/O e chamadas externas passam a participar numa transação distribuída, garantindo **atomicidade**.

## 🧪 Exemplo de Uso

```csharp
using System.Transactions;

using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
{
    var path = "C:\\temporary";
    Directory.CreateDirectory(path);

    await TransactionFile.WriteAllTextAsync($"{path}\\file1.txt", "Hello, World 1!");
    await TransactionFile.WriteAllTextAsync($"{path}\\file2.txt", "Hello, World 2!");

    using var email = new EmailApiClient();
    await email.SubmitEmailAsync(
        "teste@teste.com",
        "Email de Teste transacional",
        "Aqui teremos o corpo do email"
    );

    transaction.Complete();
}
