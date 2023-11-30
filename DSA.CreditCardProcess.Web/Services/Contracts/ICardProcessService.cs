namespace DSA.CreditCardProcess.Web.Services.Contracts
{
    public interface ICardProcessService
    {
        Task<List<string>> ProcessCard(string cardNumber);
    }
}