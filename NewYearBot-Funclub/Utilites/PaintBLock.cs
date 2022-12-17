using Discord;

namespace NewYearBot_Funclub.Utilites;



public class PaintBLock
{
    public string BlcokEmoji;

    public PaintBLock(string blcokEmoji)
    {
        this.BlcokEmoji = blcokEmoji;
    }
    

    public Task ModifyBLock(string newEmoji)
    {
        BlcokEmoji = newEmoji;
        return Task.CompletedTask;
    }
}

public static class PaintEmoji
{
    public static Emoji Black = "⬛";
    public static Emoji White = "⬜";
    public static Emoji Blue = "🟦";
    public static Emoji Brown = "🟫";
    public static Emoji Green = "🟩";
    public static Emoji Orange = "🟧";
    public static Emoji Purple = "🟪";
    public static Emoji Red = "🟥"; 
    public static Emoji Yellow = "🟨";
}