Add-Type -AssemblyName System.Drawing
$source=Join-Path $PSScriptRoot 'assets/app-icon.png'
$destination=Join-Path $PSScriptRoot 'assets/app.ico'
$image=[Drawing.Image]::FromFile($source)
$frames=@()
foreach($size in @(16,24,32,48,64,128,256)){
 $bitmap=New-Object Drawing.Bitmap $size,$size
 $g=[Drawing.Graphics]::FromImage($bitmap);$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $g.DrawImage($image,0,0,$size,$size)
 $memory=New-Object IO.MemoryStream;$bitmap.Save($memory,[Drawing.Imaging.ImageFormat]::Png)
 $frames+=,@{Size=$size;Bytes=$memory.ToArray()}
 $memory.Dispose();$g.Dispose();$bitmap.Dispose()
}
$image.Dispose()
$stream=[IO.File]::Create($destination);$writer=New-Object IO.BinaryWriter $stream
$writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$frames.Count)
$offset=6+16*$frames.Count
foreach($frame in $frames){$dimension=if($frame.Size -eq 256){0}else{$frame.Size};$writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$frame.Bytes.Length);$writer.Write([uint32]$offset);$offset+=$frame.Bytes.Length}
foreach($frame in $frames){$writer.Write([byte[]]$frame.Bytes)}
$writer.Dispose();$stream.Dispose()
