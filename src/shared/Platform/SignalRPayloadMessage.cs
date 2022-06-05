namespace pledgemanager.shared.Platform;


public class PayloadMessage
{
    public string? Target { get; set; } = "";

    public object[]? Arguments { get; set; } = new[] {new object()};
}
