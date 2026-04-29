using System;
using System.Collections.Generic;
using System.Text;

public sealed class Settings
{
    public StatusFileSettings StatusFileSettings { get; set; } = new();
    public EasyLogSettings EasyLogSettings { get; set; } = new();
}

public sealed class StatusFileSettings
{
    public string Name { get; set; } = "status";
    public string Format { get; set; } = ".json";
}

public sealed class EasyLogSettings
{
    public string DirectoryPath { get; set; } = "log";
    public string FileFormat { get; set; } = ".json";
    public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm:ss";
}