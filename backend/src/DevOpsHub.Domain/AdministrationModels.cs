namespace DevOpsHub.Domain.Administration;

public sealed class FeatureFlag : Entity
{
    private FeatureFlag() { }
    public FeatureFlag(string key, string description, bool isEnabled = false)
    {
        Key = key.Trim(); Description = description.Trim(); IsEnabled = isEnabled;
    }
    public string Key { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public void SetEnabled(bool value) { IsEnabled = value; Touch(); }
}

public sealed class SystemSetting : Entity
{
    private SystemSetting() { }
    public SystemSetting(string key, string value, string category, bool isSecret = false)
    {
        Key = key.Trim(); Value = value; Category = category.Trim(); IsSecret = isSecret;
    }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsSecret { get; private set; }
    public void UpdateValue(string value) { Value = value; Touch(); }
}
