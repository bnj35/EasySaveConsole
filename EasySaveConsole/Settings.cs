using System;
using System.Collections.Generic;
using System.Text;
using EasyLog;

public sealed class Settings
{
    public string DefaultFileFormat { get; set; } = "json";
    public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm:ss";
    public StatusFileSettings StatusFileSettings { get; set; } = new();
    public EasyLogSettings EasyLogSettings { get; set; } = new();
    public ProcessExclusionSettings ProcessExclusionSettings { get; set; } = new();

    public int BigFileSize {get; set; } = 10; //par défaut histoire de mettre quelquechose 
    public string EncryptExtensions { get; set; } = ".txt;.pdf";

}

public sealed class StatusFileSettings
{
    public string FilePath { get; set; } = "./status";
}

public sealed class EasyLogSettings
{
    public string DirectoryPath { get; set; } = "./logs";
    public LogStorage LogStorage { get; set; } = LogStorage.Both;
}
public sealed class ProcessExclusionSettings
{
    public string ExcludedProcesses { get; set; } = "";
}