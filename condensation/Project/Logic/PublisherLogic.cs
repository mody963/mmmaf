public class PublisherLogic
{
    private readonly PublisherAccess _publisher = new PublisherAccess();

    public void CreatePublisher(PublisherModel publisher)
    {
        _publisher.CreatePublisher(publisher);
    }

    public bool IsValidStudioName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Returns false if the name exists
        return !_publisher.StudioNameExists(name); 
    }
    public PublisherModel? GetByAccountId(int accountId)
    {
        return _publisher.GetByAccountId(accountId);
    }
}