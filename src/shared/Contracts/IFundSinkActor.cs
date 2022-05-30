namespace pledgemanager.shared.Contracts;

using Dapr.Actors;
using shared.Models;

public interface IFundSinkActor : IActor 
{
    public Task Fund(FundSink fund);
}