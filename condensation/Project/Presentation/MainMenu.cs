using System;
using System.Globalization;
using System.Threading;
using Spectre.Console;
using CondensationApp;

public static class MainMenu
{
    private static Cart cart = new Cart(); // Shared cart instance

    // We move the language prompt here so the Main Menu can use it when "Menu_Language" is selected
    private static readonly SelectionPrompt<string> LanguagePrompt = new SelectionPrompt<string>()
        .Title("[bold]Choose your language[/]")
        .AddChoices("English", "Nederlands", "Deutsch", "Français")
        .HighlightStyle(new Style(foreground: Color.Green));

    private static void ApplyLanguage(string lang)
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

    public static void Start()
    {
        Console.Clear();
        // 1. Do the initial language selection
        var languageChoice = AnsiConsole.Prompt(LanguagePrompt);
        SoundEffects.PlayMenuClick();
        ApplyLanguage(languageChoice);
        AnsiConsole.MarkupLine($"[green]{Texts.Get("Welcome")}[/]");

        // 2. Start the main loop
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
                    Texts.Get("Menu_Analytics"),
                    Texts.Get("Menu_About"),
                    Texts.Get("Menu_Language"),
                    Texts.Get("Menu_Exit")
                )
                .HighlightStyle(new Style(foreground: Color.Yellow));

            var choice = AnsiConsole.Prompt(mainMenu);
            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case var c when c == Texts.Get("Menu_Login"):
                    LoginMenu.Start(); // Pass the database connection down!
                    break;

                case var c when c == Texts.Get("Menu_Game"):
                    GameMenu.Start(cart); // Open the new GameMenu file! cart meegegeven voor zelfde cart bij add
                    break;

                case var c when c == Texts.Get("Menu_Cart"):
                    cart.CartOptions(); // zelde cart als voorheen als je wta toevoegd want static
                    break;

                case var c when c == Texts.Get("Menu_Checkout"):
                    CheckoutMenu.Start(cart);
                    break;

                case var c when c == Texts.Get("Menu_Orders"):
                    AnsiConsole.MarkupLine("[yellow]Orders in Sprint 3.[/]");
                    break;

                case var c when c == Texts.Get("Menu_Analytics"):
                    AnalyticsMenu.StartUserAnalytics();
                    break;

                case var c when c == Texts.Get("Menu_About"):
                    AnsiConsole.MarkupLine("[green]About us placeholder.[/]");
                    break;

                case var c when c == Texts.Get("Menu_Language"):
                    languageChoice = AnsiConsole.Prompt(LanguagePrompt);
                    ApplyLanguage(languageChoice);
                    AnsiConsole.MarkupLine($"[green]{Texts.Get("Welcome")}[/]");
                    break;

                case var c when c == Texts.Get("Menu_Exit"):
                    running = false;
                    break;
            }
        }
    }
}