using Discord;
using Serilog.Events;

namespace NewYearBot_Funclub.Utilites;

public class Extensions
{

    public static DateTimeOffset CurUpTime = DateTimeOffset.FromUnixTimeSeconds(0);
    public static async Task LoggerToChannel(string message, LogEventLevel severity, string error = null)
        {
            Color color = Color.Default;
            string title = "";
            
            switch (severity)
            {
                case LogEventLevel.Fatal:
                    color = new Color(255,0,0);
                    title = "КРИТИЧЕСКАЯ ОШИБКА";
                    // Log.Fatal(message + "\n=======\n" + error);
                    break;
                case LogEventLevel.Debug:
                    color = new Color(165,165,165);
                    title = "Дебуг";
                    // Log.Debug(message);
                    break;
                case LogEventLevel.Error:
                    color = new Color(255,78,39);
                    title = "Ошибка";
                    // Log.Error(message + "\n=======\n" + error);
                    break;
                case LogEventLevel.Information:
                    color = new Color(39,203,255);
                    title = "Информация";
                    break;
                case LogEventLevel.Verbose:
                    color = new Color(39,255,213);
                    title = "Больше информации";
                    // Log.Verbose(message);
                    break;
                case LogEventLevel.Warning:
                    color = new Color(255,226,39);
                    title = "Внимание";
                    // Log.Warning(message);
                    break;
                    
            }

            if (severity is LogEventLevel.Fatal or LogEventLevel.Error or LogEventLevel.Warning && error != null)
            {
                Embed msg;
                if(!error.StartsWith("#Server requested a reconnect ") && !error.StartsWith("#WebSocket connection was closed"))
                {
                    msg = new EmbedBuilder()
                        .WithTitle(title)
                        .WithColor(color)
                        .WithDescription($"ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠ" +
                                         $"\n```cs\n" + $"{error}```" )
                        .Build();
                    try
                    {
                        await Program.Logger.SendMessageAsync("<@!437540824524521472>", embeds: new[] { msg });
                    }
                    catch
                    {
                        // похуй
                    }
                }
                else
                {
                    msg = new EmbedBuilder()
                        .WithTitle(title)
                        .WithColor(color)
                        .WithDescription($"ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠ" +
                                         $"\n```cs\n" + $"{error}```" )
                        .Build();
                    try
                    {
                        await Program.Logger.SendMessageAsync(embeds: new[] { msg });
                    }
                    catch
                    {
                        // похуй
                    }
                }
            }
            else
            {


                var msg = new EmbedBuilder()
                    .WithTitle(title)
                    .WithColor(color)
                    .WithDescription($"ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠ" + 
                                     $"\n```{message}```")
                    .Build();
                try
                {
                    await Program.Logger.SendMessageAsync(embeds: new[] { msg });
                }
                catch
                {
                    // похуйй
                }
            }

        }
}