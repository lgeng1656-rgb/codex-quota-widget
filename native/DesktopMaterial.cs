using System;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using System.Windows.Forms;
// Only reads the user's wallpaper file. Never captures the screen or other windows.
public sealed class DesktopMaterial {
 BitmapSource wallpaper;
 readonly Border backdrop;
 public bool Light {get;set;}
 public DesktopMaterial(Border target){backdrop=target;Reload();}
 public void Reload(){
  try{
   string path;
   using(var key=Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))path=key==null?null:key.GetValue("WallPaper") as string;
   if(String.IsNullOrEmpty(path)||!File.Exists(path)){wallpaper=null;return;}
   var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.DecodePixelWidth=1024;bitmap.UriSource=new Uri(path);bitmap.EndInit();bitmap.Freeze();
   // Blur once at load, rather than rerunning blur while the window moves.
   int width=bitmap.PixelWidth,height=bitmap.PixelHeight;
   var image=new System.Windows.Controls.Image{Source=bitmap,Width=width,Height=height,Stretch=Stretch.Fill,Effect=new BlurEffect{Radius=24,RenderingBias=RenderingBias.Performance}};
   image.Measure(new System.Windows.Size(width,height));image.Arrange(new Rect(0,0,width,height));image.UpdateLayout();
   var blurred=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);blurred.Render(image);blurred.Freeze();wallpaper=blurred;
  }catch{wallpaper=null;}
 }
 public bool HasWallpaper {get{return wallpaper!=null;}}
 public void Update(IntPtr hwnd){
  if(wallpaper==null){backdrop.Background=new SolidColorBrush(Light?Color.FromRgb(237,242,248):Color.FromRgb(74,86,109));return;}
  var monitor=Screen.FromHandle(hwnd).Bounds;
  var rect=Glass.WindowBounds(hwnd);
  double scale=Math.Max((double)monitor.Width/wallpaper.PixelWidth,(double)monitor.Height/wallpaper.PixelHeight);
  double imageWidth=wallpaper.PixelWidth*scale,imageHeight=wallpaper.PixelHeight*scale;
  double imageLeft=monitor.Left+(monitor.Width-imageWidth)/2,imageTop=monitor.Top+(monitor.Height-imageHeight)/2;
  var brush=new ImageBrush(wallpaper){ViewboxUnits=BrushMappingMode.RelativeToBoundingBox,Viewbox=new Rect((rect[0]-imageLeft)/imageWidth,(rect[1]-imageTop)/imageHeight,(rect[2]-rect[0])/imageWidth,(rect[3]-rect[1])/imageHeight),Stretch=Stretch.Fill,TileMode=TileMode.None};
  backdrop.Background=brush;
 }
}
