using System;
using Spectre.Console;
public static class GameMenu
{
    public static void Start()
    {
        var gameMenu = new SelectionPrompt<string>()
            .Title($"[bold]{Texts.Get("Menu_Game")}[/]")
            .AddChoices(
                Texts.Get("Game_List"),
                Texts.Get("Game_Search"),
                Texts.Get("Game_Filter"),
                Texts.Get("Game_Back")
            )
            .HighlightStyle(new Style(foreground: Color.Green));

        var gameChoice = AnsiConsole.Prompt(gameMenu);

        switch (gameChoice)
        {
            case var c when c == Texts.Get("Game_List"):
                AnsiConsole.MarkupLine("[blue]Game list not implemented.[/]");
                break;

            case var c when c == Texts.Get("Game_Search"):
                AnsiConsole.MarkupLine("[blue]Game search not implemented.[/]");
                break;

            case var c when c == Texts.Get("Game_Filter"):
                AnsiConsole.MarkupLine("[blue]Game filter not implemented.[/]");
                break;

            case var c when c == Texts.Get("Game_Back"):
                return;
        }
    }
}