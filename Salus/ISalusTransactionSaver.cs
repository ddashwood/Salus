namespace Salus;

internal interface ISalusTransactionSaver
{
    void OnCommitting();
    Task OnCommittingAsync();
    void OnRollingBack();
}
