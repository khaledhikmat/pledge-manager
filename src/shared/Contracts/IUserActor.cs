namespace pledgemanager.shared.Contracts;

using Dapr.Actors;
using shared.Models;

public interface IUserActor : IActor 
{
    public Task Update(User user);
    public Task GenerateVerification();
    public Task<UserVerificationResponse> ValidateVerification(string code);
    public Task<bool> IsVerified();
}