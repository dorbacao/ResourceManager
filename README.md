✨ Objetivo
Este projeto mostra:

Como criar um ResourceManager customizado implementando IEnlistmentNotification.

Como associar automaticamente operações de escrita a uma transação ativa.

Como garantir que ficheiros criados são removidos se a transação falhar.

Como usar TransactionScope para coordenar operações de I/O de forma segura.

📦 Estrutura do Projeto
FileTransactionManager  
Gere instâncias de TransactionFileResourceManager associadas à transação atual.
Usa ConditionalWeakTable para garantir que cada transação tem o seu próprio resource manager.

TransactionFileResourceManager  
Implementa IEnlistmentNotification e participa no ciclo de vida da transação:

Prepare → nada a validar

Commit → mantém os ficheiros

Rollback → apaga todos os ficheiros criados durante a transação

TransactionFile  
Wrapper simples para File.WriteAllText, que além de escrever o ficheiro, regista-o no resource manager da transação.

Program.cs  
Exemplo de utilização com TransactionScope.

🧠 Como Funciona
Quando TransactionFile.WriteAllText é chamado:

O ficheiro é criado imediatamente.

O caminho é registado no resource manager da transação atual.

O TransactionFileResourceManager é automaticamente inscrito na transação através de EnlistVolatile.

Quando a transação termina:

Commit → nada é removido

Rollback → todos os ficheiros registados são apagados
