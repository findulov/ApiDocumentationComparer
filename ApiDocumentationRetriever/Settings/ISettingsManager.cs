namespace ApiDocumentationComparer.Infrastructure.Settings
{
    public interface ISettingsManager
    {
        string Get(string key);

        void Save(string key, string value);
    }
}
