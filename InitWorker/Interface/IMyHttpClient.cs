namespace InitWorker.Interface;

public interface IMyHttpClient
{
    Task GetSample(Dictionary<string, string> queryString);
	Task PostSample();
}
