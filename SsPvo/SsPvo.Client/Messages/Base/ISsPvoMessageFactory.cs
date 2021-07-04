namespace SsPvo.Client.Messages.Base
{
    public interface ISsPvoMessageFactory
    {
        SsPvoMessage Create(SsPvoMessage.Options options);
    }
}
