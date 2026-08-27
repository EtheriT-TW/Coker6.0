namespace EtheriT.Coker.Web.Public.Authentication
{
    public sealed record FrontSessionValidationResult(bool IsValid, string Error)
    {
        public static FrontSessionValidationResult Success { get; } = new(true, string.Empty);

        public static FrontSessionValidationResult Fail(string error)
        {
            return new FrontSessionValidationResult(false, error);
        }
    }
}
