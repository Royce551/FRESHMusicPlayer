using CommunityToolkit.Mvvm.ComponentModel;

namespace FRESHMusicPlayer.ViewModels;

public class ViewModelBase : ObservableRecipient
{
    public MainViewModel MainView { get; set; } = default!;

    public ViewModelBase()
    {

    }

    public virtual void AfterPageLoaded()
    {
        IsActive = true;
    }

    public virtual void OnNavigatingAway()
    {
        IsActive = false;
    }
}
