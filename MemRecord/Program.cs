global using Spectre.Console;
using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

namespace MemRecord;

internal class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Mem Record";
        
        if (!File.Exists("config.json"))
        {
            File.WriteAllText(
                "config.json",
                JObject.Parse(
                    @"{""ProcessName"":"""",""ListProcessesByWindowName"":true}").ToString());

            AnsiConsole.MarkupLine("[125]No config file was found so a new one was generated.[/]\n");
        }

        var config = JObject.Parse(File.ReadAllText("config.json"));
        string procName = config["ProcessName"].ToString();

        if (string.IsNullOrEmpty(procName))
        {
            Process[] processes;

            if ((bool)config["ListProcessesByWindowName"])
                processes = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).ToArray();
            else 
                processes = Process.GetProcesses();

            string[] procNames = new string[processes.Length];

            for (int i = 0; i < processes.Length; i++)
            {
                procNames[i] = processes[i].ProcessName;
            }

            procName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Choose the [111]process[/] you would like to record from")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more processes)[/]")
                .AddChoices(procNames)
                .HighlightStyle(Style.Parse("183")));
        }

        var mem = new MemArea(procName);

        Start:
        dynamic num = 0;

        AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the [underline 104]hex address[/] you would like to record at: ")
            .PromptStyle("223")
            .Validate(
                addrString => 
                {
                    if (addrString.StartsWith("0x")) addrString = addrString.Substring(2);

                    bool isHex;

                    if (Environment.Is64BitProcess)
                    {
                        isHex = Int64.TryParse(addrString, NumberStyles.HexNumber, null, out var result);
                        num = result;
                    }
                    else
                    {
                        isHex = int.TryParse(addrString, NumberStyles.HexNumber, null, out var result);
                        num = result;
                    }

                    return isHex;
                }));

        var address = new IntPtr(num);

        var recordSize = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter the amount of [104]bytes[/] you would like to record: ")
            .PromptStyle("223")
            .Validate(recordSize => 
            {
                return recordSize switch 
                {
                    < 1 => ValidationResult.Error("[red]Size has to be greater than 0[/]"),
                    _ => ValidationResult.Success()
                };
            }));

        int chunkSize;

        if (Environment.Is64BitProcess) chunkSize = sizeof(Int64);
        else chunkSize = sizeof(int);

        if ((recordSize % chunkSize) != 0)
        {
            recordSize = (int)(chunkSize * decimal.Ceiling(recordSize / chunkSize));
            recordSize += chunkSize;
        }

        AnsiConsole.MarkupLine("[bold]--------------------------[/]\n");

        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Hit enter when you want to [underline 160]start recording[/]")
            .HighlightStyle(Style.Plain)
            .AddChoices(new[] { "Start Recording" }));

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Line)
            .SpinnerStyle(Style.Parse("160"))
            .Start("[grey]Recording...[/] (Press [111]any key[/] to stop)", 
            _ => mem.Record(address, recordSize));

        Console.WriteLine("\n\n");

        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Hit enter to [underline 218]start over[/]")
            .HighlightStyle(Style.Plain)
            .AddChoices(new[] { "Record Another Address" }));

        goto Start;
    }
}

