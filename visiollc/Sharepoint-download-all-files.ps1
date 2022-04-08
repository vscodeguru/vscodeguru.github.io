#https://zilliontech.sharepoint.com/sites/ZillionIndiaOffice/Shared%20Documents/General/Ameren%20-%20Prabakaran%20S%20-%202021%20-%20Weekly%20Status%20Report.xlsx
$SiteURL = 'https://zilliontech.sharepoint.com/sites/ZillionIndiaOffice'
$SourceFile = '/Shared Documents/General/Ameren - Prabakaran S - 2021 - Weekly Status Report.xlsx'
$folderName = "/Shared Documents/General/Pre-Production/ITBEAS_BF_STA101 Status of Allowance/Artifacts"
$DownloadPath = 'C:\Users\sarav\Downloads\Git Files\download\'
$Filename = 'Ameren - Prabakaran S - 2021 - Weekly Status Report.xlsx'
 
 
$outputFileStream = [System.IO.FileStream]::new($DownloadPath + $Filename, [System.IO.FileMode]::Create)
try {
    $devConn = Connect-PnPOnline -Url $SiteURL -UseWebLogin
    $sourceFileStream = Get-PnPFile -Url $SourceFile -AsMemoryStream -Connection $devConn
    $sourceFileStream.WriteTo($outputFileStream)
    Write-Host "done"
}
catch {
    Write-Host -f Red "Error:" $_.Exception.Message
}
finally
{
    if($outputFileStream) {
        $outputFileStream.close()
    }
}
#