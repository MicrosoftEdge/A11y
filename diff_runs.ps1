<#
.SYNOPSIS
    This will output the difference between runs of A11y, by default the first and last runs
    Use the LastTwoRuns switch to output the two most recent runs instead

.PARAMETER LastTwoRuns
    Add this switch if you want to diff the last two runs instead of the first and last
#>
param([switch]$LastTwoRuns)

$scores  = Import-Csv .\scores.csv

switch($LastTwoRuns){
    $true {$firstRun = $scores.Length - 2; break}
    default {$firstRun = 0; break}
}

if($scores.Length -gt 1){
        $scores |
        gm -MemberType NoteProperty |
        select -expand Name |
        % {Compare-Object $scores[$firstRun] $scores[$scores.Length - 1] -Property $_ |
        Format-Table
    }
} else {
    "Run at least twice to diff"
}
