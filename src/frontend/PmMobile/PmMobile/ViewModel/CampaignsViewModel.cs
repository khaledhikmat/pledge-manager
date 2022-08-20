using System;
namespace PmMobile.ViewModel;


public class CampaignsViewModel : BaseViewModel
{
    public ObservableCollection<Campaign> Monkeys { get; } = new();

    public CampaignsViewModel()
	{
	}
}

