---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Receive-PSRuleTarget.md
schema: 2.0.0
---

# Receive-PSRuleTarget

## SYNOPSIS

Evaluate objects from receivers against matching rules.

## SYNTAX

```text
Receive-PSRuleTarget [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>]
 -Receiver <IPipelineReceiver[]> [-Limit <Int32>] [<CommonParameters>]
```

## DESCRIPTION

Evaluate objects sourced from one or more receivers against matching rules.

This cmdlet works similar to `Invoke-PSRule` except instead of evaluating objects from the pipeline, the objects originate from specific integrations called receivers.

Receivers exists for integration with:

- Azure Storage Queues
- HTTP requests

## EXAMPLES

### Example 1

```powershell
PS C:\> New-PSRuleHttpReceiver -Uri 'http://localhost:5000/' | Receive-PSRuleTarget -Limit 5 -Path '.\docs\scenarios\fruit\';
```

Listen to HTTP requests to _http://localhost:5000/_ and evaluate each request against rules stored in `.\docs\scenarios\fruit\`.

## PARAMETERS

### -Path

One or more paths to search for rule definitions within. If this parameter is not specified the current working path will be used.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: f

Required: False
Position: 1
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

The name of a specific rule to evaluate. If this parameter is not specified all rules in search paths will be evaluated.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: n

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only evaluate rules with the specified tags set. If this parameter is not specified all rules in search paths will be evaluated.

When more then one tag is used, all tags must match. Tag names are not case sensitive, tag values are case sensitive. A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Option

Additional options that configure execution. A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet. Alternatively a hashtable or path to YAML file can be specified with options.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: PSRuleOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Receiver

One or more receivers. At least one receiver must be specified.

```yaml
Type: IPipelineReceiver[]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Limit

Only process the specific number of objects. When the specified number of objects is reached, `Receive-PSRuleTarget` completes.

When `0` or a limit is not specified, the maximum number of objects processed by `Receive-PSRuleTarget` is 2147483647.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
