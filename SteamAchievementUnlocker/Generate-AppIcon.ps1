param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "App.ico")
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

function New-IconPng {
    param([int]$Size)

    $visual = [System.Windows.Media.DrawingVisual]::new()
    $drawing = $visual.RenderOpen()

    $background = [System.Windows.Media.SolidColorBrush]::new(
        [System.Windows.Media.Color]::FromRgb(0x4F, 0x70, 0xB1))
    $background.Freeze()

    $inset = [Math]::Max(0.5, $Size * 0.025)
    $radius = $Size * 0.23
    $drawing.DrawRoundedRectangle(
        $background,
        $null,
        [System.Windows.Rect]::new($inset, $inset, $Size - (2 * $inset), $Size - (2 * $inset)),
        $radius,
        $radius)

    $trophy = [System.Windows.Media.Geometry]::Parse(
        "M6,2 H18 V5 H21 V8 C21,10.76 18.76,13 16,13 H15.82 C15.4,15.02 13.93,16.65 12,17.32 V20 H16 V22 H8 V20 H12 V17.32 C10.07,16.65 8.6,15.02 8.18,13 H8 C5.24,13 3,10.76 3,8 V5 H6 Z").Clone()
    $bounds = $trophy.Bounds
    $targetSize = $Size * 0.58
    $scale = $targetSize / [Math]::Max($bounds.Width, $bounds.Height)
    $scaledWidth = $bounds.Width * $scale
    $scaledHeight = $bounds.Height * $scale
    $offsetX = (($Size - $scaledWidth) / 2) - ($bounds.X * $scale)
    $offsetY = (($Size - $scaledHeight) / 2) - ($bounds.Y * $scale)

    $transform = [System.Windows.Media.TransformGroup]::new()
    $transform.Children.Add([System.Windows.Media.ScaleTransform]::new($scale, $scale))
    $transform.Children.Add([System.Windows.Media.TranslateTransform]::new($offsetX, $offsetY))
    $trophy.Transform = $transform

    $foreground = [System.Windows.Media.SolidColorBrush]::new([System.Windows.Media.Colors]::White)
    $foreground.Freeze()
    $drawing.DrawGeometry($foreground, $null, $trophy)
    $drawing.Close()

    $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
        $Size,
        $Size,
        96,
        96,
        [System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)

    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = [System.IO.MemoryStream]::new()
    $encoder.Save($stream)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    return ,$bytes
}

$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$images = foreach ($size in $sizes) {
    [PSCustomObject]@{
        Size = $size
        Data = New-IconPng -Size $size
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}

$file = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create)
$writer = [System.IO.BinaryWriter]::new($file)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$images.Count)

    $dataOffset = 6 + (16 * $images.Count)
    foreach ($image in $images) {
        $dimension = if ($image.Size -ge 256) { 0 } else { $image.Size }
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$image.Data.Length)
        $writer.Write([uint32]$dataOffset)
        $dataOffset += $image.Data.Length
    }

    foreach ($image in $images) {
        $writer.Write($image.Data)
    }
}
finally {
    $writer.Dispose()
    $file.Dispose()
}

Write-Host "Generated $OutputPath"
