using Microsoft.Extensions.Logging;
using Progress.Common.EhnLog;
using System;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class TracerLogger : IAppLogger
  {
    private long logCount = 0;

    private readonly ILogger<TracerLogger> logger;

    public TracerLogger(ILogger<TracerLogger> logger)
    {
      this.logger = logger;

      LogContext = LogContextFactory.createLogContext("O4gl");
      if (LogContext == null)
      {
        throw new LogException("A log context required for application logger");
      }

      LogContext.initSubsystemNames();
    }

    public LogContext LogContext { get; private set; }

    public int LoggingLevel { get => 0; set { } }
    public string ComponentId { get => null; set { } }

    public long LogEntries => logCount;

    public void ehnLogDump(int dest, int severityLevel, string execEnvId, string entrytypeId, string msgText, byte[] pbData, int cbData)
    {
      logger.LogTrace("OpenEdge Trace: {Message}, {SeverityLevel}, {ExecutionEnvironmentId}, {PbData}, {CdData}",
          msgText, severityLevel, execEnvId, pbData, cbData);
    }

    public void ehnLogStackTrace(int dest, int severityLevel, string execEnvId, string entrytypeId, string msgText, Exception e)
    {
      logger.LogTrace("OpenEdge Trace: {Message}, {SeverityLevel}, {ExecutionEnvironmentId}",
          msgText, severityLevel, execEnvId);
    }

    public void ehnLogWrite(int dest, int severityLevel, string execEnvId, string entrytypeId, string msgText)
    {
      logger.LogTrace("OpenEdge Trace: {Message}, {SeverityLevel}, {ExecutionEnvironmentId}",
          msgText, severityLevel, execEnvId);
    }

    public bool ifLogBasic(long logEntries, int logEntryIndex)
    {
      return true;
    }

    public bool ifLogExtended(long logEntries, int logEntryIndex)
    {
      return true;
    }

    public bool ifLogIt(int loggingLevel, long logEntries, int logEntryIndex)
    {
      return true;
    }

    public bool ifLogLevel(int loggingLevel)
    {
      return true;
    }

    public bool ifLogVerbose(long logEntries, int logEntryIndex)
    {
      return true;
    }

    public void logAssert(bool bCondition, int entrytypeId, string msg)
    {
      if (!bCondition)
      {
        logger.LogTrace("OpenEdge Assert: {Message}, {EntryTypeId}", msg, entrytypeId);
      }
    }

    public void logBasic(int entrytypeId, string msg)
    {
      logger.LogTrace("OpenEdge Basic: {Message}, {EntryTypeId}", msg, entrytypeId);
    }

    public void logBasic(int entrytypeId, long msgId, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Basic: {MessageId}, {MessageTokens}, {EntryTypeId}", msgId, msgTokens, entrytypeId);
    }

    public void logBasic(int entrytypeId, string msgFormat, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Basic: {MessageFormat}, {MessageTokens}, {EntryTypeId}", msgFormat, msgTokens, entrytypeId);
    }

    public void logClose() {}

    public void logError(int entrytypeId, string msg)
    {
      logger.LogTrace(msg);
    }

    public void logExtended(int entrytypeId, string msg)
    {
      logger.LogTrace(msg);
    }

    public void logExtended(int entrytypeId, long msgId, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Extend: {MessageId}, {@MessageTokens}, {EntryTypeId}", msgId, msgTokens, entrytypeId);
    }

    public void logExtended(int entrytypeId, string msgFormat, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Extend: {MessageId}, {@MessageTokens}, {EntryTypeId}", msgFormat, msgTokens, entrytypeId);
    }

    public void logStackTrace(int entrytypeId, string msg, Exception except)
    {
      logger.LogError(except, "OpenEdge Error: {Message}, {EntryTypeId}", msg, entrytypeId);
    }

    public void logStackTrace(int entrytypeId, long msgId, object[] msgTokens, Exception except)
    {
      logger.LogError(except, "OpenEdge Error: {MessageId}, {@MessageTokens}, {EntryTypeId}",
        msgId, msgTokens, entrytypeId);
    }

    public void logVerbose(int entrytypeId, string msg)
    {
      logger.LogTrace("OpenEdge Verbose: {Message}, {EntryTypeId}", msg, entrytypeId);
    }

    public void logVerbose(int entrytypeId, long msgId, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Verbose: {MessageId}, {@MessageTokens}, {EntryTypeId}",
        msgId, msgTokens, entrytypeId);
    }

    public void logVerbose(int entrytypeId, string msgFormat, object[] msgTokens)
    {
      logger.LogTrace("OpenEdge Verbose: {MessageFormat}, {@MessageTokens}, {EntryTypeId}",
        msgFormat, msgTokens, entrytypeId);
    }

    public void logWriteMessage(int dest, int severityLevel, string execEnvId, string entrytypeId, string msgText)
    {
      ehnLogWrite(dest, severityLevel, execEnvId, entrytypeId, msgText);
    }

    public void logWriteMessage(int dest, int severityLevel, string execEnvId, string entrytypeId, string msgText, Exception except)
    {
      ehnLogStackTrace(dest, severityLevel, execEnvId, entrytypeId, msgText, except);
    }

    public string nameAt(int index)
    {
      throw new NotImplementedException();
    }

    public void SetLogContext(string value) {}

    public long setLogEntries(string logEntryTypes)
    {
      return 0;
    }

    public long setLogEntries(long newLogEntries, bool newsubLevelUsed, byte[] newlogSubLevels)
    {
      return 0;
    }
  }
}
