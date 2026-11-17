using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Reflection;
[assembly: AssemblyTitle("Codex Quota Widget")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
class Program {
 static Window window; static IntPtr hwnd; static bool busy,closed,failed;
 static ThemeManager theme;
 static DesktopMaterial material; static DateTime materialReload=DateTime.MinValue;
 static Process active; static DateTime next=DateTime.MinValue,updated;
 static Dictionary<string,object> five,week;
 static readonly JavaScriptSerializer json=new JavaScriptSerializer();
 static T UI<T>(string name) where T:class {return window.FindName(name) as T;}
 [STAThread] static int Main(string[] args) {
  bool verify=args.Contains("--verify-layout"),quota=args.Contains("--verify-quota");
  try {
   if(quota){var r=ReadQuota().GetAwaiter().GetResult();File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"quota-check.txt"),"QUOTA_OK: "+(r.ContainsKey("rateLimits")?"account quota returned":"response received"));return 0;}
   bool created;
   using(var mutex=new Mutex(true,"Local\\CodexQuotaWidget-"+Environment.UserName,out created)) {
    if(!created&&!verify){MessageBox.Show("Codex 额度组件已经在运行，请查看桌面或任务栏。","Codex 额度");return 0;}
    var app=new Application();
    using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Widget.xaml"))window=(Window)XamlReader.Load(stream);
    window.Title="Codex 额度 · 1.1.0";
    using(var icon=Assembly.GetExecutingAssembly().GetManifestResourceStream("App.ico"))window.Icon=System.Windows.Media.Imaging.BitmapFrame.Create(icon,System.Windows.Media.Imaging.BitmapCreateOptions.None,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
    material=new DesktopMaterial(UI<Border>("Backdrop"));
    theme=new ThemeManager(window,light=>{material.Light=light;if(hwnd!=IntPtr.Zero)material.Update(hwnd);});
    var menu=new ContextMenu();var exit=new MenuItem{Header="退出组件"};exit.Click+=(s,e)=>window.Close();menu.Items.Add(exit);window.ContextMenu=menu;
    UI<Button>("Pin").Click+=(s,e)=>{window.Topmost=!window.Topmost;UI<Button>("Pin").ToolTip=window.Topmost?"取消置顶":"置顶";UI<Button>("Pin").Opacity=window.Topmost?1:0.7;UI<Button>("Pin").Foreground=window.Topmost?(Brush)window.Resources["FiveAccent"]:(Brush)window.Resources["TextSecondary"];};
    UI<Button>("Refresh").Click+=async(s,e)=>await Refresh();
    window.SourceInitialized+=(s,e)=>{
     hwnd=new WindowInteropHelper(window).Handle;
     HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor=Colors.Transparent;
     theme.Refresh();Glass.Apply(hwnd);Glass.Attach(hwnd,window.Width);
     window.Left=SystemParameters.WorkArea.Right-window.Width-24;window.Top=60;
    };
    window.PreviewMouseLeftButtonDown+=(s,e)=>{
     DependencyObject source=e.OriginalSource as DependencyObject;
     while(source!=null){if(source is ButtonBase)return;source=source is Visual?VisualTreeHelper.GetParent(source):LogicalTreeHelper.GetParent(source);}
     Glass.StartDrag(hwnd);e.Handled=true;
    };
    // Clip the alpha content itself, including the blur/tint silhouette.
    Action clip=()=>{if(window.ActualWidth>0){window.Clip=new RectangleGeometry(new Rect(0,0,window.ActualWidth,window.ActualHeight),25.6,25.6);Glass.Clip(hwnd,window.ActualWidth);}};
    window.SizeChanged+=(s,e)=>{if(hwnd!=IntPtr.Zero)clip();};
    window.LocationChanged+=(s,e)=>{if(hwnd!=IntPtr.Zero)material.Update(hwnd);};
    var timer=new DispatcherTimer {Interval=TimeSpan.FromSeconds(1)};
    timer.Tick+=async(s,e)=>{theme.Refresh();if(DateTime.UtcNow>=materialReload){material.Reload();material.Update(hwnd);materialReload=DateTime.UtcNow.AddMinutes(1);}Render("Five",five);Render("Week",week);if(!busy&&DateTime.UtcNow>=next)await Refresh();};
    window.ContentRendered+=(s,e)=>{
     clip();material.Update(hwnd);
     if(verify){
      if(window.ShowInTaskbar||!Glass.IsToolWindow(hwnd))throw new Exception("Taskbar/tool-window configuration failed");
      if(window.Width!=312||Math.Abs(window.Height-338.4)>0.01||window.Icon==null)throw new Exception("Scale or icon failed");
      if(!window.AllowsTransparency||!(window.Clip is RectangleGeometry)||!Glass.VerifySoftBoundary(hwnd))throw new Exception("Alpha silhouette or soft boundary failed");
      foreach(string name in new[]{"FiveBar","WeekBar"}){var bar=UI<ProgressBar>(name);bar.Value=50;bar.ApplyTemplate();bar.UpdateLayout();var part=bar.Template.FindName("PART_Indicator",bar) as Border;if(part==null||part.CornerRadius.TopLeft!=4.5||Math.Abs(part.ActualWidth-bar.ActualWidth/2)>1)throw new Exception("Progress template failed");}
      var button=UI<Button>("Refresh");button.ApplyTemplate();if(button.Template.FindName("ButtonSurface",button)==null)throw new Exception("Button template failed");
      theme.Apply(false);window.UpdateLayout();
      if(((SolidColorBrush)window.Foreground).Color!=Colors.White)throw new Exception("Dark palette failed");
      theme.Apply(true);window.UpdateLayout();
      if(((SolidColorBrush)window.Foreground).Color!=Color.FromRgb(32,44,64))throw new Exception("Day palette failed");
      if(UI<Border>("Surface").Background!=window.Resources["SurfaceTint"])throw new Exception("Live theme resource update failed");
      theme.Refresh();
      File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"layout-check.txt"),"LAYOUT_OK: 80 percent scale; embedded icon; wallpaper-only material; rounded alpha corners; rounded progress; custom buttons; live dark/day palettes; system theme read");window.Close();
     }else timer.Start();
    };
    window.Closed+=(s,e)=>{closed=true;timer.Stop();theme.Dispose();if(active!=null)try{if(!active.HasExited)active.Kill();}catch{}};
    app.Run(window);return 0;
   }
  }catch(Exception ex){if(verify||quota){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"check-error.txt"),ex.Message);return 1;}MessageBox.Show("无法启动组件："+ex.Message,"Codex 额度");return 1;}
 }
 static async Task Refresh(){
  if(busy||closed)return;busy=true;UI<Button>("Refresh").IsEnabled=false;UI<TextBlock>("Status").Text="正在刷新…";
  try{
   var result=await ReadQuota();if(closed)return;
   var limits=Obj(result,"rateLimits");var buckets=Obj(result,"rateLimitsByLimitId");if(buckets!=null&&Obj(buckets,"codex")!=null)limits=Obj(buckets,"codex");
   if(limits==null)throw new Exception("Codex 未返回额度。请先在 Codex 中登录 ChatGPT 账户。");
   five=null;week=null;
   foreach(var item in new[]{Obj(limits,"primary"),Obj(limits,"secondary")}){if(item==null)continue;double minutes=Number(item,"windowDurationMins");if(minutes==300)five=item;if(minutes==10080)week=item;}
   failed=false;updated=DateTime.Now;UI<TextBlock>("Status").ToolTip=null;
  }catch(Exception ex){failed=true;if(!closed)UI<TextBlock>("Status").ToolTip=ex.Message;}
  finally{busy=false;next=DateTime.UtcNow.AddSeconds(failed?15:60);if(!closed){UI<Button>("Refresh").IsEnabled=true;UI<TextBlock>("Status").Text=failed?"读取失败 · 悬停查看原因":"更新于 "+updated.ToString("HH:mm:ss");Render("Five",five);Render("Week",week);}}
 }
 static Dictionary<string,object> Obj(Dictionary<string,object> d,string key){object value;return d!=null&&d.TryGetValue(key,out value)?value as Dictionary<string,object>:null;}
 static double Number(Dictionary<string,object> d,string key){object value;return d!=null&&d.TryGetValue(key,out value)&&value!=null?Convert.ToDouble(value):-1;}
 static void Render(string prefix,Dictionary<string,object> data){
  if(data==null){UI<TextBlock>(prefix+"Percent").Text="—";UI<ProgressBar>(prefix+"Bar").Value=0;UI<TextBlock>(prefix+"Countdown").Text=busy?"正在读取…":"暂无额度数据";UI<TextBlock>(prefix+"Date").Text="需安装 Codex 并登录 ChatGPT 账户";return;}
  double remaining=Math.Max(0,Math.Min(100,100-Number(data,"usedPercent")));UI<TextBlock>(prefix+"Percent").Text=remaining.ToString("0.#")+"% 剩余";UI<ProgressBar>(prefix+"Bar").Value=remaining;
  double unix=Number(data,"resetsAt");if(unix<0){UI<TextBlock>(prefix+"Countdown").Text="重置时间未知";UI<TextBlock>(prefix+"Date").Text="服务未返回时间";return;}
  var reset=DateTimeOffset.FromUnixTimeSeconds((long)unix);var delta=reset-DateTimeOffset.UtcNow;
  UI<TextBlock>(prefix+"Countdown").Text=delta.TotalSeconds<=0?"已到重置时间，待刷新":delta.Days>0?String.Format("{0}天 {1:00}小时 {2:00}分",delta.Days,delta.Hours,delta.Minutes):String.Format("{0:00}小时 {1:00}分 {2:00}秒",delta.Hours,delta.Minutes,delta.Seconds);
  UI<TextBlock>(prefix+"Date").Text=reset.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy年M月d日 HH:mm")+" 重置";
 }
 static string Locate(){
  var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"OpenAI","Codex","bin");
  if(Directory.Exists(root)){var files=Directory.GetDirectories(root).Select(d=>Path.Combine(d,"codex.exe")).Where(File.Exists).OrderByDescending(File.GetLastWriteTimeUtc).ToArray();if(files.Length>0)return files[0];}
  foreach(var dir in (Environment.GetEnvironmentVariable("PATH")??"").Split(';')){try{var file=Path.Combine(dir.Trim('"'),"codex.exe");if(File.Exists(file))return file;}catch{}}
  throw new Exception("未找到 Codex。请先安装并登录 Codex，再重新打开组件。");
 }
 static async Task<Dictionary<string,object>> ReadQuota(){
  var completion=new TaskCompletionSource<Dictionary<string,object>>();
  using(var process=new Process()){
   process.StartInfo=new ProcessStartInfo(Locate(),"app-server --stdio"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8};
   process.EnableRaisingEvents=true;
   process.OutputDataReceived+=(s,e)=>{
    if(e.Data==null)return;
    try{var message=json.Deserialize<Dictionary<string,object>>(e.Data);if(!message.ContainsKey("id"))return;int id=Convert.ToInt32(message["id"]);
     if(message.ContainsKey("error")){completion.TrySetException(new Exception("Codex 接口返回错误，请检查账户登录和网络。"));return;}
     if(id==1){process.StandardInput.WriteLine("{\"method\":\"initialized\"}");process.StandardInput.WriteLine("{\"id\":2,\"method\":\"account/rateLimits/read\"}");process.StandardInput.Flush();}
     if(id==2){var result=Obj(message,"result");if(result!=null)completion.TrySetResult(result);else completion.TrySetException(new Exception("额度数据格式异常"));}
    }catch{completion.TrySetException(new Exception("无法解析 Codex 额度响应"));}
   };
   process.ErrorDataReceived+=(s,e)=>{}; // Drain stderr without retaining account or configuration details.
   process.Exited+=(s,e)=>completion.TrySetException(new Exception("Codex 额度服务已退出，请确认 Codex 能正常运行。"));
   try{
    process.Start();active=process;process.BeginOutputReadLine();process.BeginErrorReadLine();
    process.StandardInput.WriteLine("{\"id\":1,\"method\":\"initialize\",\"params\":{\"clientInfo\":{\"name\":\"quota_widget\",\"version\":\"1.0.0\"}}}");process.StandardInput.Flush();
    if(await Task.WhenAny(completion.Task,Task.Delay(25000))!=completion.Task)throw new Exception("读取超时，请检查 Codex 登录及网络，15 秒后自动重试。");
    return await completion.Task;
   }finally{try{if(!process.HasExited)process.Kill();}catch{}active=null;}
  }
 }
}
