# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules
#

# Synopsis: Test rule 1
Rule 'FromFile1' -Tag @{ category = "group1"; test = "Test1" } {
    # Pass
    $True;
    $True;
}

# Synopsis: Test rule 2
Rule 'FromFile2' -Tag @{ category = "group1"; test = "Test2" } {
    # Fail
    $False;
    $True;
    $True;
}

# Synopsis: Test rule 3
Rule 'FromFile3' -Tag @{ category = "group1" } {
    # Inconclusive
}

# Synopsis: Test rule 4
Rule 'FromFile4' -Tag @{ category = "group1" } -DependsOn 'FromFile3' {
    # Inconclusive
}

# Synopsis: Null result should fail
Rule 'FromFile5' {
    $Null;
}

# Synopsis: Test for tags
Rule 'WithTag' -Tag @{ severity = 'critical'; feature = 'tag' } {
    $True;
}

# Synopsis: Test for tags
Rule 'WithTag2' -Tag @{ feature = 'tag' } {
    $True;
}

# Synopsis: Test for tags
Rule 'WithTag3' -Tag @{ severity = 'information'; feature = 'tag' } {
    $True;
}

# Synopsis: Test for tags
Rule 'WithTag4' -Tag @{ Severity = 'critical'; feature = 'tag' } {
    $True;
}

# Synopsis: Test for tags
Rule 'WithTag5' -Tag @{ severity = 'Critical'; feature = 'tag' } {
    $True;
}

# Synopsis: Test for type preconditions
Rule 'WithTypeTrue' -Type 'TestType' -Tag @{ category = 'precondition-type' } {
    $True;
}

# Synopsis: Test for type preconditions
Rule 'WithTypeFalse' -Type 'NotTestType' -Tag @{ category = 'precondition-type' } {
    $True;
}

# Synopsis: Test for script preconditions
Rule 'WithPreconditionTrue' -If { $True } -Tag @{ category = 'precondition-if' } {
    $True;
}

# Synopsis: Test for script preconditions
Rule 'WithPreconditionFalse' -If { $False } -Tag @{ category = 'precondition-if' } {
    $True;
}

# Synopsis: Should fail, because of dependency fail
Rule 'WithDependency1' -DependsOn 'WithDependency3','WithDependency2' {
    # Pass
    $True;
}

# Synopsis: Should fail, because of dependency fail
Rule 'WithDependency2' -DependsOn 'WithDependency5' {
    # Pass
    $True;
}

# Synopsis: Should pass, with a passing dependency
Rule 'WithDependency3' -DependsOn 'WithDependency4' {
    # Pass
    $True
}

# Synopsis: Pass
Rule 'WithDependency4' {
    # Pass
    $True
}

# Synopsis: Fail
Rule 'WithDependency5' {
    # Fail
    $False
}

# Synopsis: Test for constrained language
Rule 'ConstrainedTest1' {
    $True;
}

# Synopsis: Test for constrained language, should not execute
Rule 'ConstrainedTest2' {
    $Null = [Console]::WriteLine('Should fail');
    $True;
}

# Synopsis: Test for constrained language, should not execute
Rule 'ConstrainedTest3' -If { $Null = [Console]::WriteLine('Should fail'); return $True; } {
    $True;
}

# Synopsis: Test $PSRule automatic variables
Rule 'VariableContextVariable' {
    $TargetObject.Name -eq $PSRule.TargetName;
    $TargetObject.Type -eq $PSRule.TargetType;
    $TargetObject.Type -eq $PSRule.Field.Kind;
}

# Synopsis: Test $Rule automatic variables
Rule 'WithRuleVariable' {
    $TargetObject.RuleTest -eq $Rule.RuleName;
    $TargetObject.Name -eq $Rule.TargetName;
    $TargetObject.Type -eq $Rule.TargetType;
}

# Synopsis: Test $Configuration automatic variables
Rule 'WithConfiguration' {
    $Configuration.Value1 -eq 1
    $Configuration.Value2 -eq 2
} -Configure @{ Value1 = 2; Value2 = 2; }

Rule 'WithFormat' {
    $TargetObject.spec.properties.kind -eq 'Test'
    ($TargetObject.spec.properties.array.id | Measure-Object).Count -eq 2
    ($TargetObject.spec.properties.array2 | Measure-Object).Count -eq 3
}

# Synopsis: Test $LocalizedData automatic variable
Rule 'WithLocalizedData' {
    Write-Warning -Message ($LocalizedData.WithLocalizedDataMessage -f $TargetObject.Type)
    $LocalizedData.WithLocalizedDataMessage -like "LocalizedMessage for en-*"
}

# Synopsis: Test $PSScriptRoot automatic variable
Rule 'WithPSScriptRoot' {
    $PSScriptRoot -eq $TargetObject.PSScriptRoot
}

# Synopsis: Test $PWD automatic variable
Rule 'WithPWD' {
    $PWD.ToString() -eq $TargetObject.PWD.ToString()
}

# Synopsis: Test $PSCommandPath automatic variable
Rule 'WithPSCommandPath' {
    $PSCommandPath -eq $TargetObject.PSCommandPath
}

Rule 'WithCsv' {
    $True
}

Rule 'WithSleep' {
    Start-Sleep -Milliseconds 50
    $True
}

Rule 'WithNoSynopsis' {
    $True
}

# Synopsis: Test for Recommend keyword
Rule 'RecommendTest' {
    Recommend 'This is a recommendation'
}

# Synopsis: Test for Recommend keyword
Rule 'RecommendTest2' {
    Recommend 'This is a recommendation'
}

# Synopsis: Test for Recommend keyword
Rule 'TestWithSynopsis' {
    $True
}

# Description: Test for Recommend keyword
Rule 'TestWithDescription' {
    $True
}

# Synopsis: Test for Exists keyword
Rule 'ExistsTest' -Tag @{ keyword = 'Exists' } {
    Exists 'Name'
    Exists -Not 'NotName'
    Exists 'Value.Value1'
    Exists 'NotName','Value'
    Exists 'Value', 'Value2' -All
    Exists -Not 'NotName1','NotName2'
    Exists -Not 'Value','NotName2' -All
    @{ Pipeline = 'Value' } | Exists 'Pipeline'
}

# Synopsis: Test for Exists keyword
Rule 'ExistsTestNegative' -Tag @{ keyword = 'Exists' } {
    AnyOf {
        Exists 'NotName'
        Exists 'NotName1','NotName2'
        Exists 'name' -CaseSensitive
        Exists 'NotValue.Value1'
        Exists 'Properties.value'
        Exists -Not 'NotName', 'Value'
        Exists 'NotName', 'Value2' -All
        Exists -Not 'Value','Value2' -All
    }
}

# Synopsis: Test for Exists keyword
Rule 'ExistsCondition' -If { Exists 'Name' } {
    $True
}

# Synopsis: Test for Within keyword
Rule 'WithinTest' -Tag @{ keyword = 'Within' } {
    AnyOf {
        Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms'
        $TargetObject | Within 'Value.Title' 'Mr'
    }
}

# Synopsis: Test for Within keyword
Rule 'WithinTestCaseSensitive' {
    Within 'Title' 'Mr', 'Miss', 'Mrs', 'Ms' -CaseSensitive
}

# Synopsis: Test for Within keyword
Rule 'WithinNot' {
    Within 'Title' 'Mr', 'Sir' -Not
}

# Synopsis: Test for Within keyword
Rule 'WithinLike' {
    Within 'Title' -Like 'M*s'
}

# Synopsis: Test for Within keyword
Rule 'WithinTypes' {
    Within 'BooleanValue' $True
    Within 'IntValue' 0, 1, 2, 3
    Within 'NullValue' $Null
}

# Synopsis: Test for Within keyword
Rule 'WithinCondition' -If { Within 'Name' 'TestObject1' } {
    $True
}

# Synopsis: Test for Match keyword
Rule 'MatchTest' -Tag @{ keyword = 'Match' } {
    AnyOf {
        Match 'PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$', '^(0{1,3})$'
        Match 'Value.PhoneNumber' '^(\+61|0)([0-9] {0,1}){8}[0-9]$'
    }

    # Test pipelining
    [PSCustomObject]@{ Key = 'Value' } | Match 'Key' 'Value'
}

# Synopsis: Test for Match keyword
Rule 'MatchTestCaseSensitive' -Tag @{ keyword = 'Match' } {
    Match 'Title' '^(Mr|Miss|Mrs|Ms)$' -CaseSensitive
}

# Synopsis: Test for Match keyword
Rule 'MatchNot' {
    Match 'Title' '^(Mr|Sir)$' -Not
}

# Synopsis: Test for Match keyword
Rule 'MatchCondition' -If { Match -Field Name -Expression 'TestObject1' } {
    $True
}

# Synopsis: Test for TypeOf keyword
Rule 'TypeOfTest' {
    TypeOf 'System.Collections.Hashtable', 'PSRule.Test.OtherType'

    # Test pipelining
    $inlineObject = [PSCustomObject]@{ Key = 'Value' }
    $inlineObject.PSObject.TypeNames.Add('PSRule.Test.OtherOtherType')
    $inlineObject | TypeOf 'PSRule.Test.OtherOtherType'
}

# Synopsis: Test for TypeOf keyword
Rule 'TypeOfCondition' -If { TypeOf 'PSRule.Test.OtherType' } {
    $True
}

# Synopsis: Test for AllOf keyword
Rule 'AllOfTest' {
    AllOf {
        $True
        $True
    }
}

# Synopsis: Test for AllOf keyword
Rule 'AllOfTestNegative' {
    AllOf {
        $True
        $False
    }
}

# Synopsis: Test for AnyOf keyword
Rule 'AnyOfTest' {
    AnyOf {
        $True
        $False
        $False
    }
}

# Synopsis: Test for AnyOf keyword
Rule 'AnyOfTestNegative' {
    AnyOf {
        $False
        $False
        $False
    }
}

# Synopsis: Test for Reason keyword
Rule 'ReasonTest' {
    Reason "This is a reason for $($TargetObject.Name)"
    $False
}

# Synopsis: Test for Reason keyword
Rule 'ReasonTest2' {
    Reason 'This is a reason 1'
    Reason 'This is a reason 2'
    $False
}

# Synopsis: Test for Reason keyword
Rule 'ReasonTest3' {
    Reason 'This is a reason'
    $True
}
