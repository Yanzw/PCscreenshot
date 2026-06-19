Add-Type -AssemblyName System.Drawing

function Render-Frame([int]$S) {
    $bmp = New-Object System.Drawing.Bitmap($S, $S, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode     = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode  = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode    = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $m   = $S * 0.06
    $rad = $S * 0.20
    $rect = New-Object System.Drawing.RectangleF($m, $m, ($S - 2*$m), ($S - 2*$m))

    # rounded-rect path
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $rad * 2
    $path.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
    $path.AddArc($rect.Right - $d, $rect.Y, $d, $d, 270, 90)
    $path.AddArc($rect.Right - $d, $rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()

    # blue gradient background
    $c1 = [System.Drawing.Color]::FromArgb(255, 56, 178, 255)   # sky blue
    $c2 = [System.Drawing.Color]::FromArgb(255, 13, 99, 220)    # deep blue
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $c1, $c2, 60.0)
    $g.FillPath($brush, $path)

    $white = [System.Drawing.Color]::White

    # four corner brackets (screenshot selection metaphor)
    $bt  = [Math]::Max(2.0, $S * 0.07)          # bracket thickness
    $bl  = $S * 0.20                            # bracket arm length
    $pad = $S * 0.26                            # distance from edge
    $pen = New-Object System.Drawing.Pen($white, $bt)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round

    $L = $pad
    $R = $S - $pad
    # top-left
    $g.DrawLine($pen, $L, $L, ($L + $bl), $L)
    $g.DrawLine($pen, $L, $L, $L, ($L + $bl))
    # top-right
    $g.DrawLine($pen, $R, $L, ($R - $bl), $L)
    $g.DrawLine($pen, $R, $L, $R, ($L + $bl))
    # bottom-left
    $g.DrawLine($pen, $L, $R, ($L + $bl), $R)
    $g.DrawLine($pen, $L, $R, $L, ($R - $bl))
    # bottom-right
    $g.DrawLine($pen, $R, $R, ($R - $bl), $R)
    $g.DrawLine($pen, $R, $R, $R, ($R - $bl))

    # center crosshair (+) for the capture feel
    $ct = [Math]::Max(2.0, $S * 0.055)
    $cl = $S * 0.13
    $cx = $S / 2.0
    $cy = $S / 2.0
    $pen2 = New-Object System.Drawing.Pen($white, $ct)
    $pen2.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen2.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($pen2, ($cx - $cl), $cy, ($cx + $cl), $cy)
    $g.DrawLine($pen2, $cx, ($cy - $cl), $cx, ($cy + $cl))

    $pen.Dispose(); $pen2.Dispose(); $brush.Dispose(); $path.Dispose(); $g.Dispose()
    return $bmp
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$pngs = @()
foreach ($s in $sizes) {
    $bmp = Render-Frame $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngs += ,($ms.ToArray())
    $bmp.Dispose(); $ms.Dispose()
}

# ---- build .ico container ----
$out = New-Object System.IO.MemoryStream
$bw  = New-Object System.IO.BinaryWriter($out)
$bw.Write([UInt16]0)              # reserved
$bw.Write([UInt16]1)              # type = icon
$bw.Write([UInt16]$sizes.Count)   # image count

$offset = 6 + (16 * $sizes.Count)
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $len = $pngs[$i].Length
    $dim = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$dim)         # width
    $bw.Write([byte]$dim)         # height
    $bw.Write([byte]0)            # palette
    $bw.Write([byte]0)            # reserved
    $bw.Write([UInt16]1)          # color planes
    $bw.Write([UInt16]32)         # bits per pixel
    $bw.Write([UInt32]$len)       # data size
    $bw.Write([UInt32]$offset)    # data offset
    $offset += $len
}
foreach ($p in $pngs) { $bw.Write($p) }
$bw.Flush()

$target = Join-Path $PSScriptRoot "app.ico"
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$bw.Dispose(); $out.Dispose()
Write-Output "Icon written: $target ($((Get-Item $target).Length) bytes)"
