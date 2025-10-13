namespace Mail_Send.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
