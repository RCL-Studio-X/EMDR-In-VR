$repoPath = "C:\Users\tchimhan\MT Hope VR"

while ($true) {

    git fetch
    $status = git status -uno

    if ($status -match "behind") {
        Write-Host "Remote changes detected. Pulling..."
        git pull
    }

    Start-Sleep -Seconds 30
}