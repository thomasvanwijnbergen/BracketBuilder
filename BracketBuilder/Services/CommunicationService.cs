namespace BracketBuilder.Services;

/// <summary>
/// a custom (scuffed) solution to update recusrively
/// used when changing language, it will refresh everything
/// or when you want to refresh main page from component
/// 
/// im not 100% sure if its actually needed
/// i am sorry for the person who might have to change this
/// </summary>
public class CommunicationService
{
    public event Action<Guid,bool>? RefreshRequested
    {
        add => _refreshRequested += value;
        remove => _refreshRequested -= value;
    }

    private event Action<Guid,bool>? _refreshRequested = null;
    public void CallRequestRefresh(Guid guid,bool full)
    {
        
        _refreshRequested?.Invoke(guid,full);
    }
    public bool Initialized
    {
        get { return _refreshRequested != null; }
    }
}
