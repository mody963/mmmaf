using Spectre.Console;

public static class AnalyticsMenu
{
    private static readonly AnalyticsLogic _analyticsLogic = new AnalyticsLogic();

    public static void StartUserAnalytics()
    {
        bool back = false;

        while (!back)
        {
            AnsiConsole.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]User analytics[/]")
                    .AddChoices(
                        "Revenue last month",
                        "Revenue last year",
                        "Top 3 most expensive games",
                        "Top 3 cheapest games",
                        "Top 3 genres with most sold games",
                        "Back")
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );

            switch (choice)
            {
                case "Revenue last month":
                    ShowRevenue("Revenue last month", _analyticsLogic.GetRevenueLastMonth());
                    break;

                case "Revenue last year":
                    ShowRevenue("Revenue last year", _analyticsLogic.GetRevenueLastYear());
                    break;

                case "Top 3 most expensive games":
                    ShowPriceChart("Top3 most expensive games", _analyticsLogic.GetMostExpensiveGames());
                    break;

                case "Top 3 cheapest games":
                    ShowPriceChart("Top 3 cheapest games", _analyticsLogic.GetCheapestGames());
                    break;

                case "Top 3 genres with most sold games":
                    ShowSoldChart("Top 3 genres with most sold games", _analyticsLogic.GetTop3GenresMostSold());
                    break;

                case "Back":
                    back = true;
                    break;
            }
        }
    }

    public static void AdminAnalytics()
    {
        bool back = false;

        while (!back)
        {
            AnsiConsole.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold red]Admin analytics[/]")
                    .AddChoices(
                        "Top 10 most sold games of last month",
                        "Top 10 most popular genres",
                        "Back")
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );

            switch (choice)
            {
                case "Top 10 most sold games of last month":
                    ShowSoldChart("Top 10 most sold games of last month", _analyticsLogic.GetTop10GamesLastMonth());
                    break;

                case "Top 10 most popular genres":
                    ShowSoldChart("Top 10 most popular genres", _analyticsLogic.GetTop10Genres());
                    break;

                case "Back":
                    back = true;
                    break;
            }
        }
    }

    private static void ShowRevenue(string title, RevenueResult result)
    {
        AnsiConsole.Clear();

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Period start");
        table.AddColumn("Period end");
        table.AddColumn("Total revenue");

        table.AddRow(
            result.PeriodStart.ToString("d"),
            result.PeriodEnd.ToString("d"),
            $"€ {result.TotalRevenue:0.00}");

        AnsiConsole.Write(new Panel(table).Header($"[bold]{title}[/]"));
        Pause();
    }

    private static void ShowPriceChart(string title, List<PriceChartItem> items)
    {
        AnsiConsole.Clear();
    
        if (items == null || items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No data available.[/]");
            Pause();
            return;
        }
    
        var colors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Cyan1,
            Color.Orange1,
            Color.HotPink,
            Color.Purple,
            Color.Aqua,
            Color.White
        };
    
        var chart = new BarChart()
            .Width(100)
            .Label($"[bold]{title}[/]")
            .CenterLabel();
    
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var shortName = item.GameName.Length > 18
                ? item.GameName[..18] + "..."
                : item.GameName;
    
            var color = colors[i % colors.Length];
            chart.AddItem(shortName, (double)item.Price, color);
        }

        //var table = new Table().Border(TableBorder.Rounded);
        //table.AddColumn(Texts.Get("Game"));
        //table.AddColumn(Texts.Get("Price"));

        //foreach (var item in items)
        //{
            //table.AddRow(
                //item.GameName,
                //$"€ {item.Price:0.00}");
        //}

        //AnsiConsole.Write(table);
    
        AnsiConsole.Write(new Panel(chart).Header($"[bold]{title}[/]").Border(BoxBorder.Rounded));
        AnsiConsole.WriteLine();
        Pause();
    }

    private static void ShowSoldChart(string title, List<SoldChartItem> items)
    {
        AnsiConsole.Clear();

        if (items == null || items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No data available.[/]");
            Pause();
            return;
        }

        var colors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Cyan1,
            Color.Orange1,
            Color.HotPink,
            Color.Purple,
            Color.Aqua,
            Color.White
        };

        var chart = new BarChart()
            .Width(60)
            .Label($"[bold]{title}[/]")
            .CenterLabel();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var shortName = item.Name.Length > 18
                ? item.Name[..18] + "..."
                : item.Name;

            var color = colors[i % colors.Length];
            chart.AddItem(shortName, item.SoldCopies, color);
        }

        // var table = new Table().Border(TableBorder.Rounded);
        // table.AddColumn(Texts.Get("Name"));
        // table.AddColumn(Texts.Get("Sold_Copies"));
// 
        // foreach (var item in items)
        // {
            // table.AddRow(item.Name, item.SoldCopies.ToString());
        // }
// 
        // AnsiConsole.Write(table);

        AnsiConsole.Write(new Panel(chart).Header($"[bold]{title}[/]").Border(BoxBorder.Rounded));
        AnsiConsole.WriteLine();
        Pause();
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
        Console.ReadKey(true);
    }
}