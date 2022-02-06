using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MemRecord;

public class MemArea
{
    private Process _process;

    public MemArea(string procName)
    {
        var proc = Process.GetProcessesByName(procName).FirstOrDefault();

        if (proc == null)
        {
            AnsiConsole.MarkupLine($"[81]Process[/] [202]{procName}[/] [81]not found![/]");
            Console.ReadKey();
            throw new ArgumentException($"{procName} was not found");
        }

        _process = proc;

        AnsiConsole.MarkupLine($"[81]Found process[/] [202]{procName}[/]");
        AnsiConsole.MarkupLine($"[81]Main Module:[/] [202]{proc.MainModule.ModuleName}[/]");
        AnsiConsole.MarkupLine($"[81]Mod Base:[/] [202]0x{proc.MainModule.BaseAddress.ToString("X")}[/]");
        AnsiConsole.MarkupLine("[bold]--------------------------[/]");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] ReadMemory(IntPtr addr, int size)
    {
        var b = new byte[size];
        Memory.ReadProcessMemory(_process.Handle, addr, b, size, out var read);
        return b;
    }

    public void Record(IntPtr address, int size)
    {
        int chunkSize;

        if (Environment.Is64BitProcess) chunkSize = sizeof(Int64);
        else chunkSize = sizeof(int);

        var originalBytes = ReadMemory(address, size);
        dynamic originalValues;

        if (Environment.Is64BitProcess) originalValues = new List<Int64>();
        else originalValues = new List<int>();

        for (int i = 0; i < size; i += chunkSize)
        {
            if (Environment.Is64BitProcess)
                originalValues.Add(BitConverter.ToInt64(originalBytes, i));
            else 
                originalValues.Add(BitConverter.ToInt32(originalBytes, i));
        }

        var changedIndexes = new List<int>();

        while (!Console.KeyAvailable)
        {
            var bytes = ReadMemory(address, size);
            int pos = 0;

            foreach (var i in originalValues)
            {
                bool isDiff;

                if (Environment.Is64BitProcess)
                    isDiff = BitConverter.ToInt64(bytes, pos) != i;
                else
                    isDiff = BitConverter.ToInt32(bytes, pos) != i;

                if (isDiff && !changedIndexes.Contains(pos))
                {
                    changedIndexes.Add(pos);
                }

                pos += chunkSize;
            }
        }

        changedIndexes.Sort();

        var table = new Table();
        table.BorderStyle = Style.Parse("168");
        table.AddColumn(new TableColumn("[122]Changed Offsets[/]").Centered());
        table.AddColumn(new TableColumn("[122]Original Value (Int)[/]").Centered());
        table.AddColumn(new TableColumn("[122]Current Value (Int[/])").Centered());
        table.AddColumn(new TableColumn("[122]Current Value (Float)[/]").Centered());
        var currentValues = ReadMemory(address, size);

        string bold = "[bold]";
        string escape = "[/]";

        foreach (var i in changedIndexes)
        {
            string originalValue = originalValues[i / 4].ToString();
            string currentValueInt = BitConverter.ToInt32(currentValues, i).ToString();
            string currentValueFloat = BitConverter.ToSingle(currentValues, i).ToString();

            table.AddRow(
                bold + "0x" + i.ToString("X") + escape,
                bold + originalValue + escape,
                bold + currentValueInt + escape,
                bold + currentValueFloat + escape
                ).Centered();
        }

        AnsiConsole.Write(table);
    }
}

