using System.Globalization;
using System.Reflection;
using System.Threading;
using Project;

Console.WriteLine("1. English");
Console.WriteLine("2. Nederlands");

var input = Console.ReadLine();

var culture = input == "2" ? "nl-NL" : "en-US";
Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

Console.WriteLine(Texts.Get("Welcome"));