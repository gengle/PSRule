---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleHttpReceiver.md
schema: 2.0.0
---

# New-PSRuleHttpReceiver

## SYNOPSIS

Create a receiver that accepts and processes HTTP requests.

## SYNTAX

```text
New-PSRuleHttpReceiver [-Uri] <String[]> [<CommonParameters>]
```

## DESCRIPTION

Create a receiver that accepts and processes HTTP requests.

## EXAMPLES

### Example 1

```powershell
PS C:\> New-PSRuleHttpReceiver -Uri 'http://localhost:5000/';
```

{{ Add example description here }}

## PARAMETERS

### -Uri

One or more URI endpoints to listen for HTTP requests on.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
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
