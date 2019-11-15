---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleAzStorageQueueReceiver.md
schema: 2.0.0
---

# New-PSRuleAzStorageQueueReceiver

## SYNOPSIS

Create a receiver that processes messages from an Azure Storage Account queue.

## SYNTAX

```text
New-PSRuleAzStorageQueueReceiver [-Uri] <String> [-QueueName] <String> [<CommonParameters>]
```

## DESCRIPTION

Create a receiver that processes messages from an Azure Storage Account queue.

## EXAMPLES

### Example 1

```powershell
PS C:\> $receiver = New-PSRuleAzStorageQueueReceiver -Uri 'https://nnn.queue.core.windows.net' -QueueName 'q11' -SasToken (ConvertTo-SecureString -String 'nnn' -AsPlainText -Force);
PS C:\> Receive-PSRuleTarget -Receiver $receiver -Path '.\docs\scenarios\fruit\';
```

{{ Add example description here }}

## PARAMETERS

### -Uri

The URI to an Azure Storage Account.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -QueueName

The name of the queue. The queue must have been previously created within the Azure Storage Account.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### PSRule.Receivers.IPipelineReceiver

## NOTES

## RELATED LINKS

[Receive-PSRuleTarget](Receive-PSRuleTarget.md)
