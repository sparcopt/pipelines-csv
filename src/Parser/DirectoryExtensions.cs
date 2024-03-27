namespace Parser;

public static partial class Directory
{
    public static string GetTestDataFilePath(string fileName = "data.csv.gz")
    {
        var solutionPath = GetSolutionPath();
        return Path.Combine(solutionPath, fileName);
    }
    
    private static string GetSolutionPath()
    {
        var directory = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        while (directory != null && directory.GetFiles("*.sln").Length == 0)
        {
            directory = directory.Parent;
        }
        return directory!.FullName;
    }
}