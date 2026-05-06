using System.ComponentModel;

namespace HarnessSample;

public static class HarnessTools
{
    [DisplayName("GetDateTime")]
    [Description("現在の日時を取得します。")]
    public static string GetDateTime()
    {
        Console.WriteLine("[GetDateTime] called");
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
