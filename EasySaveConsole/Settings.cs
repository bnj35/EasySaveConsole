using System;
using System.Collections.Generic;
using System.Text;

public sealed class Settings
{
    public string DefaultFileFormat { get; set; } = "json";
    public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm:ss";
    public StatusFileSettings StatusFileSettings { get; set; } = new();
    public EasyLogSettings EasyLogSettings { get; set; } = new();
}

public sealed class StatusFileSettings
{
    public string FilePath { get; set; } = "./status";
}

public sealed class EasyLogSettings
{
    public string DirectoryPath { get; set; } = "./log";
}