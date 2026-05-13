using System;
using System.Collections.Generic;
using System.Text;
using EasyLog;

public sealed class Settings
{
    public LogFileFormats LogFileFormat { get; set; } = LogFileFormats.json;
    public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm:ss";
    public StatusFileSettings StatusFileSettings { get; set; } = new();
    public EasyLogSettings EasyLogSettings { get; set; } = new();
    public ProcessExclusionSettings ProcessExclusionSettings { get; set; } = new();

    public int BigFileSize {get; set; } = 10; //par défaut histoire de mettre quelquechose 
    public string PriorityExtensions { get; set; } = ".docx;.xlsx";
    public string EncryptExtensions { get; set; } = ".txt;.pdf";
    public string JobsFilePath { get; set; } = "./jobs.json";

}

public sealed class StatusFileSettings
{
    public string FilePath { get; set; } = "./status";
}

public sealed class EasyLogSettings
{
    public string DirectoryPath { get; set; } = "./logs";
    public LogStorageModes LogStorage { get; set; } = LogStorageModes.Both;
}
public sealed class ProcessExclusionSettings
{
    public string ExcludedProcesses { get; set; } = "";
}