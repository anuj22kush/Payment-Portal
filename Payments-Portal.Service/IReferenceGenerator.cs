namespace Payments_Portal.Service
{
    /// <summary>
    /// Generates unique payment reference numbers.
    /// Follows the format PAY-YYYYMMDD-#### (sequential per day).
    /// </summary>
    public interface IReferenceGenerator
    {
        Task<string> GenerateAsync();
    }
}
