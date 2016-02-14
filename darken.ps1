Add-Type -AssemblyName System.Windows.Forms
Add-Type -ReferencedAssemblies @("System.Windows.Forms", "System.Drawing") -Path $args[0]
$layer = new-object DarkenWind.Layer($args[1], $args[2])
[System.Windows.Forms.Application]::Run($layer)
