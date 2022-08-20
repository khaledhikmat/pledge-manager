using System;
namespace PmMobile.ViewModel;

// Code generation mandates that the class be defined as partial!
public partial class BaseViewModel : ObservableObject
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsNotBusy))]
	bool isBusy;

	[ObservableProperty]
	string title;

	public bool IsNotBusy => !IsBusy;

	public BaseViewModel()
	{
	}
}

