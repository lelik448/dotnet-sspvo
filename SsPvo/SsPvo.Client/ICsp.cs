namespace SsPvo.Client
{
    public interface ICsp
    {
        byte[] SignData(byte[] dataToSign);
    }
}
