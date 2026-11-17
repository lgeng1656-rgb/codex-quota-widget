using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
public sealed class ThemeManager : IDisposable {
 readonly Window window;
 readonly Action<bool> onChange;
 bool? light;
 bool disposed;
 public ThemeManager(Window target,Action<bool> changed){
  window=target;onChange=changed;Refresh();SystemEvents.UserPreferenceChanged+=PreferenceChanged;
 }
 void PreferenceChanged(object sender,UserPreferenceChangedEventArgs args){
  if(disposed||window.Dispatcher.HasShutdownStarted)return;
  window.Dispatcher.BeginInvoke(new Action(()=>{if(!disposed)Refresh();}));
 }
 public static bool ReadSystemLight(){
  try{using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")){
   object value=key==null?null:key.GetValue("AppsUseLightTheme");return value==null||Convert.ToInt32(value)!=0;
  }}catch{return true;}
 }
 public void Refresh(){bool next=ReadSystemLight();if(light!=next)Apply(next);}
 public void Apply(bool isLight){
  light=isLight;
  string[] keys={"TextPrimary","Fallback","Stroke","SurfaceTint","TextSecondary","Divider","FiveAccent","Track","TextMuted","WeekAccent","StatusAccent","Hover","Pressed","CardTint"};
  string[] dark={"#FFFFFF","#4A566D","#60FFFFFF","#633B4C69","#E4E9F3","#40FFFFFF","#9AF5BE","#466C7B99","#D1DAE9","#C3ADFF","#B5F5D0","#20FFFFFF","#35FFFFFF","#12FFFFFF"};
  string[] day={"#202C40","#EDF2F8","#82FFFFFF","#82F5F8FF","#43526A","#283C506C","#168351","#263E597A","#536079","#7550BC","#236F4C","#14334D70","#24334D70","#28FFFFFF"};
  string[] values=isLight?day:dark;
  for(int i=0;i<keys.Length;i++){var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(values[i]));brush.Freeze();window.Resources[keys[i]]=brush;}
  onChange(isLight);
 }
 public void Dispose(){disposed=true;SystemEvents.UserPreferenceChanged-=PreferenceChanged;}
}
