# File Transaction Resource Manager

Um exemplo simples e funcional de como implementar um **Resource Manager** customizado para participar em **transações distribuídas** (`System.Transactions`) aplicadas a operações de escrita em ficheiros. O objetivo é tornar operações de I/O em disco **transacionais**, garantindo que ficheiros criados durante uma transação são automaticamente removidos em caso de rollback.

## 🎯 Objetivo

Este projeto demonstra:

- **Resource manager customizado:** implementação de `IEnlistmentNotification`.
- **Integração com transações:** associação automática das escritas à transação ativa.
- **Rollback de ficheiros:** remoção de ficheiros criados se a transação falhar.
- **Uso de `TransactionScope`:** coordenação de operações de I/O de forma segura e consistente.

## 🧱 Estrutura do Projeto

### `FileTransactionManager`

- Gere instâncias de `TransactionFileResourceManager` associadas à transação atual.
- Usa `ConditionalWeakTable<Transaction, TransactionFileResourceManager>` para garantir um resource manager por transação.
- Inscreve automaticamente o resource manager na transação via `EnlistVolatile`.
- Remove o resource manager quando a transação termina (`TransactionCompleted`).

### `TransactionFileResourceManager`

- Implementa `IEnlistmentNotification` para participar no ciclo de vida da transação.
- Mantém uma `ConcurrentQueue<string>` com os caminhos dos ficheiros criados.
- Comportamento:
  - `Prepare` → validação simples.
  - `Commit` → mantém os ficheiros.
  - `Rollback` → apaga todos os ficheiros registados.

### `TransactionFile`

- Wrapper para `File.WriteAllText`.
- Após escrever o ficheiro, regista-o no resource manager da transação ativa.

### `Program.cs`

Demonstra o uso de `TransactionScope` para criar ficheiros de forma transacional.

## ⚙️ Como Funciona

1. `TransactionFile.WriteAllText` cria o ficheiro e regista o caminho no resource manager.
2. O `FileTransactionManager` inscreve automaticamente o resource manager na transação ativa.
3. Quando a transação termina:
   - **Commit** → ficheiros permanecem.
   - **Rollback** → ficheiros registados são apagados.

Assim, operações de escrita em disco passam a participar numa transação distribuída, garantindo **atomicidade**.

## 🧪 Exemplo de Uso

```csharp
using System.Transactions;

using (var transaction = new TransactionScope())
{
    var path = "C:\\temporary";
    Directory.CreateDirectory(path);

    TransactionFile.WriteAllText($"{path}\\file1.txt", "Hello, World 1!");
    TransactionFile.WriteAllText($"{path}\\file2.txt", "Hello, World 2!");

    transaction.Complete();
}
