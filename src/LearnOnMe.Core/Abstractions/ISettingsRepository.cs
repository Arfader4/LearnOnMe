namespace LearnOnMe.Core.Abstractions;

/// <summary>Simple key/value app configuration.</summary>
public interface ISettingsRepository
{
    string? Get(string key);

    void Set(string key, string value);
}
