using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
public sealed class ThemeManager : IDisposable {
 readonly Window window;
 readonly Action<bool> onChange;
 bool? light;
 bool disposed;
 bool? manual;
 readonly string preference=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CodexQuotaWidget","theme.txt");
 public ThemeManager(Window target,Action<bool> changed){
  window=target;onChange=changed;try{if(System.IO.File.Exists(preference))manual=System.IO.File.ReadAllText(preference).Trim()=="light";}catch{}Refresh();SystemEvents.UserPreferenceChanged+=PreferenceChanged;
 }
 void PreferenceChanged(object sender,UserPreferenceChangedEventArgs args){
  if(disposed||window.Dispatcher.HasShutdownStarted)return;
  window.Dispatcher.BeginInvoke(new Action(()=>{if(!disposed)Refresh();}));
 }
 public static bool ReadSystemLight(){
  try{using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")){
   object value=key==null?null:key.GetValue("SystemUsesLightTheme");
   // Follow the Windows shell/taskbar mode. Fall back only if the system value
   // is unavailable, not when system and application modes intentionally differ.
   if(value==null&&key!=null)value=key.GetValue("AppsUseLightTheme");
   return value==null||Convert.ToInt32(value)!=0;
  }}catch{return true;}
 }
 public void Refresh(){bool next=manual??ReadSystemLight();if(light!=next)Apply(next);}
 public bool IsLight {get{return light??true;}}
 public void Toggle(){manual=!IsLight;Apply(manual.Value);try{System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(preference));System.IO.File.WriteAllText(preference,manual.Value?"light":"dark");}catch{}}
 public void Apply(bool isLight){
  light=isLight;
  Color surface = isLight ? Colors.White : Color.FromRgb(47, 52, 62);
  Color highlight = isLight ? Colors.White : Color.FromRgb(73, 81, 95);
  Color shadow = isLight ? Color.FromRgb(128, 128, 128) : Color.FromRgb(15, 18, 24);
  Color recess = isLight ? Color.FromRgb(225, 225, 225) : Color.FromRgb(29, 33, 41);

  Brush face = Frozen(new SolidColorBrush(surface));
  Brush rim = Gradient(
   isLight ? Colors.White : Color.FromRgb(66, 73, 86),
   isLight ? Color.FromRgb(224, 224, 224) : Color.FromRgb(33, 37, 45));
  window.Resources["SurfaceTint"] = face;
  window.Resources["CardTint"] = face;
  window.Resources["ButtonTint"] = face;
  window.Resources["Stroke"] = rim;
  window.Resources["CardStroke"] = rim;
  window.Resources["CardBorderThickness"] = new Thickness(isLight ? 0 : 0.5);
  window.Resources["ButtonStroke"] = rim;
  window.Resources["Track"] = Frozen(new LinearGradientBrush(recess, surface, new Point(0.5, 0), new Point(0.5, 1)));
  window.Resources["ProgressFill"] = Frozen(new SolidColorBrush(Color.FromRgb(2, 133, 255)));
  window.Resources["ProgressTrack"] = Palette(isLight, "#EBEBEB", "#232731");
  window.Resources["RecessEdge"] = Gradient(shadow, highlight);
  window.Resources["Hover"] = Gradient(highlight, surface);
  window.Resources["TextPrimary"] = Palette(isLight, "#202C40", "#F4F6FC");
  window.Resources["TextSecondary"] = Palette(isLight, "#42536A", "#CFD7E4");
  window.Resources["TextMuted"] = Palette(isLight, "#4C5D74", "#B7C3D4");
  window.Resources["FiveAccent"] = Palette(isLight, "#205DB7", "#8EBBFF");
  window.Resources["StatusAccent"] = Palette(isLight, "#22694A", "#91DBB4");

  window.Resources["SoftShadow"] = Shadow(shadow, isLight ? 9 : 13, isLight ? 2 : 4, 315, isLight ? 0.24 : 0.78);
  window.Resources["SoftHighlight"] = Shadow(highlight, 10, 3, 135, isLight ? 0.9 : 0.72);
  window.Resources["OuterHighlight"] = Shadow(highlight, 13, 3, 135, isLight ? 0.15 : 0.42);
  window.Resources["OuterShadow"] = Shadow(shadow, 13, 4, 315, isLight ? 0.12 : 0.68);

  window.Resources["FrameHeight"] = 446.0;
  window.Resources["WindowInset"] = new Thickness(16);
  window.Width = 344;
  window.Height = 388.8;
  window.Resources["BrandSize"] = 26.0;
  window.Resources["HeaderSize"] = 12.0;
  window.Resources["HeadingSize"] = 19.0;
  window.Resources["PercentSize"] = 26.0;
  window.Resources["CountdownSize"] = 23.0;
  window.Resources["SmallSize"] = 13.0;
  window.Resources["CardPadding"] = new Thickness(18, 12, 18, 12);
  window.Resources["CardHeight"] = 154.0;
  onChange(isLight);
 }
 static Brush Palette(bool isLight, string day, string night) {
  return Frozen(new SolidColorBrush((Color)ColorConverter.ConvertFromString(isLight ? day : night)));
 }
 static Brush Gradient(Color topLeft, Color bottomRight) {
  return Frozen(new LinearGradientBrush(topLeft, bottomRight, new Point(0, 0), new Point(1, 1)));
 }
 static T Frozen<T>(T resource) where T : Freezable {
  resource.Freeze();
  return resource;
 }
 static DropShadowEffect Shadow(Color color, double blur, double depth, double direction, double opacity) {
  return Frozen(new DropShadowEffect {
   Color = color, BlurRadius = blur, ShadowDepth = depth, Direction = direction, Opacity = opacity
  });
 }
 public void Dispose(){disposed=true;SystemEvents.UserPreferenceChanged-=PreferenceChanged;}
}
