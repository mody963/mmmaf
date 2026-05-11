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
                    .Title($"[bold green]{Texts.Get("User_Analytics")}[/]")
                    .AddChoices(
                        Texts.Get("Analytics_Revenue_Last_Month"),
                        Texts.Get("Analytics_Revenue_Last_Year"),
                        Texts.Get("Analytics_3_Most_Expensive_Games"),
                        Texts.Get("Analytics_3_Cheapest_Games"),
                        Texts.Get("Analytics_Top_3_Genres_Most_Sold"),
                        Texts.Get("Back"))
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );
            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case var c when c == Texts.Get("Analytics_Revenue_Last_Month"):
                    ShowRevenue(Texts.Get("Analytics_Revenue_Last_Month"), _analyticsLogic.GetRevenueLastMonth());
                    break;

                case var c when c == Texts.Get("Analytics_Revenue_Last_Year"):
                    ShowRevenue(Texts.Get("Analytics_Revenue_Last_Year"), _analyticsLogic.GetRevenueLastYear());
                    break;

                case var c when c == Texts.Get("Analytics_3_Most_Expensive_Games"):
                    ShowPriceChart(Texts.Get("Analytics_3_Most_Expensive_Games"), _analyticsLogic.GetMostExpensiveGames());
                    break;

                case var c when c == Texts.Get("Analytics_3_Cheapest_Games"):
                    ShowPriceChart(Texts.Get("Analytics_3_Cheapest_Games"), _analyticsLogic.GetCheapestGames());
                    break;

                case var c when c == Texts.Get("Analytics_Top_3_Genres_Most_Sold"):
                    ShowSoldChart(Texts.Get("Analytics_Top_3_Genres_Most_Sold"), _analyticsLogic.GetTop3GenresMostSold());
                    break;

                case var c when c == Texts.Get("Back"):
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
                    .Title($"[bold red]{Texts.Get("Admin_Analytics")}[/]")
                    .AddChoices(
                        Texts.Get("Analytics_Top_10_Games_Last_Month"),
                        Texts.Get("Analytics_Top_10_Genres"),
                        Texts.Get("Back"))
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );
            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case var c when c == Texts.Get("Analytics_Top_10_Games_Last_Month"):
                    ShowSoldChart(Texts.Get("Analytics_Top_10_Games_Last_Month"), _analyticsLogic.GetTop10GamesLastMonth());
                    break;

                case var c when c == Texts.Get("Analytics_Top_10_Genres"):
                    ShowSoldChart(Texts.Get("Analytics_Top_10_Genres"), _analyticsLogic.GetTop10Genres());
                    break;

                case var c when c == Texts.Get("Back"):
                    back = true;
                    break;
            }
        }
    }

    private static void ShowRevenue(string title, RevenueResult result)
    {
        AnsiConsole.Clear();

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn(Texts.Get("Period_Start"));
        table.AddColumn(Texts.Get("Period_End"));
        table.AddColumn(Texts.Get("Total_Revenue"));

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
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_Data_Available")}[/]");
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
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_Data_Available")}[/]");
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
        AnsiConsole.MarkupLine($"\n[grey]{Texts.Get("Press_Any_Key_To_Return")}[/]");
        Console.ReadKey(true);
    }
}