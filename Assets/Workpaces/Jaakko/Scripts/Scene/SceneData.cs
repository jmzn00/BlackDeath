public class SceneData 
{
    public string Name;
    public bool IsGameplay;

    public SceneData(string sceneName, bool isGameplay) 
    {
        Name = sceneName;
        IsGameplay = isGameplay;
    }
}