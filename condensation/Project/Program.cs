using Spectre.Console;
using System.Globalization;
using System.Threading;
using Project;

// -----------------------------
// LANGUAGE SELECTION
// -----------------------------
var languagePrompt = new SelectionPrompt<string>()
    .Title("[bold]Choose your language[/]")
    .AddChoices("English", "Nederlands", "Deutsch", "Français")
    .HighlightStyle(new Style(foreground: Color.Green));

void ApplyLanguage(string lang)
{
    var culture = lang switch
    {
        "Nederlands" => "nl-NL",
        "Deutsch"    => "de-DE",
        "Français"   => "fr-FR",
        _            => "en-US"
    };

    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
}

var languageChoice = AnsiConsole.Prompt(languagePrompt);
ApplyLanguage(languageChoice);

AnsiConsole.MarkupLine($"[green]{Texts.Get("Welcome")}[/]");

// -----------------------------
// MAIN MENU LOOP
// -----------------------------
bool running = true;

while (running)
{
    var mainMenu = new SelectionPrompt<string>()
        .Title($"[bold]{Texts.Get("Menu_Main")}[/]")
        .AddChoices(
            Texts.Get("Menu_Login"),
            Texts.Get("Menu_Game"),
            Texts.Get("Menu_Cart"),
            Texts.Get("Menu_Checkout"),
            Texts.Get("Menu_Orders"),
            Texts.Get("Menu_About"),
            Texts.Get("Menu_Language"),
            Texts.Get("Menu_Exit")
        )
        .HighlightStyle(new Style(foreground: Color.Yellow));

    var choice = AnsiConsole.Prompt(mainMenu);

    switch (choice)
    {
        case var c when c == Texts.Get("Menu_Login"):
            AnsiConsole.MarkupLine("[blue]Not implemented yet.[/]");
            break;

        case var c when c == Texts.Get("Menu_Game"):
            ShowGameMenu();
            break;

        case var c when c == Texts.Get("Menu_Cart"):
            AnsiConsole.MarkupLine("[blue]Cart is empty.[/]");
            break;

        case var c when c == Texts.Get("Menu_Checkout"):
            AnsiConsole.MarkupLine("[yellow]Checkout in Sprint 2.[/]");
            break;

        case var c when c == Texts.Get("Menu_Orders"):
            AnsiConsole.MarkupLine("[yellow]Orders in Sprint 3.[/]");
            break;

        case var c when c == Texts.Get("Menu_About"):
            AnsiConsole.MarkupLine("[green]About us placeholder.[/]");
            break;

        case var c when c == Texts.Get("Menu_Language"):
            languageChoice = AnsiConsole.Prompt(languagePrompt);
            ApplyLanguage(languageChoice);
            AnsiConsole.MarkupLine($"[green]{Texts.Get("Welcome")}[/]");
            break;

        case var c when c == Texts.Get("Menu_Exit"):
            running = false;
            break;
    }
}

// -----------------------------
// GAME MENU
// -----------------------------
void ShowGameMenu()
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
